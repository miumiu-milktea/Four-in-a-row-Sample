using Four_in_a_row.Models;

namespace Four_in_a_row
{
    public static class Constant
    {
        public static readonly int DefaultRowSize = 6;
        public static readonly int DefaultColSize = 7;

        public static readonly int DefaultConnectCount = 4;

        public static readonly int MaxEstimateDepth = 6;

        public static readonly GameUser? CpuUser = GameUser.Second;
    }
}
