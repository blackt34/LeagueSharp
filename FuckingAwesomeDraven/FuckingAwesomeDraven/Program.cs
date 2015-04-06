using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace FuckingAwesomeDraven
{       enum Spells
        {
            Q, W, E, R
        }
    class Program
    {
        
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            {Spells.Q, new Spell(SpellSlot.Q, 0)},  {Spells.W, new Spell(SpellSlot.W, 0)},  {Spells.E, new Spell(SpellSlot.E, 1100)},  {Spells.R, new Spell(SpellSlot.R, 20000)}, 
        };

        public static Orbwalking.Orbwalker Orbwalker;

        public static Menu Config;
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Draven")
            {
                Notifications.AddNotification(new Notification("Not Draven? Draaaaaaaaaven.", 5));
                return;
            }

            Config = new Menu("FuckingAwesomeDraven", "FuckingAwesomeDraven", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));

            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var ComboMenu = Config.AddSubMenu(new Menu("Combo", "Combo"));

            ComboMenu.AddItem(new MenuItem("UQC", "Use Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UWC", "Use W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UEC", "Use E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("URC", "Use R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("URCM", "R Mode").SetValue(new StringList(new []{"Out of Range KS", "KS (any time)"}, 0)));
            ComboMenu.AddItem(new MenuItem("forceR", "Force R on Target").SetValue(new KeyBind('T', KeyBindType.Press)));

            var HarassMenu = Config.AddSubMenu(new Menu("Harass", "Harass"));

            HarassMenu.AddItem(new MenuItem("UQH", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UWH", "Use W").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UEH", "Use E").SetValue(true));

            var JungleMenu = Config.AddSubMenu(new Menu("MinionClear", "MinionClear"));

            JungleMenu.AddItem(new MenuItem("sdfsdf", "Jungle Clear"));
            JungleMenu.AddItem(new MenuItem("UQJ", "Use Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UWJ", "Use W").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UEJ", "Use E").SetValue(true));
            JungleMenu.AddItem(new MenuItem("sdffdsdf", "Wave Clear"));
            JungleMenu.AddItem(new MenuItem("UQWC", "Use Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("WCM", "Min Mana for Waveclear (%)").SetValue(new Slider(20, 0, 100)));

            // Axe Menu
            var axe = Config.AddSubMenu(new Menu("Axe Catching", "Axe Catching"));

            axe.AddItem(new MenuItem("catching", "Catching Enabled").SetValue(new KeyBind('M', KeyBindType.Toggle)));
            axe.AddItem(new MenuItem("useWCatch", "Use W to Catch (smart)").SetValue(false));
            axe.AddItem(
                new MenuItem("catchRadiusMode", "Catch Radius Mode").SetValue(
                    new StringList(new[] {"Mouse Mode", "Sector Mode"})));
            axe.AddItem(new MenuItem("sectorAngle", "Sector Angle").SetValue(new Slider(177, 1, 360)));
            axe.AddItem(new MenuItem("catchRadius", "Catch Radius").SetValue(new Slider(600, 300, 1500)));
            axe.AddItem(new MenuItem("ignoreTowerReticle", "Ignore Tower Reticle").SetValue(true));
            axe.AddItem(new MenuItem("clickRemoveAxes", "Remove Axes With Click").SetValue(true));

            Antispells.init();

            var draw = Config.AddSubMenu(new Menu("Draw", "Draw"));
            draw.AddItem(new MenuItem("DABR", "Disable All Drawings but Reticle").SetValue(false));
            draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "Draw R Range").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCS", "Draw Catching State").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCA", "Draw Current Axes").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCR", "Draw Catch Radius").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DAR", "Draw Axe Spots").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DKM", "Draw Killable Minion").SetValue(new Circle(true, System.Drawing.Color.White)));

            var Info = Config.AddSubMenu(new Menu("Information", "info"));
            Info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            Info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            Info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.4-0"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));

            Config.AddToMainMenu();

            Notifications.AddNotification(new Notification("Fucking Awesome Draven - Loaded", 5));
            Notifications.AddNotification("Who wants some Draven?", 5);

            spells[Spells.E].SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Orbwalker.SetAttack(false);
            Orbwalker.SetMovement(false);

            GameObject.OnCreate += AxeCatcher.OnCreate;
            GameObject.OnDelete += AxeCatcher.OnDelete;
            Obj_AI_Hero.OnProcessSpellCast += AxeCatcher.Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += eventArgs => AxeCatcher.Draw();
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += AxeCatcher.GameOnOnWndProc;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("forceR").GetValue<KeyBind>().Active && TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical).IsValidTarget()) spells[Spells.R].Cast(TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical));

            AxeCatcher.catchAxes();

            switch (Orbwalker.ActiveMode)
            {
                    case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    Jungle();
                    break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        public static void Combo()
        {
            var Q = Config.Item("UQC").GetValue<bool>();
            var W = Config.Item("UWC").GetValue<bool>();
            var E = Config.Item("UEC").GetValue<bool>();
            var R = Config.Item("URC").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>()) return;
            var Target = (Obj_AI_Hero) t;

            if (Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 200))
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }

            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }

            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }

            var t2 = TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical);
            if (R && spells[Spells.R].IsReady() && t2.IsValidTarget(spells[Spells.R].Range))
            {
                switch (Config.Item("URCM").GetValue<StringList>().SelectedIndex)
                {
                    case 1:
                        if (getRCalc(t2)) spells[Spells.R].Cast(t2);
                        break;
                    case 0: 
                        if (getRCalc(t2) && t2.Distance(Player) > 800) spells[Spells.R].Cast(t2);
                        break;
                }
            }
        }

        public static void Harass()
        {
            var Q = Config.Item("UQH").GetValue<bool>();
            var W = Config.Item("UWH").GetValue<bool>();
            var E = Config.Item("UEH").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>()) return;
            var Target = (Obj_AI_Hero) t;

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }
            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }
        }

        public static void Jungle()
        {
            var Q = Config.Item("UQJ").GetValue<bool>();
            var W = Config.Item("UWJ").GetValue<bool>();
            var E = Config.Item("UEJ").GetValue<bool>();

            var Target = MinionManager.GetMinions(
                Player.Position, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }
            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }
        }

        public static void WaveClear()
        {
            var Q = Config.Item("UQWC").GetValue<bool>();
            var Target = MinionManager.GetMinions(
                Player.Position, 700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(a => !a.Name.ToLower().Contains("ward"));
            if (Config.Item("WCM").GetValue<Slider>().Value > (Player.Mana / Player.MaxMana * 100))
                return;
            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
        }

        public static bool getRCalc(Obj_AI_Hero target)
        {
            return false;
            int totalUnits =
                spells[Spells.R].GetPrediction(target)
                    .CollisionObjects.Count(a => a.IsValidTarget());
            float distance = ObjectManager.Player.Distance(target);
            var damageReduction = ((totalUnits > 7)) ? 0.4 : (totalUnits == 0) ? 1.0 : (1 - (((totalUnits) * 8)/100));
            return spells[Spells.R].GetDamage(target) * damageReduction >= (target.Health + (distance / 2000) * target.HPRegenRate);
        }
    }
}
