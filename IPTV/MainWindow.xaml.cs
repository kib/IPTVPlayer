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

namespace IPTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            vlcPlayer.LoadMedia(@"D:\VS_Projects\test.avi");
            // play video
            vlcPlayer.Play();
        }

        private void cmFullScreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }

        private void cmItem1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void vlcPlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
        }

        private void cmItem3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmItem2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
