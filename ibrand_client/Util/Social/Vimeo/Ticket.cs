using System.Xml.Linq;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
        public class Ticket
        {
            public string _id;
            public string _endpoint;
            public string _host;
            public string _max_file_size;

            public Ticket() { }
            public Ticket(string id, string endpoint) { _id = id; _endpoint = endpoint; }
            public Ticket(string id, string endpoint, string host, string maxFileSize) { 
                _id = id; 
                _endpoint = endpoint;
                _host = host;
                _max_file_size = maxFileSize;
            }

            public static Ticket FromElement(XElement e)
        {
            return new Ticket(e.Attribute("id").Value,e.Attribute("endpoint").Value,e.Attribute("host").Value,e.Attribute("max_file_size").Value);
        }
        }
}

