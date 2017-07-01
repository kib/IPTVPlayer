using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using IPTV.TreeView;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

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
            populateInterface();
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

            /* interesting properties to resize window with later
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
                //playMedia(new Uri("udp://@239.1.1.1:8000"));
                playMedia(new Uri("D:\\VS_Projects\\test.avi"));
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
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                cmFullScreen.Header = "Fill Screen";
                changeMenuIcon(cmFullScreen, "Resources/larger.png");
                storeFullScreenPref(false);
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                cmFullScreen.Header = "Windowed Mode";
                changeMenuIcon(cmFullScreen, "Resources/smaller.png");
                storeFullScreenPref(true);
            }
        }

        private void changeMenuIcon(MenuItem menu, String IconResource)
        {
            menu.Icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri(IconResource, UriKind.Relative))
            };
        }


        private IEnumerable<string> EnumerateLines(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        string[] ReadAllResourceLines(byte[] resourceData)
        {
            using (Stream stream = new MemoryStream(resourceData))
            using (StreamReader reader = new StreamReader(stream))
            {
                return EnumerateLines(reader).ToArray();
            }
        }

        private List<Channel> importChannelList()
        {
            string[] allLines = ReadAllResourceLines(IPTV.Properties.Resources.ch);

            List<Channel> chanimport = allLines
                .Select(v => Channel.FromCSV(v))
                .ToList();
            return chanimport;
         
        }

        // populate the interface
        private void populateInterface()
        {
            List<Channel> chanimport = importChannelList();

            Category c1 = new Category() { Name = "ICC" };
            Category c2 = new Category() { Name = "TV" };
            Category c3 = new Category() { Name = "Radio" };
            
                        foreach (Channel ch in chanimport)
                {
                    switch (ch.Type)
                    {
                        case "ICC":
                            c1.Items.Add(ch);
                            addRMBItem(ch);
                            break;
                        case "TV":
                            c2.Items.Add(ch);
                            addRMBItem(ch); 
                            break;
                        case "Radio":
                            c3.Items.Add(ch);
                            addRMBItem(ch);
                            break;
                    }
                }
            // populate the treeview here
            List<Category> cats = new List<Category>();
            cats.Add(c1);
            cats.Add(c2);
            cats.Add(c3);
            ChannelView.ItemsSource = cats;
        }
        private void addRMBItem(Channel ch)
        {
            MenuItem menu = new MenuItem();
            switch (ch.Type)
            {
                case "ICC":
                    menu = (MenuItem)this.rmMenu.Items[3];
                    break;
                case "TV":
                    menu = (MenuItem)this.rmMenu.Items[4];
                    break;
                case "Radio":
                    menu = (MenuItem)this.rmMenu.Items[5];
                    break;
            }
            MenuItem newitem = new MenuItem();
            newitem.Header = ch.Lcn + ". " + ch.Name;
            newitem.Click += (sender, e) => playMedia(new Uri(ch.URL));
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