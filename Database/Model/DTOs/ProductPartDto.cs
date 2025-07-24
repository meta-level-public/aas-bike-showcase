using System;

namespace AasDemoapp.Database.Model.DTOs
{
    public class ProductPartDto
    {
        public long? Id { get; set; }
        public long? KatalogEintragId { get; set; }
        public KatalogEintragDto? KatalogEintrag { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Amount { get; set; }
        public DateTime UsageDate { get; set; }
        public string? Image { get; set; } = string.Empty;
    }
}
