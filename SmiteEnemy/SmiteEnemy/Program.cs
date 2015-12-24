using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;

namespace SmiteEnemy
{
    internal class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
		private static Obj_AI_Hero Player;
        private static Spell SmiteSlot;

        private static string WelcMsg = ("<font color = '#FFFF00'>SmiteEnemy</font> <font color = '#008000'>LOADED!</font> <font color = '#FFFFFF'>rewirte from SmiteOP by blackt34.</font>");
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        private static Menu Menu;

        private static float range = 570f;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            CreateMenu();
           
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void CreateMenu()
        {
            Menu = new Menu("SmiteEmemy", "SmiteEmemy", true);
            Menu.AddItem(new MenuItem("smite", "Smite!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle, true)));
			Menu.AddItem(new MenuItem("onlySmiteInCombo", "Only Simte in Combo").SetValue(false));
			Menu.AddItem(new MenuItem("draw", "Draw").SetValue(false));
            Menu.AddToMainMenu();

            //Targetsleector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);
			
			//Orbwalking
            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("smite").GetValue<KeyBind>().Active)
                return;
			
            setSmiteSlot();

            var target = TargetSelector.GetTarget(2500, TargetSelector.DamageType.Magical);
            if (target != null)
			{
				if (Menu.Item("draw").GetValue<bool>())
				{
					Drawing.DrawCircle(target.Position, 50f, System.Drawing.Color.Red);
				}
					
				if (target.IsValidTarget(range) && SmiteSlot.CanCast(target))
				{
					if(Menu.Item("onlySmiteInCombo").GetValue<bool>() && Orbwalker.ActiveMode.ToString() == "Combo"){
						SmiteSlot.Slot = smiteSlot;
						Player.Spellbook.CastSpell(smiteSlot, target);
					}
					else if(!Menu.Item("onlySmiteInCombo").GetValue<bool>()){
						SmiteSlot.Slot = smiteSlot;
						Player.Spellbook.CastSpell(smiteSlot, target);
					}
				}
				else
					return;
			}
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.Item("smite").GetValue<KeyBind>().Active && Menu.Item("draw").GetValue<bool>())
            {
                Drawing.DrawCircle(Player.ServerPosition, range, System.Drawing.Color.Blue);
            }
            else
                return;
        }

        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, range);
                return;
            }
			foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                SmiteSlot = new Spell(smiteSlot, range);
                return;
            }
        }
    }
}
