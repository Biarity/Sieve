using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sieve.Models
{
    public class FilterTerm : IFilterTerm
    {
        public FilterTerm() { }

        private const string EscapedPipePattern = @"(?<!($|[^\\])(\\\\)*?\\)\|";

        private static readonly string[] Operators = new string[] {
                    "==*",
                    "@=*",
                    "_=*",
                    "==",
                    "!=",
                    ">=",
                    "<=",
                    ">",
                    "<",
                    "@=",
                    "_="
        };

        public string Filter
        {
            set
            {
                var filterSplits = value.Split(Operators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()).ToArray();
                Names = Regex.Split(filterSplits[0], EscapedPipePattern).Select(t => t.Trim()).ToArray();
                Values = filterSplits.Length > 1 ? Regex.Split(filterSplits[1], EscapedPipePattern).Select(t => t.Trim()).ToArray() : null;
                Operator = Array.Find(Operators, o => value.Contains(o)) ?? "==";
                OperatorParsed = GetOperatorParsed(Operator);
                OperatorIsCaseInsensitive = Operator.Contains("*");
            }

        }

        public string[] Names { get; private set; }

        public FilterOperator OperatorParsed { get; private set; }

        public string[] Values { get; private set; }

        public string Operator { get; private set; }

        private FilterOperator GetOperatorParsed(string Operator)
        {
            switch (Operator.Trim().ToLower())
            {
                case "==":
                case "==*":
                    return FilterOperator.Equals;
                case "!=":
                    return FilterOperator.NotEquals;
                case "<":
                    return FilterOperator.LessThan;
                case ">":
                    return FilterOperator.GreaterThan;
                case ">=":
                    return FilterOperator.GreaterThanOrEqualTo;
                case "<=":
                    return FilterOperator.LessThanOrEqualTo;
                case "@=":
                case "@=*":
                    return FilterOperator.Contains;
                case "_=":
                case "_=*":
                    return FilterOperator.StartsWith;
                default:
                    return FilterOperator.Equals;
            }
        }

        public bool OperatorIsCaseInsensitive { get; private set; }
    }
}
