using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public interface IFilterTerm
    {
        string Name { get; }
        string Operator { get; }
        bool OperatorIsCaseInsensitive { get; }
        FilterOperator OperatorParsed { get; }
        string Value { get; }
    }
}