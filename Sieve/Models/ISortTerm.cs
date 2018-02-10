using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public interface ISortTerm
    {
        bool Descending { get; }
        string Name { get; }
    }
}