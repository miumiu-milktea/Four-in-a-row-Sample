using Four_in_a_row.Models;
using System;

namespace Four_in_a_row.Cpu.Estimator.Common
{
    public class BoardScore
    {
        public double First { get; }
        public double Second { get; }

        public GameResult? Result { get; }
        public int DeterminsticTurnCount { get; }

        public bool IsDeterministic => Result.HasValue;

        public BoardScore(double first, double second)
        {
            this.First = first;
            this.Second = second;
        }

        public BoardScore(GameUser user, int turnCount) : this(user.ToResult(), turnCount)
        {
        }

        public BoardScore(GameResult result, int turnCount)
        {
            this.DeterminsticTurnCount = turnCount;
            this.Result = result;
        }

        public double GetScore(GameUser user)
        {
            if (IsDeterministic)
            {
                if (Result == user.ToResult())
                {
                    return double.MaxValue;
                }
                else if (Result == user.GetNextUser().ToResult())
                {
                    return double.MinValue;
                }
                else
                {
                    return 0;
                }
            }

            var val = Math.Pow(First, 2) - Math.Pow(Second, 2);
            switch (user)
            {
                case GameUser.First:
                    return val;
                case GameUser.Second:
                    return -1 * val;
                default:
                    return 0;
            }
        }

        public override string ToString()
        {
            return IsDeterministic
                ? $"(Determinstic {Result} TurnCount = {DeterminsticTurnCount})"
                : $"({Math.Pow(First, 2) - Math.Pow(Second, 2)})";
        }
    }
}
