using System;
using Sieve.Attributes;

namespace SieveUnitTests.Entities
{
	public class Comment : BaseEntity
    {
        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}
