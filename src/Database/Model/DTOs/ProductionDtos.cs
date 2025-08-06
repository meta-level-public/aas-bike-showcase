using System;
using System.Collections.Generic;

namespace AasDemoapp.Database.Model.DTOs
{
    public class ProducedProductRequestDto
    {
        public long? Id { get; set; }
        public long ConfiguredProductId { get; set; }
        public List<BestandteilRequestDto> BestandteilRequests { get; set; } = [];
    }

    public class BestandteilRequestDto
    {
        public string GlobalAssetId { get; set; } = string.Empty;
        public DateTime UsageDate { get; set; }
        public int Amount { get; set; } = 1;
    }

    public class ProductionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProducedProductDto? ProducedProduct { get; set; }
        public string? Error { get; set; }
    }
}
