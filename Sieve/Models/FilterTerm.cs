using System;
using System.Linq;

namespace Sieve.Models
{
    public class FilterTerm : IFilterTerm
    {
        public FilterTerm() { }

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
                var filterSplits = value.Split(Operators, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
                Names = filterSplits[0].Split('|').Select(t => t.Trim()).ToArray();
                Value = filterSplits.Length > 1 ? filterSplits[1] : null;
                Operator = Array.Find(Operators, o => value.Contains(o)) ?? "==";
                OperatorParsed = GetOperatorParsed(Operator);
                OperatorIsCaseInsensitive = Operator.Contains("*");
            }

        }

        public string[] Names { get; private set; }

        public FilterOperator OperatorParsed { get; private set; }

        public string Value { get; private set; }

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
