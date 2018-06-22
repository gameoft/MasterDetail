using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        [Display(Name="Account #")]
        [Required(ErrorMessage="Devi inserire un account number.")]
        [StringLength(8, ErrorMessage="L'account number deve avere al massimo 8 caratteri.")]
        public string AccountNumber { get; set; }
        [Display(Name = "Company Name")]
        [Required(ErrorMessage = "Devi inserire un company name.")]
        [StringLength(30, ErrorMessage = "IL {0} deve avere al massimo 30 caratteri.")]
        public string CompanyName { get; set; }
        [Display(Name = "Indirizzo")]
        [Required(ErrorMessage = "Devi inserire un indirizzo.")]
        [StringLength(30, ErrorMessage = "L' {0} deve avere al massimo 30 caratteri.")]
        public string Address { get; set; }
        [Display(Name = "Città")]
        [Required(ErrorMessage = "Devi inserire una città.")]
        [StringLength(30, ErrorMessage = "La {0} deve avere al massimo 30 caratteri.")]
        public string City { get; set; }
        [Display(Name = "Stato")]
        [Required(ErrorMessage = "Devi inserire uno stato.")]
        [StringLength(2, ErrorMessage = "Lo {0} deve avere al massimo 2 caratteri.")]
        public string State { get; set; }
        [Display(Name = "ZIP Code")]
        [Required(ErrorMessage = "Devi inserire uno zip code.")]
        [StringLength(10, ErrorMessage = "Lo {0} deve avere al massimo 10 caratteri.")]
        public string ZipCode { get; set; }
        [StringLength(10, ErrorMessage = "Il {0} deve avere al massimo 10 caratteri.")]
        public string Phone { get; set; }

        public bool Cloaked { get; set; }



        public List<WorkOrder> WorkOrders { get; set; }
    }
}