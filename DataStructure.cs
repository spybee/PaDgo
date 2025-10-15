using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public class Position
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Row == other.Row && Col == other.Col;
        }

        public override int GetHashCode()
        {
            return (Row << 16) | Col;
        }
    }

    public class OrbMatch
    {
        public int Type { get; set; }
        public int Count { get; set; }

        public OrbMatch(int type, int count)
        {
            Type = type;
            Count = count;
        }
    }

    public class OrbWeights
    {
        public double Normal { get; set; }
        public double Mass { get; set; }

        public OrbWeights(double normal, double mass)
        {
            Normal = normal;
            Mass = mass;
        }
    }

    public class Solution
    {
        public int[,] Board { get; set; }
        public Position Cursor { get; set; }
        public Position InitCursor { get; set; }
        public List<int> Path { get; set; }
        public bool IsDone { get; set; }
        public double Weight { get; set; }
        public List<OrbMatch> Matches { get; set; }

        public Solution()
        {
            Path = new List<int>();
            Matches = new List<OrbMatch>();
        }

        public Solution Clone()
        {
            return new Solution
            {
                Board = (int[,])Board.Clone(),
                Cursor = new Position(Cursor.Row, Cursor.Col),
                InitCursor = new Position(InitCursor.Row, InitCursor.Col),
                Path = new List<int>(Path),
                IsDone = IsDone,
                Weight = Weight,
                Matches = Matches.Select(m => new OrbMatch(m.Type, m.Count)).ToList()
            };
        }
    }
}
