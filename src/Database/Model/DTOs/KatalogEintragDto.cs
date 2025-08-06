namespace AasDemoapp.Database.Model.DTOs
{
    public class KatalogEintragDto
    {
        public long? Id { get; set; }
        public string AasId { get; set; } = string.Empty;
        public string LocalAasId { get; set; } = string.Empty;
        public string GlobalAssetId { get; set; } = string.Empty;
        public string Kategorie { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string RemoteRepositoryUrl { get; set; } = string.Empty;
        public int? Rating { get; set; } = 0;
        public double Price { get; set; } = 0.0;
        public string? Image { get; set; } = string.Empty;
        public KatalogEintragTyp KatalogEintragTyp { get; set; } = KatalogEintragTyp.RohteilTyp;
        public InventoryStatus InventoryStatus { get; set; }
        public int Amount { get; set; } = 0;
        public int AmountToUse { get; set; } = 0;
        public long? ReferencedTypeId { get; set; }
        public KatalogEintragDto? ReferencedType { get; set; }
    }
}
