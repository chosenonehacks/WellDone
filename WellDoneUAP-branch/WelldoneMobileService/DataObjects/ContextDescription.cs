using Microsoft.WindowsAzure.Mobile.Service;
using System;

namespace WellDoneMobileService.DataObjects
{
    public class ContextDescription : EntityData
    {
        public string Name { get; set; }
        public string UserId { get; set; }
    }
}
