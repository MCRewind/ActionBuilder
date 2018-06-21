using System.IO;

namespace ActionBuilder
{
    public class JsonUtils
    {
        public static string ReadFromJson(string path)
        {
            string result;

            using (var r = new StreamReader(path))
            {
                result = r.ReadToEnd();
            }
            return result;
        }

        public static void WriteToJson(string path, Stream contents)
        {
            if (!File.Exists(path))
                Directory.CreateDirectory(Directory.GetParent(path).FullName);

            contents.Position = 0;

            string result;

            using (var r = new StreamReader(contents))
                result = r.ReadToEnd();

            File.WriteAllText(path, result);
        }
    }
}