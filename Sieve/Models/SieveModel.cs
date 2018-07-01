using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        
        public List<TFilterTerm> FiltersParsed
        {
            get
            {
                if (Filters != null)
                {
                    var value = new List<TFilterTerm>();
                    foreach (var filter in Filters.Split(','))
                    {
                        if (filter.StartsWith("("))
                        {
                            var filterOpAndVal = filter.Substring(filter.LastIndexOf(")") + 1);
                            var subfilters = filter.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
                            var filterTerm = new TFilterTerm
                            {
                                Filter = subfilters + filterOpAndVal
                            };
                            value.Add(filterTerm);
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
        }

        public List<TSortTerm> SortsParsed
        {
            get
            {
                if (Sorts != null)
                {
                    var value = new List<TSortTerm>();
                    foreach (var sort in Sorts.Split(','))
                    {
                        var sortTerm = new TSortTerm()
                        {
                            Sort = sort
                        };
                        value.Add(sortTerm);
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
}
