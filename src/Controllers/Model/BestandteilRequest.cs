using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Controllers.Model
{
    public class BestandteilRequest
    {
        public string GlobalAssetId { get; set; } = string.Empty;
        public DateTime UsageDate { get; set; }
        public int Amount { get; set; } = 1;
    }
}
