using Sieve.Attributes;
using SieveUnitTests.Abstractions.Entity;

namespace SieveUnitTests.Entities
{
    public class Comment : BaseEntity, IComment
    {
        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}
