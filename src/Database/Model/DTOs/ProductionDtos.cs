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

    // ProductionOrder DTOs
    public class ProductionOrderDto
    {
        public long? Id { get; set; }
        public long ConfiguredProductId { get; set; }
        public string ConfiguredProductName { get; set; } = string.Empty;
        public int Anzahl { get; set; }
        public AddressDto? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? FertigstellungsDatum { get; set; }
        public bool ProduktionAbgeschlossen { get; set; }
        public bool Versandt { get; set; }
        public DateTime? VersandDatum { get; set; }
    }

    public class AddressDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Vorname { get; set; }
        public string? Strasse { get; set; }
        public string? Plz { get; set; }
        public string? Ort { get; set; }
        public string? Land { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
    }

    public class CreateProductionOrderDto
    {
        public long ConfiguredProductId { get; set; }
        public int Anzahl { get; set; } = 1;
        public AddressDto? Address { get; set; }
    }

    public class ProductionOrderResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProductionOrderDto? ProductionOrder { get; set; }
        public string? Error { get; set; }
    }
}
