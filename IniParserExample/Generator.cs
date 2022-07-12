using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParserExample
{
    public class Generator
    {
        public static void GenerateIni(int sections = 2500, string name = "generated")
        {
            int minKeys = 3;
            int maxKeys = 10;
            var random = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < sections; i++)
            {
                int keys = random.Next(minKeys, maxKeys + 1);
                sb.AppendLine($"; This is a comment for section {i}.");
                sb.AppendLine($"[Section{i}]\n");
                for (int j = 0; j < keys; j++)
                {
                    sb.AppendLine($"\tkey{j} = {RandomValue()}");
                }
                sb.AppendLine("\n");
            }

            File.WriteAllText($"../../../testFiles/{name}.ini", sb.ToString());
        }

        public static string RandomValue()
        {
            var rnd = new Random();
            return rnd.Next(0, 6) switch
            {
                0 => "true",
                1 => "false",
                2 => RandomWord(rnd.Next(8,64)),
                3 => $"\"{RandomWord(rnd.Next(8, 64), true)}\"",
                4 => rnd.Next().ToString(),
                5 => rnd.NextDouble().ToString("0.############"),
                _ => "null",
            };
        }

        public static string RandomWord(int length, bool symbols = false)
        {
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (symbols) chars += "1234567890$%@!*?:^& ";
            var rand = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int num = rand.Next(0, chars.Length);
                sb.Append(chars[num]);
            }
            return sb.ToString();
        }
    }
}
