using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public class UpdateableShell : ISoftDelete
    {
        public long? Id { get; set; }
        public KatalogEintrag KatalogEintrag { get; set; } = null!;
        public DateTime UpdateFoundTimestamp { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}