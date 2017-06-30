using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using IPTV.TreeView;
using System.Windows.Controls;

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
            populateTreeView();
            populateRMBMenu();
            loadSettings();
        }

        // handling interface clicks
        private void cmFullScreen_Click(object sender, RoutedEventArgs e)
        {
            switchFullscreen();
            e.Handled = true;
        }
        private void vlcPlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switchFullscreen();
            e.Handled = true;
        }
        private void cmExit_Click(object sender, RoutedEventArgs e)
        {
            vidPlayer.Stop();
            vidPlayer.Dispose();
            Properties.Settings.Default.Save();
            e.Handled = true;
            Close();
        }
        private void ChannelView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Channel clicked = new Channel();
            try
            {
                clicked = (Channel)ChannelView.SelectedItem;
            }
            catch (InvalidCastException) {}

            if (clicked.Type == "Radio")
            {
                vidPlayer.Stop();
            }

            if (clicked.URL != null)
            {
                playMedia(new Uri(clicked.URL));
            }
            
            e.Handled = true;

            /* some debugging, remove later
            Console.Write("\r\n");
            Console.Write("vidPlayer.ActualHeight = " + vidPlayer.ActualHeight + " <\r\n" );
            Console.Write("vidPlayer.ActualWidth = " + vidPlayer.ActualWidth + " <\r\n");
            Console.Write("vidPlayer.RenderSize = " + vidPlayer.RenderSize + " <\r\n");
            Console.Write("\r\n");
            */
        }

        // media playback
        private void playMedia(Uri url)
        {
            vidPlayer.LoadMedia(url);
            storeRecentlyPlayed(url.ToString());
            vidPlayer.Play();
        }
        private void loadSettings()
        {
            Properties.Settings.Default.Reload();
            
            if (Properties.Settings.Default.ShowPanel == false)
            {
                ChannelView.Visibility = Visibility.Collapsed;
            }

            if (Properties.Settings.Default.FullScreen)
            {
                switchFullscreen();
            }
            
            if (Properties.Settings.Default.LastPlayed != "")
            {
                Console.Write("\r\n" + Properties.Settings.Default.LastPlayed);
                playMedia(new Uri(Properties.Settings.Default.LastPlayed));
            }
            else
            {
                // if nothing was stored, play the first channel
                playMedia(new Uri("udp://@239.1.1.1:8000"));
            }
        }

        // settings and interface
        private void storeRecentlyPlayed(String url)
        {
            String uri = url;
            Properties.Settings.Default.LastPlayed = uri.ToString();
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void storePanelShownPref(bool panelshown)
        {
            Properties.Settings.Default.ShowPanel = panelshown;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void storeFullScreenPref(bool fullscreen)
        {
            Properties.Settings.Default.FullScreen = fullscreen;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void switchFullscreen()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                cmFullScreen.Header = "Fill Screen";
                storeFullScreenPref(false);
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                cmFullScreen.Header = "Windowed Mode";
                storeFullScreenPref(true);
            }
        }

        // populate the interface
        private void populateTreeView()
        {
            // populate the treeview here
            List<Category> cats = new List<Category>();

            Category c1 = new Category() { Name = "ICC" };
            Category c2 = new Category() { Name = "TV" };
            Category c3 = new Category() { Name = "Radio" };

            cats.Add(c1);
            cats.Add(c2);
            cats.Add(c3);

            // ICC category
            c1.Items.Add(new Channel() { Lcn = 1, ID = 1, Name = "ICC CR1 EN", URL = "udp://@239.1.1.1:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 2, ID = 2, Name = "ICC CR1 FR", URL = "udp://@239.1.1.2:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 3, ID = 3, Name = "ICC CR2 EN", URL = "udp://@239.1.1.3:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 4, ID = 4, Name = "ICC CR2 FR", URL = "udp://@239.1.1.4:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 5, ID = 5, Name = "ICC CR3 EN", URL = "udp://@239.1.1.5:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 6, ID = 6, Name = "ICC CR3 FR", URL = "udp://@239.1.1.6:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 7, ID = 7, Name = "ICC Media Room EN", URL = "udp://@239.1.1.7:8000", Type = "ICC" });
            c1.Items.Add(new Channel() { Lcn = 8, ID = 8, Name = "ICC Media Room EN", URL = "udp://@239.1.1.8:8000", Type = "ICC" });
            // TV category

            c2.Items.Add(new Channel() { Lcn = 9, ID = 9, Name = "NPO1", URL = "udp://@224.0.251.1:8002", Type = "TV" });
            c2.Items.Add(new Channel() { Lcn = 10, ID = 10, Name = "NPO2", URL = "udp://@224.0.251.2:8004", Type = "TV" });
            c2.Items.Add(new Channel() { Lcn = 11, ID = 11, Name = "NPO3", URL = "udp://@224.0.251.3:8006", Type = "TV" });
            c2.Items.Add(new Channel() { Lcn = 12, ID = 12, Name = "RTL4", URL = "udp://@224.0.251.4:8008", Type = "TV" });
            c2.Items.Add(new Channel() { Lcn = 13, ID = 13, Name = "RTL5", URL = "udp://@224.0.251.5:8010", Type = "TV" });
            c2.Items.Add(new Channel() { Lcn = 14, ID = 14, Name = "SBS6", URL = "udp://@224.0.251.6:8012", Type = "TV" });

            // Radio category
            c3.Items.Add(new Channel() { Lcn = 15, ID = 15, Name = "NPO 3FM", URL = "udp://@224.0.251.163:8326", Type = "Radio" });
            c3.Items.Add(new Channel() { Lcn = 16, ID = 16, Name = "Radio538", URL = "udp://@224.0.251.169:8338", Type = "Radio" });
            c3.Items.Add(new Channel() { Lcn = 17, ID = 17, Name = "Radio Veronica", URL = "udp://@224.0.251.239:8478", Type = "Radio" });
            c3.Items.Add(new Channel() { Lcn = 18, ID = 18, Name = "Slam! FM", URL = "udp://@224.0.251.176:8352", Type = "Radio" });

            // populate the treeview with the list of channelgroups
            ChannelView.ItemsSource = cats;
        }
        private void populateRMBMenu()
        {
            // add a menu item programmatically
            MenuItem menu = (MenuItem)this.rmMenu.Items[3]; // ICC item
            MenuItem newitem = new MenuItem();
            newitem.Header = "ICC CR1 EN";
            newitem.Click += (sender, e) => playMedia(new Uri("udp://@239.1.1.1:8000"));
            menu.Items.Add(newitem);
        }

        private void cmDock_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelView.Visibility == Visibility.Collapsed)
            {

                ChannelView.Visibility = Visibility.Visible;
                cmDock.Header = "Hide Channel List";
                storePanelShownPref(true);
            }
            else if (ChannelView.Visibility == Visibility.Visible)
            {
                ChannelView.Visibility = Visibility.Collapsed;
                cmDock.Header = "Show Channel List";
                storePanelShownPref(false);
            }
            
            e.Handled = true;
        }
    }
}