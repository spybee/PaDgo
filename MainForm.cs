using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaDgo
{
    public partial class MainForm : Form
    {
        private const int ORB_SIZE = 60;
        private const int GRID_LEFT = 20;
        private const int GRID_TOP = 20;

        // 移除 private int[,] board = new int[5, 6]; 改用共用的 BoardManager.Board
        private PndOptimizer optimizer = new PndOptimizer();
        private List<Solution> solutions = new List<Solution>();
        private List<Point> currentPath = new List<Point>();

        IntPtr PD0WindowsHwnd;

        public MainForm()
        {
            InitializeComponent();
            //SetupUI();
            InitializeProfiles();
            BoardManager.InitializeBoard(); // 改用共用盤面初始化
            ClearButton_Click(null, null);
        }

        private void InitializeProfiles()
        {
            var profiles = new List<string>
            {
             "5色隊伍 (多目標)",
             "5色隊伍 (單目標)",
             "回覆隊伍",
             "回覆隊伍 (多目標)",
             "火隊 (多目標)",
             "火隊 (單目標)",
             "水隊 (多目標)",
             "水隊 (單一目標)",
             "木隊 (多目標)",
             "木隊 (單目標)",
             "光隊 (多目標)",
             "光隊 (單一目標)",
             "暗隊 (多目標)",
             "暗隊 (單目標)"
            };

            foreach (var profile in profiles)
            {
                profileCombo.Items.Add(profile);
            }
            profileCombo.SelectedIndex = 0;
        }

        private Brush GetOrbBrush(int orbType)
        {
            switch (orbType)
            {
                case 0: return Brushes.Red;         // 火色
                case 1: return Brushes.LightBlue;   // 水色
                case 2: return Brushes.Green;       // 木質
                case 3: return Brushes.Yellow;      // 淺色
                case 4: return Brushes.Purple;      // 深色
                case 5: return Brushes.Pink;        // 心形
                case 6: return Brushes.Gray;        // 垃圾色
                case 7: return Brushes.Purple;      // 毒珠
                default: return Brushes.White;      // 白色
            }
        }

        private void DrawOrb(Graphics g, int row, int col, int orbType)
        {
            var rect = new Rectangle(col * ORB_SIZE, row * ORB_SIZE, ORB_SIZE, ORB_SIZE);

            // 繪製寶珠背景
            var brush = GetOrbBrush(orbType);
            g.FillEllipse(brush, rect);

            // 繪製邊框
            g.DrawEllipse(Pens.Black, rect);

            // 繪製符號
            var symbol = GetOrbSymbol(orbType);
            var font = new Font("Broadway", 18, FontStyle.Bold);
            var textSize = g.MeasureString(symbol, font);
            var textPos = new PointF(
                rect.X + (ORB_SIZE - textSize.Width) / 2,
                rect.Y + (ORB_SIZE - textSize.Height) / 2
            );

            g.DrawString(symbol, font, Brushes.Black, textPos);
        }

        private string GetOrbSymbol(int orbType)
        {
            switch (orbType)
            {
                case 0: return "火";     // 火
                case 1: return "水";     // 水
                case 2: return "木";     // 木頭
                case 3: return "光";     // 光明
                case 4: return "暗";     // 黑暗
                case 5: return "心";     // 心形
                case 6: return "妨";     // 妨礙珠
                case 7: return "毒";     // 毒珠
                default: return "?";     // 未知
            }
        }

        private void DrawPath(Graphics g, List<Point> path)
        {
            if (path.Count < 2) return;

            // 繪製粗的路徑線，使用半透明顏色
            using (var pathPen = new Pen(Color.FromArgb(180, Color.Yellow), 10))
            {
                pathPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pathPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pathPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                // 繪製路徑線
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(pathPen, path[i], path[i + 1]);
                }
            }

            // 繪製路徑邊緣（增加對比）
            using (var edgePen = new Pen(Color.FromArgb(220, Color.Black), 3))
            {
                edgePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                edgePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                edgePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(edgePen, path[i], path[i + 1]);
                }
            }

            // 繪製起點和終點標記
            if (path.Count > 0)
            {
                // 起點（綠色帶黑色邊框）
                g.FillEllipse(Brushes.Lime, path[0].X - 8, path[0].Y - 8, 16, 16);
                g.DrawEllipse(Pens.Black, path[0].X - 8, path[0].Y - 8, 16, 16);

                // 終點（紅色帶黑色邊框）
                var endPoint = path[path.Count - 1];
                g.FillEllipse(Brushes.Red, endPoint.X - 8, endPoint.Y - 8, 16, 16);
                g.DrawEllipse(Pens.Black, endPoint.X - 8, endPoint.Y - 8, 16, 16);

                // 在起點和終點添加文字
                var font = new Font("Broadway", 8, FontStyle.Bold);
                g.DrawString("始", font, Brushes.Black, path[0].X - 5, path[0].Y - 6);
                g.DrawString("終", font, Brushes.White, endPoint.X - 5, endPoint.Y - 6);
            }

            // 在路徑上新增方向箭頭
            DrawDirectionArrows(g, path);
        }

        private void DrawDirectionArrows(Graphics g, List<Point> path)
        {
            if (path.Count < 2) return;

            var arrowBrush = new SolidBrush(Color.Red);

            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawArrow(g, arrowBrush, path[i], path[i + 1]);
            }

            arrowBrush.Dispose();
        }

        private void DrawArrow(Graphics g, Brush brush, Point from, Point to)
        {
            var arrowSize = 10;
            var angle = Math.Atan2(to.Y - from.Y, to.X - from.X);

            // 計算箭頭位置（在線的中間）
            var midX = (from.X + to.X) / 2;
            var midY = (from.Y + to.Y) / 2;

            var arrowPoint1 = new Point(
                (int)(midX - arrowSize * Math.Cos(angle - Math.PI / 6)),
                (int)(midY - arrowSize * Math.Sin(angle - Math.PI / 6))
            );

            var arrowPoint2 = new Point(
                (int)(midX - arrowSize * Math.Cos(angle + Math.PI / 6)),
                (int)(midY - arrowSize * Math.Sin(angle + Math.PI / 6))
            );

            // 繪製箭頭頭部
            Point[] arrowPoints = {
                new Point(midX, midY),
                arrowPoint1,
                arrowPoint2
            };
            g.FillPolygon(brush, arrowPoints);
        }

        private void ClearPath()
        {
            currentPath.Clear();
            GridPanel.Invalidate();
        }

        private OrbWeights[] GetCurrentWeights()
        {
            if (profileCombo.SelectedItem == null)
                return GetDefaultWeights();

            string selectedProfile = profileCombo.SelectedItem.ToString();

            // 配置資料映射
            var profileData = new Dictionary<string, double[]>
            {
             { "5色隊伍 (多目標)", new double[] { 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "5色隊伍 (單目標)", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "回覆隊伍", new double[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 1, 1, 0.1, 0.1, 0.1, 0.1 } },
             { "回覆隊伍 (多目標)", new double[] { 0.1, 0.3, 0.1, 0.3, 0.1, 0.3, 0.1, 0.3, 0.1, 0.3, 1, 1, 0.1, 0.1, 0.1, 0.1 } },
             { "火隊 (多目標)", new double[] { 1, 3, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "火隊 (單目標)", new double[] { 1, 1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "水隊 (多目標)", new double[] { 0.1, 0.1, 1, 3, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "水隊 (單目標)", new double[] { 0.1, 0.1, 1, 1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "木隊 (多目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 1, 3, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "木隊 (單目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 1, 1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "光隊 (多目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 1, 3, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "光隊 (單目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 1, 1, 0.1, 0.1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "暗隊 (多目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 1, 3, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } },
             { "暗隊 (單目標)", new double[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 1, 1, 0.3, 0.3, 0.1, 0.1, 0.1, 0.1 } }
            };

            if (profileData.ContainsKey(selectedProfile))
            {
                double[] weights = profileData[selectedProfile];
                return new OrbWeights[]
                {
                 new OrbWeights(weights[0], weights[1]),    // 火 (0)
                 new OrbWeights(weights[2], weights[3]),    // 水 (1)
                 new OrbWeights(weights[4], weights[5]),    // 木 (2)
                 new OrbWeights(weights[6], weights[7]),    // 光 (3)
                 new OrbWeights(weights[8], weights[9]),    // 暗 (4)
                 new OrbWeights(weights[10], weights[11]),  // 心 (5)
                 new OrbWeights(weights[12], weights[13]),  // 妨 (6)
                 new OrbWeights(weights[14], weights[15])   // 毒 (7)
                };
            }

            return GetDefaultWeights();
        }

        private OrbWeights[] GetDefaultWeights()
        {
            return new OrbWeights[]
            {
            new OrbWeights(1, 3),     // 火
            new OrbWeights(1, 3),     // 水
            new OrbWeights(1, 3),     // 木
            new OrbWeights(1, 3),     // 光
            new OrbWeights(1, 3),     // 暗
            new OrbWeights(0.3, 0.3), // 心
            new OrbWeights(0.1, 0.1), // 妨
            new OrbWeights(0.1, 0.1)  // 毒
            };
        }

        private async void SolveButton_Click(object sender, EventArgs e)
        {
            if (SolveButton.Text == "開始求解")
            {
                // 開始求解
                SolveButton.Text = "停止求解";
                SolveButton.BackColor = Color.LightCoral;
                statusLabel.Text = "解題中...";
                statusLabel.ForeColor = Color.Orange;

                solutions.Clear();
                SolutionsList.Items.Clear();
                ClearPath();

                var weights = GetCurrentWeights();

                try
                {
                    var result = await Task.Run(() =>
                    optimizer.Solve(BoardManager.Board, weights, (int)maxLengthInput.Value, allow8DirCheck.Checked)
                    );

                    solutions = result;
                    UpdateSolutionsList();
                    statusLabel.Text = $"找到 {solutions.Count} 個解法";
                    statusLabel.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    statusLabel.Text = $"錯誤: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                }
                finally
                {
                    SolveButton.Text = "開始求解";
                    SolveButton.BackColor = Color.LightGreen;
                }
            }
            else
            {
                // 停止求解（需要在中止機制）
                SolveButton.Text = "開始求解";
                SolveButton.BackColor = Color.LightGreen;
                statusLabel.Text = "已停止";
                statusLabel.ForeColor = Color.Blue;
            }
        }

        private void UpdateSolutionsList()
        {
            SolutionsList.BeginUpdate();
            SolutionsList.Items.Clear();

            foreach (var solution in solutions.Take(50)) // 只顯示前50個
            {
                var matchInfo = string.Join(" ", solution.Matches
                .GroupBy(m => m.Type)
                .Select(g => $"{GetOrbSymbol(g.Key)}×{g.Sum(m => m.Count)}"));

                SolutionsList.Items.Add($"權重:{solution.Weight:F1} 步數:{solution.Path.Count} {matchInfo}");
            }

            SolutionsList.EndUpdate();
        }

        private Point GetOrbCenter(int row, int col)
        {
            return new Point(
                col * ORB_SIZE + ORB_SIZE / 2,
                row * ORB_SIZE + ORB_SIZE / 2
            );
        }

        private void GetGameBoardButton_Click(object sender, EventArgs e)
        {
            //BoardManager.RandomizeBoard(); // 改用共用方法

            solutions.Clear();
            SolutionsList.Items.Clear();
            ClearPath();

            // 查找所有PUZZLE & DRAGONS 0 - 窗口
            var allPD0Windows = WindowHelper.FindWindowsWithTitle("PUZZLE & DRAGONS 0 - ");

            foreach (var PD0Window in allPD0Windows)
            {
                // 查找 PUZZLE & DRAGONS 0 顯示窗口
                IntPtr displayWindow = ScreenCapture.FindDisplayWindow(PD0Window);

                if (displayWindow != IntPtr.Zero)
                {
                    // 獲取窗口進程ID
                    _ = WindowHelper.GetWindowThreadProcessId(PD0Window, out uint processId);

                    // 從設置中獲取對應的PngPath
                    string pngPath = "R:\\Temp\\";

                    // 確保目錄存在
                    if (!Directory.Exists(pngPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(pngPath);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print($"創建目錄失敗: {pngPath}, 錯誤: {ex.Message}");
                            continue;
                        }
                    }

                    string fileName = $"screenshot.png";
                    string filePath = Path.Combine(pngPath, fileName);
                    PD0WindowsHwnd = PD0Window;

                    if (ScreenCapture.CaptureWithTopMostAndCopy(PD0Window, displayWindow, filePath))
                    {
                        ScreenCapture.DebugWindowInfo(displayWindow);
                        Debug.Print($"截圖成功: {fileName}，路徑: {pngPath}");
                    }
                    else
                    {
                        Debug.Print($"截圖失敗...");
                    }

                    var detector = new ImprovedOrbDetection();

                    if (File.Exists(filePath))
                    {
                        detector.DetectOrbsWithImprovements(filePath);
                    }
                    else
                    {
                        Debug.Print($"圖片不存在: {filePath}");
                    }
                }
            }

            ClearPath();
            GridPanel.Invalidate();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            BoardManager.ClearBoard(); // 改用共用方法
            ClearPath();
            GridPanel.Invalidate();
        }

        private void GridPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // 繪製寶珠
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    DrawOrb(g, row, col, BoardManager.Board[row, col]); // 改用共用盤面
                }
            }

            // 繪製路徑
            if (currentPath.Count > 1)
            {
                DrawPath(g, currentPath);
            }
        }

        private void GridPanel_MouseClick(object sender, MouseEventArgs e)
        {
            int col = e.X / ORB_SIZE;
            int row = e.Y / ORB_SIZE;

            if (row >= 0 && row < 5 && col >= 0 && col < 6)
            {
                if (e.Button == MouseButtons.Left)
                {
                    BoardManager.Board[row, col] = (BoardManager.Board[row, col] + 1) % 8; // 改用共用盤面
                }
                else if (e.Button == MouseButtons.Right)
                {
                    BoardManager.Board[row, col] = (BoardManager.Board[row, col] + 7) % 8; // 反向循环，改用共用盤面
                }

                ClearPath();
                GridPanel.Invalidate();
            }
        }

        private void SolutionsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SolutionsList.SelectedIndex >= 0 && SolutionsList.SelectedIndex < solutions.Count)
            {
                var solution = solutions[SolutionsList.SelectedIndex];
                currentPath = ConvertPathToPoints(solution);
                GridPanel.Invalidate();
            }
        }

        private List<Point> ConvertPathToPoints(Solution solution)
        {
            var points = new List<Point>();
            var current = solution.InitCursor;

            points.Add(GetOrbCenter(current.Row, current.Col));

            foreach (var dir in solution.Path)
            {
                current = optimizer.MovePosition(current, dir);
                points.Add(GetOrbCenter(current.Row, current.Col));
            }

            return points;
        }

        private void ExecutePathButton_Click(object sender, EventArgs e)
        {
            if (SolutionsList.SelectedIndex < 0)
            {
                MessageBox.Show("請先選擇一個解決方案", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (PD0WindowsHwnd == IntPtr.Zero)
            {
                MessageBox.Show("未找到遊戲窗口，請先獲取遊戲盤面", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var solution = solutions[SolutionsList.SelectedIndex];
                var pathPoints = ConvertSolutionToPoints(solution);

                if (pathPoints.Count < 2)
                {
                    MessageBox.Show("路徑點不足", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 更新UI顯示正在執行
                statusLabel.Text = "執行路徑中...";
                statusLabel.ForeColor = Color.Orange;
                ExecutePathButton.Enabled = false;
                Application.DoEvents();

                // 執行路徑
                var options = new AdvancedHwndMouseSimulator.PathOptions
                {
                    Duration = 800,
                    StepsPerSegment = 25
                };

                AdvancedHwndMouseSimulator.ExecutePathInWindow(PD0WindowsHwnd, pathPoints, options);

                // 恢復UI狀態
                statusLabel.Text = "路徑執行完成";
                statusLabel.ForeColor = Color.Green;
                ExecutePathButton.Enabled = true;

                solutions.Clear();
                SolutionsList.Items.Clear();
                ClearPath();
                //MessageBox.Show("路徑執行完成!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"執行失敗: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                ExecutePathButton.Enabled = true;

                MessageBox.Show($"執行路徑時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Point> ConvertSolutionToPoints(Solution solution)
        {
            var points = new List<Point>();
            var current = solution.InitCursor;

            // 添加起始點
            points.Add(ImprovedOrbDetection.GetOrbPoint(current.Row, current.Col));

            // 根據路徑方向添加後續點
            foreach (var dir in solution.Path)
            {
                current = optimizer.MovePosition(current, dir);
                points.Add(ImprovedOrbDetection.GetOrbPoint(current.Row, current.Col));
            }

            return points;
        }
    }
}