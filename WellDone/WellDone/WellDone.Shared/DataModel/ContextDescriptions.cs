using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WellDone.DataModel
{
    public class ContextDescription
    {
        
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "UserId")]
        public string UserId { get; set; }

        
        public DateTimeOffset CreatedAt { get; set; }
    }
}
