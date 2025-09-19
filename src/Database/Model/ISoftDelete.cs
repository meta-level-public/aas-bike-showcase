using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AasDemoapp.Database.Model
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
