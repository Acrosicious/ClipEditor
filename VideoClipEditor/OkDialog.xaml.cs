using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace VideoClipEditor
{
    /// <summary>
    /// Interaktionslogik für OkDialog.xaml
    /// </summary>
    public partial class OkDialog
    {

        private string file;

        public OkDialog(string file)
        {
            InitializeComponent();

            this.file = file;
            this.Owner = App.Current.MainWindow;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowFileBtnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(file))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/Select,\"" + file + "\"");
            }

            this.DialogResult = true;
            this.Close();
        }

        private void UploadBtnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(file))
            {
                GfycatService.UploadVideo(file);
                Console.WriteLine("Done");
            }
            else
            {
                Console.WriteLine("File not found!");
            }
        }
    }
}
