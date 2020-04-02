using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeadProxy.Dto
{
    public class ContactDto
    {
        [JsonProperty("NAME")]
        public string Name { get; set; }

        [JsonProperty("PHONE")]
        public List<PhoneDto> Phones { get; set; }
    }

    public class PhoneDto
    {
        [JsonProperty("VALUE")]
        public string Value { get; set; }
    }
}
