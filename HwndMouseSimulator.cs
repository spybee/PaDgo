using System;
using System.Collections.Generic;
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
        public static void SimulateDragUsingMessages(IntPtr hWnd, int startX, int startY, int endX, int endY,
                                                   int duration = 100, int steps = 50)
        {
            try
            {
                // 檢查窗口是否有效
                if (!IsWindow(hWnd))
                {
                    return;
                }

                // 檢查窗口是否可見
                bool isVisible = IsWindowVisible(hWnd);

                // 激活窗口
                bool activated = SetForegroundWindow(hWnd);
                Thread.Sleep(100);

                // 獲取當前焦點窗口
                IntPtr focusedWindow = GetForegroundWindow();

                // 關鍵診斷：檢查窗口的消息處理能力
                // 嘗試發送一個測試消息
                IntPtr result = SendMessage(hWnd, WM_NULL, IntPtr.Zero, IntPtr.Zero);

                uint lParamDown = (uint)((startY << 16) | startX);

                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(1000);

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

                    Thread.Sleep(stepDelay);
                }

                uint lParamUp = (uint)((endY << 16) | endX);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamUp);
                Thread.Sleep(50);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"異常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 使用消息模擬沿路徑移動鼠標
        /// </summary>
        public static void SimulatePathUsingMessages(IntPtr hWnd, List<Point> pathPoints,
                                                   int duration = 400, int stepsPerSegment = 20)
        {
            try
            {
                // 檢查窗口是否有效
                if (!IsWindow(hWnd))
                {
                    Debug.WriteLine("窗口無效");
                    return;
                }

                // 激活窗口
                bool activated = SetForegroundWindow(hWnd);
                Thread.Sleep(100);

                if (pathPoints.Count < 2)
                {
                    Debug.WriteLine("路徑點不足");
                    return;
                }

                // 在起始點按下鼠標
                Point startPoint = pathPoints[0];
                uint lParamDown = (uint)((startPoint.Y << 16) | startPoint.X);
                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(200);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(200);
                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(200);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(200);
                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamDown);
                Thread.Sleep(1000);

                // 沿著路徑移動
                for (int i = 1; i < pathPoints.Count; i++)
                {
                    Point from = pathPoints[i - 1];
                    Point to = pathPoints[i];

                    // 在兩個點之間進行平滑移動
                    int segmentSteps = Math.Max(stepsPerSegment, 10);
                    int segmentDelay = Math.Max(duration / (pathPoints.Count * segmentSteps), 5);

                    for (int j = 0; j <= segmentSteps; j++)
                    {
                        double progress = (double)j / segmentSteps;
                        double easedProgress = EaseInOutQuad(progress);

                        int currentX = (int)(from.X + (to.X - from.X) * easedProgress);
                        int currentY = (int)(from.Y + (to.Y - from.Y) * easedProgress);

                        uint lParamMove = (uint)((currentY << 16) | currentX);
                        SendMessage(hWnd, WM_MOUSEMOVE, (IntPtr)MK_LBUTTON, (IntPtr)lParamMove);

                        if (j % 5 == 0) // 每5步稍作停頓
                            Thread.Sleep(segmentDelay);
                    }
                }

                // 在終點釋放鼠標
                Point endPoint = pathPoints[pathPoints.Count - 1];
                uint lParamUp = (uint)((endPoint.Y << 16) | endPoint.X);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)0, (IntPtr)lParamUp);
                Thread.Sleep(50);

                SendMessage(hWnd, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, (IntPtr)lParamUp);
                Thread.Sleep(100);
                SendMessage(hWnd, WM_LBUTTONUP, (IntPtr)MK_LBUTTON, (IntPtr)lParamUp);
                Thread.Sleep(50);

                Debug.WriteLine($"路徑執行完成，共 {pathPoints.Count} 個點");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"執行路徑時發生異常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 緩動函數
        /// </summary>
        private static double EaseInOutQuad(double t)
        {
            return t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
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
    /// 高級Hwnd滑鼠操作類
    /// </summary>
    public class AdvancedHwndMouseSimulator
    {
        /// <summary>
        /// 拖曳選項
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
        /// 在指定視窗內執行拖曳操作
        /// </summary>
        public static void DragInWindow(IntPtr hWnd, Point start, Point end, DragOptions options = null)
        {
            if (options == null)
            {
                options = new DragOptions();
            }

            if (options.UseMessages)
            {
                HwndMouseSimulator.SimulateDragUsingMessages(hWnd, start.X, start.Y, end.X, end.Y,
                                                            options.Duration, options.Steps);
            }
        }

        public static void ExecutePathInWindow(IntPtr hWnd, List<Point> pathPoints, PathOptions options = null)
        {
            if (options == null)
            {
                options = new PathOptions();
            }

            HwndMouseSimulator.SimulatePathUsingMessages(hWnd, pathPoints,
                                                       options.Duration, options.StepsPerSegment);
        }

        public class PathOptions
        {
            public int Duration { get; set; } = 500;
            public int StepsPerSegment { get; set; } = 20;
            public int StartDelay { get; set; } = 100;
            public int EndDelay { get; set; } = 50;
        }
    }
}
