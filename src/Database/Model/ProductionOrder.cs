using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class ProductionOrder : ISoftDelete
    {
        public long? Id { get; set; }
        public long ConfiguredProductId { get; set; }
        public ConfiguredProduct ConfiguredProduct { get; set; } = null!;
        public int Anzahl { get; set; } = 1;
        public long? AddressId { get; set; }
        public Address? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? FertigstellungsDatum { get; set; }
        public bool ProduktionAbgeschlossen { get; set; } = false;
        public bool Versandt { get; set; } = false;
        public DateTime? VersandDatum { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
