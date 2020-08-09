using System.Numerics;

namespace ARFireworkAPI.Models
{
    public class Firework

    {
        
        public string Author { get; protected set; }
        public FireworkType Type { get; protected set; }

        public Vector3 Position { get; protected set; }

        // Only called from FireworReceived
        protected Firework()
        {
            
        }
        
        // Base constructor
        public Firework(string author, FireworkType type, string x, string y, string z)
        {
            this.Author = author;
            this.Type = type;
            this.Position = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        public float GetX()
        {
            return this.Position.X;
        }

        public float GetY()
        {
            return this.Position.Y;
        }

        public float GetZ()
        {
            return this.Position.Z;
        }

        public override bool Equals(object obj)
        {
            var f = obj as Firework;
            if (f == null)
            {
                return false;
            }

            if ((this.Author == f.Author && this.Type == f.Type && GetX() == f.GetX() &&
                                      GetY() == f.GetY() && GetZ() == f.GetZ()))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("{0} lance son feu d'artifice de type \"{4}\" à la position ({1}, {2}, {3})", Author,
                GetX(), GetY(), GetZ(), Type);
        }
    }
}