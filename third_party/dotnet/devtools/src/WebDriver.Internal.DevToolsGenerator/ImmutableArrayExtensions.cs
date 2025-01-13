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

        foreach (AdditionalText file in files)
        {
            if (string.Equals(Path.GetFileName(file.Path), Path.GetFileName(path), StringComparison.Ordinal))
            {
                return file;
            }
        }

        return null;
    }
}
