using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public static class OrbRecognizer
    {
        /// <summary>
        /// 識別單個寶珠類型
        /// </summary>
        public static OrbRecognitionResult RecognizeOrb(Color color)
        {
            var profiles = OrbColorDatabase.GetColorProfiles();

            foreach (var profile in profiles)
            {
                if (profile.ColorRange.IsInRange(color))
                {
                    return new OrbRecognitionResult
                    {
                        OrbType = profile.Type,
                        Confidence = CalculateConfidence(color, profile),
                        Color = color,
                        Name = profile.Name
                    };
                }
            }

            return new OrbRecognitionResult
            {
                OrbType = OrbType.Unknown,
                Confidence = 0,
                Color = color,
                Name = "未知"
            };
        }

        /// <summary>
        /// 批量識別寶珠
        /// </summary>
        public static Dictionary<Point, OrbRecognitionResult> RecognizeOrbs(Dictionary<Point, Color> orbColors)
        {
            var results = new Dictionary<Point, OrbRecognitionResult>();

            foreach (var kvp in orbColors)
            {
                results[kvp.Key] = RecognizeOrb(kvp.Value);
            }

            return results;
        }

        /// <summary>
        /// 計算匹配置信度
        /// </summary>
        private static double CalculateConfidence(Color color, OrbColorProfile profile)
        {
            // 計算與主要顏色的距離
            double distance = Math.Sqrt(
                Math.Pow(color.R - profile.PrimaryColor.R, 2) +
                Math.Pow(color.G - profile.PrimaryColor.G, 2) +
                Math.Pow(color.B - profile.PrimaryColor.B, 2)
            );

            // 轉換為置信度 (0-1)
            double maxDistance = 200; // 最大可能距離
            double confidence = 1.0 - (distance / maxDistance);

            return Math.Max(0, Math.Min(1, confidence));
        }
    }

    public class OrbRecognitionResult
    {
        public OrbType OrbType { get; set; }
        public double Confidence { get; set; } // 0-1 的置信度
        public Color Color { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} (置信度: {Confidence:P0})";
        }
    }
}
