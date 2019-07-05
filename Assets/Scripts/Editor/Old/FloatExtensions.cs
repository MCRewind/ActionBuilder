namespace Editor
{
    public static class FloatExtensions
    {
        public static bool IsWithinXOf(this float value, float range, float num)
        {
            return value > num - range && value < num + range;
        }

    }
}