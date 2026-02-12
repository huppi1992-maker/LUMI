using Lumi.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Lumi.Infrastructure
{
    public sealed class LumiBarConfigService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static event EventHandler? ConfigChanged;   // Disk Save
        public static event EventHandler? PreviewChanged;  // Live typing preview

        private static readonly object PreviewLock = new();
        private static Timer? _previewTimer;

        public string ConfigPath { get; }

        public LumiBarConfigService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LUMI");

            Directory.CreateDirectory(dir);
            ConfigPath = Path.Combine(dir, "lumi-bar.json");
        }

        public LumiBarConfig LoadOrCreateDefault()
        {
            if (!File.Exists(ConfigPath))
            {
                var cfg = CreateDefault();
                Save(cfg);
                return cfg;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                var loaded = JsonSerializer.Deserialize<LumiBarConfig>(json, JsonOptions);

                // Wenn JSON leer/kaputt ist, lieber sauber auf Default zurückfallen
                return loaded ?? CreateDefault();
            }
            catch
            {
                // IO-Probleme oder ungültige JSON dürfen das Programm nicht killen
                return CreateDefault();
            }
        }

        public void Save(LumiBarConfig config)
        {
            NormalizeOrder(config);

            var json = JsonSerializer.Serialize(config, JsonOptions);

            // Atomarer Write: verhindert beschädigte JSON bei Crash während des Schreibens
            var tmp = ConfigPath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(ConfigPath))
            {
                File.Replace(tmp, ConfigPath, destinationBackupFileName: null);
            }
            else
            {
                File.Move(tmp, ConfigPath);
            }

            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        // Call this when user edits fields (no disk write)
        public static void NotifyPreviewChangedDebounced(int delayMs = 150)
        {
            lock (PreviewLock)
            {
                _previewTimer?.Dispose();

                _previewTimer = new Timer(_ =>
                {
                    // Timer läuft auf ThreadPool. UI-Seite muss bei Bedarf per Dispatcher wechseln.
                    PreviewChanged?.Invoke(null, EventArgs.Empty);

                    // Nach Ausführung wieder entsorgen, damit nichts "hängen" bleibt
                    lock (PreviewLock)
                    {
                        _previewTimer?.Dispose();
                        _previewTimer = null;
                    }
                }, null, delayMs, Timeout.Infinite);
            }
        }

        private static void NormalizeOrder(LumiBarConfig config)
        {
            var ordered = config.Buttons.OrderBy(b => b.Order).ToList();

            for (int i = 0; i < ordered.Count; i++)
                ordered[i].Order = i;

            config.Buttons = ordered;
        }

        private static LumiBarConfig CreateDefault()
        {
            return new LumiBarConfig
            {
                Buttons =
                {
                    new()
                    {
                        Order = 0,
                        Name = "Lumi Hub",
                        Label = "Hub",
                        IconKey = "tdesign_houses_2",
                        ActionId = "open_hub",
                        FillHex = "#3FAE6A",
                        HoverHex = "#52C47A",
                        PressedHex = "#2F8E56"
                    },
                    new()
                    {
                        Order = 1,
                        Name = "Buttons verwalten",
                        Label = "Buttons",
                        IconKey = "tdesign_setting_1_filled",
                        ActionId = "open_lumibar_button_management",
                        FillHex = "#3A7BD5",
                        HoverHex = "#4C8EE6",
                        PressedHex = "#2E5FA8"
                    }
                }
            };
        }
    }
}
