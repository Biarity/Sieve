using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sieve.Models
{
    public class SieveModel : ISieveModel<IFilterTerm, ISortTerm>
    {
        public string Filters { get; set; }

        public string Sorts { get; set; }

        [Range(1, int.MaxValue)]
        public int? Page { get; set; }

        [Range(1, int.MaxValue)]
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
                            var subfilters = filter.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
                            value.Add(new FilterTerm(subfilters + filterOpAndVal));
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
