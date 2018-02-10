using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public interface ISieveModel<TFilterTerm, TSortTerm> 
        where TFilterTerm : IFilterTerm
        where TSortTerm : ISortTerm
    {
        string Filters { get; set; }

        string Sorts { get; set; }
        
        int? Page { get; set; }
        
        int? PageSize { get; set; }

        List<TFilterTerm> FiltersParsed { get; }

        List<TSortTerm> SortsParsed { get; }
    }
}
