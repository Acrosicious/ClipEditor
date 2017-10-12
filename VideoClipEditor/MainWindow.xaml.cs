using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Windows.Media.Animation;
using MediaToolkit;
using MediaToolkit.Options;
using System.Collections;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace VideoClipEditor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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

        private string currentFile;
        private bool editingEnabled = false;

        private void LoadVideo(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            dialog.DefaultExt = ".WMV"; // Default file extension 
            dialog.Filter = "MP4 file (.mp4)|*.mp4"; // Filter files by extension  

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                editingEnabled = false;
                currentFile = dialog.FileName;

                // Open document  
                var timeline = new MediaTimeline(new Uri(dialog.FileName));
                mediaPlayer.Clock = timeline.CreateClock(true) as MediaClock;
                //mediaPlayer.Source = new Uri(dialog.FileName);
            }
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
                Console.WriteLine("Time is: " + mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
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

        private void SeekToLowerValue()
        {
            if (editingEnabled)
                mediaPlayer.Clock.Controller.Seek(TimeSpan.FromSeconds(rangeSlider.LowerValue), TimeSeekOrigin.BeginTime);
        }

        private void UpperValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //SeekToLowerValue();
            //Console.WriteLine(rangeSlider.MinRange + " " + rangeSlider.MinRangeWidth);
        }

        private void LowerValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //SeekToLowerValue();
        }

        private void SeekToValue(int v)
        {
            if (editingEnabled)
                mediaPlayer.Clock.Controller.Seek(TimeSpan.FromSeconds(v), TimeSeekOrigin.BeginTime);
        }

        private async void SaveWebm(object sender, RoutedEventArgs e)
        {
            if (!editingEnabled)
                return;

            editingEnabled = false;



            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "WEBM file (.webm)|*.webm";

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                mediaPlayer.Pause();
                mediaPlayer.Stop();
                progressBar.Value = 0;
                await SaveWebmTask(dialog.FileName, (int)rangeSlider.LowerValue,
                    (int)(rangeSlider.UpperValue - rangeSlider.LowerValue), mediaPlayer.IsMuted);
                progressBar.Value = 100;
            }


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
                string command = "-ss " + start + " -i \""
                    + currentFile + "\" -t "
                    + length
                    + " -c:v libvpx -crf 4 -b:v 1500K -vf scale=1280:-1 "
                    + (isMuted ? "-an " : "")
                    + "\"" + FileName + "\"";

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

        private void ConvertCompletedEvent(object sender, ConversionCompleteEventArgs e)
        {
            convertDone = true;
            var dialog = new OkDialog();
            //dialog.ShowDialog();
            MessageBox.Show(window, "Clip succesfully converted!", "Done");
        }

        private void ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            progress = (int)(100.0 * e.ProcessedDuration.TotalMilliseconds / clipLength);
        }

        private void StartUpdatingProgress()
        {
            while (!convertDone)
            {
                this.Dispatcher.Invoke(() => progressBar.Value = progress);
                Thread.Sleep(250);
            }
        }

        private double clipLength = 0f;
        private int progress = 0;
        private bool convertDone;
        private bool updateSlider = true;
        private DateTime lastUpdate;

        public bool ClipLooped { get; set; }

        private void videoTimeSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            loopClipCheckbox.IsChecked = false;
            updateSlider = false;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConvertCompletedEvent(null, null);
        }
    }
}
