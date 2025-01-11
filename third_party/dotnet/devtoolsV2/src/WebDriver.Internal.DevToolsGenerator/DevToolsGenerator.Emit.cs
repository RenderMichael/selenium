using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace OpenQA.Selenium.Internal.DevToolsGenerator;

public partial class DevToolsGenerator
{
    private static void Execute(SourceProductionContext source, GatheredData data)
    {
        if (data.Diagnostic is not null)
        {
            source.ReportDiagnostic(data.Diagnostic);
            return;
        }

        // It's acceptable for this to be a heavier thing, since we'll only run this on settings/file changes

        // setup our DI
        var serviceProvider = new ServiceCollection()
            .AddCodeGenerationServices(data.GenerationSettings)
            .BuildServiceProvider();

        //Get the protocol Data.
        //if (!data.InputSettings.Quiet)
        //{
        //    Console.WriteLine("Loading protocol definition...");
        //}

        var protocolDefinitionData = GetProtocolDefinitionData(data);

        var protocolDefinition = protocolDefinitionData.Deserialize<ProtocolDefinition.ProtocolDefinition>(new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles });

        //Begin the code generation process.
        //if (!data.InputSettings.Quiet)
        //{
        //    Console.WriteLine("Generating protocol definition code files...");
        //}

        var protocolGenerator = serviceProvider.GetRequiredService<ICodeGenerator<ProtocolDefinition.ProtocolDefinition>>();

        var codeFiles = protocolGenerator.GenerateCode(protocolDefinition, null);

        //Delete the output folder if force is specified and it exists...
        //if (!data.InputSettings.Quiet)
        //{
        //    Console.WriteLine("Writing generated code files to {0}...", data.InputSettings.OutputPath);
        //}

        foreach (var codeFile in codeFiles)
        {
            var targetFilePath = Path.Combine(data.InputSettings.OutputPath, codeFile.Key);
            source.AddSource(targetFilePath, SourceText.From(codeFile.Value, Encoding.UTF8));
        }

        //Completed.
        //if (!data.InputSettings.Quiet)
        //{
        //    Console.WriteLine("All done!");
        //}
    }
}
