using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;
using OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition;
using System.Collections.Immutable;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Diagnostics;

namespace OpenQA.Selenium.Internal.DevToolsGenerator;

public partial class DevToolsGenerator
{
    const DiagnosticSeverity Severity = DiagnosticSeverity.Warning; // TODO change to Error when relying on this generator in build
    private static GatheredData GatherData((ImmutableArray<AdditionalText> Left, GeneratorSettings Right) data, CancellationToken ct)
    {
        if (!Debugger.IsAttached)
        {
            //       Debugger.Launch();
        }

        var (files, settings) = data;

        List<Diagnostic> diagnostic = new();
        var versionToTexts = new Dictionary<string, ImmutableArray<AdditionalText>>(StringComparer.Ordinal);

        AdditionalText? settingsFile = null;
        foreach (AdditionalText file in files)
        {
            string fileName = Path.GetFileName(file.Path);
            if (fileName == Path.GetFileName(settings.Settings))
            {
                settingsFile = file;
                continue;
            }

            int versionPathIndex = file.Path.IndexOf("chromium");
            if (versionPathIndex >= 0)
            {
                if (!Debugger.IsAttached)
                {
                    //     Debugger.Launch();
                }

                ReadOnlySpan<char> localPath = file.Path.AsSpan(versionPathIndex + "chromium".Length + 1);
                int secondSlash = localPath.IndexOf('\\');
                if (secondSlash >= 0)
                {
                    string versionString = localPath.Slice(0, secondSlash).ToString();
                    if (versionToTexts.TryGetValue(versionString, out var existingList))
                    {
                        versionToTexts[versionString] = existingList.Add(file);
                    }
                    else
                    {
                        versionToTexts.Add(versionString, [file]);
                    }

                    continue;
                }
            }

            diagnostic.Add(Diagnostic.Create(new DiagnosticDescriptor("WebDriver1001", "Unused file", "The specified file ({0}) was unused. Please check that the file is necessary.", "WebDriver", DiagnosticSeverity.Warning, true), Location.None, file.Path));
        }

        SourceText? settingsText = settingsFile?.GetText(ct);
        if (settingsText is null)
        {
            var diag = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1002", "The specified settings file could not be read", "The specified settings file ({0}) could not be read. Please check that the settings file exists.", "WebDriver", Severity, true), Location.None, settings.Settings);

            return GatheredData.FromDiagnostic(diag);
        }

        CodeGenerationSettings? generationSettings;
        try
        {
            generationSettings = JsonSerializer.Deserialize<CodeGenerationSettings>(settingsText.ToString())
                ?? throw new JsonException("Value was null");
        }
        catch (JsonException ex)
        {
            var diag = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1003", "The specified settings file must contain a JSON object", "The specified settings file ({0}) must contain a JSON object. Failed with message '{1}'.", "WebDriver", Severity, true), Location.None, settings.Settings, ex.Message);

            return GatheredData.FromDiagnostic(diag);
        }

        if (!string.IsNullOrEmpty(settings.TemplatesPath))
        {
            // TODO is this entire if block necessary?
            generationSettings.TemplatesPath = settings.TemplatesPath;

            var foundTemplate = files.FirstOrDefault(file => file.Path == Path.GetFullPath(settings.TemplatesPath));
            if (foundTemplate is not null)
            {
                generationSettings.TemplatesPath = GetParentDirectory(foundTemplate.Path);
            }
        }

        return new GatheredData(settings, versionToTexts, settingsFile, null, generationSettings, files, diagnostic);
    }

    private static string GetParentDirectory(string childPath)
    {
        var lastSlash = childPath.LastIndexOfAny(PathSeparatorChars);
        if (lastSlash < 0)
        {
            throw new InvalidOperationException("Cannot get parent of root");
        }

        return childPath.Substring(0, lastSlash);
    }

    private static readonly char[] PathSeparatorChars = ['/', '\\'];
}
