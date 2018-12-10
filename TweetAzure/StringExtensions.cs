using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TweetAzure
{
    public static class StringExtensions
    {
        public static string ConvertToUsualUrl(this string input, List<KeyValuePair<string, string>> replacements)
        {
            StringBuilder returnString = new StringBuilder(input);
            foreach (var replacement in replacements)
            {
                returnString.Replace(replacement.Key, replacement.Value);
            }

            return Regex.Replace(returnString.ToString(), @"https:\/\/t\.co\/.+$", string.Empty);
        }
    }
}
