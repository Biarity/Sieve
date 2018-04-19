using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class SieveModel: ISieveModel<IFilterTerm, ISortTerm>
    {
        public string Filters { get; set; }

        public string Sorts { get; set; }

        [Range(1, Double.MaxValue)]
        public int? Page { get; set; }

        [Range(1, Double.MaxValue)]
        public int? PageSize { get; set; }


        public List<IFilterTerm> FiltersParsed
        {
            get
            {
                if (Filters != null)
                {
                    var value = new List<IFilterTerm>();
                    foreach (var filter in Filters.Split(','))
                    {
                        if (filter.StartsWith("("))
                        {
                            var filterOpAndVal = filter.Substring(filter.LastIndexOf(")") + 1);
                            filter = filter.Replace(subfilterOpAndVal, "").Replace("(", "").Replace(")","");
                            foreach (var subfilter in filter.Split("|"))
                            {
                                value.Add(new FilterTerm(subfilter + filterOpAndVal))
                            }
                        }
                        else 
                        {
                            value.Add(new FilterTerm(filter));
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

        public List<ISortTerm> SortsParsed
        {
            get
            {
                if (Sorts != null)
                {
                    var value = new List<ISortTerm>();
                    foreach (var sort in Sorts.Split(','))
                    {
                        value.Add(new SortTerm(sort));
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
