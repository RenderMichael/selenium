using Microsoft.Extensions.DependencyInjection;

using OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition;

namespace OpenQA.Selenium.Internal.DevToolsGenerator.CodeGen;

/// <summary>
/// Represents a base implementation of a code generator.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CodeGeneratorBase<T> : ICodeGenerator<T>
    where T : class, IDefinition
{
    private readonly Lazy<GatheredDataService> m_settings;
    private readonly Lazy<TemplatesManager> m_templatesManager;

    /// <summary>
    /// Gets the service provider associated with the generator.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the code generation settings associated with the generator.
    /// </summary>
    public GatheredDataService Data => m_settings.Value;

    public CodeGenerationSettings Settings => Data.Data.GenerationSettings;

    /// <summary>
    /// Gets a template manager associated with the generator.
    /// </summary>
    public TemplatesManager TemplatesManager => m_templatesManager.Value;

    protected CodeGeneratorBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        m_settings = new Lazy<GatheredDataService>(() => ServiceProvider.GetRequiredService<GatheredDataService>());
        m_templatesManager = new Lazy<TemplatesManager>(() => ServiceProvider.GetRequiredService<TemplatesManager>());
    }

    public abstract IDictionary<string, string> GenerateCode(T item, CodeGeneratorContext? context);
}
