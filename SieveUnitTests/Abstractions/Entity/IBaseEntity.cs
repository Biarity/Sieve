using System;

namespace SieveUnitTests.Abstractions.Entity
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTimeOffset DateCreated { get; set; }
    }
}
