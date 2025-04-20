namespace SimpleEncoding
{
    public static class Base256
    {
        private static SimpleBase256 _SimpleBase256 = new SimpleBase256();

        public static string Encode(byte[] bytes)
        {
            return _SimpleBase256.Encode(bytes);
        }

        public static byte[] Decode(string str)
        {
            return _SimpleBase256.Decode(str);
        }
    }
}