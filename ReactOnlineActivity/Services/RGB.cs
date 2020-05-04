namespace ReactOnlineActivity.Services
{
    public class RGB
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public RGB(byte r = 0, byte g = 0, byte b = 0)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}