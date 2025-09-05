using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class ImportedShell : ISoftDelete
    {
        public int Id { get; set; }
        public string RemoteRegistryUrl { get; set; } = string.Empty;
        public string RemoteAasRegistryUrl { get; set; } = string.Empty;
        public string RemoteSmRegistryUrl { get; set; } = string.Empty;
        public string AasId { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
