using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace LeadProxy.Dto
{
    public class LeadDto
    {
        [JsonProperty("TITLE")]
        public string TITLE { get; set; }
        
        [JsonProperty("NAME")]
        public string NAME { get; set; }
        
        [JsonProperty("SECOND_NAME")]
        public string SECOND_NAME { get; set; }
        
        [JsonProperty("LAST_NAME")]
        public string LAST_NAME { get; set; }
        
        [JsonProperty("OPPORTUNITY")]
        public decimal OPPORTUNITY { get; set; } = 0;
        
        [JsonProperty("CURRENCY_ID")]
        public string CURRENCY_ID { get; set; } = "RUB";
    }
}
