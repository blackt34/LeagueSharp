using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;

namespace SmiteOP
{
    internal class Program
    {
        private static Obj_AI_Hero Player;
        private static Spell SmiteSlot;
        private static List<Items.Item> itemsList = new List<Items.Item>();
        private static string WelcMsg = ("<font color = '#ff3366'>SmiteOP</font><font color='#FFFFFF'> by Da'ath.</font> <font color = '#66ff33'> ~~ LOADED ~~</font> ");
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        private static Menu Menu;
        private static Items.Item s0, s1, s2, s3, s4;
        private static float range = 700f;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
             Player = ObjectManager.Player;
             Game.PrintChat(WelcMsg);

             CreateMenu();
             InitializeItems();

             Game.OnGameUpdate += Game_OnGameUpdate;
             Drawing.OnDraw += Drawing_OnDraw;
      
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("enable").GetValue<bool>())
                return;

            if (Menu.Item("draw").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Player.ServerPosition, range, Menu.Item("draw").GetValue<Circle>().Color);
            }
            else
                return;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("enable").GetValue<bool>())
                return;
            if (Player.IsDead)
                return;
            if (!CheckInv())
                return;

            setSmiteSlot();

            var enemys = ObjectManager.Get<Obj_AI_Hero>().Where(f => !f.IsAlly && !f.IsDead && Player.Distance(f, false) <= range);
            if (enemys == null)
                return;

            float dmg = Damage();
            foreach (var enemy in enemys)
            {
                if (enemy.Health <= dmg)
                {
                    //Game.PrintChat("KAPPA");
                    SmiteSlot.Slot = smiteSlot;
                    Player.Spellbook.CastSpell(smiteSlot, enemy);
                }

            }

        }

        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, range);
                return;
            }
        }

        private static void CreateMenu()
        {
            Menu = new Menu("SmiteOP", "menu", true);
            Menu.AddItem(new MenuItem("enable", "Enable").SetValue(true));
            Menu.AddItem(new MenuItem("draw", "Draw Smite Range").SetValue(new Circle(true, Color.Blue)));
            Menu.AddToMainMenu();
        }

        private static bool CheckInv()
        {
            bool b = false;
            foreach(var item in itemsList)
            {
                if(Player.InventoryItems.Any(f => f.Id == (ItemId)item.Id))
                {
                    b = true;
                }
            }
            return b;
        }
        private static void InitializeItems()
        {
            s0 = new Items.Item(3710, range);
            itemsList.Add(s0);
            s1 = new Items.Item(3709, range);
            itemsList.Add(s1);
            s2 = new Items.Item(3708, range);
            itemsList.Add(s2);
            s3 = new Items.Item(3707, range);
            itemsList.Add(s3);
            s4 = new Items.Item(3706, range);
            itemsList.Add(s4);
        }
        private static float Damage()
        {
            int lvl = Player.Level;
            int damage = (20 + 8 * lvl);

            return damage;
        }
    }
}
