using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AasDemoapp.Utils.Shells;

public class SaveShellResult
{
    public string AasId { get; set; } = string.Empty;
    public Dictionary<string, string> OldNewFileNames { get; set; } = [];

    [JsonIgnore]
    public AasCore.Aas3_0.Environment? Environment { get; set; }
}
