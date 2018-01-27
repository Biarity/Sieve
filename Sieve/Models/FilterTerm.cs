using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class FilterTerm
    {
        private string _filter;

        public FilterTerm(string filter)
        {
            _filter = filter;
        }

        public string Name
        {
            get
            {
                return _filter.Split(' ')[0];
            }
        }

        public string Operator
        {
            get
            {
                return _filter.Split(' ')[1];
            }
        }


        public string Value {
            get
            {
                return _filter.Split(' ')[2];
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