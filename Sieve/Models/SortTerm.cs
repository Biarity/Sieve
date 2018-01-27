using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sieve.Models
{
    public class SortTerm
    {
        private string _sort;

        public SortTerm(string sort)
        {
            _sort = sort;
        }

        public string Name
        {
            get
            {
                if (_sort.StartsWith('-'))
                {
                    return _sort.Substring(1);
                }
                else
                {
                    return _sort;
                }
            }
        }

        public bool Descending
        {
            get
            {
                if (_sort.StartsWith('-'))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}