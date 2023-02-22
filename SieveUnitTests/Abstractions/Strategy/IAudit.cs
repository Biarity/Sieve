using System;

namespace SieveUnitTests.Abstractions.Strategy
{
    public interface IAudit
    {
        string CreatedBy { get; }
        DateTime? CreatedAt { get; }
        string UpdatedBy { get; }
        DateTime? UpdatedAt { get; }
    }
}
