using Microsoft.CodeAnalysis;

using System.Collections.Immutable;
using System.Diagnostics;

namespace OpenQA.Selenium.DevToolsGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class DevToolsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<GeneratorSettings> settingsProvider = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
            {
                var devGenSettings = new GeneratorSettings
                (
                    ForceDownload: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_ForceDownload", out string? fd) ? bool.Parse(fd) : false,
                    Quiet: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_Quiet", out string? q) ? bool.Parse(q) : false,
                    ForceOverwrite: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_ForceOverwrite", out string? fo) ? bool.Parse(fo) : false,
                    OutputPath: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_OutputPath", out string? outputPath) ? outputPath : "./OutputProtocol",
                    BrowserProtocolPath: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_BrowserProtocolPath", out string? bpp) ? bpp : "./browser_protocol.json",
                    JavaScriptProtocolPath: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_JavaScriptProtocolPath", out string? jspp) ? jspp : "./js_protocol.json",
                    TemplatesPath: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_TemplatesPath", out string? tp) ? tp : string.Empty,
                    Settings: options.GlobalOptions.TryGetValue("build_property.DevtoolsGenerator_Settings", out string? s) ? s : "./Templates/settings.json"
                );

                return devGenSettings;
            }).WithTrackingName("Settings");

            IncrementalValueProvider<ImmutableArray<AdditionalText>> additionalTexts = context.AdditionalTextsProvider.Select((file, ct) =>
            {
                return file;
            }).Collect().WithTrackingName("Files");

            IncrementalValueProvider<GatheredData> parsedFiles = additionalTexts.Combine(settingsProvider).Select(GatherData).WithTrackingName("Data");
            context.RegisterSourceOutput(parsedFiles, Execute);
        }
    }
}
