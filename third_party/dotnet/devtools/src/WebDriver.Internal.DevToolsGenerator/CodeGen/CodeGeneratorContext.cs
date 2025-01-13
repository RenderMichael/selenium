using OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition;

namespace OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;

/// <summary>
/// Represents the current context of the code generator.
/// </summary>
public sealed class CodeGeneratorContext(DomainDefinition domain, Dictionary<string, TypeInfo>? knownTypes)
{
    public DomainDefinition? Domain { get; set; } = domain;

    public Dictionary<string, TypeInfo>? KnownTypes { get; set; } = knownTypes;
}
