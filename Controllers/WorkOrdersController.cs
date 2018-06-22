using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using log4net;
using System.Data.Entity.Infrastructure;

namespace MasterDetail.Controllers
{
    [Authorize]
    public class WorkOrdersController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();
       //  private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: WorkOrders
        public async Task<ActionResult> Index()
        {

            //  log.Error("test error q111..", null);
            //Log4NetHelper.Log("Hello, sailor!", LogLevel.INFO, "TEST", 0, "Tester", null);

            var workOrders = _applicationDbContext.WorkOrders.Include(w => w.CurrentWorker).Include(w => w.Customer);
            return View(await workOrders.ToListAsync());
        }

     
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }
            return View(workOrder);
        }

        
        public ActionResult Create()
        {
            //ViewBag.CurrentWorkerId = new SelectList(_applicationDbContext.ApplicationUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers.Where(c => c.Cloaked == false), "CustomerId", "CompanyName");
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {

                workOrder.CurrentWorkerId = User.Identity.GetUserId();
                _applicationDbContext.WorkOrders.Add(workOrder);
                await _applicationDbContext.SaveChangesAsync();

                Log4NetHelper.Log(String.Format("Work Order {0} created", workOrder.WorkOrderId), LogLevel.INFO, "WorkOrders", workOrder.WorkOrderId, User.Identity.Name, null);

                return RedirectToAction("Edit", new { controller = "WorkOrders", action = "Edit", Id = workOrder.WorkOrderId });
            }


            ViewBag.CustomerId = new SelectList(_applicationDbContext.Customers.Where(c => c.Cloaked == false), "CustomerId", "AccountNumber", workOrder.CustomerId);
            return View(workOrder);
        }

        
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }

            //If a different user has claimed the work order since you refreshed the worklist, redirect to worklist error message
            if (workOrder.CurrentWorkerId != null && workOrder.CurrentWorkerId != User.Identity.GetUserId())
            {
                //per trovare il nome dell'utente che ha lo wo dallo id
                ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string claimedUserName = userManager.FindByEmail(workOrder.CurrentWorkerId).UserName;

                string message = String.Format("User {0} has claimed work order {1} before user {2} could, and so the work order remains claimed by {0}", workOrder.CurrentWorkerName, workOrder.WorkOrderId, User.Identity.Name);

                TempData["MessageToClient"] = message;

                Log4NetHelper.Log(message, LogLevel.INFO, workOrder.EntityFormalNamePlural, workOrder.WorkOrderId, User.Identity.Name, null);


                return RedirectToAction("Index", "WorkList");
            }

            if (workOrder.Status.Substring(workOrder.Status.Length - 3, 3) != "ing")
                return View("Claim", workOrder);
        
            return View(workOrder.Status, workOrder);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId,ReworkNotes,RowVersion")] WorkOrder workOrder, string command)
        {
            
            if (ModelState.IsValid)
            {

                //Populate Parts and Labors
                workOrder.Parts = _applicationDbContext.Parts.Where(p => p.WorkOrderId == workOrder.WorkOrderId).ToList();
                workOrder.Labors = _applicationDbContext.Labors.Where(p => p.WorkOrderId == workOrder.WorkOrderId).ToList();

                PromotionResult promotionResult = new PromotionResult();

                if (command == "Save")
                    promotionResult.Success = true;
                else if (command == "Claim")
                    promotionResult = workOrder.ClaimWorkListItem(User.Identity.GetUserId());
                else if (command == "Relinquish")
                    promotionResult = workOrder.RelinquishWorkListItem();
                else
                    promotionResult = workOrder.PromoteWorkListItem(command);

                if (!promotionResult.Success)
                    TempData["MessageToClient"] = promotionResult.Message;

                //try
                //{
                //    var zero = 0;
                //    var test = 1 / zero;
                //}
                //catch (Exception ex)
                //{

                //    Log4NetHelper.Log(null, LogLevel.ERROR, "WorkOrders", workOrder.WorkOrderId, User.Identity.Name, ex);
                //}

                _applicationDbContext.Entry(workOrder).State = EntityState.Modified;
                try
                {
                    await _applicationDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (command == "Claim")
                        TempData["MessageToClient"] = String.Format("Someone else has claimed work order {0} since you retrieved it.", workOrder.WorkOrderId);
                    else
                        TempData["MessageToClient"] = String.Format("Someone else has modified work order {0} since you retrieved it.", workOrder.WorkOrderId);

                    return RedirectToAction("Index", "WorkList");
                }
                
                


                //Audit Trail
                Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, workOrder.EntityFormalNamePlural, workOrder.Id, User.Identity.Name, null);

                if (command == "Claim" && promotionResult.Success)
                    return RedirectToAction("Edit", workOrder.WorkOrderId);


                return RedirectToAction("Index", "WorkList");
            }
            return View(workOrder);
        }

      
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }
            return View(workOrder);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            WorkOrder workOrder = await _applicationDbContext.WorkOrders.FindAsync(id);
            _applicationDbContext.WorkOrders.Remove(workOrder);
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
