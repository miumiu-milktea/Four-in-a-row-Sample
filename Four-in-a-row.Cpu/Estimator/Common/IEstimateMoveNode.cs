using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System.Collections.Generic;

namespace Four_in_a_row.Cpu.Estimator.Common
{
    internal interface IEstimateMoveNode
    {
        ISimpleBoard Board { get; }
        IReadOnlyList<EstimateMoveNode> Children { get; }
        int Depth { get; }
        BoardScore ExpectedScore { get; }
        bool Finished { get; }
        StoneMove Move { get; }
        EstimateMoveNode Parent { get; }
        ISimpleBoard RawBoard { get; }
        GameResult? Result { get; }
        GameUser Turn { get; }
    }
}