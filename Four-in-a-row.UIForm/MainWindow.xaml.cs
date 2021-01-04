using Four_in_a_row.UIForm.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Four_in_a_row.UIForm
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameBoardViewModel ViewModel => this.DataContext as GameBoardViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_LayoutUpdated(object sender, System.EventArgs e) => SetBorderLayout(sender);
        private void Border_Loaded(object sender, RoutedEventArgs e) => SetBorderLayout(sender);

        private void SetBorderLayout(object sender)
        {
            var frame = sender as FrameworkElement;

            if (null == ViewModel || null == frame) return;

            ViewModel.DrawingWidth = frame.ActualWidth;
            ViewModel.DrawingHeight = frame.ActualHeight;
        }

        private void image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ViewModel?.SetHighlightTile(e.GetPosition(sender as Image));
        }

        private void image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ViewModel?.RemoveHighlightTile();
        }

        private void image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel?.AddStone(e.GetPosition(sender as Image));
        }
    }
}
