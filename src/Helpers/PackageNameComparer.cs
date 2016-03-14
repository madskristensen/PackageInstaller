using System;

namespace PackageInstaller
{
    internal class PackageNameComparer : StringComparer
    {
        static char[] _separators = { '.', '-', '_' };
        const StringComparison _ignore = StringComparison.OrdinalIgnoreCase;

        public override int Compare(string x, string y)
        {
            if (x.EndsWith("js", _ignore)) x = x.Substring(0, x.Length - 2);
            if (y.EndsWith("js", _ignore)) y = y.Substring(0, y.Length - 2);

            string right = x.TrimEnd(_separators);
            string left = y.TrimEnd(_separators);

            foreach (var sep in _separators)
            {
                if (right.StartsWith(left + sep, _ignore))
                    return 1;

                if (left.StartsWith(right + sep, _ignore))
                    return -1;
            }

            return string.Compare(right, left, _ignore);
        }

        public override bool Equals(string x, string y)
        {
            return string.Compare(x, y, _ignore) == 0;
        }

        public override int GetHashCode(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return 0;

            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}
