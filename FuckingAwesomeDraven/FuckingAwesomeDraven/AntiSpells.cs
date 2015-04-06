using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeDraven
{
    class Antispells
    {
        public static void init()
        {
            var mainMenu = Program.Config.AddSubMenu(new Menu("Anti GapCloser", "Anti GapCloser"));
            var spellMenu = mainMenu.AddSubMenu(new Menu("Enabled Spells", "Enabled SpellsAnti GapCloser"));
            mainMenu.AddItem(new MenuItem("EnabledGC", "Enabled").SetValue(false));

            var mainMenuinterrupter = Program.Config.AddSubMenu(new Menu("Interrupter", "Interrupter"));
            mainMenuinterrupter.AddItem(new MenuItem("EnabledInterrupter", "Enabled").SetValue(false));
            mainMenuinterrupter.AddItem(new MenuItem("minChannel", "Minimum Channel Priority").SetValue(new StringList(new[] { "HIGH", "MEDIUM", "LOW" })));


            foreach (var champ in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var champmenu = spellMenu.AddSubMenu(new Menu(champ.ChampionName, champ.ChampionName + "GC"));
                foreach (var gcSpell in AntiGapcloser.Spells)
                {
                    if (gcSpell.ChampionName == champ.ChampionName)
                    {
                        champmenu.AddItem(
                            new MenuItem(gcSpell.SpellName, gcSpell.SpellName + "- " + gcSpell.Slot).SetValue(true));
                    }
                }
            }

            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Program.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()) == null)
                return;
            if (!Program.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()).GetValue<bool>() || !gapcloser.Sender.IsValidTarget()) return;
            if (Program.spells[Spells.E].IsReady() && gapcloser.Sender.IsValidTarget(Program.spells[Spells.E].Range))
            {
                Program.spells[Spells.E].Cast(gapcloser.Sender);
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Program.Config.Item("EnabledInterrupter").GetValue<bool>() || !sender.IsValidTarget()) return;
            Interrupter2.DangerLevel a;
            switch (Program.Config.Item("minChannel").GetValue<StringList>().SelectedValue)
            {
                case "HIGH":
                    a = Interrupter2.DangerLevel.High;
                    break;
                case "MEDIUM":
                    a = Interrupter2.DangerLevel.Medium;
                    break;
                default:
                    a = Interrupter2.DangerLevel.Low;
                    break;
            }

            if (args.DangerLevel == Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.Medium &&
                a != Interrupter2.DangerLevel.High)
            {
                if (Program.spells[Spells.E].IsReady() && sender.IsValidTarget(Program.spells[Spells.E].Range))
                {
                    Program.spells[Spells.E].Cast(sender);
                }
            }
        }
    }
}
