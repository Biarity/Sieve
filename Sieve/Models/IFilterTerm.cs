namespace Sieve.Models
{
    public interface IFilterTerm
    {
        string Filter { set; }
        string[] Names { get; }
        string Operator { get; }
        bool OperatorIsCaseInsensitive { get; }
        bool OperatorIsNegated { get; }
        FilterOperator OperatorParsed { get; }
        string[] Values { get; }
    }
}
