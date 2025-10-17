using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PaDgo
{
    public static class SimpleMLOrbClassifier
    {
        /// <summary>
        /// 使用KNN算法分類寶珠
        /// </summary>
        public static OrbRecognitionResult ClassifyWithKNN(Color color, int k = 3)
        {
            var trainingData = GetTrainingFeatures();
            if (trainingData.Count == 0)
            {
                return OrbRecognizer.RecognizeOrb(color); // 退回基礎方法
            }

            // 計算與所有訓練樣本的距離
            var distances = new List<(OrbType type, double distance)>();
            var features = new double[] { color.R, color.G, color.B };

            foreach (var sample in trainingData)
            {
                double distance = CalculateEuclideanDistance(features, sample.Features);
                distances.Add((sample.OrbType, distance));
            }

            // 取最近的k個鄰居
            var nearestNeighbors = distances.OrderBy(d => d.distance).Take(k).ToList();

            // 多數投票
            var voteCount = nearestNeighbors.GroupBy(n => n.type)
                                           .Select(g => new { Type = g.Key, Count = g.Count() })
                                           .OrderByDescending(x => x.Count)
                                           .ToList();

            var predictedType = voteCount.First().Type;
            double confidence = (double)voteCount.First().Count / k;

            return new OrbRecognitionResult
            {
                OrbType = predictedType,
                Confidence = confidence,
                Color = color,
                Name = GetOrbName(predictedType)
            };
        }

        private static double CalculateEuclideanDistance(double[] features1, double[] features2)
        {
            double sum = 0;
            for (int i = 0; i < features1.Length; i++)
            {
                sum += Math.Pow(features1[i] - features2[i], 2);
            }
            return Math.Sqrt(sum);
        }

        private static List<TrainingSample> GetTrainingFeatures()
        {
            var samples = new List<TrainingSample>();
            // 這裡可以從文件或數據庫加載訓練數據
            return samples;
        }

        private class TrainingSample
        {
            public OrbType OrbType { get; set; }
            public double[] Features { get; set; } // [R, G, B]
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
    }
}
