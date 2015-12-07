using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WellDone.DataModel
{
    public class WellDoneTask
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Topic")]
        public string Topic { get; set; }   
        [JsonProperty(PropertyName = "DueDate")]                     
        public DateTime? DueDate { get; set; }

        [JsonProperty(PropertyName = "IsComplete")]
        public bool IsComplete { get; set; }

        [JsonProperty(PropertyName = "IsProject")]
        public bool IsProject { get; set; }

        [JsonProperty(PropertyName = "ProjectId")]
        public string ProjectId { get; set; }

        [JsonProperty(PropertyName = "ContextId")]
        public string ContextId { get; set; } 
   
        [JsonProperty(PropertyName = "UserId")]
        public string UserId { get; set; }

        //[JsonProperty(PropertyName = "CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

    }
}
