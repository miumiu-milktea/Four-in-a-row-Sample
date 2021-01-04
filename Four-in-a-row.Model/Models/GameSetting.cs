namespace Four_in_a_row.Models
{
    public class GameSetting
    {
        public int RowSize { get; }
        public int ColSize { get; }

        public int ConnectCount { get; }

        public GameSetting(int rowSize, int colSize, int? connectCount = null)
        {
            this.RowSize = rowSize;
            this.ColSize = colSize;

            this.ConnectCount = connectCount ?? Constant.DefaultConnectCount;
        }
    }
}
