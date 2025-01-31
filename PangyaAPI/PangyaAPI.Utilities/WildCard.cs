using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PangyaAPI.Utilities
{
    public class Wildcard : Regex
    {
        public Wildcard(string pattern) : base(WildcardToRegex(pattern))
        {
        }

        public Wildcard(string pattern, RegexOptions options) : base(WildcardToRegex(pattern), options)
        {
        }

        public static string WildcardToRegex(string pattern) =>
            ("^" + Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
    }
}
