using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public static class AdvancedOrbRecognizer
    {
        /// <summary>
        /// 寶珠採樣點配置
        /// </summary>
        private static readonly List<Point> OrbSamplePoints = new List<Point>
    {
        new Point(0, 0),    // 中心
        new Point(-5, 0),   // 左
        new Point(5, 0),    // 右
        new Point(0, -5),   // 上
        new Point(0, 5),    // 下
        new Point(-3, -3),  // 左上
        new Point(3, -3),   // 右上
        new Point(-3, 3),   // 左下
        new Point(3, 3)     // 右下
    };

        /// <summary>
        /// 多點採樣獲取寶珠平均顏色
        /// </summary>
        public static Color GetOrbAverageColor(Bitmap bmp, Point centerPoint)
        {
            var validColors = new List<Color>();

            foreach (var offset in OrbSamplePoints)
            {
                var samplePoint = new Point(centerPoint.X + offset.X, centerPoint.Y + offset.Y);
                var color = FastPixelOperator.GetPixelColorFast(bmp, samplePoint.X, samplePoint.Y);
                if (color.HasValue)
                {
                    validColors.Add(color.Value);
                }
            }

            if (validColors.Count == 0)
                return Color.Black;

            // 計算平均顏色
            int r = 0, g = 0, b = 0;
            foreach (var color in validColors)
            {
                r += color.R;
                g += color.G;
                b += color.B;
            }

            return Color.FromArgb(
                r / validColors.Count,
                g / validColors.Count,
                b / validColors.Count
            );
        }

        /// <summary>
        /// 改進的寶珠識別（多點採樣）
        /// </summary>
        public static OrbRecognitionResult RecognizeOrbAdvanced(Bitmap bmp, Point centerPoint)
        {
            var averageColor = GetOrbAverageColor(bmp, centerPoint);
            return OrbRecognizer.RecognizeOrb(averageColor);
        }
    }
}
