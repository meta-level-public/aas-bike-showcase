using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class Address
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
}
