using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Anti_AFK
{
    class Program
    {
        private static Menu Config;
        private static Spell Q, W, E, R;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            Config = new Menu("Anti-AFK", "afk", true);
            Config.AddItem(new MenuItem("afk", "Enabled").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Config.AddToMainMenu();
            Game.PrintChat("<font color='#00ff00'>["+Utils.FormatTime(Game.ClockTime)+"] Barack Obama:</font> Got me to challenger in one day Kappa");
            Game.OnGameUpdate += Game_OnGameUpdate;
        }
        private static int one;
        private static int two;
        static void Game_OnGameUpdate(EventArgs args)
        {
            if(Config.Item("afk").GetValue<KeyBind>().Active)
            {
//                Game.PrintChat("Team: "+ObjectManager.Player.Team+" My " + ObjectManager.Player.Position);
                if(one == 0)
                {
                    Orbwalking.Orbwalk(null, new Vector3(3224, 1258, 95));
                    if (hasWalked(new Vector3(3224, 1258, 95)))
                        one = 1;
                }
                if(one == 1 && two == 0)
                {
                    Orbwalking.Orbwalk(null, new Vector3(3012, 3014, 95));
                    if (hasWalked(new Vector3(3012, 3014, 95)))
                        two = 1;
                }
                if(one == 1 && two == 1)
                {

                    Orbwalking.Orbwalk(null, new Vector3(1172, 3346, 95));
                    if (hasWalked(new Vector3(1172, 3346, 95)))
                    {
                        one = 0;
                        two = 0;
                    }
                }
            }
            else
            {
                one = 0;
                two = 0;
            }
        }
        static Vector3 getOne()
        {
            return ObjectManager.Player.Team == GameObjectTeam.Order ? new Vector3(3224, 1258, 95) : new Vector3(11522, 13656, 91);
        }
        static Vector3 getTwo()
        {
            return ObjectManager.Player.Team == GameObjectTeam.Order ? new Vector3(3012,3014,95) : new Vector3(11476, 1234, 91);
        }
        static Vector3 getThree()
        {
            return ObjectManager.Player.Team == GameObjectTeam.Order ? new Vector3(1172,3346,95) : new Vector3(113662, 11536, 91);
        }
        static bool hasWalked(Vector3 coor)
        {
            if (ObjectManager.Player.Distance(coor) < 80)
            {
                if (Q.IsReady()) Q.Cast(ObjectManager.Player);
                if (W.IsReady()) W.Cast(ObjectManager.Player);
                if (E.IsReady()) E.Cast(ObjectManager.Player);
                if (R.IsReady()) R.Cast(ObjectManager.Player);
            }
            return ObjectManager.Player.Distance(coor) < 80;
        }
    }
}
