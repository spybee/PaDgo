using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace PaDgo
{
    public class ImprovedOrbDetection
    {
        public void DetectOrbsWithImprovements(string filePath)
        {
            using (var bmp = new Bitmap(filePath))
            {
                var points = new List<Point>
                {
                    // 1280 x 720 分辨率下的珠子中心點
                    //new Point(758, 222), new Point(847, 222), new Point(936, 222), new Point(1025, 222), new Point(1114, 222), new Point(1203, 222),
                    //new Point(758, 311), new Point(847, 311), new Point(936, 311), new Point(1025, 311), new Point(1114, 311), new Point(1203, 311),
                    //new Point(758, 400), new Point(847, 400), new Point(936, 400), new Point(1025, 400), new Point(1114, 400), new Point(1203, 400),
                    //new Point(758, 489), new Point(847, 489), new Point(936, 489), new Point(1025, 489), new Point(1114, 489), new Point(1203, 489),
                    //new Point(758, 578), new Point(847, 578), new Point(936, 578), new Point(1025, 578), new Point(1114, 578), new Point(1203, 578)

                    // 888 x 500 分辨率下的珠子中心點
                    new Point(525, 154), new Point(587, 154), new Point(649, 154), new Point(711, 154), new Point(773, 154), new Point(835, 154),
                    new Point(525, 216), new Point(587, 216), new Point(649, 216), new Point(711, 216), new Point(773, 216), new Point(835, 216),
                    new Point(525, 277), new Point(587, 277), new Point(649, 277), new Point(711, 277), new Point(773, 277), new Point(835, 277),
                    new Point(525, 340), new Point(587, 340), new Point(649, 340), new Point(711, 340), new Point(773, 340), new Point(835, 340),
                    new Point(525, 402), new Point(587, 402), new Point(649, 402), new Point(711, 402), new Point(773, 402), new Point(835, 402)

                    // 1333 x 750 分辨率下的珠子中心點
                    //new Point(789, 233), new Point(882, 233), new Point(975, 233), new Point(1068, 233), new Point(1161, 233), new Point(1254, 233),
                    //new Point(789, 326), new Point(882, 326), new Point(975, 326), new Point(1068, 326), new Point(1161, 326), new Point(1254, 326),
                    //new Point(789, 419), new Point(882, 419), new Point(975, 419), new Point(1068, 419), new Point(1161, 419), new Point(1254, 419),
                    //new Point(789, 512), new Point(882, 512), new Point(975, 512), new Point(1068, 512), new Point(1161, 512), new Point(1254, 512),
                    //new Point(789, 605), new Point(882, 605), new Point(975, 605), new Point(1068, 605), new Point(1161, 605), new Point(1254, 605)
                };

                Debug.Print("=== 寶珠識別 ===");

                var results = new Dictionary<Point, OrbRecognitionResult>();
                var customProfiles = GetCustomColorProfiles();

                foreach (var point in points)
                {
                    var color = AdvancedOrbRecognizer.GetOrbAverageColor(bmp, point);
                    var result = RecognizeWithCustomProfiles(color, customProfiles);

                    results[point] = result;

                    Debug.Print($"位置({point.X}, {point.Y}): {result} - RGB({color.R},{color.G},{color.B})");

                    // 自動收集訓練數據（所有識別成功的）
                    if (result.OrbType != OrbType.Unknown && result.Confidence > 0.5)
                    {
                        ColorRangeTrainer.AddTrainingSample(result.OrbType, color);
                    }
                }

                // 強制保存訓練數據
                ColorRangeTrainer.SaveTrainingData("orb_training_data.json");
                Debug.Print("訓練數據已保存！");

                // 生成盤面
                var board = GenerateBoardLayout(results, points);
                PrintBoard(board);

                // 更新共用盤面
                BoardManager.UpdateBoardFromDetection(board);

                PrintStatistics(results);
            }
        }

        // 其餘方法保持不變...
        private List<OrbColorProfile> GetCustomColorProfiles()
        {
            return new List<OrbColorProfile>
        {
            // 火珠
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
            
            // 水珠 82,116,233
            new OrbColorProfile
            {
                Type = OrbType.Water,
                Name = "水珠",
                PrimaryColor = Color.FromArgb(105, 137, 239),
                ColorRange = new ColorRange
                {
                    RMin = 78, RMax = 120,
                    GMin = 113, GMax = 150,
                    BMin = 220, BMax = 255
                }
            },
            
            // 木珠
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
            
            // 心珠
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
            
            // 光珠 247,245,54
            new OrbColorProfile
            {
                Type = OrbType.Light,
                Name = "光珠",
                PrimaryColor = Color.FromArgb(249, 246, 61),
                ColorRange = new ColorRange
                {
                    RMin = 246, RMax = 252,
                    GMin = 244, GMax = 250,
                    BMin = 52, BMax = 90
                }
            },
            
            // 暗珠 152,52,133
            new OrbColorProfile
            {
                Type = OrbType.Dark,
                Name = "暗珠",
                PrimaryColor = Color.FromArgb(120, 80, 160),
                ColorRange = new ColorRange
                {
                    RMin = 80, RMax = 185,
                    GMin = 51, GMax = 110,
                    BMin = 120, BMax = 190
                }
            },
            
            // 妨礙珠 77,115,127
            new OrbColorProfile
            {
                Type = OrbType.Jammer,
                Name = "妨礙珠",
                PrimaryColor = Color.FromArgb(81, 117, 128),
                ColorRange = new ColorRange
                {
                    RMin = 75, RMax = 87,
                    GMin = 110, GMax = 120,
                    BMin = 121, BMax = 134
                }
            },

            // 毒珠 152,150,157
            new OrbColorProfile
            {
                Type = OrbType.Poison, // 確保有一個 OrbType.Poison 枚舉值
                Name = "毒珠",
                PrimaryColor = Color.FromArgb(153,150,159),
                ColorRange = new ColorRange
                {
                    RMin = 150, RMax = 157,
                    GMin = 142, GMax = 154,
                    BMin = 155, BMax = 162
                }
            }

        };
        }

        private OrbRecognitionResult RecognizeWithCustomProfiles(Color color, List<OrbColorProfile> profiles)
        {
            foreach (var profile in profiles)
            {
                if (profile.ColorRange.IsInRange(color))
                {
                    double confidence = CalculateConfidence(color, profile.PrimaryColor);
                    return new OrbRecognitionResult
                    {
                        OrbType = profile.Type,
                        Confidence = confidence,
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

        private double CalculateConfidence(Color actual, Color expected)
        {
            double distance = Math.Sqrt(
                Math.Pow(actual.R - expected.R, 2) +
                Math.Pow(actual.G - expected.G, 2) +
                Math.Pow(actual.B - expected.B, 2)
            );
            return Math.Max(0, 1 - distance / 150); // 調整分母提高置信度
        }

        // 其他輔助方法保持不變...
        private OrbType[,] GenerateBoardLayout(Dictionary<Point, OrbRecognitionResult> results, List<Point> points)
        {
            var board = new OrbType[6, 5];
            var sortedPoints = points.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();

            int index = 0;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    if (index < sortedPoints.Count)
                    {
                        var point = sortedPoints[index];
                        if (results.ContainsKey(point))
                        {
                            board[col, row] = results[point].OrbType;
                        }
                        index++;
                    }
                }
            }
            return board;
        }

        private void PrintBoard(OrbType[,] board)
        {
            var typeToChar = new Dictionary<OrbType, char>
        {
            { OrbType.Fire, '火' }, { OrbType.Water, '水' }, { OrbType.Wood, '木' },
            { OrbType.Light, '光' }, { OrbType.Dark, '暗' }, { OrbType.Heart, '心' },
            { OrbType.Jammer, '妨' }, { OrbType.Poison, '毒' }, { OrbType.Unknown, '?' }
        };

            for (int row = 0; row < 5; row++)
            {
                var rowText = "";
                for (int col = 0; col < 6; col++)
                {
                    rowText += typeToChar[board[col, row]] + " ";
                }
                Debug.Print(rowText);
            }
        }

        private void PrintStatistics(Dictionary<Point, OrbRecognitionResult> results)
        {
            var stats = results.Values.GroupBy(r => r.OrbType)
                                     .Select(g => new { Type = g.Key, Count = g.Count(), AvgConfidence = g.Average(r => r.Confidence) })
                                     .OrderByDescending(x => x.Count);

            Debug.Print("\n=== 統計信息 ===");
            foreach (var stat in stats)
            {
                Debug.Print($"{stat.Type}: {stat.Count}個 (平均置信度: {stat.AvgConfidence:P0})");
            }
        }

        // 在 ImprovedOrbDetection.cs 中添加
        public static List<Point> GetOrbPoints()
        {
            // 返回 888 x 500 分辨率下的珠子中心點
            return new List<Point>
                {
                    new Point(525, 154), new Point(587, 154), new Point(649, 154), new Point(711, 154), new Point(773, 154), new Point(835, 154),
                    new Point(525, 216), new Point(587, 216), new Point(649, 216), new Point(711, 216), new Point(773, 216), new Point(835, 216),
                    new Point(525, 277), new Point(587, 277), new Point(649, 277), new Point(711, 277), new Point(773, 277), new Point(835, 277),
                    new Point(525, 340), new Point(587, 340), new Point(649, 340), new Point(711, 340), new Point(773, 340), new Point(835, 340),
                    new Point(525, 402), new Point(587, 402), new Point(649, 402), new Point(711, 402), new Point(773, 402), new Point(835, 402)
                };
        }

        public static Point GetOrbPoint(int row, int col)
        {
            var points = GetOrbPoints();
            int index = row * 6 + col; // 6列，行優先排序
            if (index >= 0 && index < points.Count)
            {
                return points[index];
            }
            throw new ArgumentException($"無效的行列座標: row={row}, col={col}");
        }
    }
}