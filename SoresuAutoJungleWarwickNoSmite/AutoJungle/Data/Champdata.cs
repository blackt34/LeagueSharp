using System;
using System.Linq;
using AutoJungle.Data;
using LeagueSharp;
using LeagueSharp.Common;
using UnderratedAIO.Helpers;

namespace AutoJungle
{
    internal class Champdata
    {
        public Obj_AI_Hero Hero = null;
        public BuildType Type;

        public Func<bool> JungleClear;
        public Func<bool> Combo;
        public Spell R, Q, W, E;
        public AutoLeveler Autolvl;

        public Champdata()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "MasterYi":
                    Hero = ObjectManager.Player;
                    Type = BuildType.AS;

                    Q = new Spell(SpellSlot.Q, 600);
                    Q.SetTargetted(0.5f, float.MaxValue);
                    W = new Spell(SpellSlot.W);
                    E = new Spell(SpellSlot.E);
                    R = new Spell(SpellSlot.R);

                    Autolvl = new AutoLeveler(new int[] { 0, 2, 1, 0, 0, 3, 0, 2, 0, 2, 3, 2, 2, 1, 1, 3, 1, 1 });

                    JungleClear = MasteryiJungleClear;
                    Combo = MasteryiCombo;
                    Console.WriteLine("Masteryi added");
                    break;

                case "Warwick":
                    Hero = ObjectManager.Player;
                    Type = BuildType.AS;

                    Q = new Spell(SpellSlot.Q, 400, TargetSelector.DamageType.Magical);
                    Q.SetTargetted(0.5f, float.MaxValue);
                    W = new Spell(SpellSlot.W, 1250);
                    E = new Spell(SpellSlot.E);
                    R = new Spell(SpellSlot.R, 700, TargetSelector.DamageType.Magical);
                    R.SetTargetted(0.5f, float.MaxValue);

                    Autolvl = new AutoLeveler(new int[] { 0, 1, 2, 0, 0, 3, 0, 1, 0, 1, 3, 1, 1, 2, 2, 3, 2, 2 });

                    JungleClear = WarwickJungleClear;
                    Combo = WarwickCombo;

                    Console.WriteLine("Warwick added");
                    break;

                case "Shyvana":
                    Hero = ObjectManager.Player;
                    Type = BuildType.AS;

                    Q = new Spell(SpellSlot.Q);
                    W = new Spell(SpellSlot.W, 350f);
                    E = new Spell(SpellSlot.E, 925f);
                    E.SetSkillshot(0.25f, 60f, 1500, false, SkillshotType.SkillshotLine);
                    R = new Spell(SpellSlot.R, 1000f);
                    R.SetSkillshot(0.25f, 150f, 1500, false, SkillshotType.SkillshotLine);

                    Autolvl = new AutoLeveler(new int[] { 1, 2, 0, 1, 1, 3, 1, 0, 1, 0, 3, 0, 0, 2, 2, 3, 2, 2 });

                    JungleClear = ShyvanaJungleClear;
                    Combo = ShyvanaCombo;

