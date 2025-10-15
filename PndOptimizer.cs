using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public class PndOptimizer
    {
        private const int ROWS = 5;
        private const int COLS = 6;
        private const int TYPES = 7;
        private const double MULTI_ORB_BONUS = 0.25;
        private const double COMBO_BONUS = 0.25;
        private const int MAX_SOLUTIONS_COUNT = ROWS * COLS * 8 * 2;

        public int[,] CreateEmptyBoard()
        {
            return new int[ROWS, COLS];
        }

        public List<OrbMatch> FindMatches(int[,] board)
        {
            var matchBoard = new int[ROWS, COLS];
            var matches = new List<OrbMatch>();

            // Horizontal matches
            for (int i = 0; i < ROWS; i++)
            {
                int prev1 = -1, prev2 = -1;
                for (int j = 0; j < COLS; j++)
                {
                    int current = board[i, j];
                    if (prev1 == prev2 && prev2 == current && current >= 0)
                    {
                        matchBoard[i, j] = current;
                        matchBoard[i, j - 1] = current;
                        matchBoard[i, j - 2] = current;
                    }
                    prev1 = prev2;
                    prev2 = current;
                }
            }

            // Vertical matches
            for (int j = 0; j < COLS; j++)
            {
                int prev1 = -1, prev2 = -1;
                for (int i = 0; i < ROWS; i++)
                {
                    int current = board[i, j];
                    if (prev1 == prev2 && prev2 == current && current >= 0)
                    {
                        matchBoard[i, j] = current;
                        matchBoard[i - 1, j] = current;
                        matchBoard[i - 2, j] = current;
                    }
                    prev1 = prev2;
                    prev2 = current;
                }
            }

            // Flood fill to count matches
            var scratchBoard = (int[,])matchBoard.Clone();

            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    if (scratchBoard[i, j] < 0) continue;

                    int currentOrb = scratchBoard[i, j];
                    var stack = new Stack<Position>();
                    stack.Push(new Position(i, j));
                    int count = 0;

                    while (stack.Count > 0)
                    {
                        var pos = stack.Pop();
                        if (scratchBoard[pos.Row, pos.Col] != currentOrb) continue;

                        count++;
                        scratchBoard[pos.Row, pos.Col] = -1;

                        if (pos.Row > 0) stack.Push(new Position(pos.Row - 1, pos.Col));
                        if (pos.Row < ROWS - 1) stack.Push(new Position(pos.Row + 1, pos.Col));
                        if (pos.Col > 0) stack.Push(new Position(pos.Row, pos.Col - 1));
                        if (pos.Col < COLS - 1) stack.Push(new Position(pos.Row, pos.Col + 1));
                    }

                    if (count > 0)
                    {
                        matches.Add(new OrbMatch(currentOrb, count));
                    }
                }
            }

            return matches;
        }

        public double ComputeWeight(List<OrbMatch> matches, OrbWeights[] weights)
        {
            double totalWeight = 0;

            foreach (var match in matches)
            {
                var weight = match.Count >= 5 ? weights[match.Type].Mass : weights[match.Type].Normal;
                double multiOrbBonus = (match.Count - 3) * MULTI_ORB_BONUS + 1;
                totalWeight += multiOrbBonus * weight;
            }

            double comboBonus = (matches.Count - 1) * COMBO_BONUS + 1;
            return totalWeight * comboBonus;
        }

        public bool CanMoveOrb(Position rc, int dir)
        {
            switch (dir)
            {
                case 0: return rc.Col < COLS - 1;
                case 1: return rc.Row < ROWS - 1 && rc.Col < COLS - 1;
                case 2: return rc.Row < ROWS - 1;
                case 3: return rc.Row < ROWS - 1 && rc.Col > 0;
                case 4: return rc.Col > 0;
                case 5: return rc.Row > 0 && rc.Col > 0;
                case 6: return rc.Row > 0;
                case 7: return rc.Row > 0 && rc.Col < COLS - 1;
            }
            return false;
        }

        public Position MovePosition(Position rc, int dir)
        {
            var newPos = new Position(rc.Row, rc.Col);

            switch (dir)
            {
                case 0: newPos.Col += 1; break;
                case 1: newPos.Row += 1; newPos.Col += 1; break;
                case 2: newPos.Row += 1; break;
                case 3: newPos.Row += 1; newPos.Col -= 1; break;
                case 4: newPos.Col -= 1; break;
                case 5: newPos.Row -= 1; newPos.Col -= 1; break;
                case 6: newPos.Row -= 1; break;
                case 7: newPos.Row -= 1; newPos.Col += 1; break;
            }

            return newPos;
        }

        public void SwapOrbs(int[,] board, Position rc, int dir)
        {
            var newPos = MovePosition(rc, dir);
            int temp = board[rc.Row, rc.Col];
            board[rc.Row, rc.Col] = board[newPos.Row, newPos.Col];
            board[newPos.Row, newPos.Col] = temp;
        }

        public List<Solution> Solve(int[,] board, OrbWeights[] weights, int maxLength, bool allow8Dir)
        {
            var solutions = new List<Solution>();
            var seedSolution = new Solution
            {
                Board = (int[,])board.Clone(),
                Cursor = new Position(0, 0),
                InitCursor = new Position(0, 0)
            };

            EvaluateSolution(seedSolution, weights);

            // Initialize solutions with all starting positions
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    var solution = seedSolution.Clone();
                    solution.Cursor = new Position(i, j);
                    solution.InitCursor = new Position(i, j);
                    solutions.Add(solution);
                }
            }

            int dirStep = allow8Dir ? 1 : 2;

            for (int p = 0; p < maxLength; p++)
            {
                var newSolutions = new List<Solution>();

                foreach (var solution in solutions)
                {
                    if (solution.IsDone) continue;

                    for (int dir = 0; dir < 8; dir += dirStep)
                    {
                        if (!CanMoveOrb(solution.Cursor, dir)) continue;
                        if (solution.Path.Count > 0 && solution.Path.Last() == (dir + 4) % 8) continue;

                        var newSolution = solution.Clone();
                        SwapOrbs(newSolution.Board, newSolution.Cursor, dir);
                        newSolution.Cursor = MovePosition(newSolution.Cursor, dir);
                        newSolution.Path.Add(dir);

                        EvaluateSolution(newSolution, weights);
                        newSolutions.Add(newSolution);
                    }

                    solution.IsDone = true;
                }

                solutions.AddRange(newSolutions);
                solutions = solutions.OrderByDescending(s => s.Weight).Take(MAX_SOLUTIONS_COUNT).ToList();
            }

            return solutions;
        }

        public void EvaluateSolution(Solution solution, OrbWeights[] weights)
        {
            var currentBoard = (int[,])solution.Board.Clone();
            var allMatches = new List<OrbMatch>();

            while (true)
            {
                var matches = FindMatches(currentBoard);
                if (matches.Count == 0) break;

                // Remove matches and drop orbs logic would go here
                // This is simplified - actual implementation needs full cascade logic
                allMatches.AddRange(matches);
                break; // Simplified for example
            }

            solution.Weight = ComputeWeight(allMatches, weights);
            solution.Matches = allMatches;
        }
    }
}
