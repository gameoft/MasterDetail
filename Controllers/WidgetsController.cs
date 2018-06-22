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

namespace MasterDetail.Controllers
{
    public class WidgetsController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

    
        public async Task<ActionResult> Index()
        {
            var widgets = _applicationDbContext.Widgets;
            return View(await widgets.ToListAsync());
        }

        // GET: Widgets/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Widget widget = await _applicationDbContext.Widgets.FindAsync(id);
            if (widget == null)
            {
                return HttpNotFound();
            }
            return View(widget);
        }

      
        public ActionResult Create()
        {
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "WidgetId,Description,MainBusCode,TestPassDateTime,WidgetStatus,CurrentWorkerId")] Widget widget)
        {
            if (ModelState.IsValid)
            {
                //Do not assign a user because the WidgetStatus is Created from the very beginning of its lifecycle
                _applicationDbContext.Widgets.Add(widget);
                await _applicationDbContext.SaveChangesAsync();


                Log4NetHelper.Log(String.Format("Widget {0} created", widget.WidgetId), LogLevel.INFO, widget.EntityFormalNamePlural, widget.WidgetId, User.Identity.Name, null);
                
                return RedirectToAction("Index", "WorkList");
            }

            return View(widget);
        }

        // GET: Widgets/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Widget widget = await _applicationDbContext.Widgets.FindAsync(id);
            if (widget == null)
            {
                return HttpNotFound();
            }
            
            if (widget.Status.Substring(widget.Status.Length - 3, 3) != "ing")
                return View("Claim", widget);

            return View(widget.Status, widget);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "WidgetId,Description,MainBusCode,TestPassDateTime,WidgetStatus,CurrentWorkerId")] Widget widget, string command)
        {
            if (ModelState.IsValid)
            {
                PromotionResult promotionResult = new PromotionResult();

                if (command == "Save")
                {
                    promotionResult.Success = true;
                    promotionResult.Message = String.Format("Changes to widget {0} have been successfully saved.", widget.WidgetId);
                }
                else if (command == "Claim")
                    promotionResult = widget.ClaimWorkListItem(User.Identity.GetUserId());
                else if (command == "Relinquish")
                    promotionResult = widget.RelinquishWorkListItem();
                else
                    promotionResult = widget.PromoteWorkListItem(command);

                if (!promotionResult.Success)
                    TempData["MessageToClient"] = promotionResult.Message;


                _applicationDbContext.Entry(widget).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();
                
                if (command == "Claim" && promotionResult.Success)
                    return RedirectToAction("Edit", widget.WidgetId);
                
                return RedirectToAction("Index", "WorkList");
            }
            
            return View(widget);
        }

      
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Widget widget = await _applicationDbContext.Widgets.FindAsync(id);
            if (widget == null)
            {
                return HttpNotFound();
            }
            return View(widget);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Widget widget = await _applicationDbContext.Widgets.FindAsync(id);
            _applicationDbContext.Widgets.Remove(widget);
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
