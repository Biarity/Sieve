using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public interface ISieveModel
    {
        string Filters { get; set; }

        string Sorts { get; set; }
        
        int? Page { get; set; }
        
        int? PageSize { get; set; }

        List<FilterTerm> FiltersParsed { get; }

        List<SortTerm> SortsParsed { get; }
    }
}
