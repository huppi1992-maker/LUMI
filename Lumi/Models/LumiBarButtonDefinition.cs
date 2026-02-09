using System.Text.Json.Serialization;

namespace Lumi.Models
{
    public sealed class LumiBarButtonDefinition
    {
        // Stabile interne ID
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        // Interner Name (wird in Settings angezeigt)
        public string Name { get; set; } = "Neuer Button";
        // Optionales UI-Label (kann später im Button angezeigt werden)
        public string Label { get; set; } = "";
        public string IconKey { get; set; } = "";

        // Hex Strings statt Brush, weil JSON
        public string FillHex { get; set; } = "#3A7BD5";
        public string HoverHex { get; set; } = "#4C8EE6";
        public string PressedHex { get; set; } = "#2E5FA8";

        public bool IsEnabled { get; set; } = true;

        // Reihenfolge explizit speichern
        public int Order { get; set; }

        // Später für Aktionen (z.B. "open_settings", "open_calc", "launch_app")
        public string ActionId { get; set; } = "";
    }
}
