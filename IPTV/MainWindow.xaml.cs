using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using IPTV.Objects;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace IPTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public const int APPCOMMAND_VOLUME_UP = 0xA0000;
        public const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        public const int WM_APPCOMMAND = 0x319;

        public MainWindow()
        {
            InitializeComponent();
            populateInterface();
            loadSettings();
        }

        // clicks
        private void cmFullScreen_Click(object sender, RoutedEventArgs e)
        {
            switchState_Fullscreen();
            e.Handled = true;
        }

        private void cmExit_Click(object sender, RoutedEventArgs e)
        {
            applicationQuit();
        }

        private void cmChannels_Click(object sender, RoutedEventArgs e)
        {
            switchState_ChannelList();
            e.Handled = true;
        }

        private void cmMute_Click(object sender, RoutedEventArgs e)
        {
            switchState_Muted();
            e.Handled = true;
        }

        private void vidPlayer_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            switchState_Fullscreen();
            e.Handled = true;
        }

        private void ChannelView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Channel clicked = new Channel();
            try
            {
                clicked = (Channel)ChannelView.SelectedItem;
            }
            catch (InvalidCastException) { }

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

        // keypresses
        private void dpMain_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.C:
                    switchState_ChannelList();
                    break;
                case Key.L:
                    switchState_ChannelList();
                    break;
                case Key.F:
                    switchState_Fullscreen();
                    break;
                case Key.M:
                    switchState_Muted();
                    break;
                case Key.Q:
                    applicationQuit();
                    break;
                case Key.OemPlus:
                    Audio_VolUp();
                    break;
                case Key.Add:
                    Audio_VolUp();
                    break;
                case Key.OemMinus:
                    Audio_VolDown();
                    break;
                case Key.Subtract:
                    Audio_VolDown();
                    break;
            }
        }
     
        // audio and video
        private void playMedia(Uri url)
        {
            vidPlayer.LoadMedia(url);
            storePref_RecentlyPlayed(url.ToString());
            vidPlayer.Play();
        }

        private void Audio_VolDown()
        {
            SendMessageW(new WindowInteropHelper(this).Handle, WM_APPCOMMAND, new WindowInteropHelper(this).Handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void Audio_VolUp()
        {
            SendMessageW(new WindowInteropHelper(this).Handle, WM_APPCOMMAND, new WindowInteropHelper(this).Handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        // settings and interface
        private void loadSettings()
        {
            Properties.Settings.Default.Reload();

            if (Properties.Settings.Default.ShowPanel)
            {
                if (ChannelView.Visibility == Visibility.Collapsed)
                {
                    switchState_ChannelList();
                }
            } else if (Properties.Settings.Default.ShowPanel == false)
            {
                if (ChannelView.Visibility == Visibility.Visible)
                {
                    switchState_ChannelList();
                }
            }

            if (Properties.Settings.Default.FullScreen)
            {
                switchState_Fullscreen();
            }

            if (Properties.Settings.Default.LastPlayed != "")
            {
                playMedia(new Uri(Properties.Settings.Default.LastPlayed));
            }
            else
            {
                // if nothing was stored, play the first channel
                //playMedia(new Uri("udp://@239.1.1.1:8000"));
                playMedia(new Uri("D:\\VS_Projects\\test.avi"));
            }
        }

        private void storePref_RecentlyPlayed(String url)
        {
            String uri = url;
            Properties.Settings.Default.LastPlayed = uri.ToString();
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void storePref_PanelShown(bool panelshown)
        {
            Properties.Settings.Default.ShowPanel = panelshown;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void storePref_FullScreen(bool fullscreen)
        {
            Properties.Settings.Default.FullScreen = fullscreen;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
        }

        private void switchState_Fullscreen()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                changeMenuItem(cmFullScreen, "FullScreen" ,"Resources/larger.png");
                storePref_FullScreen(false);
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                changeMenuItem(cmFullScreen, "Windowed", "Resources/smaller.png");
                storePref_FullScreen(true);
            }
        }

        private void switchState_ChannelList()
        {
            if (ChannelView.Visibility == Visibility.Collapsed)
            {
                changeMenuItem(cmChannels, "Hide Channels", "Resources/onepanel.png");
                ChannelView.Visibility = Visibility.Visible;
                storePref_PanelShown(true);
            }
            else if (ChannelView.Visibility == Visibility.Visible)
            {
                changeMenuItem(cmChannels, "Show Channels", "Resources/twopanel.png");
                ChannelView.Visibility = Visibility.Collapsed;
                storePref_PanelShown(false);
            }
        }

        private void switchState_Muted()
        {
            vidPlayer.ToggleMute();
            if (vidPlayer.IsMute)
            {
                changeMenuItem(cmMute, "Unmute", "Resources/unmuted.png");
            }
            else
            {
                changeMenuItem(cmMute, "Mute", "Resources/muted.png");
            }
        }

        private void changeMenuItem(MenuItem menu, String header, String IconResource)
        {
            menu.Icon = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri(IconResource, UriKind.Relative))
            };
            menu.Header = header;
        }

        // read channels
        private IEnumerable<string> EnumerateLines(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        private string[] ReadAllResourceLines(byte[] resourceData)
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
                    menu = (MenuItem)this.rmMenu.Items[4];
                    break;
                case "TV":
                    menu = (MenuItem)this.rmMenu.Items[5];
                    break;
                case "Radio":
                    menu = (MenuItem)this.rmMenu.Items[6];
                    break;
            }
            MenuItem newitem = new MenuItem();
            newitem.Header = ch.Name;
            newitem.InputGestureText = ch.Lcn.ToString();
            newitem.Click += (sender, e) => playMedia(new Uri(ch.URL));
            menu.Items.Add(newitem);
        }

        private void applicationQuit()
        {
            vidPlayer.Stop();
            vidPlayer.Dispose();
            Properties.Settings.Default.Save();
            Close();
        }

    }
}
