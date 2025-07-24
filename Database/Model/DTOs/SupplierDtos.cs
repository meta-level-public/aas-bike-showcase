namespace AasDemoapp.Database.Model.DTOs
{
    public class SupplierDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string RemoteRepositoryUrl { get; set; } = string.Empty;
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string RemoteRepositoryUrl { get; set; } = string.Empty;
    }

    public class SupplierResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SupplierDto? Supplier { get; set; }
        public string? Error { get; set; }
    }
}
