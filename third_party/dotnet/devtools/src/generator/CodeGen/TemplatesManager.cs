using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using Humanizer;
using System.Linq;
using System.Text;
using OpenQA.Selenium.DevToolsGenerator.ProtocolDefinition;
using Microsoft.CodeAnalysis;

namespace OpenQA.Selenium.DevToolsGenerator.CodeGen
{
    /// <summary>
    /// Represents a class that manages templates and their associated generators.
    /// </summary>
    public sealed class TemplatesManager
    {
        private readonly Dictionary<AdditionalText, Func<object, string>> m_templateGenerators = new Dictionary<AdditionalText, Func<object, string>>();

        /// <summary>
        /// Gets the code generation settings associated with the protocol generator
        /// </summary>
        public GatheredDataService Settings { get; }

        public TemplatesManager(GatheredDataService settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Returns a generator singleton for the specified template settings.
        /// </summary>
        /// <param name="templateSettings">The settings for a generator.</param>
        /// <returns></returns>
        public Func<object, string> GetGeneratorForTemplate(CodeGenerationTemplateSettings templateSettings)
        {
            var templatePath = templateSettings.TemplatePath!;

            AdditionalText? templateFile = Settings.Data.AllFiles.GetByPath(templatePath);
            if (templateFile is not null && m_templateGenerators.TryGetValue(templateFile, out var cachedTemplateFunc))
            {
                return cachedTemplateFunc;
            }

            if (templateFile is null && !Path.IsPathRooted(templatePath))
            {
                templateFile = Settings.Data.AllFiles.GetByPath(Path.Combine(Settings.Data.GenerationSettings.TemplatesPath, templatePath));
            }

            if (templateFile is null)
            {
                throw new TemplateFileNotFoundException($"Unable to locate a template at {templatePath} - please ensure that a template file exists at this location.", templatePath);
            }

            var templateContents = templateFile.GetText()?.ToString() ?? throw new IOException($"TemplatesManager - Unable to read from file {templateFile.Path}");

            Handlebars.RegisterHelper("dehumanize", (writer, context, arguments) =>
            {
                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{humanize}} helper must have exactly one argument");
                }

                var str = arguments[0].ToString();

                //Some overrides for values that start with '-' -- this fixes two instances in Runtime.UnserializableValue
                if (str.StartsWith("-"))
                {
                    str = $"Negative{str.Dehumanize()}";
                }
                else
                {
                    str = str.Dehumanize();
                }

                writer.WriteSafeString(str.Dehumanize());
            });

            Handlebars.RegisterHelper("xml-code-comment", (writer, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    throw new HandlebarsException("{{code-comment}} helper must have at least one argument");
                }

                var str = arguments[0] == null ? "" : arguments[0].ToString();

                if (string.IsNullOrWhiteSpace(str))
                {
                    switch (context)
                    {
                        case ProtocolDefinitionItem pdi:
                            str = $"{pdi.Name}";
                            break;
                        default:
                            str = context.className;
                            break;
                    }
                }

                var frontPaddingObj = arguments.ElementAtOrDefault(1);
                var frontPadding = 1;
                if (frontPaddingObj != null)
                {
                    int.TryParse(frontPaddingObj.ToString(), out frontPadding);
                }

                str = Utility.ReplaceLineEndings(str, Utility.NewLine + new StringBuilder(4 * frontPadding).Insert(0, "    ", frontPadding) + "/// ");

                writer.WriteSafeString(str);
            });

            Handlebars.RegisterHelper("typemap", (writer, context, arguments) =>
            {
                if (context is not TypeDefinition typeDefinition)
                {
                    throw new HandlebarsException("{{typemap}} helper expects to be in the context of a TypeDefinition.");
                }

                if (arguments.Length != 1)
                {
                    throw new HandlebarsException("{{typemap}} helper expects exactly one argument - the CodeGeneratorContext.");
                }

                if (arguments[0] is not CodeGeneratorContext codeGenContext)
                {
                    throw new InvalidOperationException("Expected context argument to be non-null.");
                }

                var mappedType = Utility.GetTypeMappingForType(typeDefinition, codeGenContext.Domain!, codeGenContext.KnownTypes!);
                writer.WriteSafeString(mappedType);
            });

            Handlebars.Configuration.TextEncoder = null;
            return Handlebars.Compile(templateContents);
        }

        public sealed class TemplateFileNotFoundException(string message, string filePath) : FileNotFoundException(message)
        {
            public string FilePath { get; } = filePath;
        }

    }
}
