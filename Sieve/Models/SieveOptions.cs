namespace Sieve.Models
{
    public class SieveOptions
    {
        /// <summary>
        /// If flag is set, property names have to match including case sensitivity.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Fallback value of no page size is specified in the request.
        /// </summary>
        /// <remarks>Values less or equal to 0 disable paging.</remarks>
        public int DefaultPageSize { get; set; } = 0;

        /// <summary>
        /// Specifies the upper limit of a page size to be requested.
        /// </summary>
        /// <remarks>Values less or equal to 0 are ignored.</remarks>
        public int MaxPageSize { get; set; } = 0;

        /// <summary>
        /// If flag is set, Sieve throws exception otherwise exceptions are caught and the already processed
        /// result is returned.
        /// </summary>
        public bool ThrowExceptions { get; set; } = true;
    }
}
