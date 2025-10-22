using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public enum OrbType
    {
        Fire,       // 火
        Water,      // 水
        Wood,       // 木
        Light,      // 光
        Dark,       // 暗
        Heart,      // 心
        Jammer,     // 妨礙
        Unknown     // 未知
    }

    public class OrbColorProfile
    {
        public OrbType Type { get; set; }
        public Color PrimaryColor { get; set; }
        public ColorRange ColorRange { get; set; }
        public string Name { get; set; }
    }

    public class ColorRange
    {
        public int RMin { get; set; }
        public int RMax { get; set; }
        public int GMin { get; set; }
        public int GMax { get; set; }
        public int BMin { get; set; }
        public int BMax { get; set; }

        public bool IsInRange(Color color)
        {
            return color.R >= RMin && color.R <= RMax &&
                   color.G >= GMin && color.G <= GMax &&
                   color.B >= BMin && color.B <= BMax;
        }
    }

    public static class OrbColorDatabase
    {
        public static List<OrbColorProfile> GetColorProfiles()
        {
            return new List<OrbColorProfile>
        {
            // 火珠 - 根據你的數據調整
            new OrbColorProfile
            {
                Type = OrbType.Fire,
                Name = "火珠",
                PrimaryColor = Color.FromArgb(254, 110, 84),
                ColorRange = new ColorRange
                {
                    RMin = 240, RMax = 255,
                    GMin = 80, GMax = 120,
                    BMin = 70, BMax = 100
                }
            },
            
            // 水珠 - 根據你的數據調整
            new OrbColorProfile
            {
                Type = OrbType.Water,
                Name = "水珠",
                PrimaryColor = Color.FromArgb(105, 137, 239),
                ColorRange = new ColorRange
                {
                    RMin = 90, RMax = 120,
                    GMin = 120, GMax = 150,
                    BMin = 220, BMax = 255
                }
            },
            
            // 木珠 - 新增，根據你的綠色數據
            new OrbColorProfile
            {
                Type = OrbType.Wood,
                Name = "木珠",
                PrimaryColor = Color.FromArgb(99, 199, 149),
                ColorRange = new ColorRange
                {
                    RMin = 80, RMax = 120,
                    GMin = 180, GMax = 220,
                    BMin = 130, BMax = 170
                }
            },
            
            // 心珠 - 新增，根據你的粉色數據
            new OrbColorProfile
            {
                Type = OrbType.Heart,
                Name = "心珠",
                PrimaryColor = Color.FromArgb(223, 170, 176),
                ColorRange = new ColorRange
                {
                    RMin = 200, RMax = 240,
                    GMin = 150, GMax = 190,
                    BMin = 160, BMax = 200
                }
            },
            
            // 光珠 - 保持原設定
            new OrbColorProfile
            {
                Type = OrbType.Light,
                Name = "光珠",
                PrimaryColor = Color.FromArgb(255, 255, 150),
                ColorRange = new ColorRange
                {
                    RMin = 200, RMax = 255,
                    GMin = 200, GMax = 255,
                    BMin = 100, BMax = 200
                }
            },
            
            // 暗珠 - 保持原設定
            new OrbColorProfile
            {
                Type = OrbType.Dark,
                Name = "暗珠",
                PrimaryColor = Color.FromArgb(100, 50, 150),
                ColorRange = new ColorRange
                {
                    RMin = 70, RMax = 130,
                    GMin = 30, GMax = 80,
                    BMin = 100, BMax = 180
                }
            }
        };
        }
    }
}
