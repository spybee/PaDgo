using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PaDgo
{
    /// <summary>
    /// 螢幕截圖工具類，支援多種截圖方法和進階功能
    /// </summary>
    public static class ScreenCapture
    {
        // Windows API 聲明
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // 常數定義
        private const uint PW_RENDERFULLCONTENT = 0x00000002;
        private const uint WM_PRINT = 0x0317;
        private const uint PRF_CLIENT = 0x00000004;
        private const uint PRF_CHILDREN = 0x00000010;
        private const uint PRF_ERASEBKGND = 0x00000008;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_ERASE = 0x0004;
        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const int GWL_STYLE = -16;
        private const uint WS_VISIBLE = 0x10000000;
        private const int SW_MINIMIZE = 6;

        // 視窗置頂相關常數
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// 尋找顯示視窗
        /// </summary>
        public static IntPtr FindDisplayWindow(IntPtr parentHwnd)
        {
            const string methodName = "FindR4DisplayWindow";

            try
            {
                Debug.Print($"開始查找顯示視窗，父窗口: 0x{parentHwnd.ToInt64():X}");

                IntPtr hwnd = WindowHelper.FindWindowEx(parentHwnd, IntPtr.Zero, "subWin", null);
                if (hwnd != IntPtr.Zero)
                {
                    string title = WindowHelper.GetTitle(hwnd);
                    Debug.Print($"找到顯示視窗 (Win10): 0x{hwnd.ToInt64():X}, 標題: '{title}'");
                    return hwnd;
                }

                Debug.Print("未找到顯示視窗，已嘗試兩種窗口類型");
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.Print($"{methodName} 異常: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 使用 CopyFromScreen 方法捕獲窗口
        /// </summary>
        public static bool CaptureWithCopyFromScreen(IntPtr hWnd, string filePath)
        {
            try
            {
                // 獲取窗口矩形
                if (!GetWindowRect(hWnd, out RECT windowRect))
                {
                    Debug.Print("GetWindowRect 失敗");
                    return false;
                }

                int width = windowRect.Right - windowRect.Left;
                int height = windowRect.Bottom - windowRect.Top;

                if (width <= 0 || height <= 0)
                {
                    Debug.Print($"窗口尺寸無效: {width}x{height}");
                    return false;
                }

                Debug.Print($"CopyFromScreen捕獲: {width}x{height}, 位置: ({windowRect.Left},{windowRect.Top})");

                // 創建位圖
                using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    // 使用Graphics從屏幕複製
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0,
                            new Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    // 保存為PNG
                    bmp.Save(filePath, ImageFormat.Png);
                }

                Debug.Print("CopyFromScreen捕獲完成");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print($"CopyFromScreen捕獲異常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 臨時置頂視窗並使用 CopyFromScreen 截圖
        /// </summary>
        public static bool CaptureWithTopMostAndCopy(IntPtr MainHWnd, IntPtr DisplayHWnd, string filePath)
        {
            IntPtr originalForeground = GetForegroundWindow();
            bool wasTopMost = IsWindowTopMost(MainHWnd);
            bool wasMinimized = IsIconic(MainHWnd);

            try
            {
                Debug.Print("開始使用置頂+CopyFromScreen方法截圖");

                // 保存原始狀態
                Debug.Print($"原始狀態 - 最上層: {wasTopMost}, 最小化: {wasMinimized}");

                // 如果視窗最小化，則還原
                if (wasMinimized)
                {
                    ShowWindow(MainHWnd, SW_RESTORE);
                    Thread.Sleep(100);
                }

                // 臨時設置為最上層
                if (!wasTopMost)
                {
                    SetWindowPos(MainHWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS | SWP_NOACTIVATE);
                    Thread.Sleep(50);
                }

                // 激活視窗但不搶奪焦點
                WindowHelper.SetForegroundWindow(MainHWnd);
                Thread.Sleep(100);

                // 使用 CopyFromScreen 截圖
                bool success = CaptureWithCopyFromScreen(DisplayHWnd, filePath);

                if (success)
                {
                    Debug.Print("置頂+CopyFromScreen截圖成功");
                }
                else
                {
                    Debug.Print("置頂+CopyFromScreen截圖失敗");
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.Print($"置頂+CopyFromScreen截圖異常: {ex.Message}");
                return false;
            }
            finally
            {
                // 恢復原始狀態
                try
                {
                    // 恢復最上層狀態
                    if (!wasTopMost)
                    {
                        SetWindowPos(MainHWnd, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS | SWP_NOACTIVATE);
                    }

                    // 如果原本是最小化的，恢復最小化狀態
                    if (wasMinimized)
                    {
                        ShowWindow(MainHWnd, SW_MINIMIZE);
                    }

                    // 恢復原始活動視窗
                    if (originalForeground != IntPtr.Zero && originalForeground != MainHWnd)
                    {
                        WindowHelper.SetForegroundWindow(originalForeground);
                    }

                    Debug.Print("已恢復視窗原始狀態");
                }
                catch (Exception ex)
                {
                    Debug.Print($"恢復視窗狀態時發生異常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 檢查視窗是否為最上層
        /// </summary>
        private static bool IsWindowTopMost(IntPtr hWnd)
        {
            int exStyle = WindowHelper.GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & 0x00000008) != 0; // WS_EX_TOPMOST
        }

        /// <summary>
        /// 診斷視窗信息
        /// </summary>
        public static void DebugWindowInfo(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out RECT rect);
            string title = WindowHelper.GetTitle(hWnd);

            Debug.Print($"=== 視窗診斷資訊 ===");
            Debug.Print($"標題: '{title}'");
            Debug.Print($"視窗大小: {rect.Right - rect.Left}x{rect.Bottom - rect.Top}");
            Debug.Print($"位置: ({rect.Left}, {rect.Top})");

            // 檢查視窗樣式
            int style = WindowHelper.GetWindowLong(hWnd, -16); // GWL_STYLE
            int exStyle = WindowHelper.GetWindowLong(hWnd, GWL_EXSTYLE);

            Debug.Print($"視窗樣式: 0x{style:X8}");
            Debug.Print($"擴展樣式: 0x{exStyle:X8}");

            // 檢查是否為分層視窗
            bool isLayered = (exStyle & WS_EX_LAYERED) != 0;
            Debug.Print($"是否分層視窗: {isLayered}");

            // 檢查是否為最上層
            bool isTopMost = IsWindowTopMost(hWnd);
            Debug.Print($"是否最上層: {isTopMost}");

            // 檢查是否最小化
            bool isMinimized = IsIconic(hWnd);
            Debug.Print($"是否最小化: {isMinimized}");
        }

        // 在 ScreenCapture 類中添加以下方法

        /// <summary>
        /// 高效截圖並檢查顏色
        /// </summary>
        public static (bool success, Color? pixelColor) CaptureAndCheckColorFast(IntPtr hWnd, string filePath, int checkX = 10, int checkY = 10)
        {
            try
            {
                if (!GetWindowRect(hWnd, out RECT windowRect))
                {
                    Debug.Print("GetWindowRect 失敗");
                    return (false, null);
                }

                int width = windowRect.Right - windowRect.Left;
                int height = windowRect.Bottom - windowRect.Top;

                if (width <= 0 || height <= 0)
                {
                    Debug.Print($"窗口尺寸無效: {width}x{height}");
                    return (false, null);
                }

                using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0,
                            new Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    // 使用 LockBits 高效獲取顏色
                    Color? pixelColor = FastPixelOperator.GetPixelColorFast(bmp, checkX, checkY);

                    if (pixelColor.HasValue)
                    {
                        Debug.Print($"位置 ({checkX},{checkY}) 顏色: R={pixelColor.Value.R}, G={pixelColor.Value.G}, B={pixelColor.Value.B}");
                    }

                    bmp.Save(filePath, ImageFormat.Png);
                    return (true, pixelColor);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"高效截圖檢查異常: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// 截圖並搜索特定顏色
        /// </summary>
        public static (bool success, List<Point> positions) CaptureAndFindColor(IntPtr hWnd, string filePath, Color targetColor, int tolerance = 10)
        {
            try
            {
                if (!GetWindowRect(hWnd, out RECT windowRect))
                    return (false, null);

                int width = windowRect.Right - windowRect.Left;
                int height = windowRect.Bottom - windowRect.Top;

                if (width <= 0 || height <= 0)
                    return (false, null);

                using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0,
                            new Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    // 使用 LockBits 搜索顏色
                    var positions = FastPixelOperator.FindColorPositions(bmp, targetColor, tolerance);
                    Debug.Print($"找到 {positions.Count} 個匹配的像素位置");

                    bmp.Save(filePath, ImageFormat.Png);
                    return (true, positions);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"搜索顏色截圖異常: {ex.Message}");
                return (false, null);
            }
        }
    }


}

//[DllImport("user32.dll", SetLastError = true)]
//private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

//// 旗標常數（可組合使用）
//private const uint SWP_NOMOVE = 0x0002;  // 不改變位置
//private const uint SWP_NOZORDER = 0x0004;  // 不改變 Z-order
//private const uint SWP_SHOWWINDOW = 0x0040;  // 顯示視窗


//// 設定新大小：寬度 1298, 高度 759
//// 如果想保留原位置，只改變大小，使用 SWP_NOMOVE | SWP_NOZORDER
//bool result = SetWindowPos(MainHWnd, IntPtr.Zero, 0, 0, 1298, 759, SWP_NOMOVE | SWP_NOZORDER);
//if (!result)
//{
//    Debug.Print("SetWindowPos 失敗，錯誤碼: " + Marshal.GetLastWin32Error());
//}
//else
//{
//    Debug.Print("SetWindowPos 成功，視窗大小已更改為 1298x759");

//    DebugWindowInfo(MainHWnd);

//    // 使用 PrintWindow 捕獲
//    return CaptureWithCopyFromScreen(DisplayHWnd, filePath);
//}
//return false;