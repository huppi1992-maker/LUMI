using System.Collections.Generic;

namespace Lumi.Models
{
    public sealed class LumiBarConfig
    {
        public List<LumiBarButtonDefinition> Buttons { get; set; } = new();
    }
}
