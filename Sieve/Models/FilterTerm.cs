using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class FilterTerm
    {
        private string _filter;
        private string[] operators = new string[] {
                    "==",
                    "!=",
                    ">",
                    "<",
                    ">=",
                    "<=",
                    "@=",
                    "_=" };

        public FilterTerm(string filter)
        {
            _filter = filter;
        }

        public string Name
        {
            get
            {
                var tokens = _filter.Split(operators, StringSplitOptions.RemoveEmptyEntries);
                return tokens.Length > 0 ? tokens[0].Trim() : "";
                
            }
        }

        public string Operator
        {
            get
            {
                foreach (var op in operators)
                {
                    if (_filter.IndexOf(op) != -1)
                    {
                        return op;
                    }
                }

                // Custom operators not supported
                // var tokens = _filter.Split(' ');
                // return tokens.Length > 2 ? tokens[1] : "";
                return "";
            }
        }


        public string Value {
            get
            {
                var tokens = _filter.Split(operators, StringSplitOptions.RemoveEmptyEntries);
                return tokens.Length > 1 ? tokens[1].Trim() : null;
            }
        }

        public FilterOperator OperatorParsed {
            get
            {
                switch (Operator.Trim().ToLower())
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
                        return FilterOperator.Contains;
                    case "_=":
                        return FilterOperator.StartsWith;
                    default:
                        return FilterOperator.Equals;
                }
            }
        }

    }
}