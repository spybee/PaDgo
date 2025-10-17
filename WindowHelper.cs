using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PaDgo
{
    public static class WindowHelper
    {
        // ------------------ WinAPI 匯入 ------------------
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // ------------------ 常數 ------------------
        // 視窗顯示狀態常量
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MAXIMIZE = 0xF030;

        // ------------------ 方法 ------------------
        /// <summary>確認視窗是否有效存在</summary>
        public static bool Exists(IntPtr hWnd) => hWnd != IntPtr.Zero && IsWindow(hWnd);

        /// <summary>更改視窗標題名稱</summary>
        
        public static bool SetTitle(IntPtr hWnd, string title) =>
            hWnd != IntPtr.Zero && SetWindowText(hWnd, title);

        /// <summary>將視窗帶到前景</summary>
        public static bool BringToFront(IntPtr hWnd) => hWnd != IntPtr.Zero && SetForegroundWindow(hWnd);

        /// <summary>最大化視窗 (多重 fallback)</summary>
        public static bool Maximize(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            // Step 1: 標準最大化
            ShowWindow(hWnd, SW_MAXIMIZE);
            Thread.Sleep(200);
            if (IsZoomed(hWnd)) return true;

            // Step 2: 系統命令最大化
            PostMessage(hWnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
            Thread.Sleep(200);
            if (IsZoomed(hWnd)) return true;

            // Step 3: 持續重試
            for (int i = 0; i < 5; i++)
            {
                ShowWindow(hWnd, SW_MAXIMIZE);
                Thread.Sleep(500);
                if (IsZoomed(hWnd)) return true;
            }

            return false;
        }

        /// <summary>最小化視窗</summary>
        public static bool Minimize(IntPtr hWnd) =>
            hWnd != IntPtr.Zero && ShowWindow(hWnd, SW_MINIMIZE);

        /// <summary>還原視窗</summary>
        public static bool Restore(IntPtr hWnd) =>
            hWnd != IntPtr.Zero && ShowWindow(hWnd, SW_RESTORE);

        /// <summary>檢查視窗是否允許最大化</summary>
        public static bool CanMaximize(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            int style = GetWindowLong(hWnd, GWL_STYLE);
            return (style & WS_MAXIMIZEBOX) != 0;
        }

        /// <summary>取得視窗標題</summary>
        public static string GetTitle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return string.Empty;
            StringBuilder sb = new StringBuilder(256);
            _ = GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>向視窗發送鍵盤按鍵</summary>
        public static bool SendKey(IntPtr hWnd, int virtualKey, bool bringToFront = false)
        {
            if (hWnd == IntPtr.Zero) return false;

            if (bringToFront) BringToFront(hWnd);

            bool down = PostMessage(hWnd, WM_KEYDOWN, virtualKey, 0);
            bool up = PostMessage(hWnd, WM_KEYUP, virtualKey, 0);

            return down && up;
        }

        public static IntPtr FindWindowByProcessId(int pid, string titlePrefix)
        {
            IntPtr foundHwnd = IntPtr.Zero;

            EnumWindows((hWnd, param) =>
            {
                _ = GetWindowThreadProcessId(hWnd, out uint windowPid);

                if ((int)windowPid == pid)
                {
                    string title = WindowHelper.GetTitle(hWnd);

                    if (title.StartsWith(titlePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        foundHwnd = hWnd;
                        return false; // 停止枚舉
                    }
                }
                return true;
            }, IntPtr.Zero);

            return foundHwnd;
        }

        /// <summary>
        /// 將指定視窗最大化（包含多重 fallback 機制）
        /// </summary>
        /// <param name="hWnd">視窗控制代碼</param>
        /// <returns>成功返回 true，否則返回 false</returns>
        public static bool MaximizeWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            // Step 1: 嘗試一般 ShowWindow
            ShowWindow(hWnd, SW_MAXIMIZE);
            Thread.Sleep(200);
            if (IsZoomed(hWnd)) return true;

            // Step 2: 如果失敗，改送系統命令
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MAXIMIZE = 0xF030;
            PostMessage(hWnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);

            Thread.Sleep(200);
            if (IsZoomed(hWnd)) return true;

            // Step 3: 持續嘗試數次，確保視窗真的進入最大化
            for (int i = 0; i < 5; i++)
            {
                ShowWindow(hWnd, SW_MAXIMIZE);
                Thread.Sleep(500);
                if (IsZoomed(hWnd)) return true;
            }

            return false;
        }

        /// <summary>
        /// 檢查視窗是否處於最大化狀態
        /// </summary>
        /// <param name="hWnd">視窗控制代碼</param>
        /// <returns>是最大化返回 true，否則返回 false</returns>
        public static bool IsWindowMaximized(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            return IsZoomed(hWnd);
        }

        /// <summary>
        /// 還原指定視窗（從最大化或最小化狀態恢復）
        /// </summary>
        /// <param name="hWnd">視窗控制代碼</param>
        /// <returns>成功返回 true，否則返回 false</returns>
        public static bool RestoreWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            return ShowWindow(hWnd, SW_RESTORE);
        }

        /// <summary>
        /// 切換視窗的最大化狀態（如果已最大化則還原，否則最大化）
        /// </summary>
        /// <param name="hWnd">視窗控制代碼</param>
        /// <returns>成功返回 true，否則返回 false</returns>
        public static bool ToggleMaximizeWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            if (IsWindowMaximized(hWnd))
                return RestoreWindow(hWnd);
            else
                return MaximizeWindow(hWnd);
        }

        /// <summary>
        /// 確保視窗處於最大化狀態
        /// 如果已經最大化則不動，如果不是就最大化
        /// </summary>
        /// <param name="hWnd">視窗控制代碼</param>
        /// <returns>若視窗最終是最大化狀態，返回 true；否則 false</returns>
        public static bool EnsureMaximizedWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            // 如果已經最大化，就直接回傳 true
            if (IsWindowMaximized(hWnd))
                return true;

            // 如果沒有最大化，就嘗試最大化
            return MaximizeWindow(hWnd);
        }

        // 查找所有包含指定標題的窗口
        public static List<IntPtr> FindWindowsWithTitle(string titleFragment)
        {
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);

                if (sb.ToString().Contains(titleFragment))
                {
                    windows.Add(hWnd);
                }

                return true;
            }, IntPtr.Zero);

            return windows;
        }
    }
}
