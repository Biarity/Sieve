using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Services
{
    public interface ISieveCustomFilterMethods
    {
    }

    public interface ISieveCustomFilterMethods<TEntity>
    where TEntity : class
    {
    }
}
