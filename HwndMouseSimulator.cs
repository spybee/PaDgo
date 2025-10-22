using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace PaDgo
{
    /// <summary>
    /// 基于窗口句柄的鼠标操作模拟类
    /// </summary>
    public class HwndMouseSimulator
    {
        // Windows API常量
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint MK_LBUTTON = 0x0001;
        public static void SimulateDragUsingMessagesDebug(IntPtr hWnd, int startX, int startY, int endX, int endY,
                                                   int duration = 500, int steps = 50)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("無效的窗口句柄");

            try
            {
                Debug.WriteLine("=== 開始拖拽診斷 ===");

                // 檢查窗口是否有效
                if (!IsWindow(hWnd))
                {
                    Debug.WriteLine("錯誤：窗口句柄無效");
                    return;
                }

                // 檢查窗口是否可見
                bool isVisible = IsWindowVisible(hWnd);
                Debug.WriteLine($"窗口可見: {isVisible}");

                // 激活窗口
                bool activated = SetForegroundWindow(hWnd);
                Debug.WriteLine($"窗口激活結果: {activated}");
                Thread.Sleep(100);

                // 獲取當前焦點窗口
                IntPtr focusedWindow = GetForegroundWindow();
                Debug.WriteLine($"當前焦點窗口匹配: {focusedWindow == hWnd}");

                // ★ 關鍵診斷：檢查窗口的消息處理能力
                // 嘗試發送一個測試消息
                IntPtr result = SendMessage(hWnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                Debug.WriteLine($"窗口響應測試消息: {result}");

                // 清除可能的殘留狀態
                uint lParamClear = (uint)((startY << 16) | startX);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamClear);
                Thread.Sleep(50);
                SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)0, (IntPtr)lParamClear);
                Thread.Sleep(100);

                Debug.WriteLine($"發送 LBUTTONDOWN 到 ({startX}, {startY})");
                uint lParamDown = (uint)((startY << 16) | startX);
                IntPtr downResult = SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Debug.WriteLine($"LBUTTONDOWN 返回值: {downResult}");
                Thread.Sleep(50);

                // 發送移動消息
                int stepDelay = Math.Max(duration / steps, 10);
                for (int i = 0; i <= steps; i++)
                {
                    double progress = (double)i / steps;
                    double easedProgress = EaseInOutQuad(progress);

                    int currentX = (int)(startX + (endX - startX) * easedProgress);
                    int currentY = (int)(startY + (endY - startY) * easedProgress);

                    uint lParamMove = (uint)((currentY << 16) | currentX);
                    SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)MK_LBUTTON, (IntPtr)lParamMove);

                    if (i % 10 == 0)
                        Debug.WriteLine($"移動到 ({currentX}, {currentY})");

                    Thread.Sleep(stepDelay);
                }

                Debug.WriteLine($"發送 LBUTTONUP 到 ({endX}, {endY})");
                uint lParamUp = (uint)((endY << 16) | endX);
                IntPtr upResult = SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamUp);
                Debug.WriteLine($"LBUTTONUP 返回值: {upResult}");
                Thread.Sleep(50);

                SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)0, (IntPtr)lParamUp);
                Thread.Sleep(100);

                Debug.WriteLine("=== 拖拽完成 ===\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"異常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 缓动函数
        /// </summary>
        private static double EaseInOutQuad(double t)
        {
            return t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
        }

        public static void SimulateDragWithDelay(IntPtr hWnd, int startX, int startY, int endX, int endY,
                                        int duration = 500, int steps = 50,
                                        int resetDelay = 200) // ★ 新增重置延遲參數
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException("無效的窗口句柄");

            try
            {
                SetForegroundWindow(hWnd);
                Thread.Sleep(100);

                // 簡單重置
                uint lParamStart = (uint)((startY << 16) | startX);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamStart);
                SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)0, (IntPtr)lParamStart);

                // ★ 關鍵：給應用程序足夠的時間重置內部狀態
                Thread.Sleep(resetDelay);

                // 正常拖拽
                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamStart);
                Thread.Sleep(50);

                int stepDelay = duration / steps;
                for (int i = 0; i <= steps; i++)
                {
                    double progress = (double)i / steps;
                    double easedProgress = EaseInOutQuad(progress);

                    int currentX = (int)(startX + (endX - startX) * easedProgress);
                    int currentY = (int)(startY + (endY - startY) * easedProgress);

                    uint lParamMove = (uint)((currentY << 16) | currentX);
                    SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)MK_LBUTTON, (IntPtr)lParamMove);
                    Thread.Sleep(stepDelay);
                }

                uint lParamEnd = (uint)((endY << 16) | endX);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamEnd);
                SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)0, (IntPtr)lParamEnd);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"拖拽失敗: {ex.Message}");
                throw;
            }
        }

        // 添加必要的 API 聲明
        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const uint WM_NULL = 0x0000;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>
    /// 高级Hwnd鼠标操作类
    /// </summary>
    public class AdvancedHwndMouseSimulator
    {
        /// <summary>
        /// 拖拽选项
        /// </summary>
        public class DragOptions
        {
            public int Duration { get; set; } = 500;
            public int Steps { get; set; } = 50;
            public int StartDelay { get; set; } = 100;
            public int EndDelay { get; set; } = 50;
            public bool UseMessages { get; set; } = false;
            public bool UseClientCoordinates { get; set; } = false;
        }

        /// <summary>
        /// 在指定窗口内执行拖拽操作
        /// </summary>
        public static void DragInWindow(IntPtr hWnd, Point start, Point end, DragOptions options = null)
        {
            if (options == null)
            {
                options = new DragOptions();
            }

            if (options.UseMessages)
            {
                HwndMouseSimulator.SimulateDragWithDelay(hWnd, start.X, start.Y, end.X, end.Y,
                                                            options.Duration, options.Steps, 1000);
            }
        }
    }
}
