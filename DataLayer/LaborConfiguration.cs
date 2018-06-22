using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class LaborConfiguration: EntityTypeConfiguration<Labor>
    {
        public LaborConfiguration()
        {
            Property(si => si.ServiceItemCode)
               .HasMaxLength(15)
               .IsRequired()
               .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("AK_Labor", 2) { IsUnique = true }));

            Property(p => p.WorkOrderId)
                 .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("AK_Labor", 1) { IsUnique = true }));

            Property(si => si.ServiceItemName)
              .HasMaxLength(80)
              .IsRequired();

            Property(si => si.LaborHours).HasPrecision(18, 2);

            Property(si => si.Rate).HasPrecision(18, 2);

            Property(si => si.ExtendedPrice).HasPrecision(18, 2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            Property(si => si.Notes)
                 .HasMaxLength(140)
                 .IsOptional();
        }
    }
}