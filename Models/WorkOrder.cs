using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MasterDetail.Models
{
    public class WorkOrder : IWorkListItem
    {

        public WorkOrder()
        {
            WorkOrderStatus = WorkOrderStatus.Creating;
            Parts = new List<Part>();
            Labors = new List<Labor>();

        }
        public int WorkOrderId { get; set; }
        [Display(Name="Customer")]
        [Required(ErrorMessage="Devi scegliere un customer.")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [Display(Name = "Order Date")]
        public DateTime OrderDateTime { get; set; }
        [Display(Name = "Target Date")]
        public DateTime? TargetDateTime { get; set; }
        [Display(Name = "Drop Dead Date")]
        public DateTime? DropDeadDateTime { get; set; }
        [StringLength(256, ErrorMessage="La descrizione deve essere al massimo di 256 caratteri.")]
        public string Description { get; set; }
        [Display(Name = "Status")]
        public WorkOrderStatus WorkOrderStatus { get; set; }
        public decimal Total { get; set; }
        [Display(Name = "Cert. Req.'s")]
        [StringLength(120, ErrorMessage = "I {0} devono essere al massimo di 120 caratteri.")]
        public string CertificationRequirements { get; set; }

        public virtual ApplicationUser CurrentWorker { get; set; }
        //to store the FK
        public string CurrentWorkerId { get; set; }

        public virtual List<Part> Parts { get; set; }
        public virtual List<Labor> Labors { get; set; }
        [Display(Name = "Rework Notes")]
        [StringLength(256, ErrorMessage = "Rework Notes must be 256 character or fewer." )]
        public string ReworkNotes { get; set; }

        public byte[] RowVersion { get; set; }

        public PromotionResult ClaimWorkListItem(string userId)
        {
            PromotionResult promotionResult = WorkListBusinessRules.CanClaimWorkListItem(userId);

            if (!promotionResult.Success)
                return promotionResult;

            switch (WorkOrderStatus)
            {
                case WorkOrderStatus.Rejected:
                    promotionResult = PromoteToProcessing();
                    break;
                case WorkOrderStatus.Created:
                    promotionResult = PromoteToProcessing();
                    break;

                case WorkOrderStatus.Processed:
                    promotionResult = PromoteToCertifying();
                    break;

                case WorkOrderStatus.Certified:
                    promotionResult = PromoteToApproving();
                    break;

            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, "WorkOrders", WorkOrderId, HttpContext.Current.User.Identity.Name, null);


            if (promotionResult.Success)
                CurrentWorkerId = userId;

                return promotionResult;

        }

        public PromotionResult PromoteWorkListItem(string command)
        {
            PromotionResult promotionResult = new PromotionResult();


            switch (command)
            {
                case "PromoteToCreated":
                    promotionResult = PromoteToCreated();
                    break;
                case "PromoteToProcessed":
                    promotionResult = PromoteToProcessed();
                    break;

                case "PromoteToCertified":
                    promotionResult = PromoteToCertified();
                    break;

                case "PromoteToApproved":
                    promotionResult = PromoteToApproved();
                    break;

                case "DemoteToCreated":
                    promotionResult = DemoteToCreated();
                    break;
                case "DemoteToRejected":
                    promotionResult = DemoteToRejected();
                    break;
                case "DemoteToCanceled":
                    promotionResult = DemoteToCanceled();
                    break;


            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, "WorkOrders", WorkOrderId, HttpContext.Current.User.Identity.Name, null);

            if (promotionResult.Success)
            {
                CurrentWorker = null;
                CurrentWorkerId = null;

                //Attempt auto-promotion from Certified to Approved
                if (WorkOrderStatus == WorkOrderStatus.Certified && Parts.Sum(p => p.ExtendedPrice) + Labors.Sum(l => l.ExtendedPrice) < 5000)
                {
                    PromotionResult autoPromotionReuslt = PromoteToApproved();

                    if (autoPromotionReuslt.Success)
                    {
                        promotionResult = autoPromotionReuslt;
                        promotionResult.Message = "AUTOMATIC PROMOTION: " + promotionResult.Message;
                    
                    }
                
                }

            }
            

            return promotionResult;

        }

        private PromotionResult PromoteToCreated()
        {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true; 

            if (WorkOrderStatus != WorkOrderStatus.Creating)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because its current status prevented it.";
            }
            else if (String.IsNullOrWhiteSpace(TargetDateTime.ToString()) ||
                     String.IsNullOrWhiteSpace(DropDeadDateTime.ToString()) ||
                     String.IsNullOrWhiteSpace(Description))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because it required a Target Date, Drop Dead Date and a Description.";
            
            }
                

            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Created;
                promotionResult.Message = String.Format("Work Order {0} successfully promortedto status {1}",
                    WorkOrderId,
                    WorkOrderStatus);        
            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;

        }


        private PromotionResult PromoteToCertified()
        {
            PromotionResult promotionResult = new PromotionResult { Success = true };
            promotionResult = CertifyingTest(promotionResult);
            promotionResult = CertificationRequirementsTest(promotionResult);
            promotionResult = OnePartAndLaborTest(promotionResult);
            promotionResult = PartsInstalledLaborCompleteTest(promotionResult);
            
            //Tests refactored: ma non esce subito

            //if (WorkOrderStatus != WorkOrderStatus.Certifying)
            //{
            //    promotionResult.Success = false;
            //    promotionResult.Message = "Failed to promote the work order to Certified status because its current status prevented it.";
            //}

            //if (String.IsNullOrWhiteSpace(CertificationRequirements))
            //{
            //    promotionResult.Success = false;
            //    promotionResult.Message = "Failed to promote the work order to Certified status because Certification Requirements were not present.";
            
            //}
            //else if (Parts.Count == 0 || Labors.Count == 0)
            //{
            //    promotionResult.Success = false;
            //    promotionResult.Message = "Failed to promote the work order to Created status because it did not contain Part or Labor.";

            //}
            //else if (Parts.Count(p => p.IsInstalled == false) > 0 || Labors.Count(l => l.PercentComplete < 100) > 0)
            //{
            //    promotionResult.Success = false;
            //    promotionResult.Message = "Failed to promote the work order to Created status because not all parts have been installed or labors been completed.";

            //}


            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Certified;
                promotionResult.Message = String.Format("Work Order {0} successfully promortedto status {1}",
                    WorkOrderId,
                    WorkOrderStatus);
            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;

        }

        private PromotionResult CertifyingTest(PromotionResult promotionResult)
        {
            if (WorkOrderStatus != WorkOrderStatus.Certifying)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because its current status prevented it.";
            }

            return promotionResult;
        }

        private PromotionResult CertificationRequirementsTest(PromotionResult promotionResult)
        {
            if (String.IsNullOrWhiteSpace(CertificationRequirements))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Certified status because Certification Requirements were not present.";

            }
            return promotionResult;
        }

        private PromotionResult OnePartAndLaborTest(PromotionResult promotionResult)
        {
            if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because it did not contain Part or Labor.";

            }
            return promotionResult;
        }

        private PromotionResult PartsInstalledLaborCompleteTest(PromotionResult promotionResult)
        {
            if (Parts.Count(p => p.IsInstalled == false) > 0 || Labors.Count(l => l.PercentComplete < 100) > 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Created status because not all parts have been installed or labors been completed.";

            }
            return promotionResult;
        }

        private PromotionResult PromoteToApproved()
        {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving && WorkOrderStatus != WorkOrderStatus.Certified)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Approved status because its current status prevented it.";
            }


            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Approved;
                promotionResult.Message = String.Format("Work Order {0} successfully promortedto status {1}",
                    WorkOrderId,
                    WorkOrderStatus);
            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;

        }

        private PromotionResult PromoteToProcessed()
         {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Processing)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because its current status prevented it.";
            }
            else if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because it did not contain at leaset one part or labor.";
            }
            else if (String.IsNullOrWhiteSpace(Description))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to promote the work order to Processed status because it requires a Description.";

            }


            if (promotionResult.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Processed;
                promotionResult.Message = String.Format("Work Order {0} successfully promortedto status {1}",
                    WorkOrderId,
                    WorkOrderStatus);
            }

            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;

        }

        private PromotionResult PromoteToApproving()
        {
            if (WorkOrderStatus == WorkOrderStatus.Certified)
            {
                WorkOrderStatus = WorkOrderStatus.Approving;
            }

            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Approving;

            if (promotionResult.Success)
                promotionResult.Message = String.Format("Work Order {0} successfully claimed by {1} and promoted to status {2}.",
                    WorkOrderId,
                    HttpContext.Current.User.Identity.Name,
                    WorkOrderStatus);
            else
                promotionResult.Message = "Failed to promote the work order to Approving status because its current status prevented it.";

            return promotionResult;

        }

        private PromotionResult PromoteToProcessing()
        {
            if (WorkOrderStatus == WorkOrderStatus.Created || WorkOrderStatus == WorkOrderStatus.Rejected)
            {
                WorkOrderStatus = WorkOrderStatus.Processing;
            }

            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Processing;

            if (promotionResult.Success)
                promotionResult.Message = String.Format("Work Order {0} successfully claimed by {1} and promoted to status {2}.",
                    WorkOrderId,
                    HttpContext.Current.User.Identity.Name,
                    WorkOrderStatus);
            else
                promotionResult.Message = "Failed to promote the work order to Processing status because its current status prevented it.";


            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;
            
        }

        private PromotionResult PromoteToCertifying()
        {
            if (WorkOrderStatus == WorkOrderStatus.Processed)
            {
                WorkOrderStatus = WorkOrderStatus.Certifying;
            }

            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = WorkOrderStatus == WorkOrderStatus.Certifying;

            if (promotionResult.Success)
                promotionResult.Message = String.Format("Work Order {0} successfully claimed by {1} and promoted to status {2}.",
                    WorkOrderId,
                    HttpContext.Current.User.Identity.Name,
                    WorkOrderStatus);
            else
                promotionResult.Message = "Failed to promote the work order to Certifying status because its current status prevented it.";
            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);
            return promotionResult;
            
        }


        private PromotionResult DemoteToCanceled()
        {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Canceled status because its current status prevented it.";
            }

            if (promotionResult.Success)
                WorkOrderStatus = WorkOrderStatus.Canceled;
                promotionResult.Message = String.Format("Work Order {0} successfully demoted to status {1}.",
                    WorkOrderId,
                    WorkOrderStatus);
           
            return promotionResult;
           
        }

        private PromotionResult DemoteToCreated()
        {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus == WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Created status because its current status prevented it.";
            }

            if (String.IsNullOrWhiteSpace(ReworkNotes))
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Created status because Rework Notes must be present.";
            }

            if (promotionResult.Success)
                WorkOrderStatus = WorkOrderStatus.Created;
            promotionResult.Message = String.Format("Work Order {0} successfully demoted to status {1}.",
                WorkOrderId,
                WorkOrderStatus);

            return promotionResult;

        }

        private PromotionResult DemoteToRejected()
        {
            PromotionResult promotionResult = new PromotionResult();
            promotionResult.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotionResult.Success = false;
                promotionResult.Message = "Failed to demote the work order to Rejected status because its current status prevented it.";
            }

         
            if (promotionResult.Success)
                WorkOrderStatus = WorkOrderStatus.Rejected;
            promotionResult.Message = String.Format("Work Order {0} successfully demoted to status {1}.",
                WorkOrderId,
                WorkOrderStatus);

            return promotionResult;

        }


        public int Id
        {
            get { return WorkOrderId; }
        }

        public string Status
        {
            get { return WorkOrderStatus.ToString(); }
        }

        public string CurrentWorkerName
        {
            get 
            {
                if (CurrentWorker == null)
                    return String.Empty;

                return CurrentWorker.FullName;
            }
        }

        public string EntityFamiliarName
        {
            get { return "Work Order"; }
        }

        public string EntityFamiliarNamePlural
        {
            get { return "Work Orders"; }
        }

        public string EntityFormalNamePlural
        {
            get { return "WorkOrders"; }
        }

        public string EntityFormalName
        {
            get { return "WorkOrder"; }
        }

        public int PriorityScore
        {
            get 
            {
                int priorityScore = (int)WorkOrderStatus;

                if (Total >= 1000000m) priorityScore += 70;
                else if (Total >= 500000m) priorityScore += 60;
                else if (Total >= 250000m) priorityScore += 50;
                else if (Total >= 100000m) priorityScore += 40;
                else if (Total >= 25000m) priorityScore += 30;
                else if (Total >= 10000m) priorityScore += 20;
                else if (Total >= 1000m) priorityScore += 10;

                if (priorityScore < 0) priorityScore = 0;
                if (priorityScore > 100) priorityScore = 100;

                return priorityScore;  
            }
        }

        public IEnumerable<string> RolesWhichCanClaim
        {
            get 
            {
                List<string> rolesWhichCanClaim = new List<string>();

                switch (WorkOrderStatus)
                { 
                    case WorkOrderStatus.Created:
                        rolesWhichCanClaim.Add("Admin");
                        rolesWhichCanClaim.Add("Clerk");
                        rolesWhichCanClaim.Add("Manager");
                        break;

                    case WorkOrderStatus.Processed:
                        rolesWhichCanClaim.Add("Admin");
                        rolesWhichCanClaim.Add("Manager");
                        break;
                    case WorkOrderStatus.Certified:
                        rolesWhichCanClaim.Add("Admin");
                        break;
                    case WorkOrderStatus.Rejected:
                        rolesWhichCanClaim.Add("Admin");
                        rolesWhichCanClaim.Add("Manager");
                        break;
                }

                return rolesWhichCanClaim;
            }
        }

        public PromotionResult RelinquishWorkListItem()
        {
            PromotionResult promotionResult = new PromotionResult { Success = true };

            if (CurrentWorkerId == null || Status.Substring(Status.Length - 3, 3) != "ing")
            {
                promotionResult.Success = false;
                promotionResult.Message = String.Format("Work Order {0} cannot be relinquished because it is not currently being worked on.", WorkOrderId);
            }

            if (promotionResult.Success)
            {
                CurrentWorker = null;
                CurrentWorkerId = null;

                switch (WorkOrderStatus)
                { 
                    case WorkOrderStatus.Processing:
                        WorkOrderStatus = WorkOrderStatus.Created;
                        break;
                    case WorkOrderStatus.Certifying:
                        WorkOrderStatus = WorkOrderStatus.Processed;
                        break;
                    case WorkOrderStatus.Approving:
                        WorkOrderStatus = WorkOrderStatus.Certified;
                        break;
                
                }

                promotionResult.Message = String.Format("Work Order {0} was successfully relinquished and its status was reset to {1}.", WorkOrderId, WorkOrderStatus);
            
            }


            Log4NetHelper.Log(promotionResult.Message, LogLevel.INFO, EntityFormalNamePlural, WorkOrderId, HttpContext.Current.User.Identity.Name, null);

            return promotionResult;


        }
    }

    public enum WorkOrderStatus
    {
        Creating = 5,
        Created = 10,
        Processing = 15,
        Processed = 20,
        Certifying = 25,
        Certified = 30,
        Approving = 35,
        Approved = 40,
        Rejected = -10,
        Canceled = -20
        
    }
}