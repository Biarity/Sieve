using System;
using System.Linq;

namespace Sieve.Models
{
    public class FilterTerm : IFilterTerm
    {
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

        public FilterTerm(string filter)
        {
            var filterSplits = filter.Split(Operators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
            Names = filterSplits[0].Split('|').Select(t => t.Trim()).ToArray();
            Value = filterSplits.Length > 1 ? filterSplits[1] : null;
            Operator = Array.Find(Operators, o => filter.Contains(o)) ?? "==";
            OperatorParsed = GetOperatorParsed(Operator);
            OperatorIsCaseInsensitive = Operator.Contains("*");
        }

        public string[] Names { get; }

        public FilterOperator OperatorParsed { get; }

        public string Value { get; }

        public string Operator { get; }

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

        public bool OperatorIsCaseInsensitive { get; }
    }
}