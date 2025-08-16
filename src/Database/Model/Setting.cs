using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class Setting
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class SecuritySetting {
        public string? Certificate { get; set; }
        public string? CertificatePassword { get; set; }
        public List<HeaderParameter> HeaderParameters { get; set; } = new List<HeaderParameter>();

    }
    public class HeaderParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}