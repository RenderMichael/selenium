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

        AdditionalText? browserProtocolFile = null;
        AdditionalText? jsProtocolFile = null;
        AdditionalText? templatesFile = null;
        AdditionalText? settingsFile = null;
        foreach (AdditionalText file in files)
        {
            string fullPath = Path.GetFileName(file.Path);
            if (fullPath == Path.GetFileName(settings.BrowserProtocolPath))
            {
                browserProtocolFile = file;
            }
            else if (fullPath == Path.GetFileName(settings.JavaScriptProtocolPath))
            {
                jsProtocolFile = file;
            }
            else if (fullPath == Path.GetFileName(settings.TemplatesPath))
            {
                templatesFile = file;
            }
            else if (fullPath == Path.GetFileName(settings.Settings))
            {
                settingsFile = file;
            }
        }
        if (settingsFile is null)
        {
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1001", "The specified settings file could not be found", "The specified settings file ({0}) could not be found. Please check that the settings file exists.", "WebDriver", Severity, true), Location.None, settings.Settings);

            return GatheredData.FromDiagnostic(diagnostic);
        }

        SourceText? settingsText = settingsFile.GetText(ct);

        if (settingsText is null)
        {
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1002", "The specified settings file could not be read", "The specified settings file ({0}) could not be read. Please check that the settings file exists.", "WebDriver", Severity, true), Location.None, settings.Settings);

            return GatheredData.FromDiagnostic(diagnostic);
        }

        CodeGenerationSettings? generationSettings;
        try
        {
            generationSettings = JsonSerializer.Deserialize<CodeGenerationSettings>(settingsText.ToString());
        }
        catch (JsonException)
        {
            generationSettings = null;
        }

        if (generationSettings is null)
        {
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1003", "The specified settings file must contain a JSON object", "The specified settings file ({0}) must contain a JSON object. Please check that the settings file is accurate.", "WebDriver", Severity, true), Location.None, settings.Settings);

            return GatheredData.FromDiagnostic(diagnostic);
        }

        if (!string.IsNullOrEmpty(settings.TemplatesPath))
        {
            generationSettings.TemplatesPath = settings.TemplatesPath;

            var foundTemplate = files.FirstOrDefault(file => file.Path == Path.GetFullPath(settings.TemplatesPath));
            if (foundTemplate is not null)
            {
                generationSettings.TemplatesPath = GetParentDirectory(foundTemplate.Path);
            }
        }

        return new GatheredData(settings, browserProtocolFile, jsProtocolFile, templatesFile, settingsFile, null, generationSettings, files);

        static string GetParentDirectory(string childPath)
        {
            var lastSlash = childPath.LastIndexOfAny(['/', '\\']);
            if (lastSlash < 0)
            {
                throw new InvalidOperationException("Cannot get parent of root");
            }

            return childPath.Substring(0, lastSlash);
        }
    }
}
