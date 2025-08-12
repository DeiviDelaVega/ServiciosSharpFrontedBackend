using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TokenResponse
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
