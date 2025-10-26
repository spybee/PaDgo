// BoardManager.cs
using System;

namespace PaDgo
{
    public static class BoardManager
    {
        public static int[,] Board { get; set; } = new int[5, 6];

        public static void InitializeBoard()
        {
            Board = new int[5, 6];
        }

        public static void RandomizeBoard()
        {
            var random = new Random();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Board[i, j] = random.Next(7);
                }
            }
        }

        public static void ClearBoard()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Board[i, j] = 8; // 全部設為?珠
                }
            }
        }

        public static void UpdateBoardFromDetection(OrbType[,] detectedBoard)
        {
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    // 將 OrbType 轉換為對應的整數值
                    Board[row, col] = (int)detectedBoard[col, row];
                }
            }
        }
    }
}