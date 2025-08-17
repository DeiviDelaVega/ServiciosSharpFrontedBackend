using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Shared.Models
{
    public sealed class ClienteLookupDto
    {
        public int ID_Cliente { get; set; }  // PropertyNameCaseInsensitive=true se encargará del casing
    }
}
