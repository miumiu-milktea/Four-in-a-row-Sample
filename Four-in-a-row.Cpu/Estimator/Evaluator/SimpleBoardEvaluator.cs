using Four_in_a_row.Cpu.Estimator.Common;
using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Four_in_a_row.Cpu.Estimator.Evaluator
{
    internal class SimpleBoardEvaluator : IBoardEvaluator
    {
        private readonly GameSetting Setting;

        public SimpleBoardEvaluator(GameSetting setting)
        {
            this.Setting = setting;
        }

        public BoardScore Evaluate(ISimpleBoard board)
        {
            var score = board.CreateEnumerableIndex()
                .Select(index => EvaluateCellValue(board, index))
                .Aggregate((a, b) => (a.first + b.first, a.second + b.second));

            return new BoardScore(score.first, score.second);
        }

        private IEnumerable<BoardIndex> GetIndexEnumerable(BoardIndex index, IndexDirection direction)
        {
            var ary = new BoardIndex[Setting.ConnectCount];
            foreach (var ii in Enumerable.Range(0, Setting.ConnectCount))
            {
                ary[ii] = index;
                index = index.GetIndex(direction);
            }
            return ary;
        }

        private (double first, double second) EvaluateCellValue(ISimpleBoard board, BoardIndex index)
        {
            return new IndexDirection[] { IndexDirection.Down, IndexDirection.Right, IndexDirection.DownLeft, IndexDirection.DownRight }
                .Select(d => EvaluateCellDirectionValue(board, index, d))
                .Aggregate((a, b) => (a.first + b.first, a.second + b.second));
        }

        private (double first, double second) EvaluateCellDirectionValue(ISimpleBoard board, BoardIndex index, IndexDirection direction)
        {
            var count_first = 0;
            var count_second = 0; 
            var count_available = 0;
            var count_void = 0;
            var count_out = 0;

            foreach (var cellVal in GetIndexEnumerable(index, direction).Select(idx => board.GetCellStatus(idx)))
            {
                switch (cellVal)
                {
                    case CellStatus.First:
                        count_first++;
                        break;
                    case CellStatus.Second:
                        count_second++;
                        break;
                    case CellStatus.Available:
                        count_available++;
                        break;
                    case CellStatus.Void:
                        count_void++;
                        break;
                    case CellStatus.OutOfOrder:
                        count_out++;
                        break;
                }
            }


            // 枠内で上がることがない。単純個数評価
            if (0 < count_out)
            {
                return (count_first, count_second);
            }

            // 枠内混在
            if (0 < count_first && 0 < count_second)
            {
                return (count_first, count_second);
            }

            // 単一で埋まっている場合は高評価。空きがavailableでない場合妨害がしにくいため評価を上げてみる。
            var ratio = count_void > 0 ? 2.0 : 1.5;
            return (ratio * Math.Pow(count_first, 2), ratio * Math.Pow(count_second, 2));
        }
    }
}
