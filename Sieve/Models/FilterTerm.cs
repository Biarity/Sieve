using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sieve.Models
{
    public class FilterTerm : IFilterTerm, IEquatable<FilterTerm>
    {
        private const string EscapedPipePattern = @"(?<!($|[^\\]|^)(\\\\)*?\\)\|";
        private const string PipeToEscape = @"\|";
        private const string BackslashToEscape = @"\\";
        private const string OperatorsRegEx = @"(!@=\*|!_=\*|!=\*|!@=|!_=|==\*|@=\*|_=\*|==|!=|>=|<=|>|<|@=|_=)";
        private const string EscapeNegPatternForOper = @"(?<!\\)" + OperatorsRegEx;
        private const string EscapePosPatternForOper = @"(?<=\\)" + OperatorsRegEx;

        public string Filter
        {
            set
            {
                var filterSplits = Regex.Split(value,EscapeNegPatternForOper).Select(t => t.Trim()).ToArray();
                
                Names = Regex.Split(filterSplits[0], EscapedPipePattern).Select(t => t.Trim()).ToArray();

                if (filterSplits.Length > 2)
                {
                    foreach (var match in Regex.Matches(filterSplits[2],EscapePosPatternForOper))
                    {
                        var matchStr = match.ToString();
                        filterSplits[2] = filterSplits[2].Replace('\\' + matchStr, matchStr);
                    }

                    Values = Regex.Split(filterSplits[2], EscapedPipePattern)
                        .Select(t => t.Replace(PipeToEscape, "|").Trim())
                        .Select(t => t.Replace(BackslashToEscape, "\\").Trim())
                        .ToArray();
                }
 
                Operator = Regex.Match(value,EscapeNegPatternForOper).Value;
                OperatorParsed = GetOperatorParsed(Operator);
                OperatorIsCaseInsensitive = Operator.EndsWith("*");
                OperatorIsNegated = OperatorParsed != FilterOperator.NotEquals && Operator.StartsWith("!");
            }

        }

        public string[] Names { get; private set; }

        public FilterOperator OperatorParsed { get; private set; }

        public string[] Values { get; private set; }

        public string Operator { get; private set; }

        private FilterOperator GetOperatorParsed(string @operator)
        {
            switch (@operator.TrimEnd('*'))
            {
                case "==":
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
                case "!@=":
                    return FilterOperator.Contains;
                case "_=":
                case "!_=":
                    return FilterOperator.StartsWith;
                default:
                    return FilterOperator.Equals;
            }
        }

        public bool OperatorIsCaseInsensitive { get; private set; }

        public bool OperatorIsNegated { get; private set; }

        public bool Equals(FilterTerm other)
        {
            return Names.SequenceEqual(other.Names)
                && Values.SequenceEqual(other.Values)
                && Operator == other.Operator;
        }
    }
}
