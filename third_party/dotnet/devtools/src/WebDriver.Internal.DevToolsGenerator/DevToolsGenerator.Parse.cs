using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;
using OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition;
using System.Collections.Immutable;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace OpenQA.Selenium.Internal.DevToolsGenerator;

public partial class DevToolsGenerator
{
    const DiagnosticSeverity Severity = DiagnosticSeverity.Warning; // TODO change to Error when relying on this generator in build
    private static GatheredData GatherData((ImmutableArray<AdditionalText> Left, GeneratorSettings Right) data, CancellationToken ct)
    {
        var (files, settings) = data;

        settings.BrowserProtocolPath = Path.GetFullPath(settings.BrowserProtocolPath);
        settings.JavaScriptProtocolPath = Path.GetFullPath(settings.JavaScriptProtocolPath);
        try // TODO remove the exception catching
        {
            settings.TemplatesPath = Path.GetFullPath(settings.TemplatesPath);
        }
        catch (ArgumentException) { }
        settings.Settings = Path.GetFullPath(settings.Settings);

        AdditionalText? browserProtocolFile = null;
        AdditionalText? jsProtocolFile = null;
        AdditionalText? templatesFile = null;
        AdditionalText? settingsFile = null;
        foreach (AdditionalText file in files)
        {
            string fullPath = Path.GetFullPath(file.Path);
            if (fullPath == settings.BrowserProtocolPath)
            {
                browserProtocolFile = file;
            }
            else if (fullPath == settings.JavaScriptProtocolPath)
            {
                jsProtocolFile = file;
            }
            else if (fullPath == settings.TemplatesPath)
            {
                templatesFile = file;
            }
            else if (fullPath == settings.Settings)
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
            generationSettings = JsonSerializer.Deserialize<CodeGenerationSettings>(settingsText.ToString())
                ?? throw new InvalidDataException($"CodeGenerationSettings JSON cannot be null (path: {settingsFile.Path})");
        }
        catch (JsonException)
        {
            generationSettings = null;
        }

        if (generationSettings is null)
        {
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("WebDriver1003", "The specified settings file contains invalid JSON", "The specified settings file ({0}) contains invalid JSON. Please check that the settings file is accurate.", "WebDriver", Severity, true), Location.None, settings.Settings);

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

    public static JsonObject GetProtocolDefinitionData(GatheredData args)
    {
        AdditionalText? browserProtocolPath = args.BrowserProtocolFile;
        if (args.BrowserProtocolFile is null)
        {
            browserProtocolPath = args.BrowserProtocolFile = args.AllFiles.GetByPath(Path.Combine(args.InputSettings.BrowserProtocolPath, "browser_protocol.json"));
            if (args.BrowserProtocolFile is null)
            {
                // TODO
            }
        }

        AdditionalText? jsProtocolPath = args.JsProtocolFile;
        if (args.JsProtocolFile is null)
        {
            jsProtocolPath = args.JsProtocolFile = args.AllFiles.GetByPath(Path.Combine(args.InputSettings.JavaScriptProtocolPath, "js_protocol.json"));

            if (args.JsProtocolFile is null)
            {
                // TODO
            }
        }

        JsonObject? browserProtocol = browserProtocolPath?.GetText()?.ToString() is string browserProtocolString ? JsonNode.Parse(browserProtocolString) as JsonObject : null;
        JsonObject? jsProtocol = jsProtocolPath?.GetText()?.ToString() is string jsProtocolString ? JsonNode.Parse(jsProtocolString) as JsonObject : null;

        ProtocolVersionDefinition currentVersion = new ProtocolVersionDefinition();
        currentVersion.ProtocolVersion = "1.3";
        currentVersion.Browser = "Chrome/86.0";

        JsonObject protocolData = MergeJavaScriptProtocolDefinitions(browserProtocol, jsProtocol);
        protocolData["browserVersion"] = JsonSerializer.SerializeToNode(currentVersion);

        return protocolData;
    }

    /// <summary>
    /// Merges a browserProtocol and jsProtocol into a single protocol definition.
    /// </summary>
    /// <param name="browserProtocol"></param>
    /// <param name="jsProtocol"></param>
    /// <returns></returns>
    public static JsonObject MergeJavaScriptProtocolDefinitions(JsonObject? browserProtocol, JsonObject? jsProtocol)
    {
        //Merge the 2 protocols together.
        if (jsProtocol!["version"]!["majorVersion"] != browserProtocol!["version"]!["majorVersion"] ||
            jsProtocol!["version"]!["minorVersion"] != browserProtocol!["version"]!["minorVersion"])
        {
            throw new InvalidOperationException("Protocol mismatch -- The WebKit and V8 protocol versions should match.");
        }

        var result = browserProtocol.DeepClone().AsObject();
        foreach (var domain in jsProtocol["domains"]!.AsArray())
        {
            JsonArray jDomains = result["domains"]!.AsArray();
            jDomains.Add(domain);
        }

        return result;
    }
}
