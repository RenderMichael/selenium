namespace OpenQA.Selenium.Internal.DevToolsGenerator.ProtocolDefinition
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.Json.Serialization;

    public sealed class CommandDefinition : ProtocolDefinitionItem
    {
        [JsonPropertyName("handlers")]
        public ICollection<string> Handlers { get; set; } = new HashSet<string>();

        [JsonPropertyName("parameters")]
        public ICollection<TypeDefinition> Parameters { get; set; } = new Collection<TypeDefinition>();

        [JsonPropertyName("returns")]
        public ICollection<TypeDefinition> Returns { get; set; } = new Collection<TypeDefinition>();

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; }

        [JsonIgnore]
        public bool NoParameters => Parameters == null || Parameters.Count == 0;
    }
}
