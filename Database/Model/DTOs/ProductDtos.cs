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

    // KatalogEintrag DTO
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

    // Production DTOs
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

    // Response DTOs
    public class ProductionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProducedProductDto? ProducedProduct { get; set; }
        public string? Error { get; set; }
    }

    // HandoverDocumentation DTO
    public class HandoverDocumentationDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SubmodelSummaryDto? Submodel { get; set; }
        public string? Error { get; set; }
    }

    public class SubmodelSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string? IdShort { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? Revision { get; set; }
        public int ElementCount { get; set; }
    }
}
