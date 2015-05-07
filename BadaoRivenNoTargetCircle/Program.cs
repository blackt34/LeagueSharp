using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common.Data;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace BadaoRiven
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static SpellSlot flash = Player.GetSpellSlot("summonerflash");

        private static Menu Menu;

        private static bool qE, qQ, qAA, qW, qTiamat, qR1, qR2, midAA, canAA, forceQ, forceW, forceT, forceR, waitR, castR, waitR2, forceEburst, qGap
            , R2style;

        private static AttackableUnit TTTar = null;

        private static float cE, cQ, cAA, cW, cTiamt, cR1, cR2, Wind, countforce, Qstate, Rstate, R2countdonw;

        static void Main(string[] args)
        {
            //CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            if (args == null)
            {
                return;
            }
            if (Game.Mode == GameMode.Running)
            {
                OnStart(new EventArgs());
            }
            Game.OnStart += OnStart;

        }

        private static void OnStart(EventArgs args)
        {
            if (Player.ChampionName != "Riven")
                return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);

            Menu = new Menu("Heaven Strike " + Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            spellMenu.AddItem(new MenuItem("RcomboAlways", "RcomboAlways").SetValue(false));
            spellMenu.AddItem(new MenuItem("RcomboKillable", "RcomboKillable").SetValue(true));
            spellMenu.AddItem(new MenuItem("R2comboKS", "R2comboKS").SetValue(true));
            spellMenu.AddItem(new MenuItem("R2comboMaxdmg", "RcomboMaxdmg").SetValue(true));
            spellMenu.AddItem(new MenuItem("R2 Badao Style", "R2 Badao Style").SetValue(true));
            spellMenu.AddItem(new MenuItem("Ecombo", "Ecombo").SetValue(true));
            spellMenu.AddItem(new MenuItem("Q Gap", "Q Gap").SetValue(false));
            spellMenu.AddItem(new MenuItem("Use Q Before Expiry", "Use Q Before Expiry").SetValue(true));
            spellMenu.AddItem(new MenuItem("Q strange Cancel", "Q strange Cancel").SetValue(true));
            Menu BurstCombo = spellMenu.AddSubMenu(new Menu("Burst Combo", "Burst Combo"));
            BurstCombo.AddItem(new MenuItem("Burst", "Burst").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            BurstCombo.AddItem(new MenuItem("Use Flash", "Use Flash").SetValue(false));
            Menu Misc = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Misc.AddItem(new MenuItem("W interrupt", "W interrupt").SetValue(true));
            Misc.AddItem(new MenuItem("W gapcloser", "W gapcloser").SetValue(true));
            Menu Draw = Menu.AddSubMenu(new Menu("Draw", "Draw"));
            Draw.AddItem(new MenuItem("Draw dmg text", "Draw dmg text").SetValue(false));
			Draw.AddItem(new MenuItem("DrawTargetCircle", "Draw Target Circle").SetValue(false));
            Menu other = Menu.AddSubMenu(new Menu("other", "other"));
            other.AddItem(new MenuItem("Flee", "Flee").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            other.AddItem(new MenuItem("WallJumpHelper", "WallJumpHelper").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            other.AddItem(new MenuItem("FastHarass", "FastHarass").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            Menu Clear = Menu.AddSubMenu(new Menu("Clear", "Clear"));
            Clear.AddItem(new MenuItem("Use Tiamat", "Use Tiamat").SetValue(true));
            Clear.AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            Clear.AddItem(new MenuItem("Use W", "Use W").SetValue(true));
            Clear.AddItem(new MenuItem("Use E", "Use E").SetValue(true));
            TargetSelector.AddToMenu(ts);
            Menu.AddToMainMenu();

            Drawing.OnDraw += OnDraw;

            Game.OnUpdate += Game_OnGameUpdate;
            //Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.OnAttack += OnAttack;
            Obj_AI_Base.OnProcessSpellCast += oncast;
            Interrupter2.OnInterruptableTarget += interrupt;
            AntiGapcloser.OnEnemyGapcloser += gapcloser;
            Game.PrintChat("Welcome to Heaven Strike Riven");
        }
        public static void flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var x = Player.Position.Extend(Game.CursorPos, 300);
            if (Q.IsReady() && !Player.IsDashing()) Q.Cast(Game.CursorPos);
            if (E.IsReady() && !Player.IsDashing()) E.Cast(x);
        }
        public static void walljump()
        {
            var x = Player.Position.Extend(Game.CursorPos, 100);
            var y = Player.Position.Extend(Game.CursorPos, 30);
            if (!x.IsWall() && !y.IsWall()) Player.IssueOrder(GameObjectOrder.MoveTo, x);
            if (x.IsWall() && !y.IsWall()) Player.IssueOrder(GameObjectOrder.MoveTo, y);
            //if (y.IsWall() && Q.IsReady()) { Player.IssueOrder(GameObjectOrder.HoldPosition,Player.Position); Q.Cast(Game.CursorPos); }
            if (Prediction.GetPrediction(Player, 500).UnitPosition.Distance(Player.Position) <= 10) { Q.Cast(Game.CursorPos); }
        }
        public static void fastharass()
        {
            var target = TargetSelector.GetTarget(500, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget() || target.IsZombie || Orbwalking.CanMove(80)) Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (target != null && target.IsValidTarget() && !target.IsZombie && Orbwalking.CanAttack() && Orbwalker.InAutoAttackRange(target)) Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                if (!Player.IsDashing() && E.IsReady() && Orbwalking.CanMove(80) && Q.IsReady())
                    E.Cast(Player.Position.Extend(target.Position, 200));
            }
        }
        public static void interrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy && W.IsReady() && sender.IsValidTarget() && !sender.IsZombie && Menu.Item("W interrupt").GetValue<bool>())
            {
                if (sender.IsValidTarget(125 + Player.BoundingRadius + sender.BoundingRadius)) W.Cast();
            }
        }
        public static void gapcloser(ActiveGapcloser gapcloser)
        {
            var target = gapcloser.Sender;
            if (target.IsEnemy && W.IsReady() && target.IsValidTarget() && !target.IsZombie && Menu.Item("W gapcloser").GetValue<bool>())
            {
                if (target.IsValidTarget(125 + Player.BoundingRadius + target.BoundingRadius)) W.Cast();
            }
        }
        public static void OnDraw(EventArgs args)
        {
			if (Player.IsDead) return;
			if (Menu.Item("DrawTargetCircle").GetValue<bool>())
			{
				var target = TargetSelector.GetSelectedTarget();
				if (target != null && target.IsValidTarget() && !target.IsZombie)
					Render.Circle.DrawCircle(target.Position, 150, Color.AliceBlue, 15);
            }
			if (Menu.Item("Draw dmg text").GetValue<bool>())
			{
                foreach (var hero in HeroManager.Enemies)
                {
                    if (hero.IsValidTarget(1500))
                    {
                        var dmg = totaldame(hero) > hero.Health ? 100 : totaldame(hero) * 100 / hero.Health;
                        var dmg1 = Math.Round(dmg);
                        var x = Drawing.WorldToScreen(hero.Position);
                        Color mau = dmg1 == 100 ? Color.Red : Color.Yellow;
                        Drawing.DrawText(x[0], x[1], mau, dmg1.ToString() + " %");
                    }
                }
			}
        }
        public static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                TTTar = target;
                if (HasItem())
                    forceT = true;
                if (Q.IsReady())
                    forceQ = true;
                if (W.IsReady())
                    forceW = true;
            }
        }
        public static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            //Game.PrintChat(spell.Name);
            if (spell.Name.Contains("ItemTiamatCleave"))
            {
                if (Menu.Item("Burst").GetValue<KeyBind>().Active || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    forceT = false;
                if (Menu.Item("Burst").GetValue<KeyBind>().Active && waitR2)
                    R2countdonw = 1;
            }
            if (spell.Name.Contains("RivenBasicAttack"))
            {
                midAA = true;
                qAA = false;
                cAA = Environment.TickCount;
                if (Menu.Item("Burst").GetValue<KeyBind>().Active && waitR2)
                    R2countdonw = 2;
            }
            if (spell.Name.Contains("RivenTriCleave"))
            {
                //Orbwalker.SetOrbwalkingPoint(Game.CursorPos);


                forceQ = false;
                //Orbwalking.DisableNextAttack = true;
                if (Menu.Item("Q strange Cancel").GetValue<bool>()) Utility.DelayAction.Add(40, () => Game.Say("/d"));
                if (!Menu.Item("WallJumpHelper").GetValue<KeyBind>().Active) Utility.DelayAction.Add(40, () => Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                //Utility.DelayAction.Add(40, () => Game.Say("/d");
                Utility.DelayAction.Add(40, () => reset());
                //Utility.DelayAction.Add(350 - Game.Ping / 2, () => Orbwalking.DisableNextAttack = false);
                qQ = false;
                cQ = Environment.TickCount;
                Qstate = Qstate + 1;
                if (qGap == true)
                {
                    Utility.DelayAction.Add(300, () => qGap = false);
                }

            }
            if (spell.Name.Contains("RivenMartyr"))
            {
                forceW = false;
                qW = false;
                cW = Environment.TickCount;
            }
            if (spell.Name.Contains("RivenFient"))
            {
                forceEburst = false;
                cE = Environment.TickCount;
                qE = false;
                //Utility.DelayAction.Add(200, () => Game.Say("/d"));
                //Utility.DelayAction.Add(200, () => Orbwalking.ResetAutoAttackTimer());
            }

            if (spell.Name.Contains("RivenFengShuiEngine"))
            {
                cR1 = Environment.TickCount;
                castR = false;
                waitR = false; forceR = false;
                qR1 = false;
                Rstate = 1;
                Utility.DelayAction.Add(40, () => Game.Say("/d"));
                if (Environment.TickCount - cAA <= Player.AttackCastDelay * 1000 + 100) Orbwalking.ResetAutoAttackTimer();
            }
            if (spell.Name.Contains("rivenizunablade"))
            {
                R2countdonw = 0;
                cR2 = Environment.TickCount;
                waitR2 = false;
                qR2 = false;
                Rstate = 2;
                R2style = false;
            }
        }
        public static void OnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Menu.Item("Burst").GetValue<KeyBind>().Active)
            {
                if (forceQ || forceW || forceT) { Game.Say("/d"); Orbwalking.ResetAutoAttackTimer(); }
                if (Environment.TickCount - cQ <= 150 - Game.Ping && Environment.TickCount > cQ) { Game.Say("/d"); Orbwalking.ResetAutoAttackTimer(); }
            }
            if (Environment.TickCount - cQ > 150 - Game.Ping && !forceQ && !forceW && !forceT)
            {
                if (Menu.Item("Burst").GetValue<KeyBind>().Active || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Utility.DelayAction.Add((int)(Player.AttackCastDelay * 1000 + 80 - Game.Ping / 2), () => fforce(target));
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && HeroManager.Enemies.Contains(target))
                {
                    Utility.DelayAction.Add((int)(Player.AttackCastDelay * 1000 + 80 - Game.Ping / 2), () => fforce(target));
                }
                if (Menu.Item("FastHarass").GetValue<KeyBind>().Active)
                {
                    Utility.DelayAction.Add((int)(Player.AttackCastDelay * 1000 + 80 - Game.Ping / 2), () => fforce(target));
                }
            }
        }
        public static void fforce(AttackableUnit target)
        {
            //if (Menu.Item("Burst").GetValue<KeyBind>().Active && waitR2)
            //    R2countdonw = 2;
            //if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            //{
            //    if (Menu.Item("Use Q").GetValue<bool>() && Q.IsReady()) forceQ = true;
            //    if (Menu.Item("Use W").GetValue<bool>() && W.IsReady()) forceW = true;
            //    if (HasItem()) forceT = true;
            //}
            //if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            //{
                countforce = Environment.TickCount;
                TTTar = target;
                if (HasItem())
                    forceT = true;
                if (Q.IsReady())
                    forceQ = true;
                if (W.IsReady())
                    forceW = true;
            //}
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Menu.Item("Use Q Before Expiry").GetValue<bool>() && !Player.IsRecalling()) Qexpiry();
            if (!Menu.Item("Burst").GetValue<KeyBind>().Active) { R2countdonw = 0; forceEburst = false; }
            if (Environment.TickCount - countforce >= 700) { forceQ = false; forceT = false; forceW = false; }
            if (Player.IsDashing()) { Orbwalker.SetAttack(false); } else { Orbwalker.SetAttack(true); }
            check();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Menu.Item("Burst").GetValue<KeyBind>().Active)
            { beforetest(); test(); }
            if (Menu.Item("Burst").GetValue<KeyBind>().Active)
            { Burst(); }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            { beforetest(); }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            { beforetest(); }
            if (Menu.Item("Flee").GetValue<KeyBind>().Active)
            { flee(); }
            if (Menu.Item("WallJumpHelper").GetValue<KeyBind>().Active)
            { walljump(); }
            if (Menu.Item("FastHarass").GetValue<KeyBind>().Active)
            { fastharass(); beforetest(); }
        }
        public static void Qexpiry()
        {
            //Game.PrintChat((Environment.TickCount - cQ).ToString());
            if (Qstate != 1 && Environment.TickCount - cQ <= 3800 - Game.Ping / 2 && Environment.TickCount - cQ >= 3300 - Game.Ping / 2) { Q.Cast(Game.CursorPos); }
        }
        public static void check()
        {
            if (!Q.IsReady(1000)) Qstate = 1;
            if (R.IsReady() && Environment.TickCount - cR1 <= 15000) Rstate = 1;
            if (!R.IsReady() && Environment.TickCount - cR1 <= 15000) Rstate = 2;
            if (!R.IsReady() && Environment.TickCount - cR1 > 15000) Rstate = 0;
            if (R.IsReady() && cR2 > cR1) Rstate = 0;
        }
        public static void beforetest()
        {
            //if (TTTar != null && TTTar.IsValidTarget() && !TTTar.IsZombie && Player.Distance(TTTar.Position) <= 500)
            //{
            //if (qGap == true) Q.Cast(Game.CursorPos);
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (TTTar.IsValidTarget() && Player.Distance(TTTar.Position) <= 300)
                {
                    if (R2style == true && !forceW && !forceT)
                        if (Qstate == 3 || !Q.IsReady(1000))
                            R.Cast(TTTar.Position);
                    if (castR == true) R.Cast();
                    if (forceT == true && !W.IsReady()) CastItem();
                    if (forceW == true) W.Cast();
                    if (forceQ == true && !W.IsReady() && forceT == false) Q.Cast(TTTar.Position);
                }
                if (TTTar.IsValidTarget() && Player.Distance(TTTar.Position) >= 500)
                {
                    R2style = false;
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (forceT == true && Menu.Item("Use Tiamat").GetValue<bool>()) CastItem(); else forceT = false;
                if (forceW == true && Menu.Item("Use W").GetValue<bool>()) W.Cast(); else forceW = false;
                if (forceQ == true && Menu.Item("Use Q").GetValue<bool>()) Q.Cast(Game.CursorPos); else forceQ = false;
                if (TTTar.IsValidTarget() && Player.Distance(TTTar.Position) <= 300 && E.IsReady() && Orbwalking.CanMove(80) && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Menu.Item("Use E").GetValue<bool>()) E.Cast(TTTar.Position);
            }
            //}
        }
        public static void test()
        {
            var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (target != null && target.IsValidTarget() && !target.IsZombie && !Menu.Item("Burst").GetValue<KeyBind>().Active)
            {
                if (target.IsValidTarget(500) && !Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && E.IsReady() && !Player.IsDashing() && Menu.Item("Ecombo").GetValue<bool>())
                {
                    if (R.IsReady() && Rstate == 0 && Menu.Item("RcomboAlways").GetValue<bool>())
                    {
                        if (target.Health <= basicdmg(target) && Utility.CountEnemiesInRange(target.Position, 600) >= 2)
                            R.Cast();
                        if (target.Health > basicdmg(target))
                            R.Cast();
                    }
                    if (R.IsReady() && Rstate == 0 && Menu.Item("RcomboKillable").GetValue<bool>() && target.Health + 50 <= totaldame(target))
                    {
                        if (target.Health <= basicdmg(target) && Utility.CountEnemiesInRange(target.Position, 600) >= 2)
                            R.Cast();
                        if (target.Health > basicdmg(target))
                            R.Cast();
                    }
                    E.Cast(Player.Position.Extend(target.Position, 200));
                }
                if (target.IsValidTarget(500) && Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && E.IsReady() && !Player.IsDashing() && Menu.Item("Ecombo").GetValue<bool>())
                {
                    E.Cast(Player.Position.Extend(target.Position, 200));
                }
                if (!Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && Q.IsReady() && !Player.IsDashing() && !forceQ && Menu.Item("Q Gap").GetValue<bool>()
                    && Environment.TickCount - cQ >= 1000)
                {
                    //Game.PrintChat("ottoke");
                    //Orbwalker.SetMovement(false);
                    //if (qGap == false) Player.IssueOrder(GameObjectOrder.MoveTo, target);
                    //qGap = true;
                    //if (qGap == true) Q.Cast(Game.CursorPos);
                    //Orbwalker.SetMovement(true);
                    //Game.PrintChat("gap");
                    //qGap = true;
                    if (Prediction.GetPrediction(Player, 100).UnitPosition.Distance(target.Position) <= Player.Distance(target.Position))
                        //Game.PrintChat("uni");
                        Q.Cast(Game.CursorPos);
                }
                if (R.IsReady() && Rstate == 0)
                {
                    if (Menu.Item("RcomboAlways").GetValue<bool>())
                    {
                        if (Orbwalking.InAutoAttackRange(target))
                        {
                            if (target.Health <= basicdmg(target) && Utility.CountEnemiesInRange(target.Position, 600) >= 2)
                                castR = true;
                            if (target.Health > basicdmg(target))
                                castR = true;
                        }
                    }
                    if (Menu.Item("RcomboKillable").GetValue<bool>() && target.Health + 50 <= totaldame(target))
                    {
                        if (Orbwalking.InAutoAttackRange(target))
                        {
                            if (target.Health <= basicdmg(target) && Utility.CountEnemiesInRange(target.Position, 600) >= 2)
                                castR = true;
                            if (target.Health > basicdmg(target))
                                castR = true;
                        }
                    }
                }
                if (R.IsReady() && Rstate == 1)
                {
                    if (Menu.Item("R2comboKS").GetValue<bool>())
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(1000) && !x.IsZombie))
                        {
                            if (hero.Health <= Rdame(hero, hero.Health))
                            {
                                var x = Prediction.GetPrediction(hero, (2200 - hero.MoveSpeed) / hero.Distance(Player.Position)).UnitPosition.Distance(Player.Position);
                                var y = Prediction.GetPrediction(hero, (2200 - hero.MoveSpeed) / hero.Distance(Player.Position)).UnitPosition;
                                if (x <= 900)
                                    R.Cast(y);
                            }
                        }
                    }
                    if (Menu.Item("R2comboMaxdmg").GetValue<bool>())
                    {
                        if (target.Health / target.MaxHealth <= 0.25)
                            R.Cast(target.Position);
                    }
                    if (Menu.Item("R2 Badao Style").GetValue<bool>())
                    {
                        R2style = true;
                    }
                }
            }
        }
        public static void Burst()
        {
            if (Menu.Item("Burst").GetValue<KeyBind>().Active && waitR2 && R2countdonw == 2 && !HasItem())
                R2countdonw = 1;
            var target = TargetSelector.GetSelectedTarget();
            if (forceEburst == true) E.Cast(Player.Position.Extend(target.Position, 200));
            if (target == null || !target.IsValidTarget() || target.IsZombie || Orbwalking.CanMove(80) && Menu.Item("Burst").GetValue<KeyBind>().Active) Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (target != null && target.IsValidTarget() && !target.IsZombie && Orbwalking.CanAttack() && Orbwalker.InAutoAttackRange(target) && Menu.Item("Burst").GetValue<KeyBind>().Active) Player.IssueOrder(GameObjectOrder.AttackUnit, target);

            if (target != null && target.IsValidTarget() && !target.IsZombie && Menu.Item("Burst").GetValue<KeyBind>().Active && R.IsReady() && Rstate != 2)
            {
                if (Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && R.IsReady() && !waitR2)
                {
                    if (Rstate == 0) R.Cast();
                    W.Cast();
                    waitR2 = true;
                    R2countdonw = 3;
                }
                if (!Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && E.IsReady() && R.IsReady() && Player.Distance(target.Position) <= E.Range + Player.BoundingRadius + target.BoundingRadius && !waitR2)
                {
                    if (Rstate == 0) R.Cast();
                    //E.Cast(Player.Position.Extend(target.Position, 200));
                    forceEburst = true;
                    W.Cast();
                    waitR2 = true;
                    R2countdonw = 3;
                }
                if (!Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && !E.IsReady() && R.IsReady() && Player.Distance(target.Position) <= 425 + Player.BoundingRadius + target.BoundingRadius
                    && flash != SpellSlot.Unknown && flash.IsReady() && Menu.Item("Use Flash").GetValue<bool>() & Player.Distance(target.Position) <= 425 + Player.BoundingRadius + target.BoundingRadius && !waitR2)
                {
                    if (Rstate == 0) R.Cast();
                    var x = Player.Distance(target.Position) > 425 ? Player.Position.Extend(target.Position, 425) : target.Position;
                    Player.Spellbook.CastSpell(flash, x);
                    W.Cast();
                    waitR2 = true;
                    R2countdonw = 3;
                }
                if (!Orbwalking.InAutoAttackRange(target) && Orbwalking.CanMove(80) == true && E.IsReady() && flash != SpellSlot.Unknown && flash.IsReady() && Menu.Item("Use Flash").GetValue<bool>()
                    && R.IsReady() && Player.Distance(target.Position) <= E.Range + Player.BoundingRadius + target.BoundingRadius + 425
                    && Player.Distance(target.Position) > Player.BoundingRadius + target.BoundingRadius + 425 && !waitR2)
                {
                    if (Rstate == 0) R.Cast();
                    forceEburst = true;
                    Utility.DelayAction.Add(550, () => flashstun(target));
                    waitR2 = true;
                    R2countdonw = 3;
                }
            }
            if (target != null && target.IsValidTarget() && !target.IsZombie && Menu.Item("Burst").GetValue<KeyBind>().Active && R.IsReady() && Rstate == 1 && waitR2)
            {
                if (!Orbwalking.CanAttack() && forceT == false && target.Distance(Player.Position) <= 600 && R2countdonw == 1)
                {
                    R.Cast(target.Position);
                }
            }
        }
        public static void flashstun(Obj_AI_Base target)
        {
            var x = Player.Distance(target.Position) > 425 ? Player.Position.Extend(target.Position, 425) : target.Position;
            Player.Spellbook.CastSpell(flash, x);
            W.Cast();
        }
        public static void cast()
        {
            var target = TargetSelector.GetTarget(500, TargetSelector.DamageType.Physical);
            if (qW == true)
                W.Cast();
            if (qE == true && qW == false && qQ == false)
                E.Cast(Player.Position.Extend(target.Position, 200));
            if (qQ == true && !W.IsReady())
                Q.Cast(Game.CursorPos);
        }
        public static bool HasItem()
        {
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void CastItem()
        {

            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
        }
        public static void reset()
        {
            Orbwalking.ResetAutoAttackTimer();
        }
        public static void checkbuff()
        {
            String temp = "";
            foreach (var buff in Player.Buffs)
            {
                temp += (buff.Name + "(" + buff.Count + ")" + ", ");
            }
            Game.PrintChat(temp);
        }
        public static double basicdmg(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18) { passivenhan = 0.5; }
                else if (Player.Level >= 15) { passivenhan = 0.45; }
                else if (Player.Level >= 12) { passivenhan = 0.4; }
                else if (Player.Level >= 9) { passivenhan = 0.35; }
                else if (Player.Level >= 6) { passivenhan = 0.3; }
                else if (Player.Level >= 3) { passivenhan = 0.25; }
                else { passivenhan = 0.2; }
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Qstate;
                    dmg = dmg + Q.GetDamage(target) * qnhan + Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }
                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                return dmg;
            }
            else { return 0; }
        }
        public static double totaldame(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan = 0;
                if (Player.Level >= 18) { passivenhan = 0.5; }
                else if (Player.Level >= 15) { passivenhan = 0.45; }
                else if (Player.Level >= 12) { passivenhan = 0.4; }
                else if (Player.Level >= 9) { passivenhan = 0.35; }
                else if (Player.Level >= 6) { passivenhan = 0.3; }
                else if (Player.Level >= 3) { passivenhan = 0.25; }
                else { passivenhan = 0.2; }
                if (HasItem()) dmg = dmg + Player.GetAutoAttackDamage(target) * 0.7;
                if (W.IsReady()) dmg = dmg + W.GetDamage(target);
                if (Q.IsReady())
                {
                    var qnhan = 4 - Qstate;
                    dmg = dmg + Q.GetDamage(target) * qnhan + Player.GetAutoAttackDamage(target) * qnhan * (1 + passivenhan);
                }
                dmg = dmg + Player.GetAutoAttackDamage(target) * (1 + passivenhan);
                if (R.IsReady())
                {
                    if (Rstate == 0)
                    {
                        var rdmg = Rdame(target, target.Health - dmg * 1.2);
                        return dmg * 1.2 + rdmg;
                    }
                    else if (Rstate == 1)
                    {
                        var rdmg = Rdame(target, target.Health - dmg);
                        return rdmg + dmg;
                    }
                    else return dmg;
                }
                else return dmg;
            }
            else return 0;
        }
        public static double Rdame(Obj_AI_Base target, double health)
        {
            if (target != null)
            {
                var missinghealth = (target.MaxHealth - health) / target.MaxHealth > 0.75 ? 0.75 : (target.MaxHealth - health) / target.MaxHealth;
                var pluspercent = missinghealth * (8 / 3);
                var rawdmg = new double[] { 80, 120, 160 }[R.Level - 1] + 0.6 * Player.FlatPhysicalDamageMod;
                return Player.CalcDamage(target, Damage.DamageType.Physical, rawdmg * (1 + pluspercent));
            }
            else return 0;
        }
    }

}
