using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class ProductPart: ISoftDelete
    {
        public long? Id { get; set; }
        public KatalogEintrag KatalogEintrag { get; set; } = null!;
        public long? KatalogEintragId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Amount {get; set; }
        public DateTime UsageDate { get; set;}

        public ConfiguredProduct? ConfiguredProduct { get; set; }
        public long? ConfiguredProductId { get; set; }
        public ProducedProduct? ProducedProduct { get; set; }
        public long? ProducedProductId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}