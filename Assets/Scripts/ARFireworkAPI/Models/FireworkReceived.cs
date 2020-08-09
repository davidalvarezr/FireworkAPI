using System;
using System.Numerics;
using Newtonsoft.Json.Linq;

namespace ARFireworkAPI.Models
{
    public class FireworkReceived : Firework
    {
        public uint Id { get; } = 0u;
        
        public string Author { get; private set; }
        
        public FireworkReceived(JObject @event)
        {
            var msg = JObject.Parse((string) @event["data"])["message"];
            Id = uint.Parse(((string) msg["id"]));
            Author = (string) msg["author"];
            string x = (string) msg["x"];
            string y = (string) msg["y"];
            string z = (string) msg["z"];
            Position = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
            Enum.TryParse((string) msg["type"], out FireworkType fireworkType);
            Type = fireworkType;
        }

        public override bool Equals(object obj)
        {
            var f = obj as FireworkReceived;
            if (f == null)
            {
                return false;
            }

            if ((this.Id == f.Id))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("Auteur: {0}\nId: {5}\nType {4}\nPosition ({1}, {2}, {3})",
                Author, GetX(), GetY(), GetZ(), Type, Id);
        }
    }
}