using Microsoft.Extensions.Options;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SieveUnitTests
{
    public class SieveOptionsAccessor : IOptions<SieveOptions>
    {
        private SieveOptions _value;

        public SieveOptions Value
        {
            get
            {
                return _value;
            }
        }

        public SieveOptionsAccessor()
        {
            _value = new SieveOptions()
            {
                ThrowExceptions = true
            };
        }
    }
}
