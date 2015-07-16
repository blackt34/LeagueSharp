using System;
using System.IO;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace OneKeyMessage
{
    internal class FileCheck
    {
        public static string Folder = Config.AppDataDirectory + @"\OneKeyMessage\";
        public static string MyMessagesTxt;

        public static void DoChecks()
        {
            MyMessagesTxt = Folder + "MyMessages.txt";

            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
            if (!File.Exists(MyMessagesTxt))
            {
                var newfile = File.Create(MyMessagesTxt);
                newfile.Close();
                const string content = " Hi\n Hello\n Good day\n Team Fight\n GoGoGo\n";
                var separator = new[] { "\n" };
                string[] lines = content.Split(separator, StringSplitOptions.None);
                File.WriteAllLines(MyMessagesTxt, lines, Encoding.UTF8);
            }
        }
    }
}
