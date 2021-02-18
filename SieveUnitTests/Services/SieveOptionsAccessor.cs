using Microsoft.Extensions.Options;
using Sieve.Models;

namespace SieveUnitTests.Services
{
    public class SieveOptionsAccessor : IOptions<SieveOptions>
    {
        public SieveOptions Value { get; }

        public SieveOptionsAccessor()
        {
            Value = new SieveOptions()
            {
                ThrowExceptions = true
            };
        }
    }
}
