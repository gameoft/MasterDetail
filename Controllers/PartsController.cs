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

namespace MasterDetail.Controllers
{
    public class PartsController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

        // GET: Parts
        //Only top-level async controllers are supported. This is all based upon the async HttpHandler plumbing. And it'd not make much sense to have a child action as async if the top level controller isn't as well, since it'd be consuming the thread resources while the child is async.
        //public async Task<ActionResult> Index(int workOrderId)
        //{
        //    ViewBag.WorkOrderId = workOrderId;
        //    var parts = await _applicationDbContext.Parts
        //        //.Include(p => p.WorkOrder)
        //                        .Where(p => p.WorkOrderId == workOrderId)
        //                        .OrderBy(p => p.InventoryItemCode)
        //                        .ToListAsync();

        //    return PartialView("_Index", parts);
        //    //return PartialView("_Index", await parts.ToListAsync());
        //    //return View(await parts.ToListAsync());
        //}

        public ActionResult Index(int workOrderId, bool? readOnly)
        {
            ViewBag.WorkOrderId = workOrderId;
            var parts = _applicationDbContext.Parts
                //.Include(p => p.WorkOrder)
                                .Where(p => p.WorkOrderId == workOrderId)
                                .OrderBy(p => p.InventoryItemCode)
                                .ToList();

            string partialViewName = readOnly == true ? "_IndexReadonly" : "_Index";

            return PartialView(partialViewName, parts);
            //return PartialView("_Index", await parts.ToListAsync());
            //return View(await parts.ToListAsync());
        }

     

        // GET: Parts/Create
        public ActionResult Create(int workOrderId)
        {
            Part part = new Part();
            part.WorkOrderId = workOrderId;
            return PartialView("_Create", part);
        }

        // POST: Parts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PartId,WorkOrderId,InventoryItemCode,InventoryItemName,Quantity,UnitPrice,Notes,IsInstalled")] Part part)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Parts.Add(part);
                await _applicationDbContext.SaveChangesAsync();
                return Json(new { success = true });
            }

            Log4NetHelper.Log(String.Format("Part item {0} has been added to work order {1}.", part.InventoryItemCode, part.WorkOrderId), LogLevel.INFO, "WorkOrders", part.WorkOrderId, User.Identity.Name, null);

            return PartialView("_Create", part);
        }

        // GET: Parts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = await _applicationDbContext.Parts.FindAsync(id);
            if (part == null)
            {
                return HttpNotFound();
            }
           
            return PartialView("_Edit", part);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PartId,WorkOrderId,InventoryItemCode,InventoryItemName,Quantity,UnitPrice,Notes,IsInstalled")] Part part)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Entry(part).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();
                return Json(new { success = true });
            }
            return PartialView("_Edit", part);
        }

        // GET: Parts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = await _applicationDbContext.Parts.FindAsync(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Part part = await _applicationDbContext.Parts.FindAsync(id);
            _applicationDbContext.Parts.Remove(part);
            await _applicationDbContext.SaveChangesAsync();
            return Json(new { success = true });
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
