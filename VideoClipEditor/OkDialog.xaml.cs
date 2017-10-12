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
            this.file = file;

            InitializeComponent();
            this.Owner = App.Current.MainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists(file))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/Select,\"" + file + "\"");
            }

            this.Close();
        }
    }
}
