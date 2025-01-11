namespace OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.Json.Serialization;

    public sealed class EventDefinition : ProtocolDefinitionItem
    {
        [JsonPropertyName("parameters")]
        public ICollection<TypeDefinition> Parameters { get; set; } = new Collection<TypeDefinition>();
    }
}
