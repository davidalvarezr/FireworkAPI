using System.Numerics;

namespace ARFireworkAPI.Models
{
    public class Firework

    {
        public FireworkType Type { get; protected set; }

        public Vector3 Position { get; protected set; }

        // Only called from FireworReceived
        protected Firework()
        {
        }

        // Base constructor
        public Firework(FireworkType type, string x, string y, string z)
        {
            this.Type = type;

            this.Position = new Vector3(
                float.Parse(x.Replace(",", ".")),
                float.Parse(y.Replace(",", ".")),
                float.Parse(z.Replace(",", "."))
            );
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

            if ((this.Type == f.Type && GetX() == f.GetX() &&
                 GetY() == f.GetY() && GetZ() == f.GetZ()))
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("Type:\"{0}\"\nPosition ({1}, {2}, {3})", Type, GetX(), GetY(), GetZ());
        }
    }
}