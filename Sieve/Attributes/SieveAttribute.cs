using System;
using System.Collections.Generic;
using System.Text;

namespace Sieve.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class SieveAttribute : Attribute
    {
        /// <summary>
        /// Override name used 
        /// </summary>
        public string Name { get; set; }

        public bool CanSort { get; set; }
        public bool CanFilter { get; set; }
    }
}
