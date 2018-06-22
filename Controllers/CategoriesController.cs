using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.Models;
using MasterDetail.DataLayer;
using System.Threading.Tasks;
using TreeUtility;
using MasterDetail.ViewModels;
using System.Data.Entity.Infrastructure;

namespace MasterDetail.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext _applicationDbContext = new ApplicationDbContext();


        private List<Category> GetListOfNodes()
        {
            List<Category> sourceCategories = _applicationDbContext.Categories.ToList();
            List<Category> categories = new List<Category>();
            foreach (Category sourceCategory in sourceCategories)
            {
                Category c = new Category();
                c.Id = sourceCategory.Id;
                c.CategoryName = sourceCategory.CategoryName;
                if (sourceCategory.ParentCategoryId != null)
                {
                    c.Parent = new Category();
                    c.Parent.Id = (int)sourceCategory.ParentCategoryId;
                }
                categories.Add(c);
            }
            return categories;

        }

        private void ValidateParentsAreParentless(Category category)
        {
            //here is no parent
            if (category.ParentCategoryId == null)
                return;

            //the parent has a parent
            Category parentcategory = _applicationDbContext.Categories.Find(category.ParentCategoryId);
            if (parentcategory.ParentCategoryId != null)
                throw new InvalidOperationException("you cannot nest this category more than two levels deep");

            //the parent dows not have a parent, but the category being nested has children
            int numberOfChildren = _applicationDbContext.Categories.Count(c => c.ParentCategoryId == category.Id);
            if (numberOfChildren > 0)
                throw new InvalidOperationException("you cannot nest this category's children more than two levels deep");

        }


        // GET: /Categories/
        public async Task<ActionResult> Index()
        {
            //return View( await _applicationDbContext.Categories.ToListAsync());
            string fullString = "<ul>";
            //fullString += "<li>Hello!";
            //fullString += "<ul><li>Child of Hello</li></ul>";
            //fullString += "</li>";

            IList<Category> listOfNodes = GetListOfNodes();
            IList<Category> topLevelCategories = TreeHelper.ConvertToForest(listOfNodes);

            foreach (var category in topLevelCategories)
                fullString += EnumerateNodes(category);


            fullString += "</ul>";
            return View((object)fullString);


        }

        private string EnumerateNodes(Category parent)
        {
            string content = String.Empty;

            content += "<li class=\"treenode\">";
            content += parent.CategoryName;
            content += String.Format("<a href=\"/Categories/Edit/{0}\" class=\"btn btn-primary btn-xs treenodeeditbutton\">Edit</a>", parent.Id);
            content += String.Format("<a href=\"/Categories/Delete/{0}\" class=\"btn btn-danger btn-xs treenodedeletebutton\">Delete</a>", parent.Id);


            if (parent.Children.Count == 0)
                content += "</li>";
            else
                content += "<ul>";

            int numberOfChildren = parent.Children.Count;
            for (int i = 0; i <= numberOfChildren; i++)
            {
                if (numberOfChildren > 0 && i < numberOfChildren)
                {
                    Category child = parent.Children[i];
                    content += EnumerateNodes(child);
                }

                if (numberOfChildren > 0 && i == numberOfChildren)
                {
                    content += "</ul>";
                }


            }
            return content;

        }

        //// GET: /Categories/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Category category = db.Categories.Find(id);
        //    if (category == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(category);
        //}

        // GET: /Categories/Create
        public ActionResult Create()
        {
            //ViewBag.ParentCategoryIdSelectList = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(null);
            return View();
        }

        // POST: /Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ParentCategoryId,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    ValidateParentsAreParentless(category);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    //ViewBag.ParentCategoryIdSelectList = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
                    ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(null);
                    return View(category);
                }

                _applicationDbContext.Categories.Add(category);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(category);
        }



        private SelectList PopulateParentCategorySelectList(int? id)
        {
            SelectList selectList;

            if (id == null)
                selectList = new SelectList(
                    _applicationDbContext
                    .Categories
                    .Where(c => c.ParentCategoryId == null), "Id", "CategoryName");
            else if (_applicationDbContext.Categories.Count(c => c.ParentCategoryId == id) == 0)
                selectList = new SelectList(
                    _applicationDbContext
                    .Categories
                    .Where(c => c.ParentCategoryId == null && c.Id != id), "Id", "CategoryName");
            else
                selectList = new SelectList(
                    _applicationDbContext
                    .Categories
                    .Where(c => false), "Id", "CategoryName");

            return selectList;


        }

        // GET: /Categories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _applicationDbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            //Wind-up a Category viewmodel
            CategoryViewModel categoryViewModel = new CategoryViewModel();
            categoryViewModel.Id = category.Id;
            categoryViewModel.ParentCategoryId = category.ParentCategoryId;
            categoryViewModel.CategoryName = category.CategoryName;

            //ViewBag.ParentCategoryIdSelectList = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id);
            return View(categoryViewModel);
        }

        // POST: /Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentCategoryId,CategoryName")] CategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {

                //Unwind back to a Category
                Category editedCategory = new Category();
                try
                {
                    editedCategory.Id = categoryViewModel.Id;
                    editedCategory.ParentCategoryId = categoryViewModel.ParentCategoryId;
                    editedCategory.CategoryName = categoryViewModel.CategoryName;
                    ValidateParentsAreParentless(editedCategory);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    //ViewBag.ParentCategoryIdSelectList = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
                    ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id);
                    return View("Edit", categoryViewModel);
                }



                _applicationDbContext.Entry(editedCategory).State = EntityState.Modified;
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.ParentCategoryIdSelectList = new SelectList(_applicationDbContext.Categories, "Id", "CategoryName");
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id);
            return View(categoryViewModel);
        }

        // GET: /Categories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await _applicationDbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Category category = await _applicationDbContext.Categories.FindAsync(id);

            try
            {
                _applicationDbContext.Categories.Remove(category);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "You attempted to delete a category that had child categories associated with it.");
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
            }

            return View("Delete", category);

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
