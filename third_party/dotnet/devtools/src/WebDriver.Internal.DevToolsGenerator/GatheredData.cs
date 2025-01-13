using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;

namespace OpenQA.Selenium.Internal.DevToolsGenerator
{
    public record struct GatheredData(
        GeneratorSettings InputSettings,
        AdditionalText? BrowserProtocolFile,
        AdditionalText? JsProtocolFile,
        AdditionalText? TemplatesFile,
        AdditionalText? SettingsFile,
        Diagnostic? Diagnostic,
        CodeGenerationSettings GenerationSettings,
        ImmutableArray<AdditionalText> AllFiles)
    {
        public static GatheredData FromDiagnostic(Diagnostic diag) => default(GatheredData) with { Diagnostic = diag };
    }
}
