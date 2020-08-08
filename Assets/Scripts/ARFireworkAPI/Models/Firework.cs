namespace ARFireworkAPI.Models
{
    using System.Numerics;

    public class Firework

    {
        public uint Id { get; }
        public string Author { get; }
        public FireworkType Type { get; }

        private Vector3 position;

        // Base constructor
        public Firework(string author, FireworkType type, string x, string y, string z)
        {
            this.Author = author;
            this.Type = type;
            this.position.X = float.Parse(x);
            this.position.Y = float.Parse(y);
            this.position.Z = float.Parse(z);
        }

        public Firework(string name, string surname, FireworkType type, string x, string y, string z)
        {
            this.Author = $"{surname} {name}";
            this.Type = type;
            this.position.X = float.Parse(x);
            this.position.Y = float.Parse(y);
            this.position.Z = float.Parse(z);
        }

        // Constructor with id
        public Firework(uint id, string author, FireworkType type, string x, string y, string z) : this(author, type, x,
            y, z)
        {
            this.Id = id;
        }

        public float GetX()
        {
            return this.position.X;
        }

        public float GetY()
        {
            return this.position.Y;
        }

        public float GetZ()
        {
            return this.position.Z;
        }

        public override bool Equals(object obj)
        {
            var f = obj as Firework;
            if (f == null)
            {
                return false;
            }

            if ((this.Id == f.Id) || (this.Author == f.Author && this.Type == f.Type && GetX() == f.GetX() &&
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