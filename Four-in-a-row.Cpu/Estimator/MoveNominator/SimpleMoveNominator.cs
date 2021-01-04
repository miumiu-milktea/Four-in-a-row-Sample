using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Four_in_a_row.Cpu.Estimator.MoveNominator
{
    internal class SimpleMoveNominator : IMoveNominator
    {
        private readonly GameSetting Setting;

        public SimpleMoveNominator(GameSetting setting)
        {
            this.Setting = setting;
        }

        public IEnumerable<StoneMove> GetCandidateStoneMove(ISimpleBoard board)
        {
            if (null != board.GetConnectIndex())
            {
                return null;
            }

            // 候補手を中央寄りにしておく
            return board.GetCandidateStoneMove().OrderBy(m => Math.Pow(Setting.ColSize / 2.0 - m.Index.ColIndex, 2)).ThenByDescending(m => m.Index.RowIndex);
        }
    }
}
