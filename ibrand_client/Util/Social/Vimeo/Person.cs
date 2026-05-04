using System.Collections.Generic;
using System.Xml.Linq;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class Person
    {
        public string _created_on;
        public string _id = "-1";
        public bool _is_contact;
        public bool _is_plus;
        public bool _is_staff;
        public bool _is_subscribed_to;

        public string _username;
        public string _display_name;
        public string _location;
        public string _url;
        public string _bio;

        public int _number_of_contacts;
        public int _number_of_uploads;
        public int _number_of_likes;
        public int _number_of_videos;
        public int _number_of_videos_appears_in;
        public int _number_of_albums;
        public int _number_of_channels;
        public int _number_of_groups;

        public string _profileurl;
        public string _videosurl;

        public List<Thumbnail> _portraits;

        public static Person FromElement(XElement e)
        {
            var p = new Person();
            try
            {
                p._created_on = e.Attribute("created_on").Value;
                p._id = e.Attribute("id").Value;
                p._is_contact = e.Attribute("is_contact").Value == "1";
                p._is_plus = e.Attribute("is_plus").Value == "1";
                p._is_staff = e.Attribute("is_staff").Value == "1";
                p._is_subscribed_to = e.Attribute("is_subscribed_to").Value == "1";
                p._username = e.Element("username").Value;
                p._display_name = e.Element("display_name").Value;
                p._location = e.Element("location").Value;
                p._url = e.Element("url").Value;
                p._bio = e.Element("bio").Value;
                p._number_of_contacts = int.Parse(e.Element("number_of_contacts").Value);
                p._number_of_uploads = int.Parse(e.Element("number_of_uploads").Value);
                p._number_of_videos = int.Parse(e.Element("number_of_videos").Value);
                p._number_of_videos_appears_in = int.Parse(e.Element("number_of_videos_appears_in").Value);
                p._number_of_albums = int.Parse(e.Element("number_of_albums").Value);
                p._number_of_channels = int.Parse(e.Element("number_of_channels").Value);
                p._number_of_groups = int.Parse(e.Element("number_of_groups").Value);
                p._number_of_likes = int.Parse(e.Element("number_of_likes").Value);
                p._profileurl = e.Element("profileurl").Value;
                p._videosurl = e.Element("videosurl").Value;

                p._portraits = GetPortraits(e.Element("portraits"));
            }
            catch
            { }
            return p;
        }

        public static List<Thumbnail> GetPortraits(XElement e)
        {
            var portraits = new List<Thumbnail>();
            /*foreach (XElement portrait in e.Elements("portrait").ToList())
            {
                Thumbnail t = new Thumbnail();
                t.Height = int.Parse(portrait.Attribute("height").Value);
                t.Width = int.Parse(portrait.Attribute("width").Value);
                t.Url = portrait.Value;
                portraits.Add(t);
            }*/
            return portraits;
        }
    }
}
