using Humanizer;
using OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition;
using System;
using System.Collections.Generic;

namespace OpenQA.Selenium.DevToolsGenerator.CodeGen
{
    /// <summary>
    /// Generates code for Event Definitions
    /// </summary>
    public sealed class EventGenerator : CodeGeneratorBase<EventDefinition>
    {
        public EventGenerator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override IDictionary<string, string> GenerateCode(EventDefinition eventDefinition, CodeGeneratorContext context, string versionString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(Settings.DefinitionTemplates.EventTemplate.TemplatePath))
            {
                return result;
            }

            var eventGenerator = TemplatesManager.GetGeneratorForTemplate(Settings.DefinitionTemplates.EventTemplate);

            var className = eventDefinition.Name.Dehumanize();

            string codeResult = eventGenerator(new
            {
                @event = eventDefinition,
                className = className,
                domain = context.Domain,
                rootNamespace = Settings.RootNamespace + "." + versionString,
                context = context
            });

            var outputPath = Utility.ReplaceTokensInPath(Settings.DefinitionTemplates.EventTemplate.OutputPath!, className, context, Settings, versionString);
            result.Add(outputPath, codeResult);

            return result;
        }
    }
}
