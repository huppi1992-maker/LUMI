using System.Windows.Input;
using System.Windows.Media;

namespace Lumi.Infrastructure
{
    public sealed class LumiBarRuntimeButton
    {
        public string Name { get; init; } = "";
        public string Label { get; init; } = "";

        public Geometry IconData { get; init; } = Geometry.Empty;

        public Brush IconFill { get; init; } = Brushes.White;
        public Brush IconFillHover { get; init; } = Brushes.White;
        public Brush IconFillPressed { get; init; } = Brushes.White;

        public ICommand Command { get; init; } = default!;
    }
}
