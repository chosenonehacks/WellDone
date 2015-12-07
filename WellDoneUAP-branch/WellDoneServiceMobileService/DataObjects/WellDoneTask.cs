using Microsoft.WindowsAzure.Mobile.Service;
using System;

namespace WellDoneMobileService.DataObjects
{
    public class WellDoneTask : EntityData
    {
        public string Id { get; set; }
        public string Topic { get; set; }                        
        public DateTime? DueDate { get; set; }
        public bool IsComplete { get; set; }
        public bool IsProject { get; set; }
        public string ProjectId { get; set; }
        public string ContextId { get; set; }
        public string UserId { get; set; }
    }
}
