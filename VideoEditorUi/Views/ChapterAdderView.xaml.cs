using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPFVideoPlayer;
using MVVMFramework.ViewNavigator;
using MVVMFramework.Views;
using VideoEditorUi.ViewModels;
using static VideoEditorUi.Utilities.UtilityClass;

namespace VideoEditorUi.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ChapterAdderView : ViewBaseControl
    {
        private Thumb _thumb;
        private Thumb thumb => _thumb ?? (_thumb = (slider.Template.FindName("PART_Track", slider) as Track)?.Thumb);
        private readonly DispatcherTimer timer;
        private bool isDragging;
        private readonly ChapterAdderViewModel viewModel;

        public ChapterAdderView() : base(Navigator.Instance.CurrentViewModel)
        {
            InitializeComponent();
            viewModel = Navigator.Instance.CurrentViewModel as ChapterAdderViewModel;
            viewModel.Slider = slider;
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += timer_Tick;
            //InitializePlayer(player);
            viewModel.Player = player;
            player.MediaOpened += Player_MediaOpened;
            player.MediaClosed += Player_MediaClosed;
            slider.ApplyTemplate();
            thumb.MouseEnter += Thumb_MouseEnter;
        }

        public override void ViewBaseControl_Unloaded(object sender, RoutedEventArgs e)
        {
            player.MediaOpened -= Player_MediaOpened;
            player.MediaClosed -= Player_MediaClosed;
            timer.Tick -= timer_Tick;
            thumb.MouseEnter -= Thumb_MouseEnter;
            base.ViewBaseControl_Unloaded(sender, e);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
                slider.Value = player.CurrentTime.TotalMilliseconds;
        }

        private void slider_DragStarted(object sender, DragStartedEventArgs e) => isDragging = true;

        private void slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            SetPlayerPosition(player, slider.Value);
            viewModel.PositionChanged?.Invoke(player.CurrentTime);
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            var ts = player.Duration;
            slider.Maximum = ts.TotalMilliseconds;
            slider.SmallChange = 1;
            slider.LargeChange = Math.Min(10, ts.Milliseconds / 10);
            timer.Start();
        }

        private void Player_MediaClosed(object sender, EventArgs e) => player.Close();

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetPlayerPosition(player, slider.Value);
                viewModel.PositionChanged?.Invoke(player.CurrentTime);
            }
        }

        private void Grid_OnDrop(object sender, DragEventArgs e) => ControlMethods.ImagePanel_Drop(e, viewModel.DragFiles);
        private void OnDragEnter(object sender, DragEventArgs e) => ControlMethods.SetBackgroundBrush(sender as Label, false);

        private void OnPreviewDragLeave(object sender, DragEventArgs e) => ControlMethods.SetBackgroundBrush(sender as Label, true);
    }
}
