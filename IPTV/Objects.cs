using System;
using System.Collections.ObjectModel;

namespace IPTV.Objects
{
    public class Channel
    {
        public int Lcn { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string URL { get; set; }
        public string Type { get; set; }

        public static Channel FromCSV(string csvLine)
        {
            string[] values = csvLine.Split(';');
            Channel channels = new Channel();
            channels.Lcn = Convert.ToInt16(values[0]);
            channels.Name = Convert.ToString(values[1]);
            channels.Logo = Convert.ToString(values[2]);
            channels.URL = Convert.ToString(values[3]);
            channels.Type = Convert.ToString(values[4]);
            return channels;
        }

        public string Items { get; }
    }

    public class Category
    {
        public Category()
        {
            this.Items = new ObservableCollection<Channel>();
        }

        public string Name { get; set; }
        public string Lcn { get; }

        public ObservableCollection<Channel> Items { get; set; }
    }
}
