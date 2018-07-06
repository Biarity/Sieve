using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Sieve.Models
{
    public class SieveModel : SieveModel<FilterTerm, SortTerm> { }

    [DataContract]
    public class SieveModel<TFilterTerm, TSortTerm> : ISieveModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        [DataMember]
        public string Filters { get; set; }

        [DataMember]
        public string Sorts { get; set; }

        [DataMember, Range(1, int.MaxValue)]
        public int? Page { get; set; }

        [DataMember, Range(1, int.MaxValue)]
        public int? PageSize { get; set; }

        public List<TFilterTerm> GetFiltersParsed()
        {
            if (Filters != null)
            {
                var value = new List<TFilterTerm>();
                foreach (var filter in Filters.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(filter)) continue;

                    if (filter.StartsWith("("))
                    {
                        var filterOpAndVal = filter.Substring(filter.LastIndexOf(")") + 1);
                        var subfilters = filter.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
                        var filterTerm = new TFilterTerm
                        {
                            Filter = subfilters + filterOpAndVal
                        };
                        if (!value.Any(f => f.Names.Any(n => filterTerm.Names.Any(n2 => n2 == n))))
                        {
                            value.Add(filterTerm);
                        }
                    }
                    else
                    {
                        var filterTerm = new TFilterTerm
                        {
                            Filter = filter
                        };
                        value.Add(filterTerm);
                    }
                }
                return value;
            }
            else
            {
                return null;
            }
        }

        public List<TSortTerm> GetSortsParsed()
        {
            if (Sorts != null)
            {
                var value = new List<TSortTerm>();
                foreach (var sort in Sorts.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(sort)) continue;

                    var sortTerm = new TSortTerm()
                    {
                        Sort = sort
                    };
                    if (!value.Any(s => s.Name == sortTerm.Name))
                    {
                        value.Add(sortTerm);
                    }
                }
                return value;
            }
            else
            {
                return null;
            }

        }
    }
}
