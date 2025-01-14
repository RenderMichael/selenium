namespace OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition;

    /// <summary>
    /// Contains various utility methods.
    /// </summary>
    public static class Utility
    {
        public static readonly string NewLine = @"
";

        /// <summary>
        /// Replaces tokens in the target path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="className"></param>
        /// <param name="context"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ReplaceTokensInPath(string path, string className, CodeGeneratorContext context, CodeGenerationSettings settings, string versionString)
        {
            if (versionString is null)
            {
                throw new ArgumentNullException(nameof(versionString));
            }

            path = path.Replace("{{className}}", className);
            path = path.Replace("{{rootNamespace}}", settings.RootNamespace + "." + versionString);
            path = path.Replace("{{templatePath}}", settings.TemplatesPath);
            path = path.Replace("{{domainName}}", context.Domain!.Name);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            path = path.Replace("{{separator}}", Path.DirectorySeparatorChar.ToString());
            return path;
        }

        /// <summary>
        /// For the given type, gets the associated type mapping given the domain and known types.
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="domainDefinition"></param>
        /// <param name="knownTypes"></param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        public static string GetTypeMappingForType(TypeDefinition typeDefinition, DomainDefinition domainDefinition, IDictionary<string, TypeInfo> knownTypes, bool isArray = false)
        {
            var type = typeDefinition.Type;

            if (string.IsNullOrWhiteSpace(type))
            {
                type = typeDefinition.TypeReference;
            }

            string? mappedType = null;
            if (type!.Contains(".") && knownTypes.ContainsKey(type))
            {
                var typeInfo = knownTypes[type];
                if (typeInfo.IsPrimitive)
                {
                    var primitiveType = typeInfo.TypeName!;

                    if (typeDefinition.Optional && typeInfo.ByRef)
                    {
                        primitiveType += "?";
                    }

                    if (isArray)
                    {
                        primitiveType += "[]";
                    }

                    return primitiveType;
                }
                mappedType = $"{typeInfo.Namespace}.{typeInfo.TypeName}";
                if (typeDefinition.Optional && typeInfo.ByRef)
                {
                    mappedType += "?";
                }
            }

            if (mappedType == null)
            {
                var fullyQualifiedTypeName = $"{domainDefinition.Name}.{type}";

                if (knownTypes.ContainsKey(fullyQualifiedTypeName))
                {
                    var typeInfo = knownTypes[fullyQualifiedTypeName];

                    mappedType = typeInfo.TypeName;
                    if (typeInfo.ByRef && typeDefinition.Optional)
                    {
                        mappedType += "?";
                    }
                }
            }


            mappedType ??= type switch
            {
                "number" => typeDefinition.Optional ? "double?" : "double",
                "integer" => typeDefinition.Optional ? "long?" : "long",
                "boolean" => typeDefinition.Optional ? "bool?" : "bool",
                "string" => "string",
                "object" or "any" => "object",
                "binary" => "byte[]",
                "array" => GetTypeMappingForType(typeDefinition.Items!, domainDefinition, knownTypes, true),
                _ => throw new InvalidOperationException($"Unmapped data type: {type}"),
            };

            if (isArray)
            {
                mappedType += "[]";
            }

            return mappedType;
        }

        public static string? ReplaceLineEndings(string? value, string? replacement = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            replacement ??= string.Empty;

            return Regex.Replace(value, @"\r\n?|\n|\u2028|\u2029", replacement, RegexOptions.Compiled);
        }
    }
}