                    Console.WriteLine("Shyvana added");
                    break;
                default:
                    Console.WriteLine("Not Supported");
                    break;
            }
        }

        private bool ShyvanaCombo()
        {
            var targetHero = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling)
            {
                return false;
            }
            if (W.IsReady() && Hero.Distance(targetHero) < W.Range + 100)
            {
                W.Cast();
            }
            ItemHandler.UseItemsJungle();
            if (Hero.IsWindingUp)
            {
                return false;
            }
            if (Q.IsReady() && Orbwalking.GetRealAutoAttackRange(targetHero) > Hero.Distance(targetHero))
            {
                Q.Cast();
            }
            if (E.IsReady() && E.CanCast(targetHero))
            {
                E.Cast(targetHero);
            }
            if (R.IsReady() && Hero.Mana == 100 &&
                targetHero.CountEnemiesInRange(GameInfo.ChampionRange) <=
                targetHero.CountAlliesInRange(GameInfo.ChampionRange) &&
                !Hero.Position.Extend(targetHero.Position, GameInfo.ChampionRange).UnderTurret(true))
            {
                R.CastIfHitchanceEquals(targetHero, HitChance.VeryHigh);
            }

            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetHero);
            return false;
        }

        private bool ShyvanaJungleClear()
        {
            var targetMob = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling)
            {
                return false;
            }
            var structure = Helpers.CheckStructure();
            if (structure != null)
            {
                Hero.IssueOrder(GameObjectOrder.AttackUnit, structure);
                return false;
            }
            if (targetMob == null)
            {
                return false;
            }
            if (W.IsReady() && Hero.Distance(targetMob) < W.Range &&
                (Helpers.getMobs(Hero.Position, W.Range).Count >= 2 ||
                 targetMob.Health > W.GetDamage(targetMob) * 7 + Hero.GetAutoAttackDamage(targetMob, true) * 2))
            {
                W.Cast();
            }
            ItemHandler.UseItemsJungle();
            if (Hero.IsWindingUp)
            {
                return false;
            }
            if (Q.IsReady())
            {
                Q.Cast();
                Hero.IssueOrder(GameObjectOrder.AutoAttack, targetMob);
            }
            if (E.IsReady() && E.CanCast(targetMob))
            {
                E.Cast(targetMob);
            }
            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetMob);
            return false;
        }

        private bool WarwickCombo()
        {
            var targetHero = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling)
            {
                return false;
            }
            if (Program.menu.Item("ComboSmite").GetValue<Boolean>())
            {
                Jungle.CastSmiteHero((Obj_AI_Hero) targetHero);
            }
            if (Hero.IsWindingUp)
            {
                return false;
            }
            if (Q.IsReady() && Q.CanCast(targetHero))
            {
                Q.CastOnUnit(targetHero);
            }
            if (W.IsReady() && Hero.Distance(targetHero) < 300)
            {
                if (Hero.Mana > Q.ManaCost + W.ManaCost || Hero.HealthPercent > 70)
                {
                    W.Cast();
                }
            }
            if (R.IsReady() && R.CanCast(targetHero) && !targetHero.MagicImmune)
            {
                R.CastOnUnit(targetHero);
            }
            if (E.IsReady() && Hero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 && Hero.Distance(targetHero) < 1000)
            {
                E.Cast();
            }
            ItemHandler.UseItemsCombo(targetHero, !R.IsReady());
            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetHero);
            return false;
        }

        private bool WarwickJungleClear()
        {
            var targetMob = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling)
            {
                return false;
            }
            if (Hero.IsWindingUp)
            {
                return false;
            }
            var structure = Helpers.CheckStructure();
            if (structure != null)
            {
                Hero.IssueOrder(GameObjectOrder.AttackUnit, structure);
                return false;
            }
            if (targetMob == null)
            {
                return false;
            }
            if (Q.IsReady() && Q.CanCast(targetMob) &&
                (Hero.ManaPercent > 50 || Hero.MaxHealth - Hero.Health > Q.GetDamage(targetMob) * 0.8f))
            {
                Q.CastOnUnit(targetMob);
            }
            if (W.IsReady() && Hero.Distance(targetMob) < 300 && (Program._GameInfo.SmiteableMob != null) ||
                Program._GameInfo.MinionsAround > 3)
            {
                if (Hero.Mana > Q.ManaCost + W.ManaCost || Hero.HealthPercent > 70)
                {
                    W.Cast();
                }
            }
            if (E.IsReady() && Hero.Spellbook.GetSpell(SpellSlot.E).ToggleState != 1 && Hero.Distance(targetMob) < 500)
            {
                E.Cast();
            }
            ItemHandler.UseItemsJungle();
            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetMob);
            return false;
        }

        private bool MasteryiJungleClear()
        {
            var targetMob = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling)
            {
                return false;
            }
            if (E.IsReady() && Hero.IsWindingUp)
            {
                E.Cast();
            }
            var structure = Helpers.CheckStructure();
            if (structure != null)
            {
                Hero.IssueOrder(GameObjectOrder.AttackUnit, structure);
                return false;
            }
            if (targetMob == null)
            {
                return false;
            }
            if (R.IsReady() && Hero.Position.Distance(Hero.Position) < 300 &&
                Jungle.bosses.Any(n => targetMob.Name.Contains(n)))
            {
                R.Cast();
            }
            if (Hero.IsWindingUp)
            {
                return false;
            }
            if (Q.IsReady() && Q.CanCast(targetMob) && targetMob.Health < targetMob.MaxHealth)
            {
                Q.CastOnUnit(targetMob);
            }
            if (W.IsReady() && Hero.HealthPercent < 50)
            {
                W.Cast();
            }
            ItemHandler.UseItemsJungle();
            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetMob);
            return false;
        }

        private bool MasteryiCombo()
        {
            var targetHero = Program._GameInfo.Target;
            if (Hero.Spellbook.IsChanneling &&
                targetHero.Health > Program.player.GetAutoAttackDamage(targetHero, true) * 2)
            {
                return false;
            }
            if (E.IsReady() && Hero.IsWindingUp)
            {
                E.Cast();
            }
            if (R.IsReady() && Hero.Distance(targetHero) < 600)
            {
                R.Cast();
            }
            if (Hero.IsWindingUp)
            {
                return false;
            }
            if (Q.IsReady())
            {
                Q.CastOnUnit(targetHero);
            }
            if (W.IsReady() && Hero.HealthPercent < 25 || Program._GameInfo.DamageTaken >= Hero.Health / 3)
            {
                W.Cast();
            }
            ItemHandler.UseItemsCombo(targetHero, !Q.IsReady());
            Hero.IssueOrder(GameObjectOrder.AttackUnit, targetHero);
            return false;
        }
    }
}