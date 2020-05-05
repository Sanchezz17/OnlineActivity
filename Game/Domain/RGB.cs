namespace Game.Domain
{
    public class Rgb
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Rgb(byte r = 0, byte g = 0, byte b = 0)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}