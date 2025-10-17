using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public class PADOrbDetector
    {
        private ImprovedOrbDetection _detector;

        public PADOrbDetector()
        {
            _detector = new ImprovedOrbDetection();
        }

        /// <summary>
        /// 截圖並識別寶珠
        /// </summary>
        public void CaptureAndDetectOrbs(IntPtr gameWindowHandle)
        {
            string screenshotPath = Path.Combine(Path.GetTempPath(), $"pad_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            try
            {
                // 使用你的 ScreenCapture 方法截圖
                bool success = ScreenCapture.CaptureWithTopMostAndCopy(gameWindowHandle, gameWindowHandle, screenshotPath);

                if (success && File.Exists(screenshotPath))
                {
                    Debug.Print("截圖成功，開始識別寶珠...");

                    // 使用改進的寶珠識別
                    _detector.DetectOrbsWithImprovements(screenshotPath);

                    // 可選：刪除臨時文件
                    File.Delete(screenshotPath);
                }
                else
                {
                    Debug.Print("截圖失敗");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"寶珠識別失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 實時監控模式
        /// </summary>
        public void StartRealTimeDetection(IntPtr gameWindowHandle, int intervalMs = 1000)
        {
            var timer = new System.Timers.Timer(intervalMs);
            timer.Elapsed += (sender, e) =>
            {
                CaptureAndDetectOrbs(gameWindowHandle);
            };
            timer.Start();

            Debug.Print($"開始實時寶珠監控，間隔: {intervalMs}ms");
        }
    }
}
