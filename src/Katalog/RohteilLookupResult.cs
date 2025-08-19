using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Katalog
{
    public class RohteilLookupResult
    {
        public KatalogEintrag? TypeKatalogEintrag { get; set; }
        public string AasId { get; set; } = string.Empty;
        public string GlobalAssetId { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

    }
}