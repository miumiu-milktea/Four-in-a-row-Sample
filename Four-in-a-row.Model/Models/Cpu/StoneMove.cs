using Four_in_a_row.Models.Board;

namespace Four_in_a_row.Models.Cpu
{
    public class StoneMove
    {
        public GameUser Turn { get; }

        public BoardIndex Index { get; }

        public StoneMove(BoardIndex index, GameUser turn)
        {
            this.Index = index;
            this.Turn = turn;
        }
    }
}
