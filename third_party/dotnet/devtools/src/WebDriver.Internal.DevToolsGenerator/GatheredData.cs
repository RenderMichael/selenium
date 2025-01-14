using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;

namespace OpenQA.Selenium.Internal.DevToolsGenerator
{
    public record struct GatheredData(
        GeneratorSettings InputSettings,
        Dictionary<string, ImmutableArray<AdditionalText>> VersionToTexts,
        AdditionalText? SettingsFile,
        Diagnostic? ErrorDiagnostic,
        CodeGenerationSettings GenerationSettings,
        ImmutableArray<AdditionalText> AllFiles,
        List<Diagnostic> InfoDiagnostics)
    {
        public static GatheredData FromDiagnostic(Diagnostic diag) => default(GatheredData) with { ErrorDiagnostic = diag };
    }
}
