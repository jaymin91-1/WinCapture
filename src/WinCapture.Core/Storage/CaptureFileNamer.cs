using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace WinCapture.Core.Storage;

/// <summary>Builds screenshot file names and save paths from the file name pattern (SPEC F-09).</summary>
public static class CaptureFileNamer
{
    public const string ScreenshotSubfolder = "Screenshots";
    private const string CounterToken = "{counter}";
    private const char Separator = '\\';
    private static readonly char[] InvalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    /// <summary>
    /// Returns the full save path. <paramref name="expandEnvironment"/> expands %VAR% in the save folder
    /// (injected so Core does not touch the OS).
    /// </summary>
    public static string BuildPath(
        string pattern,
        DateTime now,
        int width,
        int height,
        string type,
        string saveFolder,
        bool useTypeSubfolders,
        Func<string, string> expandEnvironment,
        ISaveFileSystem fileSystem)
    {
        var root = expandEnvironment(saveFolder).TrimEnd('\\', '/');
        var folder = useTypeSubfolders ? root + Separator + ScreenshotSubfolder : root;

        var resolved = ResolveTokens(pattern, now, width, height, type);
        string baseName;
        if (resolved.Contains(CounterToken, StringComparison.Ordinal))
        {
            var counter = NextCounter(pattern, type, fileSystem.GetFileNames(folder));
            baseName = Sanitize(resolved.Replace(CounterToken, counter.ToString("D4", CultureInfo.InvariantCulture), StringComparison.Ordinal));
        }
        else
        {
            baseName = Sanitize(resolved);
        }

        var path = folder + Separator + baseName + "." + type;
        for (var i = 1; fileSystem.FileExists(path); i++)
        {
            path = folder + Separator + baseName + "_" + i.ToString(CultureInfo.InvariantCulture) + "." + type;
        }

        return path;
    }

    /// <summary>Replaces every character that is invalid in a Windows file name with '_'.</summary>
    public static string Sanitize(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            sb.Append(Array.IndexOf(InvalidChars, c) >= 0 ? '_' : c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Largest number among existing files matching the pattern, plus one; 1 when none match.
    /// {date}, {time}, {w}, {h} match any value; {type} and literal text must match. A trailing _N duplicate suffix is tolerated.
    /// </summary>
    public static int NextCounter(string pattern, string type, IEnumerable<string> existingFileNames)
    {
        var regex = new StringBuilder("^");
        var counterSeen = false;
        var i = 0;
        var literal = new StringBuilder();

        void FlushLiteral()
        {
            if (literal.Length > 0)
            {
                regex.Append(Regex.Escape(Sanitize(literal.ToString())));
                literal.Clear();
            }
        }

        while (i < pattern.Length)
        {
            string? replacement = null;
            var tokenLength = 0;
            foreach (var (token, rx) in WildcardTokens)
            {
                if (string.CompareOrdinal(pattern, i, token, 0, token.Length) == 0)
                {
                    replacement = rx;
                    tokenLength = token.Length;
                    break;
                }
            }

            if (replacement is null && string.CompareOrdinal(pattern, i, CounterToken, 0, CounterToken.Length) == 0)
            {
                replacement = counterSeen ? "\\d+" : "(\\d+)";
                counterSeen = true;
                tokenLength = CounterToken.Length;
            }

            if (replacement is null && string.CompareOrdinal(pattern, i, "{type}", 0, 6) == 0)
            {
                literal.Append(type);
                i += 6;
                continue;
            }

            if (replacement is null)
            {
                literal.Append(pattern[i]);
                i++;
                continue;
            }

            FlushLiteral();
            regex.Append(replacement);
            i += tokenLength;
        }

        FlushLiteral();
        regex.Append("(?:_\\d+)?\\.").Append(Regex.Escape(type)).Append('$');
        var re = new Regex(regex.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var max = 0;
        foreach (var name in existingFileNames)
        {
            var m = re.Match(name);
            if (m.Success && m.Groups.Count > 1 && int.TryParse(m.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > max)
            {
                max = n;
            }
        }

        return max + 1;
    }

    private static readonly (string Token, string Regex)[] WildcardTokens =
    {
        ("{date}", "\\d{8}"),
        ("{time}", "\\d{6}"),
        ("{w}", "\\d+"),
        ("{h}", "\\d+"),
    };

    private static string ResolveTokens(string pattern, DateTime now, int width, int height, string type) =>
        pattern
            .Replace("{date}", now.ToString("yyyyMMdd", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{time}", now.ToString("HHmmss", CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{w}", width.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{h}", height.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{type}", type, StringComparison.Ordinal);
}
