using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.IO;  // 用於 File 類
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;  // 用於 JsonConvert 類

namespace PaDgo
{
    public static class ColorRangeTrainer
    {
        private static Dictionary<OrbType, List<Color>> trainingSamples = new Dictionary<OrbType, List<Color>>();

        /// <summary>
        /// 添加訓練樣本
        /// </summary>
        public static void AddTrainingSample(OrbType orbType, Color color)
        {
            if (!trainingSamples.ContainsKey(orbType))
            {
                trainingSamples[orbType] = new List<Color>();
            }
            trainingSamples[orbType].Add(color);

            Debug.Print($"添加訓練樣本: {orbType} - R:{color.R} G:{color.G} B:{color.B}");
        }

        /// <summary>
        /// 根據訓練數據生成顏色範圍
        /// </summary>
        public static List<OrbColorProfile> GenerateColorProfilesFromTraining()
        {
            var profiles = new List<OrbColorProfile>();

            foreach (var kvp in trainingSamples)
            {
                if (kvp.Value.Count > 0)
                {
                    var profile = CalculateColorProfile(kvp.Key, kvp.Value);
                    profiles.Add(profile);
                    Debug.Print($"生成顏色範圍: {profile.Name} - R({profile.ColorRange.RMin}-{profile.ColorRange.RMax}) G({profile.ColorRange.GMin}-{profile.ColorRange.GMax}) B({profile.ColorRange.BMin}-{profile.ColorRange.BMax})");
                }
            }

            return profiles;
        }

        private static OrbColorProfile CalculateColorProfile(OrbType orbType, List<Color> samples)
        {
            int rMin = 255, rMax = 0, gMin = 255, gMax = 0, bMin = 255, bMax = 0;
            int rSum = 0, gSum = 0, bSum = 0;

            foreach (var color in samples)
            {
                rMin = Math.Min(rMin, color.R);
                rMax = Math.Max(rMax, color.R);
                gMin = Math.Min(gMin, color.G);
                gMax = Math.Max(gMax, color.G);
                bMin = Math.Min(bMin, color.B);
                bMax = Math.Max(bMax, color.B);

                rSum += color.R;
                gSum += color.G;
                bSum += color.B;
            }

            // 擴大範圍以包含更多變體，但添加緩衝區
            int buffer = 15; // 緩衝區大小
            return new OrbColorProfile
            {
                Type = orbType,
                Name = GetOrbName(orbType),
                PrimaryColor = Color.FromArgb(rSum / samples.Count, gSum / samples.Count, bSum / samples.Count),
                ColorRange = new ColorRange
                {
                    RMin = Math.Max(0, rMin - buffer),
                    RMax = Math.Min(255, rMax + buffer),
                    GMin = Math.Max(0, gMin - buffer),
                    GMax = Math.Min(255, gMax + buffer),
                    BMin = Math.Max(0, bMin - buffer),
                    BMax = Math.Min(255, bMax + buffer)
                }
            };
        }

        private static string GetOrbName(OrbType orbType)
        {
            switch (orbType)
            {
                case OrbType.Fire:
                    return "火珠";
                case OrbType.Water:
                    return "水珠";
                case OrbType.Wood:
                    return "木珠";
                case OrbType.Light:
                    return "光珠";
                case OrbType.Dark:
                    return "暗珠";
                case OrbType.Heart:
                    return "心珠";
                case OrbType.Jammer:
                    return "妨礙珠";
                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 保存訓練數據到文件
        /// </summary>
        public static void SaveTrainingData(string filePath)
        {
            var data = new TrainingData { Samples = trainingSamples };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Print($"訓練數據已保存到: {filePath}");
        }

        /// <summary>
        /// 從文件加載訓練數據
        /// </summary>
        public static void LoadTrainingData(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<TrainingData>(json);
                trainingSamples = data.Samples ?? new Dictionary<OrbType, List<Color>>();
                Debug.Print($"訓練數據已加載: {trainingSamples.Sum(x => x.Value.Count)} 個樣本");
            }
        }

        [Serializable]
        private class TrainingData
        {
            public Dictionary<OrbType, List<Color>> Samples { get; set; }
        }
    }
}
