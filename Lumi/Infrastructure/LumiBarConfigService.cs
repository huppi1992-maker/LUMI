using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Lumi.Models;

namespace Lumi.Infrastructure
{
    public sealed class LumiBarConfigService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

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

            var json = File.ReadAllText(ConfigPath);
            var loaded = JsonSerializer.Deserialize<LumiBarConfig>(json, JsonOptions);

            // Guard gegen kaputtes/leer JSON
            return loaded ?? CreateDefault();
        }

        public void Save(LumiBarConfig config)
        {
            NormalizeOrder(config);

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }

        private static void NormalizeOrder(LumiBarConfig config)
        {
            // Order sauber durchnummerieren, verhindert Chaos nach Move/Remove
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
