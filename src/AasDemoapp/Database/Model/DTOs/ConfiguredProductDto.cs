using System.Collections.Generic;

namespace AasDemoapp.Database.Model.DTOs
{
    public class ConfiguredProductDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string? AasId { get; set; } = string.Empty;
        public string? GlobalAssetId { get; set; } = string.Empty;
        public List<ProductPartDto> Bestandteile { get; set; } = [];
        public int ProducedProductsCount { get; set; }
    }
}
