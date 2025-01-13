namespace OpenQA.Selenium.Internal.DevToolsGenerator
{
    public sealed class GatheredDataService
    {
        public GatheredDataService(GatheredData data)
        {
            Data = data;
        }

        public GatheredData Data { get; }
    }
}
