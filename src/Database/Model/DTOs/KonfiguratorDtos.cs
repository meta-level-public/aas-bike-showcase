using System;
using System.Collections.Generic;

namespace AasDemoapp.Database.Model.DTOs
{
    public class CreateConfiguredProductDto
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public List<CreateProductPartDto> Bestandteile { get; set; } = [];
    }

    public class CreateProductPartDto
    {
        public long KatalogEintragId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Amount { get; set; }
        public DateTime UsageDate { get; set; }
    }

    public class ConfigurationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ConfiguredProductDto? ConfiguredProduct { get; set; }
        public string? Error { get; set; }
    }
}
