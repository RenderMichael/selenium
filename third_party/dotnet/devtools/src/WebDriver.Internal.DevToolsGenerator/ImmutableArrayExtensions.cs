using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace OpenQA.Selenium.Internal.DevToolsGenerator;

public static class ImmutableArrayExtensions
{
    public static AdditionalText? GetByPath(this ImmutableArray<AdditionalText> files, string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        path = Path.GetFullPath(path);
        foreach (AdditionalText file in files)
        {
            string thisPath = Path.GetFullPath(file.Path);

            if (string.Equals(thisPath, path, StringComparison.Ordinal))
            {
                return file;
            }
        }

        return null;
    }
}
