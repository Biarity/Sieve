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
                return _filter.Split(operators, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                
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

                var tokens = _filter.Split(' ');
                return tokens.Length > 1 ? tokens[1] : "";
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
                    case "equals":
                    case "eq":
                    case "==":
                        return FilterOperator.Equals;
                    case "notequals":
                    case "nq":
                    case "!=":
                        return FilterOperator.NotEquals;
                    case "lessthan":
                    case "lt":
                    case "<":
                        return FilterOperator.LessThan;
                    case "greaterthan":
                    case "gt":
                    case ">":
                        return FilterOperator.GreaterThan;
                    case "greaterthanorequalto":
                    case "gte":
                    case ">=":
                        return FilterOperator.GreaterThanOrEqualTo;
                    case "lessthanorequalto":
                    case "lte":
                    case "<=":
                        return FilterOperator.LessThanOrEqualTo;
                    case "contains":
                    case "co":
                    case "@=":
                        return FilterOperator.Contains;
                    case "startswith":
                    case "sw":
                    case "_=":
                        return FilterOperator.StartsWith;
                    default:
                        return FilterOperator.Equals;
                }
            }
        }

    }
}