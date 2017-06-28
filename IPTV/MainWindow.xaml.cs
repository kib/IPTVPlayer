using System.Windows;
using System.Windows.Input;

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
            // fill the list
            populateTreeView();
            vlcPlayer.LoadMedia(@"D:\VS_Projects\test.avi");
            // play video
            vlcPlayer.Play();
        }

        private void populateTreeView()
        {
            // populate the treeview here
        }

        private void cmFullScreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
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
            vlcPlayer.Stop();
            vlcPlayer.Dispose();
            Close();
        }

        private void cmItem1_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}