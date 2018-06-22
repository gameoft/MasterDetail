namespace MasterDetail.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using MasterDetail.DataLayer;
    using MasterDetail.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApplicationDbContext context)
        {

            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };

                var roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));

                string name = "pluralsightnimda@gmail.com";
                string password = "Pluralsight#1";
                string firstName = "Admin";
                string roleName = "Admin";

                var role = roleManager.FindByName(roleName);

                if (role == null)
                {
                    role = new ApplicationRole(roleName);
                    var roleResult = roleManager.Create(role);
                }

                var user = userManager.FindByName(name);

                if (user == null)
                {
                    user = new ApplicationUser { UserName = name, Email = name, FirstName = firstName };
                    var result = userManager.Create(user, password);
                    result = userManager.SetLockoutEnabled(user.Id, false);
                }

                var rolesForUser = userManager.GetRoles(user.Id);

                if (!rolesForUser.Contains(role.Name))
                {
                    var result = userManager.AddToRole(user.Id, role.Name);
                }


                ////////////////////

                //string accountNumber = "ABC123";

                //context.Customers.AddOrUpdate(
                //    c => c.AccountNumber,
                //    new Customer 
                //    {
                //     AccountNumber = accountNumber,
                //     CompanyName = "ABC Company of America",
                //     Address = "123 Main St.",
                //     City = "Anytown",
                //     State = "GA",
                //     ZipCode = "30071"
                //    });

                //context.SaveChanges();

                ////lo devo ricaricare per sapere la PK assegnata in automatico
                //Customer customer = context.Customers.First(c => c.AccountNumber == accountNumber);

                //string description = "Just another work order";

                //context.WorkOrders.AddOrUpdate(
                //    wo => wo.Description,
                //    new WorkOrder { Description = description, CustomerId = customer.CustomerId, WorkOrderStatus = WorkOrderStatus.Created, CurrentWorkerId = user.Id }
                //    );

                //context.SaveChanges();

                //WorkOrder workOrder = context.WorkOrders.First(wo => wo.Description == description);

                //context.Parts.AddOrUpdate(
                //    p => p.InventoryItemCode,
                //    new Part { InventoryItemCode = "THING1", InventoryItemName = "Thing Number 1", Quantity = 1, UnitPrice = 1.23m, WorkOrderId = workOrder.WorkOrderId });

                //context.Labors.AddOrUpdate(
                //  p => p.ServiceItemCode,
                //  new Labor { ServiceItemCode = "INSTALL", ServiceItemName = "Installation", LaborHours = 9.87m, Rate = 35.75m, WorkOrderId = workOrder.WorkOrderId });

                //string categoryName = "Devices";

                //context.Categories.AddOrUpdate(
                //p => p.CategoryName,
                //new Category { CategoryName = categoryName });

                //context.SaveChanges();

                //Category category = context.Categories.First(c => c.CategoryName == categoryName);

                //context.InventoryItems.AddOrUpdate(
                //p => p.InventoryItemCode,
                //new InventoryItem { InventoryItemCode = "THING2", InventoryItemName = "A second kind of thing", UnitPrice = 3.33m, CategoryId = category.CategoryId  });

                //context.ServiceItems.AddOrUpdate(
                //p => p.ServiceItemCode,
                //new ServiceItem { ServiceItemCode = "CLEAN", ServiceItemName = "General Cleaning", Rate = 23.50m });
              
                
                /////////////Category
                string categoryName = "Housing";

                context.Categories.AddOrUpdate(
                c => c.CategoryName,
                new Category { CategoryName = categoryName });

                context.SaveChanges();

                Category category = context.Categories.First(c => c.CategoryName == categoryName);

                context.Categories.AddOrUpdate(
                        c => c.CategoryName,
                        new Category { CategoryName = "Furniture", ParentCategoryId = category.Id },
                        new Category { CategoryName = "Fixtures", ParentCategoryId = category.Id },
                        new Category { CategoryName = "Building Materials", ParentCategoryId = category.Id }
                        );

                categoryName = "Learning Materials";

                context.Categories.AddOrUpdate(
                    c => c.CategoryName,
                    new Category { CategoryName = categoryName });

                context.SaveChanges();

                category = context.Categories.First(c => c.CategoryName == categoryName);

                context.Categories.AddOrUpdate(
                      c => c.CategoryName,
                      new Category { CategoryName = "Books", ParentCategoryId = category.Id },
                      new Category { CategoryName = "Supplies", ParentCategoryId = category.Id }

                      );

                context.Categories.AddOrUpdate(
                    c => c.CategoryName,
                    new Category { CategoryName = "Food and Water" }

                    );

                context.SaveChanges();

                ////////////////////////////////

                category = context.Categories.First(c => c.CategoryName == "Furniture");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "STUDENTDESK", InventoryItemName = "Student Desk", UnitPrice = 10m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "TESCHERDESK", InventoryItemName = "Teacher Desk", UnitPrice = 20m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "CHAIR", InventoryItemName = "Chair", UnitPrice = 6.95m }

                    );

                category = context.Categories.First(c => c.CategoryName == "Books");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "SCIENCETEXT", InventoryItemName = "Science Textbook", UnitPrice = 5.99m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "ARTTEXT", InventoryItemName = "Art History Textbook", UnitPrice = 6.40m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "POETRYTEXT", InventoryItemName = "Greatest Poems", UnitPrice = 3.23m }

                    );

                category = context.Categories.First(c => c.CategoryName == "Supplies");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "STUDENTSOUP", InventoryItemName = "Student Soup", UnitPrice = 3.45m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "TECHERSOUP", InventoryItemName = "Teacher Soup", UnitPrice = 4.45m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "MYSOUP", InventoryItemName = "My Soup", UnitPrice = 2.45m }

                    );


                
                category = context.Categories.First(c => c.CategoryName == "Housing");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "CLASSROOM", InventoryItemName = "Pre-fabricated classroom", UnitPrice = 3.45m }
                 
                    );


                category = context.Categories.First(c => c.CategoryName == "Fixtures");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "WHITEBOARD", InventoryItemName = "Whiteboard", UnitPrice = 3.45m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "ARMOR", InventoryItemName = "Armor Plating Kit", UnitPrice = 3.45m }
                 
                    );


                  category = context.Categories.First(c => c.CategoryName == "Building Materials");

                context.InventoryItems.AddOrUpdate(
                    ii => ii.InventoryItemName,
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "BUILDING1", InventoryItemName = "Material 1", UnitPrice = 3.45m },
                    new InventoryItem { CategoryId = category.Id, InventoryItemCode = "BUILDING2", InventoryItemName = "Material 2", UnitPrice = 3.45m }
                 
                    );

                context.SaveChanges();


                context.ServiceItems.AddOrUpdate(
                   ii => ii.ServiceItemName,
                   new ServiceItem { ServiceItemCode = "FORMANDOPUR", ServiceItemName = "Form and Pur", Rate = 35.56m },
                   new ServiceItem { ServiceItemCode = "ERECTPREFS", ServiceItemName = "Prefabbgrifatoi", Rate = 45.56m }
                   

                   );


                context.SaveChanges();


                /////////
                context.Customers.AddOrUpdate(
                    cu => cu.AccountNumber,
                    new Customer { AccountNumber = "GSTEMS", CompanyName = "Girls STEM School", Address = "35 Achievement Way", City = "Detroit", State = "MI", Phone = "123456", ZipCode = "43233" },
                    new Customer { AccountNumber = "YWLS", CompanyName = "Young Women's Literary Society", Address = "15213 Aruna Lane", City = "Empoli", State = "IT", Phone = "1234565", ZipCode = "00100" },
                    new Customer { AccountNumber = "TRS", CompanyName = "The Roosvelt School", Address = "731 Krasmer Street", City = "Parigi", State = "FR", Phone = "125555", ZipCode = "50100" }
                );


                context.SaveChanges();
             
            }
            catch (DbEntityValidationException e)
            {

                if (System.Diagnostics.Debugger.IsAttached == false)
                    System.Diagnostics.Debugger.Launch();

                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                if (System.Diagnostics.Debugger.IsAttached == false)
                     System.Diagnostics.Debugger.Launch();
                throw;
            }

        
        }
    }
}
