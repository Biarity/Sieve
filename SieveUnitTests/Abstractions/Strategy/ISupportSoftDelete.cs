using System;

namespace SieveUnitTests.Abstractions.Strategy
{
    public interface ISupportSoftDelete
    {
        string DeletedBy { get; }
        DateTime? DeletedAt { get; }
    }
}
