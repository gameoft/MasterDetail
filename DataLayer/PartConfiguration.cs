using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class PartConfiguration : EntityTypeConfiguration<Part>
    {
        public PartConfiguration()
        {
            Property(si => si.InventoryItemCode)
               .HasMaxLength(15)
               .IsRequired()
               .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("AK_Part", 2) { IsUnique = true }));

            Property(p => p.WorkOrderId)
                 .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("AK_Part", 1) { IsUnique = true }));

            Property(si => si.InventoryItemName)
              .HasMaxLength(80)
              .IsRequired();

            Property(si => si.UnitPrice).HasPrecision(18, 2);

            Property(si => si.ExtendedPrice).HasPrecision(18, 2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            Property(si => si.Notes)
                 .HasMaxLength(140)
                 .IsOptional();
        }
    }
}