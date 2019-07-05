namespace Editor
{
    public static class IntExtensions
    {
        public static bool IsWithinXOf(this int value, int range, int num)
        {
            return value > num - range && value < num + range;
        }
    }
}