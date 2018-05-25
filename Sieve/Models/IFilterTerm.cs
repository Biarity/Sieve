namespace Sieve.Models
{
    public interface IFilterTerm
    {
        string[] Names { get; }
        string Operator { get; }
        bool OperatorIsCaseInsensitive { get; }
        FilterOperator OperatorParsed { get; }
        string Value { get; }
    }
}