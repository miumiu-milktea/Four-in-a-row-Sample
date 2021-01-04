using Four_in_a_row.Cpu.Estimator.Common;
using Four_in_a_row.Cpu.Estimator.Evaluator;
using Four_in_a_row.Cpu.Estimator.MoveNominator;
using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Four_in_a_row.Cpu.Estimator
{
    internal class SimpleMoveEstimator : IMoveEstimator
    {
        private readonly GameSetting Setting;
        private readonly IBoardEvaluator boardEvaluator;
        private readonly IMoveNominator moveNominator;
        private readonly int maxDepth;

        public SimpleMoveEstimator(GameSetting setting, IBoardEvaluator boardEvaluator, IMoveNominator moveNominator, int maxDepth)
        {
            this.Setting = setting;
            this.boardEvaluator = boardEvaluator;
            this.moveNominator = moveNominator;
            this.maxDepth = maxDepth;
        }

        public IList<StoneMove> EstimateStoneMove(ISimpleBoard board, int fetchCount = 1)
        {
            var root = new EstimateMoveNode(board);

            BuildNode(root, this.maxDepth);
            root.DebugOutputEstimateInternal(3);

            return root.Children
                .OrderByDescending(c => c.ExpectedScore.GetScore(c.Turn))
                .ThenByDescending(c => c.ExpectedScore.DeterminsticTurnCount)
                .ThenByDescending(c => c.AtomicScore?.GetScore(c.Turn) ?? 0)
                .Take(fetchCount)
                .Select(c => c.Move)
                .ToList();
        }

        private void BuildNode(EstimateMoveNode node, int maxDepth)
        {
            // tree終了条件
            if (node.Depth < maxDepth && !node.Finished)
            {
                var isAbsoluteNextTurnMine = node.IsAbsoluteNextTurnMine;

                var _lock = new object();
                int? minWinTurnCount = null;
                var isBreak = false;

                Action<StoneMove> childProcess = move =>
                {
                    if (isBreak) return;

                    var child = node.AddChild(move);
                    BuildNode(child, minWinTurnCount ?? maxDepth);

                    // 勝利手数の最小を保持する。
                    if (child.ExpectedScore.Result == child.Turn.ToResult())
                    {
                        lock (_lock)
                        {
                            if (!minWinTurnCount.HasValue || minWinTurnCount > child.ExpectedScore.DeterminsticTurnCount)
                            {
                                minWinTurnCount = child.ExpectedScore.DeterminsticTurnCount;
                            }
                        }
                    }

                    // 敗北確定手を発見したならそこで止める。
                    if (child.Result == node.Turn.GetNextUser().ToResult())
                    {
                        isBreak = true;
                        return;
                    }

                    if (isAbsoluteNextTurnMine && child.ExpectedScore.Result == child.Turn.ToResult())
                    {
                        // 勝利確定手発見時は探索中止。勝ち方にこだわらない。
                        // ここを調整すると賢く見えそう。
                        // 相手のターンでも打ち切っていいんだけど、難しい手の時にあきらめ挙動になってしまうのを回避する。
                        isBreak = true;
                        return;
                    }
                };

                if (0 >= node.Depth)
                {
                    moveNominator.GetCandidateStoneMove(node.Board).AsParallel().ForAll(childProcess);
                }
                else
                {
                    moveNominator.GetCandidateStoneMove(node.Board).ForEach(childProcess);
                }
            }
            ScoringNode(node);

            node.ReleaseData();
        }

        private void ScoringNode(EstimateMoveNode node)
        {
            // 頓死の確認
            if (node.Children.Any(c => c.Finished && c.Result != GameResult.Draw))
            {
                // 次手で終わる＝敗北確定手
                node.ExpectedScore = new BoardScore(node.Turn.GetNextUser(), node.Depth + 1);
                return;
            }

            // 確定結果の統計取得
            var determinResults = node.Children
                .Select(c => c.ExpectedScore)
                .GroupBy(s => s?.Result)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Result = g.Key,
                    Count = g.Count(),
                    MinTurnCount = g.Min(s => s.DeterminsticTurnCount),
                    MaxTurnCount = g.Max(s => s.DeterminsticTurnCount),
                })
                .ToArray();


            if (determinResults.Length == 1)
            {
                var result = determinResults.First();
                if (result.Result.HasValue)
                {
                    // 何をやっても結果が確定
                    node.ExpectedScore = new BoardScore(result.Result.Value, result.Result == node.Turn.GetNextUser().ToResult() ? result.MinTurnCount : result.MaxTurnCount);
                    return;
                }
            }

            var nextTurnResult = determinResults.FirstOrDefault(s => s.Result == node.Turn.GetNextUser().ToResult());
            if (null != nextTurnResult)
            {
                // 次手の最善で敗北確定
                node.ExpectedScore = new BoardScore(node.Turn.GetNextUser().ToResult(), nextTurnResult.MinTurnCount);
                return;
            }

            // 読み切り範囲外なので機械的なScoringに頼る
            if (node.Children?.IsEmpty() ?? true)
            {
                node.AtomicScore = boardEvaluator.Evaluate(node.Board);
                node.ExpectedScore = node.AtomicScore;
            }
            else
            {
                node.ExpectedScore = node.Children.OrderByDescending(c => c.ExpectedScore.GetScore(c.Turn)).Select(c => c.ExpectedScore).First();
            }
        }
    }
}
