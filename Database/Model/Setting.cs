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
        public string value { get; set; } = string.Empty;
    }
}