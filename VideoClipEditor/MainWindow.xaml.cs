using MediaToolkit;
using MediaToolkit.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VideoClipEditor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private double clipLength = 0f;
        private bool convertDone;
        private string currentFile;
        private string saveFileLocation;
        private bool editingEnabled = false;
        private DateTime lastUpdate;
        private int progress = 0;
        private bool updateSlider = true;
        public bool ClipLooped { get; set; }

        static readonly string[] video_file_formats = new string[] { ".mp4" };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // Stellt sicher, dass Template besetzt
            videoTimeSlider.ApplyTemplate();

            // Thumb ist child vom Track "PART_Track" im Objects view des Sliders
            var thumb = (videoTimeSlider.Template.FindName("PART_Track", videoTimeSlider) as Track).Thumb;
            thumb.MouseEnter += new MouseEventHandler(videoTimeSlider_ThumbMouseEnter);
        }

        private void ConvertCompletedEvent(object sender, ConversionCompleteEventArgs e)
        {
            convertDone = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new OkDialog(saveFileLocation);
                if (dialog.ShowDialog() == true)
                {
                    Console.WriteLine("Done");
                }
            });

            //dialog.ShowDialog();
            //MessageBox.Show(window, "Clip succesfully converted!", "Done");
        }

        private void ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            progress = (int)(100.0 * e.ProcessedDuration.TotalMilliseconds / clipLength);
        }

        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name
            dialog.DefaultExt = ".WMV"; // Default file extension
            dialog.Filter = "MP4 file (.mp4)|*.mp4"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                OpenFile(dialog.FileName);
            }
        }

        private void OpenFile(string filepath)
        {
            editingEnabled = false;
            currentFile = filepath;

            // Open document
            var timeline = new MediaTimeline(new Uri(filepath));
            mediaPlayer.Clock = timeline.CreateClock(true) as MediaClock;
            PlayVideo();
            //mediaPlayer.Source = new Uri(dialog.FileName);
        }

        private void LowerValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //SeekToLowerValue();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ConvertCompletedEvent(null, null);
        }

        private async void SaveWebm_Click(object sender, RoutedEventArgs e)
        {
            if (!editingEnabled)
                return;

            editingEnabled = false;

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "WEBM file (.webm)|*.webm";

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                PauseVideo();
                progressBar.Value = 0;
                saveFileLocation = dialog.FileName;
                await SaveWebmTask(dialog.FileName, (int)rangeSlider.LowerValue,
                    (int)(rangeSlider.UpperValue - rangeSlider.LowerValue), mediaPlayer.IsMuted);
                progressBar.Value = 100;
            }

            editingEnabled = true;
        }

        private Task SaveWebmTask(string FileName, int start, int length, bool isMuted)
        {
            return Task.Run(() =>
            {
                var engine = new Engine();

                new MediaToolkit.Options.ConversionOptions
                {
                    VideoSize = VideoSize.Hd720,
                };

                string command = VideoEditorSettings.currentQuality.command(start, currentFile, length, isMuted, FileName);

                clipLength = length * 1000;
                Console.WriteLine(command);

                convertDone = false;
                Thread t = new Thread(() => StartUpdatingProgress());
                t.Start();

                engine.ConvertProgressEvent += ConvertProgressEvent;
                engine.ConversionCompleteEvent += ConvertCompletedEvent;
                engine.CustomCommand(command);
            });
        }

        private void SeekToLowerValue()
        {
            if (editingEnabled)
                mediaPlayer.Clock.Controller.Seek(TimeSpan.FromSeconds(rangeSlider.LowerValue), TimeSeekOrigin.BeginTime);
        }

        private void SeekToValue(int v)
        {
            if (editingEnabled)
                mediaPlayer.Clock.Controller.Seek(TimeSpan.FromSeconds(v), TimeSeekOrigin.BeginTime);
        }

        private void StartUpdatingProgress()
        {
            while (!convertDone)
            {
                this.Dispatcher.Invoke(() => progressBar.Value = progress);
                Thread.Sleep(250);
            }
        }

        private void TimeChanged(object sender, EventArgs e)
        {
            if (!mediaPlayer.IsLoaded || !mediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            timeLabel.Content = "Clip length: " + (rangeSlider.UpperValue - rangeSlider.LowerValue) + "s  |  " +
                mediaPlayer.Position.Seconds + " : " + mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

            if (!updateSlider)
                return;

            videoTimeSlider.Value = mediaPlayer.Position.TotalMilliseconds / 10.0;

            if (ClipLooped && (mediaPlayer.Position.Seconds >= rangeSlider.UpperValue || mediaPlayer.Position.Seconds < rangeSlider.LowerValue))
            {
                SeekToLowerValue();
            }
        }

        private void UpperValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //SeekToLowerValue();
            //Console.WriteLine(rangeSlider.MinRange + " " + rangeSlider.MinRangeWidth);
        }

        private void VideoOpened(object sender, RoutedEventArgs e)
        {
            editingEnabled = true;
            progressBar.Value = 100;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                rangeSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                rangeSlider.LowerValue = 0;
                rangeSlider.UpperValue = rangeSlider.Maximum;
                videoTimeSlider.Minimum = 0;
                videoTimeSlider.Maximum = rangeSlider.Maximum * 100;

                mediaPlayer.Clock.CurrentTimeInvalidated += TimeChanged;
                mediaPlayer.Clock.Completed += VideoEnded;
                Console.WriteLine("Time is: " + mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            }
        }

        private void VideoEnded(object sender, EventArgs e)
        {
            SeekToValue(0);
        }

        private void videoTimeSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SeekToValue((int)(videoTimeSlider.Value / 100.0));
            updateSlider = true;
        }

        private void videoTimeSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = DateTime.Now;
            if ((t - lastUpdate).TotalMilliseconds > 100)
            {
                Console.WriteLine(sender);
                SeekToValue((int)(videoTimeSlider.Value / 100.0));
                lastUpdate = t;
            }
        }

        private void videoTimeSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            //loopClipCheckbox.IsChecked = false;
            updateSlider = false;
        }

        private void videoTimeSlider_ThumbMouseEnter(object sender, MouseEventArgs e)
        {
            // Links gedrückt und Maus nicht vorher gefangen
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                args.RoutedEvent = MouseLeftButtonDownEvent;
                (sender as Thumb).RaiseEvent(args);

                SeekToValue((int)(videoTimeSlider.Value / 100.0));
            }
        }

        private void PauseVideo()
        {
            mediaPlayer.Clock.Controller.Pause();
            VisualStateManager.GoToState(playPauseButton, "Paused", true);
        }

        private void PlayVideo()
        {
            mediaPlayer.Clock.Controller.Resume();
            VisualStateManager.GoToState(playPauseButton, "Playing", true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Clock.IsPaused)
                PlayVideo();
            else
                PauseVideo();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                if (video_file_formats.Where(x => files[0].ToLower().EndsWith(x)).Count() > 0)
                {
                    OpenFile(files[0]);
                }
            }
        }

        private void QualitySelectionChanged(object sender, RoutedEventArgs e)
        {
            var cmd = (string)((RadioButton)sender).CommandParameter;
            foreach(var quality in VideoEditorSettings.videoQualityCommands)
            {
                if(quality.commandLabel == cmd)
                {
                    VideoEditorSettings.currentQuality = quality;
                }
            }
        }
    }
}