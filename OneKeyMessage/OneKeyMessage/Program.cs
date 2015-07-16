using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace OneKeyMessage
{
    class Program
    {
        public static Menu Menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("One Key Message", "OneKeyMessage", true);
            Menu.AddItem(new MenuItem("enabled", "Enable").SetValue(true));
			//Menu.AddItem(new MenuItem("key1", "Message 1").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddItem(new MenuItem("key1", "Message 1").SetValue(new KeyBind(97, KeyBindType.Press))); //Num Pad 1
            Menu.AddItem(new MenuItem("key2", "Message 2").SetValue(new KeyBind(98, KeyBindType.Press))); //Num Pad 2
            Menu.AddItem(new MenuItem("key3", "Message 3").SetValue(new KeyBind(99, KeyBindType.Press))); //Num Pad 3
            Menu.AddItem(new MenuItem("key4", "Message 4").SetValue(new KeyBind(100, KeyBindType.Press))); //Num Pad 4
			Menu.AddItem(new MenuItem("key5", "Message 5").SetValue(new KeyBind(101, KeyBindType.Press))); //Num Pad 5
            Menu.AddToMainMenu();
            FileCheck.DoChecks();

            Game.OnUpdate += Game_OnGameUpdate;
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
			if(Menu.Item("enabled").GetValue<bool>()){

				string[] MyMessages = File.ReadAllLines(FileCheck.MyMessagesTxt, Encoding.UTF8);

				//message 1
				if (Menu.Item("key1").GetValue<KeyBind>().Active)
				{
					Game.Say(MyMessages[0]);
					System.Threading.Thread.Sleep(100);
				}

				//message 2
				if (Menu.Item("key2").GetValue<KeyBind>().Active)
				{
					Game.Say(MyMessages[1]);
					System.Threading.Thread.Sleep(100);
				}

				//message 3
				if (Menu.Item("key3").GetValue<KeyBind>().Active)
				{
					Game.Say(MyMessages[2]);
					System.Threading.Thread.Sleep(100);
				}

				//message 4
				if (Menu.Item("key4").GetValue<KeyBind>().Active)
				{
					Game.Say(MyMessages[3]);
					System.Threading.Thread.Sleep(100);
				}

				//message 5
				if (Menu.Item("key5").GetValue<KeyBind>().Active)
				{
					Game.Say(MyMessages[4]);
					System.Threading.Thread.Sleep(100);
				}
			}
        }
    }
}
