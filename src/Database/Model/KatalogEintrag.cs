using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class KatalogEintrag : ISoftDelete
    {
        public long? Id { get; set; }
        public string AasId { get; set; } = string.Empty;
        public string LocalAasId { get; set; } = string.Empty;
        public string GlobalAssetId { get; set; } = string.Empty;
        public string Kategorie { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RemoteRepositoryUrl { get; set; } = string.Empty; // hier besser auf die Konfiguration verweisen, da wir darüber die security Settings haben
        public Supplier Supplier { get; set; }
        public long SupplierId { get; set; }

        public int? Rating { get; set; } = 0;
        public double Price { get; set; } = 0.0;
        public string? Image { get; set; } = string.Empty;

        public KatalogEintragTyp KatalogEintragTyp { get; set; } = KatalogEintragTyp.RohteilTyp;

        [NotMapped]
        public InventoryStatus InventoryStatus
        {
            get
            {
                switch (Amount)
                {
                    case 0: return InventoryStatus.OUTOFSTOCK;
                    case < 10: return InventoryStatus.LOWSTOCK;
                    case >= 10: return InventoryStatus.INSTOCK;
                }
            }
        }

        public int Amount { get; set; } = 0;
        [NotMapped]
        public int AmountToUse { get; set; } = 0;
        public KatalogEintrag? ReferencedType { get; set; }
        public long? ReferencedTypeId { get; set; }
        public List<ConfiguredProduct>? ConfiguredProducts { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }

    public enum KatalogEintragTyp
    {
        RohteilTyp,
        RohteilInstanz
    }
}