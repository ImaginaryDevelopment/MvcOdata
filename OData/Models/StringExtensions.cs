using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OData.Models
{
    public static class StringExtensions
    {

        /// <summary>
        ///     Don't hate me because I'm beautiful.
        ///     http://stackoverflow.com/a/791065/57883
        ///     http://stackoverflow.com/questions/790810/is-extending-string-class-with-isnullorempty-confusing
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        ///     Don't hate me because I'm beautiful.
        ///     http://stackoverflow.com/a/791065/57883
        ///     http://stackoverflow.com/questions/790810/is-extending-string-class-with-isnullorempty-confusing
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string Humanize(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var sb = new StringBuilder();

            var last = char.MinValue;

            foreach (var c in source)
            {
                if (char.IsLower(last) && char.IsUpper(c))
                {
                    sb.Append(' ');
                }

                sb.Append(c);
                last = c;
            }

            return sb.ToString();
        }
    }
}