using System;
using System.Collections.Generic;

namespace rWCMS.Utilities
{
    public static class PathMatcher
    {
        private static readonly HashSet<string> ValidLanguages = new HashSet<string> { "en", "tc", "sc" };

        public static bool AppliesTo(string queryPath, string permPath, bool isGlobal = false)
        {
            if (isGlobal)
            {
                return queryPath == permPath;
            }

            if (!permPath.Contains('(') || !permPath.Contains(')'))
            {
                return queryPath.StartsWith(permPath);
            }

            int start = permPath.IndexOf('(');
            int end = permPath.IndexOf(')', start + 1);
            if (start < 0 || end < 0 || end <= start)
            {
                return false; // Invalid pattern
            }

            string prefix = permPath.Substring(0, start);
            string suffix = permPath.Substring(end + 1);
            string langsStr = permPath.Substring(start + 1, end - start - 1);
            string[] langs = langsStr.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (langs.Length == 0 || langs.Any(l => !ValidLanguages.Contains(l)))
            {
                return false; // Invalid or empty languages
            }

            foreach (var lang in langs)
            {
                string expanded = prefix + lang + suffix;
                if (queryPath.StartsWith(expanded))
                {
                    return true;
                }
            }

            return false;
        }
    }
}