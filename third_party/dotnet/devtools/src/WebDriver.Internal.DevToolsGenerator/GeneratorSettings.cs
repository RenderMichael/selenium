namespace OpenQA.Selenium.Internal.DevToolsGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ForceDownload">Forces the Chrome Protocol Definition to be downloaded from source even if it already exists.</param>
    /// <param name="Quiet">Suppresses console output.</param>
    /// <param name="ForceOverwrite">Forces the output directory to be overwritten</param>
    /// <param name="OutputPath">Indicates the folder that will contain the generated class library [Default: ./OutputProtocol]</param>
    /// <param name="BrowserProtocolPath">Indicates the path to the Chromium Debugging Browser Protocol JSON file to use. [Default: browser_protocol.json]</param>
    /// <param name="JavaScriptProtocolPath">Indicates the path to the Chromium Debugging JavaScript Protocol JSON file to use. [Default: js_protocol.json]</param>
    /// <param name="TemplatesPath">Indicates the path to the code generation templates file.</param>
    /// <param name="Settings">Indicates the path to the code generation settings file. [Default: ./Templates/settings.json]</param>
    public record struct GeneratorSettings(
        bool ForceDownload,
        bool Quiet,
        bool ForceOverwrite,
        string OutputPath,
        string BrowserProtocolPath,
        string JavaScriptProtocolPath,
        string TemplatesPath,
        string Settings);
}
