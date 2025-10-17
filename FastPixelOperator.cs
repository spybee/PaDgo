using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaDgo
{
    /// <summary>
    /// 使用 LockBits 高效像素操作工具類
    /// </summary>
    public static class FastPixelOperator
    {
        /// <summary>
        /// 高效獲取單個像素顏色
        /// </summary>
        public static Color? GetPixelColorFast(Bitmap bmp, int x, int y)
        {
            if (bmp == null || x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height)
                return null;

            BitmapData bmpData = null;
            try
            {
                // 鎖定位圖數據
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int stride = bmpData.Stride;

                // 計算目標像素的內存位置
                int index = y * stride + x * bytesPerPixel;

                // 讀取像素數據
                byte[] pixelData = new byte[bytesPerPixel];
                Marshal.Copy(ptr + index, pixelData, 0, bytesPerPixel);

                // 根據像素格式解析顏色
                return ParsePixelColor(pixelData, bmp.PixelFormat);
            }
            catch (Exception ex)
            {
                Debug.Print($"LockBits 獲取像素顏色異常: {ex.Message}");
                return null;
            }
            finally
            {
                // 確保解鎖
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// 批量獲取多個像素顏色（高效版本）
        /// </summary>
        public static Dictionary<Point, Color> GetMultiplePixelsFast(Bitmap bmp, List<Point> points)
        {
            var results = new Dictionary<Point, Color>();
            if (bmp == null || points == null || points.Count == 0)
                return results;

            BitmapData bmpData = null;
            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int stride = bmpData.Stride;

                foreach (var point in points)
                {
                    if (point.X >= 0 && point.X < bmp.Width && point.Y >= 0 && point.Y < bmp.Height)
                    {
                        int index = point.Y * stride + point.X * bytesPerPixel;
                        byte[] pixelData = new byte[bytesPerPixel];
                        Marshal.Copy(ptr + index, pixelData, 0, bytesPerPixel);

                        var color = ParsePixelColor(pixelData, bmp.PixelFormat);
                        if (color.HasValue)
                            results[point] = color.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"LockBits 批量獲取像素異常: {ex.Message}");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);
            }

            return results;
        }

        /// <summary>
        /// 搜索特定顏色的像素位置
        /// </summary>
        public static List<Point> FindColorPositions(Bitmap bmp, Color targetColor, int tolerance = 0)
        {
            var positions = new List<Point>();
            if (bmp == null)
                return positions;

            BitmapData bmpData = null;
            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int stride = bmpData.Stride;
                int bytes = Math.Abs(stride) * bmp.Height;
                byte[] rgbValues = new byte[bytes];

                // 一次性複製所有像素數據
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                // 遍歷所有像素
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int index = y * stride + x * bytesPerPixel;

                        if (index + bytesPerPixel <= rgbValues.Length)
                        {
                            var pixelColor = ParsePixelColor(rgbValues, index, bmp.PixelFormat);
                            if (pixelColor.HasValue && IsColorMatch(pixelColor.Value, targetColor, tolerance))
                            {
                                positions.Add(new Point(x, y));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"LockBits 搜索顏色異常: {ex.Message}");
            }
            finally
            {
                if (bmpData != null)
                    bmp.UnlockBits(bmpData);
            }

            return positions;
        }

        /// <summary>
        /// 解析像素數據為顏色
        /// </summary>
        private static Color? ParsePixelColor(byte[] pixelData, PixelFormat format)
        {
            return ParsePixelColor(pixelData, 0, format);
        }

        private static Color? ParsePixelColor(byte[] rgbValues, int startIndex, PixelFormat format)
        {
            try
            {
                switch (format)
                {
                    case PixelFormat.Format32bppArgb:
                        if (startIndex + 3 < rgbValues.Length)
                            return Color.FromArgb(
                                rgbValues[startIndex + 3], // A
                                rgbValues[startIndex + 2], // R
                                rgbValues[startIndex + 1], // G
                                rgbValues[startIndex]      // B
                            );
                        break;

                    case PixelFormat.Format24bppRgb:
                        if (startIndex + 2 < rgbValues.Length)
                            return Color.FromArgb(
                                rgbValues[startIndex + 2], // R
                                rgbValues[startIndex + 1], // G
                                rgbValues[startIndex]      // B
                            );
                        break;

                    case PixelFormat.Format32bppRgb:
                        if (startIndex + 3 < rgbValues.Length)
                            return Color.FromArgb(
                                rgbValues[startIndex + 2], // R
                                rgbValues[startIndex + 1], // G
                                rgbValues[startIndex]      // B
                            );
                        break;

                    default:
                        Debug.Print($"不支持的像素格式: {format}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"解析像素顏色異常: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 檢查顏色是否匹配（帶容差）
        /// </summary>
        private static bool IsColorMatch(Color color1, Color color2, int tolerance)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance &&
                   Math.Abs(color1.A - color2.A) <= tolerance;
        }
    }
}
