using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Sieve.Models
{
    public class SieveModel : SieveModel<FilterTerm, SortTerm> { }

    [DataContract]
    public class SieveModel<TFilterTerm, TSortTerm> : ISieveModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        private const string EscapedCommaPattern = @"(?<!($|[^\\])(\\\\)*?\\),\s*";
        private const string CommaToEscape = @"\,";

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
                foreach (var filter in Regex.Split(Filters, EscapedCommaPattern))
                {
                    if (string.IsNullOrWhiteSpace(filter)) continue;

                    var escapedFilter = filter
                        .Replace(CommaToEscape, ",");

                    if (escapedFilter.StartsWith("("))
                    {
                        var filterOpAndVal = escapedFilter.Substring(escapedFilter.LastIndexOf(")") + 1);
                        var subfilters = escapedFilter.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
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
                            Filter = escapedFilter
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
                foreach (var sort in Regex.Split(Sorts, EscapedCommaPattern))
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
