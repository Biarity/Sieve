using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Services
{
    //public interface ISieveCustomSortMethods
    //{
    //}

    public interface ISieveCustomSortMethods<TEntity>
        where TEntity : class
    {
    }
}
