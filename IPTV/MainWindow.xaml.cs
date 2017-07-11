using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using IPTV.Objects;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AudioSwitcher.AudioApi.CoreAudio;
using IPTV.WindowPlacement;
using System.ComponentModel;

namespace IPTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private DispatcherTimer cpTimer; // currently playing timer
        private DispatcherTimer biTimer; // buffered input timer
        private String BufferedInput;
        private List<Channel> chanimport;
        private CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        public Channel currentChannel;

        private static double TVRATIO = 0.5515;
        private static double RATIO =  0.5765625;

        private static readonly IDictionary<Key, String> NumericKeys = new Dictionary<Key, String> {
            { Key.D0, "0" },
            { Key.D1, "1" },
            { Key.D2, "2" },
            { Key.D3, "3" },
            { Key.D4, "4" },
            { Key.D5, "5" },
            { Key.D6, "6" },
            { Key.D7, "7" },
            { Key.D8, "8" },
            { Key.D9, "9" },
            { Key.NumPad0, "0" },
            { Key.NumPad1, "1" },
            { Key.NumPad2, "2" },
            { Key.NumPad3, "3" },
            { Key.NumPad4, "4" },
            { Key.NumPad5, "5" },
            { Key.NumPad6, "6" },
            { Key.NumPad7, "7" },
            { Key.NumPad8, "8" },
            { Key.NumPad9, "9" }
        };

        public MainWindow()
        {
            InitializeComponent();
            populateInterface();
            createCurrentlyPlayingTimer();
            createBufferedInputTimer();
            loadSettings();
        }

        private void createCurrentlyPlayingTimer()
        {
            cpTimer = new DispatcherTimer();
            cpTimer.Tick += new EventHandler(cpTimer_Tick);
            cpTimer.Interval = new TimeSpan(0, 0, 8);
        }

        private void createBufferedInputTimer()
        {
            BufferedInput = "";
            biTimer = new DispatcherTimer();
            biTimer.Tick += new EventHandler(biTimer_Tick);
            biTimer.Interval = new TimeSpan(0, 0, 2);
        }

        private void biTimer_Tick(object sender, EventArgs e)
        {
            jumpBufferedImput();
            BufferedInputLabel.Visibility = Visibility.Collapsed;
            BufferedInput = "";
            biTimer.IsEnabled = false;
        }
        
        private void cpTimer_Tick(object sender, EventArgs e)
        {
            CurrentlyPlayingLabel.Visibility = Visibility.Collapsed;
            cpTimer.IsEnabled = false;
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
            e.Handled = true;
        }

        private void cmChannels_Click(object sender, RoutedEventArgs e)
        {
            switchState_ChannelList();
            e.Handled = true;
        }

        private void cmMute_Click(object sender, RoutedEventArgs e)
        {
            switchState_Muted(!vidPlayer.IsMute);
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
            } catch (InvalidCastException) { }

            if (clicked.URL != null)
            {
                playMedia(clicked);
            }
            e.Handled = true;
        }

        private void cmAlwaysonTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            cmAlwaysOnTop.IsChecked = this.Topmost;
            e.Handled = true;
        }

        // Handle keypresses
        private void dpMain_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
            e.Handled = true;
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
                    switchState_Muted(!vidPlayer.IsMute);
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
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    addBufferedInput(NumericKeys[e.Key]);
                    break;
                case Key.Enter:
                    jumpBufferedImput();
                    break;
            }
            e.Handled = true;
        }

        private void jumpBufferedImput()
        {
            Channel biChannel = chanimport.FirstOrDefault(ch => ch.Lcn == Convert.ToInt32(BufferedInput));
            if (biChannel != null)
            {
                playMedia(biChannel);
                BufferedInputLabel.Content = BufferedInput + ": " + biChannel.Name + " ";
            }
        }

        private void addBufferedInput(string v)
        {
            biTimer.Stop();
            BufferedInput += v;
            BufferedInputLabel.Content = "" + BufferedInput + ": ";
            BufferedInputLabel.Visibility = Visibility.Visible;
            biTimer.Start();
        }

        // audio and video
        private void playMedia(Channel ch)
        {
            storePref_RecentlyPlayed(ch);
            currentChannel = ch;
            updateCurrentPlayingLabel(currentChannel);
            vidPlayer.LoadMedia(new Uri(ch.URL));
            if (ch.Type == "Radio")
            {
                vidPlayer.Stop();
            }
            vidPlayer.Play();
            Resize_WindowtoChannel(ch);
        }

        private void updateCurrentPlayingLabel(Channel ch)
        {
            cpTimer.Stop();
            if (ch.Type != "Radio")
            {
                RadioCenterIcon.Visibility = Visibility.Collapsed;
                cpTimer.Start();
            } else
            {
                RadioCenterIcon.Visibility = Visibility.Visible;
            }
            
            CurrentlyPlayingLabel.Content = "Volume: " + defaultPlaybackDevice.Volume.ToString() + "% | Channel: " + ch.Name + " (ch. " + ch.Lcn + ")";
            CurrentlyPlayingLabel.Visibility = Visibility.Visible;
        }

        private void Audio_VolDown()
        {
            double vol = defaultPlaybackDevice.Volume;
            if ( vol > 0)
            {
                defaultPlaybackDevice.Volume = vol-1;
                updateCurrentPlayingLabel(currentChannel);
            }
            if (vol == 1)
            {
                switchState_Muted(true);
            }
          
        }

        private void Audio_VolUp()
        {
            double vol = defaultPlaybackDevice.Volume;
            if (vidPlayer.IsMute)
            {
                switchState_Muted(false);
            }
            if (vol < 100)
            {
                defaultPlaybackDevice.Volume = vol + 1;
                updateCurrentPlayingLabel(currentChannel);
            }
        }

        // settings and interface
        private void loadSettings()
        {
            Properties.Settings.Default.Reload();

            if (Properties.Settings.Default.ShowPanel == true)
            {
                cmChannels.IsChecked = true;
                if (ChannelView.Visibility == Visibility.Collapsed)
                {
                    switchState_ChannelList();
                }
            } else if (Properties.Settings.Default.ShowPanel == false)
            {
                cmChannels.IsChecked = false;
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
                Channel ch = new Channel();
                ch.Lcn = Properties.Settings.Default.LastPlayedNum;
                ch.Name = Properties.Settings.Default.LastPlayedName;
                ch.Type = Properties.Settings.Default.LastPlayedType;
                ch.URL = Properties.Settings.Default.LastPlayed;
                playMedia(ch);
            }
            else
            {
                // if nothing was stored, play the first channel
                Channel ch = new Channel();
                ch.Lcn = 1;
                ch.Name = "ICC CR1 EN";
                ch.Type = "ICC";
                ch.URL = "udp://@239.1.1.1:8000";
                playMedia(ch);
            }
        }

        private void storePref_RecentlyPlayed(Channel ch)
        {
            Properties.Settings.Default.LastPlayed = ch.URL;
            Properties.Settings.Default.LastPlayedName = ch.Name;
            Properties.Settings.Default.LastPlayedType = ch.Type;
            Properties.Settings.Default.LastPlayedNum = ch.Lcn;
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
            Channel ch = currentChannel;
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                WindowState = WindowState.Normal;
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
                cmChannels.IsChecked = true;
                channelColumn.Width = new GridLength(220);
                ChannelView.Visibility = Visibility.Visible;
                storePref_PanelShown(true);
                Application.Current.MainWindow.Width = Application.Current.MainWindow.Width + 220;
            }
            else if (ChannelView.Visibility == Visibility.Visible)
            {
                cmChannels.IsChecked = false;
                channelColumn.Width = new GridLength(0);
                ChannelView.Visibility = Visibility.Collapsed;
                storePref_PanelShown(false);
                Application.Current.MainWindow.Width = Application.Current.MainWindow.Width - 220;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(Properties.Settings.Default.MainWindowPlacement);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }

        private void switchState_Muted(Boolean iwillmute)
        {
            if (iwillmute)
            {
                MutedIcon.Visibility = Visibility.Visible;
                cmMute.Click += (sender, e) => switchState_Muted(false);
            }
            else
            {
                MutedIcon.Visibility = Visibility.Collapsed;
                cmMute.Click += (sender, e) => switchState_Muted(true);
            }
            cmMute.IsChecked = iwillmute;
            vidPlayer.ToggleMute();
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
            //start listening for window close events //
            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            Application.Current.MainWindow.SizeChanged += new SizeChangedEventHandler(Mainwindow_SizeChanged);

            // import channel list
            chanimport = importChannelList();

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

        private void Mainwindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resize_WindowtoChannel(currentChannel);
        }

        private void Resize_WindowtoChannel(Channel ch)
        {
            double chratio = 0;
            double H_act = vidPlayer.ActualHeight;
            double W_act = vidPlayer.ActualWidth;
            switch (ch.Type) {
                case "ICC":
                    chratio = RATIO;
                    break;
                case "TV":
                    chratio = TVRATIO;
                    break;             
            }
            if (chratio != 0)
            {
                double H_opt = Math.Ceiling(W_act * chratio);
                double W_opt = Math.Ceiling(H_act / chratio);
                double H_less = new double();
                double W_less = new double();
                if (H_opt < H_act && H_act != 0) // bars are on the left and right side
                {
                    H_less = H_act - H_opt;
                    W_less = 0;
                }
                else if (W_opt < W_act && W_act != 0) // bars are on the top and bottom side
                {
                    H_less = 0;
                    W_less = W_act - W_opt;
                }
                double H_new = Application.Current.MainWindow.Height - H_less;
                double W_new = Application.Current.MainWindow.Width - W_less;
                Application.Current.MainWindow.Height = H_new;
                Application.Current.MainWindow.Width = W_new;
            }
        }

        private void addRMBItem(Channel ch)
        {
            MenuItem menu = new MenuItem();
            switch (ch.Type)
            {
                case "ICC":
                    menu = (MenuItem)this.rmMenu.Items[5];
                    break;
                case "TV":
                    menu = (MenuItem)this.rmMenu.Items[6];
                    break;
                case "Radio":
                    menu = (MenuItem)this.rmMenu.Items[7];
                    break;
            }
            MenuItem newitem = new MenuItem();
            newitem.Header = ch.Name;
            newitem.InputGestureText = ch.Lcn.ToString();
            newitem.Click += (sender, e) => playMedia(ch);
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
