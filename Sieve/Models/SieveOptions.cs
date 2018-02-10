using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Models
{
    public class SieveOptions
    {
        public bool CaseSensitive { get; set; } = false;

        public int DefaultPageSize { get; set; } = 0;
    }
}