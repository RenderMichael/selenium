using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.DevToolsGenerator.CodeGen;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Collections.Generic;
using System.IO;
using System;

namespace OpenQA.Selenium.DevToolsGenerator
{
    public partial class DevToolsGenerator
    {
        private static void Execute(SourceProductionContext source, GatheredData data)
        {
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }

            foreach (Diagnostic diag in data.InfoDiagnostics)
            {
                source.ReportDiagnostic(diag);
            }

            if (data.ErrorDiagnostic is not null)
            {
                source.ReportDiagnostic(data.ErrorDiagnostic);
                return;
            }

            // It's acceptable for this to be a heavier thing, since we'll only run this on settings/file changes

            // setup our DI
            var serviceProvider = new ServiceCollection()
                .AddCodeGenerationServices(new GatheredDataService(data))
                .BuildServiceProvider();

            //Get the protocol Data.
            //if (!data.InputSettings.Quiet)
            //{
            //    Console.WriteLine("Loading protocol definition...");
            //}

            foreach (var x in data.VersionToTexts)
            {
                var browserFile = x.Value.GetByPath("browser_protocol.json");
                if (browserFile is null)
                {
                    source.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("WebDriver1006", "browser_protocol.json file missing", "Version {0} does not contain a browser_protocol.json file", "WebDriver", Severity, true), Location.None, x.Key));
                    return;
                }

                var jsFile = x.Value.GetByPath("js_protocol.json");
                if (jsFile is null)
                {
                    source.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("WebDriver1007", "js_protocol.json file missing", "Version {0} does not contain a js_protocol.json file", "WebDriver", Severity, true), Location.None, x.Key));
                    return;
                }

                var protocolDefinitionData = GetProtocolDefinitionData(browserFile, jsFile);

                var protocolDefinition = protocolDefinitionData.Deserialize<ProtocolDefinition.ProtocolDefinition>(new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles })!;

                //Begin the code generation process.
                //if (!data.InputSettings.Quiet)
                //{
                //    Console.WriteLine("Generating protocol definition code files...");
                //}

                var protocolGenerator = serviceProvider.GetRequiredService<ICodeGenerator<ProtocolDefinition.ProtocolDefinition>>();

                IDictionary<string, string> codeFiles;
                try
                {
                    codeFiles = protocolGenerator.GenerateCode(protocolDefinition, null, x.Key.ToUpperInvariant());
                }
                catch (TemplatesManager.TemplateFileNotFoundException ex)
                {
                    source.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("WebDriver1005", "Template file missing", "Unable to locate a template at {0} - please ensure that a template file exists at this location.", "WebDriver", Severity, isEnabledByDefault: true), Location.None, ex.FilePath));
                    return;
                }
                //Delete the output folder if force is specified and it exists...
                //if (!data.InputSettings.Quiet)
                //{
                //    Console.WriteLine("Writing generated code files to {0}...", data.InputSettings.OutputPath);
                //}

                foreach (var codeFile in codeFiles)
                {
                    string targetFilePath = Path.Combine(x.Key, codeFile.Key);
                    source.AddSource(targetFilePath, SourceText.From(codeFile.Value, Encoding.UTF8));
                }
            }

            //Completed.
            //if (!data.InputSettings.Quiet)
            //{
            //    Console.WriteLine("All done!");
            //}
        }

        public static JsonObject GetProtocolDefinitionData(AdditionalText browserFile, AdditionalText jsFile)
        {
            JsonObject? browserProtocol = browserFile.GetText()?.ToString() is string browserProtocolString ? JsonNode.Parse(browserProtocolString) as JsonObject : null;
            JsonObject? jsProtocol = jsFile.GetText()?.ToString() is string jsProtocolString ? JsonNode.Parse(jsProtocolString) as JsonObject : null;

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
                jDomains.Add(domain!.DeepClone());
            }

            return result;
        }
    }
}
