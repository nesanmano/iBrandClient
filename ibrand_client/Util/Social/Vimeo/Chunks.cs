using System.Collections.Generic;
using System.Xml.Linq;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class Chunks
    {
        public class Chunk
        {
            public int _id;
            public long _size;

            public Chunk(int id, long size)
            {
                _id = id;
                _size = size;
            }

            public static Chunk FromElement(XElement e)
            {
                return new Chunk( int.Parse(e.Attribute("id").Value),
                    long.Parse(e.Attribute("size").Value));
                
            }
        }

        public string _ticket_id;
        public List<Chunk> _items;

        public Chunks(string ticketId, List<Chunk> items)
        {
            _ticket_id = ticketId;
            _items = items;
        }

        public static Chunks FromElement(XElement e)
        {
            var r = new Chunks(e.Attribute("id").Value, new List<Chunk>());

            foreach (var item in e.Element("chunks").Elements("chunk"))
            {
                r._items.Add(Chunk.FromElement(item));
            }
            return r;
        }
    }
}