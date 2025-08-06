using System;
using System.Collections.Generic;

namespace AasDemoapp.Database.Model.DTOs
{
    public class ProducedProductDto
    {
        public long? Id { get; set; }
        public long ConfiguredProductId { get; set; }
        public string ConfiguredProductName { get; set; } = string.Empty;
        public List<ProductPartDto> Bestandteile { get; set; } = [];
        public string AasId { get; set; } = string.Empty;
        public string GlobalAssetId { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
    }
}
