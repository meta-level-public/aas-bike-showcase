using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class ToolRepo
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
}
