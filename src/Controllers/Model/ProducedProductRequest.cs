using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Controllers.Model;

namespace AasDemoapp.Database.Model
{
    public class ProducedProductRequest
    {
        public long? Id { get; set; }
        public long ConfiguredProductId { get; set; }
        public long ProductionOrderId { get; set; }
        public List<BestandteilRequest> BestandteilRequests { get; set; } = [];
    }
}
