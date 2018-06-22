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
    public class LaborsController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

        public ActionResult Index(int workOrderId, bool? readOnly)
        {
            ViewBag.WorkOrderId = workOrderId;
            var labors = _applicationDbContext.Labors
                                .Where(l => l.WorkOrderId == workOrderId)
                                .OrderBy(l => l.ServiceItemCode)
                                .ToList();
            
            string partialViewName = readOnly == true ? "_IndexReadonly" : "_Index";

            return PartialView(partialViewName, labors);
            //return PartialView("_Index", await parts.ToListAsync());
            //return View(await parts.ToListAsync());
        }


        // GET: Labors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await _applicationDbContext.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }
            return View(labor);
        }

        // GET: Labors/Create
        public ActionResult Create(int workOrderId)
        {
            Labor labor = new Labor();
            labor.WorkOrderId = workOrderId;
            return PartialView("_Create", labor);
        }

        // POST: Labors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LaborId,WorkOrderId,ServiceItemCode,ServiceItemName,LaborHours,Rate,Notes,PercentComplete")] Labor labor)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Labors.Add(labor);
                await _applicationDbContext.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_Create", labor);
        }

        // GET: Labors/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await _applicationDbContext.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }

            return PartialView("_Edit", labor);
        }

        // POST: Labors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "LaborId,WorkOrderId,ServiceItemCode,ServiceItemName,LaborHours,Rate,Notes,PercentComplete")] Labor labor)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Entry(labor).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();
                return Json(new { success = true });
            }
            return PartialView("_Edit", labor);
        }

        // GET: Labors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await _applicationDbContext.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", labor);
        }

        // POST: Labors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Labor labor = await _applicationDbContext.Labors.FindAsync(id);
            _applicationDbContext.Labors.Remove(labor);
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
