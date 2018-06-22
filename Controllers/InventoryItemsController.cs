using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.Models;
using MasterDetail.DataLayer;
using PagedList;

namespace MasterDetail.Controllers
{
    public class InventoryItemsController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();

        // GET: /InventoryItems/
        public ActionResult Index(string sort, string search, int? page)
        {
            ViewBag.CategorySort = String.IsNullOrEmpty(sort) ? "category_desc" : string.Empty;
            ViewBag.ItemCodeSort = sort == "itemcode" ? "itemcode_desc" : "itemcode";
            ViewBag.NameSort = sort == "name" ? "name_desc" : "name";
            ViewBag.UnitPriceSort = sort == "unitprice" ? "unitprice_desc" : "unitprice";

            ViewBag.CurrentSort = sort;
            ViewBag.CurrentSearch = search;


            IQueryable<InventoryItem> inventoryitems = _applicationDbContext.InventoryItems.Include(i => i.Category);


            if (!String.IsNullOrEmpty(search))
                inventoryitems = inventoryitems.Where(ii => ii.InventoryItemCode.StartsWith(search) || ii.InventoryItemName.StartsWith(search));


            switch (sort)
            { 
                case "category_desc":
                    inventoryitems = inventoryitems.OrderByDescending(ii => ii.Category.CategoryName)
                                                   .ThenBy(ii => ii.InventoryItemName);
                    break;
                case "itemcode":
                    inventoryitems = inventoryitems.OrderBy(ii => ii.InventoryItemCode);
                    break;
                case "itemcode_desc":
                    inventoryitems = inventoryitems.OrderByDescending(ii => ii.InventoryItemCode);
                    break;
                case "name":
                    inventoryitems = inventoryitems.OrderBy(ii => ii.InventoryItemName);
                    break;
                case "name_desc":
                    inventoryitems = inventoryitems.OrderByDescending(ii => ii.InventoryItemName);
                    break;
                case "unitprice":
                    inventoryitems = inventoryitems.OrderBy(ii => ii.UnitPrice);
                    break;
                case "unitprice_desc":
                    inventoryitems = inventoryitems.OrderByDescending(ii => ii.UnitPrice);
                    break;
                default:
                    inventoryitems = inventoryitems.OrderBy(ii => ii.Category.CategoryName)
                                                   .ThenBy(ii => ii.InventoryItemName);
                    break;
            
            }

            int pageSize = 3;
            int pageNumber = page ?? 1;

            //return View(await inventoryitems.ToListAsync());
            return View(inventoryitems.ToPagedList(pageNumber, pageSize));
            
        }

        // GET: /InventoryItems/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryitem = await _applicationDbContext.InventoryItems.FindAsync(id);
            if (inventoryitem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryitem);
        }

        // GET: /InventoryItems/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: /InventoryItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="InventoryItemId,InventoryItemCode,InventoryItemName,UnitPrice,CategoryId")] InventoryItem inventoryitem)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.InventoryItems.Add(inventoryitem);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName", inventoryitem.CategoryId);
            return View(inventoryitem);
        }

        // GET: /InventoryItems/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryitem = await _applicationDbContext.InventoryItems.FindAsync(id);
            if (inventoryitem == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName", inventoryitem.CategoryId);
            return View(inventoryitem);
        }

        // POST: /InventoryItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include="InventoryItemId,InventoryItemCode,InventoryItemName,UnitPrice,CategoryId")] InventoryItem inventoryitem)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Entry(inventoryitem).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName", inventoryitem.CategoryId);
            return View(inventoryitem);
        }

        // GET: /InventoryItems/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryitem = await _applicationDbContext.InventoryItems.FindAsync(id);
            if (inventoryitem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryitem);
        }

        // POST: /InventoryItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            InventoryItem inventoryitem = await _applicationDbContext.InventoryItems.FindAsync(id);
            _applicationDbContext.InventoryItems.Remove(inventoryitem);
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


        public JsonResult GetInventoryItemsForAutoComplete(string term)
        {
            InventoryItem[] matchingInventoryItems = String.IsNullOrWhiteSpace(term)
                ? null
                : _applicationDbContext.InventoryItems.Where(
                    ii =>
                        ii.InventoryItemCode.Contains(term) ||
                        ii.InventoryItemName.Contains(term)).ToArray();

            return Json(matchingInventoryItems.Select(m => new
                {
                    id = m.InventoryItemCode,
                    value = m.InventoryItemCode,
                    label = String.Format("{0}: {1}", m.InventoryItemCode, m.InventoryItemName),
                    InventoryItemName = m.InventoryItemName,
                    UnitPrice = m.UnitPrice
                }), JsonRequestBehavior.AllowGet);
        
        }




    }
}
