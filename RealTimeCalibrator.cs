using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public static class RealTimeCalibrator
    {
        private static Dictionary<OrbType, Queue<Color>> recentSamples = new Dictionary<OrbType, Queue<Color>>();
        private const int MaxSampleSize = 50;

        /// <summary>
        /// 添加實時樣本用於校準
        /// </summary>
        public static void AddRealTimeSample(OrbType orbType, Color color)
        {
            if (!recentSamples.ContainsKey(orbType))
            {
                recentSamples[orbType] = new Queue<Color>();
            }

            var queue = recentSamples[orbType];
            queue.Enqueue(color);

            // 保持隊列大小
            while (queue.Count > MaxSampleSize)
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// 獲取動態調整的顏色範圍
        /// </summary>
        public static List<OrbColorProfile> GetDynamicColorProfiles()
        {
            var profiles = OrbColorDatabase.GetColorProfiles();

            foreach (var profile in profiles)
            {
                if (recentSamples.ContainsKey(profile.Type) && recentSamples[profile.Type].Count > 10)
                {
                    // 根據最近樣本調整顏色範圍
                    var samples = recentSamples[profile.Type].ToList();
                    profile.ColorRange = CalculateDynamicRange(samples, profile.ColorRange);
                }
            }

            return profiles;
        }

        private static ColorRange CalculateDynamicRange(List<Color> samples, ColorRange baseRange)
        {
            int rMin = samples.Min(c => c.R);
            int rMax = samples.Max(c => c.R);
            int gMin = samples.Min(c => c.G);
            int gMax = samples.Max(c => c.G);
            int bMin = samples.Min(c => c.B);
            int bMax = samples.Max(c => c.B);

            // 與基礎範圍結合，避免過度調整
            return new ColorRange
            {
                RMin = Math.Max(baseRange.RMin, rMin - 5),
                RMax = Math.Min(baseRange.RMax, rMax + 5),
                GMin = Math.Max(baseRange.GMin, gMin - 5),
                GMax = Math.Min(baseRange.GMax, gMax + 5),
                BMin = Math.Max(baseRange.BMin, bMin - 5),
                BMax = Math.Min(baseRange.BMax, bMax + 5)
            };
        }
    }
}
