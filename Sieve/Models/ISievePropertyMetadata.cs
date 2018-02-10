using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sieve.Models
{
    public interface ISievePropertyMetadata
    {
        string Name { get; set; }
        bool CanFilter { get; set; }
        bool CanSort { get; set; }
    }
}
