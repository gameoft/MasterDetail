using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class InventoryItemConfiguration  : EntityTypeConfiguration<InventoryItem>
    {
        public InventoryItemConfiguration()
        {
            Property(si => si.InventoryItemCode)
                .HasMaxLength(15)
                .IsRequired()
                .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("AK_InventoryItem_InventoryItemCode") { IsUnique = true }));

            Property(si => si.InventoryItemName)
              .HasMaxLength(80)
              .IsRequired()
              .HasColumnAnnotation("Index",
              new IndexAnnotation(new IndexAttribute("AK_InventoryItem_InventoryItemName") { IsUnique = true }));

            Property(si => si.UnitPrice).HasPrecision(18, 2);

            //unidirectional navigation
            //HasRequired(wo => wo.Category).WithMany(au => au.InventoryItems).WillCascadeOnDelete(false);
            HasRequired(wo => wo.Category).WithMany().WillCascadeOnDelete(false);
            

        }
    }
  
}