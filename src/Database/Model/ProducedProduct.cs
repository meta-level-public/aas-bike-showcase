using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class ProducedProduct : ISoftDelete
    {
        public long? Id { get; set; }
        public ConfiguredProduct ConfiguredProduct { get; set; } = null!;
        public long ConfiguredProductId { get; set; }
        public List<ProductPart> Bestandteile { get; set; } = [];
        public string AasId { get; set; } = string.Empty;
        public string GlobalAssetId { get; set; } = string.Empty;

        public DateTime ProductionDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public double PCFValue { get; set; }
    }
}
