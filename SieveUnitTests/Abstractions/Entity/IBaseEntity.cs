using System;
using SieveUnitTests.Abstractions.Strategy;

namespace SieveUnitTests.Abstractions.Entity
{
    public interface IBaseEntity: ISupportSoftDelete
    {
        int Id { get; set; }
        DateTimeOffset DateCreated { get; set; }
    }
}
