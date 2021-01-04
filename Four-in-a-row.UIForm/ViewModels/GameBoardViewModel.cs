using Four_in_a_row.Cpu;
using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Four_in_a_row.UIForm.ViewModels
{
    class GameBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string InputRowSize { get; set; } = Constant.DefaultRowSize.ToString();
        public string InputColSize { get; set; } = Constant.DefaultColSize.ToString();

        public double DrawingWidth { get; set; } = 800;
        public double DrawingHeight { get; set; } = 530;

        public double CellWidth { get; private set; }
        public double CellHeight { get; private set; }

        private readonly StartGameCommandImpl _startGameCommand;
        public ICommand StartGameCommand => _startGameCommand;

        public Game Game { get; private set; }

        public int RowSize => Game?.Setting.RowSize ?? 0;
        public int ColSize => Game?.Setting.ColSize ?? 0;

        public bool IsGameStatusSetting => (Game?.Status).IsSetting();
        public bool IsInGame => (Game?.Status).IsInGame();

        public DrawingCollection GeometryDrawingCollection => null == geometryDrawings ? new DrawingCollection() : new DrawingCollection(geometryDrawings);


        private IList<GeometryDrawing> geometryDrawings;
        private GeometryDrawing[,] tileGeometryDrawing;
        private GeometryDrawing[,] stoneGeometryDrawing;
        private GeometryDrawing hoverTile = null;

        public GameBoardViewModel()
        {
            this._startGameCommand = new StartGameCommandImpl(this);
        }

        private IMoveEstimator CreateMoveEvaluator(GameSetting gameSetting)
        {
            return Four_in_a_row_CpuInitializer.CreateMoveEvaluator(gameSetting);
        }

        public void StartGame(GameSetting gameSetting)
        {
            this.Game = new Game(gameSetting);
            this.Game.SetCpu(CreateMoveEvaluator(gameSetting), Constant.CpuUser);
            this.Game.GameStart();

            this.InitializeDrawingObject();

            this.ProcessCpuTurn();
        }

        private void InitializeDrawingObject()
        {
            this.CellWidth = DrawingWidth / ColSize;
            this.CellHeight = DrawingHeight / RowSize;

            geometryDrawings = new List<GeometryDrawing>();
            tileGeometryDrawing = new GeometryDrawing[RowSize, ColSize];
            stoneGeometryDrawing = new GeometryDrawing[RowSize, ColSize];

            AddGridLine();
            AddHighlightTile();

            NotifyDrawing();
        }

        public void EndGame()
        {
            if (Game.Status.IsGameSet())
            {
                MessageBox.Show("ゲーム終了。", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            RemoveHighlightTile();
            this.Game = null;

            NotifyGameStatusChange();
        }

        private void NotifyDrawing()
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeometryDrawingCollection)));
        }

        private void NotifyGameStatusChange()
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameBoardViewModel.IsGameStatusSetting)));
            this._startGameCommand.NotifyGameStatusChange();
        }

        #region DrawingMethod

        private enum TileStyle
        {
            InVisible = 0,
            FrameOnly = 1,
            Highlight = 2,
        }

        private void AddGridLine()
        {
            var gridBrush = Brushes.Black;
            var gridPen = new Pen(Brushes.Black, 0.5)
            {
                DashStyle = new DashStyle(new double[] { 0.5, 8 }, 0)
            };

            foreach (var ii in Enumerable.Range(1, ColSize - 1))
            {
                this.geometryDrawings.Add(
                    new GeometryDrawing(gridBrush, gridPen, new LineGeometry(new Point(ii * CellWidth, 0), new Point(ii * CellWidth, DrawingHeight)))
                );
            }
            foreach (var ii in Enumerable.Range(1, RowSize - 1))
            {
                this.geometryDrawings.Add(
                    new GeometryDrawing(gridBrush, gridPen, new LineGeometry(new Point(0, ii * CellHeight), new Point(DrawingWidth, ii * CellHeight)))
                );
            }
        }

        private void AddHighlightTile()
        {
            var margin = 3.0;
            foreach (var ii in Enumerable.Range(0, ColSize))
            {
                foreach (var kk in Enumerable.Range(0, RowSize))
                {
                    var geometryDrawing = new GeometryDrawing(
                        null,
                        null,
                        new RectangleGeometry(new Rect(new Point(ii * CellWidth + margin, kk * CellHeight + margin), new Point((ii + 1) * CellWidth - margin, (kk + 1) * CellHeight - margin)), margin, margin)
                    );
                    SetTileStyle(geometryDrawing);

                    tileGeometryDrawing[kk, ii] = geometryDrawing;
                    geometryDrawings.Add(geometryDrawing);
                }
            }
        }

        public void SetHighlightTile(Point point)
        {
            if (!IsInGame) return;

            var colIndex = (int)(point.X / CellWidth);
            var rowIndex = (int)(point.Y / CellHeight);

            var tile = tileGeometryDrawing[rowIndex, colIndex];
            if (null == tile || tile == hoverTile) return;

            if (null != hoverTile)
            {
                SetTileStyle(hoverTile);
                hoverTile = null;
            }
            switch (Game.Board[new BoardIndex(rowIndex, colIndex)])
            {
                case CellStatus.Available:
                    hoverTile = tile;
                    SetTileStyle(hoverTile, TileStyle.Highlight);
                    break;
                case CellStatus.Void:
                    hoverTile = tile;
                    SetTileStyle(hoverTile, TileStyle.FrameOnly);
                    break;
                default:
                    break;
            }
            NotifyDrawing();
        }

        public void RemoveHighlightTile()
        {
            if (null == hoverTile) return;

            SetTileStyle(hoverTile);
            hoverTile = null;

            NotifyDrawing();
        }

        public void AddStone(Point point)
        {
            if (!IsInGame) return;

            var colIndex = (int)(point.X / CellWidth);
            var rowIndex = (int)(point.Y / CellHeight);

            AddStone(new BoardIndex(rowIndex, colIndex));
        }

        public void AddStone(BoardIndex index)
        {
            if (CellStatus.Available != Game.Board[index]) return;

            var turn = Game.CurrentTurn;
            Game.AddUserStone(index);
            AddStoneGeometry(index, turn);

            if (Game.Status.IsGameSet())
            {
                EndGame();
                return;
            }

            if (!Game.IsUserTurn)
            {
                ProcessCpuTurn();
            }
        }

        private void ProcessCpuTurn()
        {
            if (!Game.IsUserTurn)
            {
                var move = Game.AddCpuStone(Game.CurrentTurn);
                AddStoneGeometry(move.Index, move.Turn);

                if (Game.Status.IsGameSet())
                {
                    EndGame();
                    return;
                }
            }
        }

        private void AddStoneGeometry(BoardIndex index, GameUser turn)
        {
            var margin = 8.0;
            var geometryDrawing = new GeometryDrawing(
                null,
                null,
                new RectangleGeometry(new Rect(new Point(index.ColIndex * CellWidth + margin, index.RowIndex * CellHeight + margin), new Point((index.ColIndex + 1) * CellWidth - margin, (index.RowIndex + 1) * CellHeight - margin)), margin, margin)
            );
            SetStoneStyle(geometryDrawing, turn);

            stoneGeometryDrawing[index.RowIndex, index.ColIndex] = geometryDrawing;
            geometryDrawings.Add(geometryDrawing);

            SetTileStyle(hoverTile);
            hoverTile = null;

            NotifyDrawing();
        }

        private void SetStoneStyle(GeometryDrawing geometryDrawing, GameUser turn)
        {
            if (GameUser.First == turn)
            {
                geometryDrawing.Brush = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
                geometryDrawing.Pen = new Pen(Brushes.Black, 0.5);
            }
            else
            {
                geometryDrawing.Brush = new SolidColorBrush(Color.FromArgb(5, 0, 0, 0));
                geometryDrawing.Pen = new Pen(Brushes.Black, 0.5);
            }
        }

        private void SetTileStyle(GeometryDrawing geometryDrawing, TileStyle style = TileStyle.InVisible)
        {
            if (null == geometryDrawing) return;
            switch (style)
            {
                case TileStyle.InVisible:
                    geometryDrawing.Brush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 0));
                    geometryDrawing.Pen = new Pen(Brushes.Black, 0);
                    break;
                case TileStyle.FrameOnly:
                    geometryDrawing.Brush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 0));
                    geometryDrawing.Pen = new Pen(Brushes.Black, 0.15);
                    break;
                case TileStyle.Highlight:
                    geometryDrawing.Brush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 0));
                    geometryDrawing.Pen = new Pen(Brushes.Black, 0.15);
                    break;
            }
        }
        #endregion


        private class StartGameCommandImpl : ICommand
        {
            private readonly GameBoardViewModel viewModel;
            public event System.EventHandler CanExecuteChanged;

            public StartGameCommandImpl(GameBoardViewModel viewModel)
            {
                this.viewModel = viewModel;
            }

            public bool CanExecute(object parameter) => GameStatus.Setting == (viewModel.Game?.Status ?? GameStatus.Setting);

            public void Execute(object parameter)
            {
                if (!int.TryParse(viewModel.InputColSize.Trim(), out var colnum))
                {
                    MessageBox.Show("列数が無効です。数値で指定してください。", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (colnum <= 0 || 100 <= colnum)
                {
                    MessageBox.Show("列数が無効です。1 ～ 99の数値を指定してください。", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(viewModel.InputRowSize.Trim(), out var rownum))
                {
                    MessageBox.Show("行数が無効です。数値で指定してください。", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (rownum <= 0 || 100 <= rownum)
                {
                    MessageBox.Show("行数が無効です。1 ～ 99の数値を指定してください。", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var gameSetting = new GameSetting(rownum, colnum);
                viewModel.StartGame(gameSetting);

                viewModel.NotifyGameStatusChange();
            }

            public void NotifyGameStatusChange()
            {
                this.CanExecuteChanged?.Invoke(this, null);
            }
        }
    }
}
