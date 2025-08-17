namespace AasDemoapp.Database.Model.DTOs
{
    public class SupplierDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string RemoteAasRepositoryUrl { get; set; } = string.Empty;
        public string RemoteSmRepositoryUrl { get; set; } = string.Empty;
        public string RemoteAasRegistryUrl { get; set; } = string.Empty;
        public string RemoteSmRegistryUrl { get; set; } = string.Empty;
        public string RemoteDiscoveryUrl { get; set; } = string.Empty;
        public string RemoteCdRepositoryUrl { get; set; } = string.Empty;
        public SecuritySetting SecuritySetting { get; set; } = new SecuritySetting();
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string RemoteAasRepositoryUrl { get; set; } = string.Empty;
        public string RemoteSmRepositoryUrl { get; set; } = string.Empty;
        public string RemoteAasRegistryUrl { get; set; } = string.Empty;
        public string RemoteSmRegistryUrl { get; set; } = string.Empty;
        public string RemoteDiscoveryUrl { get; set; } = string.Empty;
        public string RemoteCdRepositoryUrl { get; set; } = string.Empty;
        public SecuritySetting SecuritySetting { get; set; } = new SecuritySetting();
    }

    public class UpdateSupplierDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string RemoteAasRepositoryUrl { get; set; } = string.Empty;
        public string RemoteSmRepositoryUrl { get; set; } = string.Empty;
        public string RemoteAasRegistryUrl { get; set; } = string.Empty;
        public string RemoteSmRegistryUrl { get; set; } = string.Empty;
        public string RemoteDiscoveryUrl { get; set; } = string.Empty;
        public string RemoteCdRepositoryUrl { get; set; } = string.Empty;
        public SecuritySetting SecuritySetting { get; set; } = new SecuritySetting();
    }

    public class SupplierResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SupplierDto? Supplier { get; set; }
        public string? Error { get; set; }
        public SecuritySetting? SecuritySetting { get; set; }
    }
}
