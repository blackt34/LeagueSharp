using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeDraven
{

    class Axe
    {
        public Axe(GameObject obj)
        {
            AxeObj = obj;
            EndTick = Environment.TickCount + 1200;
        }

        public int EndTick;
        public GameObject AxeObj;
    }

    class AxeCatcher
    {
        public static List<Axe> AxeSpots = new List<Axe>();
        private static Obj_AI_Minion _prevMinion;
        public static int CurrentAxes;
        public static int LastAa;
        public static int LastQ;
        public static List<String> AxesList = new List<string>()
        {
            "Draven_Base_Q_reticle.troy" , "Draven_Skin01_Q_reticle.troy" ,"Draven_Skin03_Q_reticle.troy"
        };

        public static List<String> QBuffList = new List<string>()
        {
            "Draven_Base_Q_buf.troy", "Draven_Skin01_Q_buf.troy", "Draven_Skin02_Q_buf.troy", "Draven_Skin03_Q_buf.troy"
        };

        public static Orbwalking.Orbwalker Orbwalker { get {return Program.Orbwalker; } }
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static int MidAirAxes
        {
            get { return AxeSpots.Count(a => a.AxeObj.IsValid && a.EndTick < Environment.TickCount); }
        }
        public static float RealAutoAttack(Obj_AI_Base target){
                return (float) Player.CalcDamage(target, Damage.DamageType.Physical, (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod +
                    (((Program.spells[Spells.Q].Level) > 0 && HasQBuff ? new float[] {45, 55, 65, 75, 85 }[Program.spells[Spells.Q].Level - 1] : 0 ) / 100 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod))));
        }

        public static void GameOnOnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN || !Program.Config.Item("clickRemoveAxes").GetValue<bool>())
            {
                return;
            }

            for (var i = 0; i < AxeSpots.Count; i++)
            {
                if ((AxeSpots[i].AxeObj.Position.Distance(Game.CursorPos) < 110))
                {
                    AxeSpots.RemoveAt(i);
                    Notifications.AddNotification(new Notification("Removed Axe", 1));
                }
            }
        }

        public static void Draw()
        {
            if (Program.Config.Item("DCR").GetValue<Circle>().Active)
            {
                var mode = Program.Config.Item("catchRadiusMode").GetValue<StringList>().SelectedIndex;

                switch (mode)
                {
                    case 1:
                        new Geometry.Sector(Player.Position.To2D(), Game.CursorPos.To2D() - Player.Position.To2D() , Program.Config.Item("sectorAngle").GetValue<Slider>().Value * (float) Math.PI / 180, Program.Config.Item("catchRadius").GetValue<Slider>().Value).ToPolygon().Draw(Program.Config.Item("DCR").GetValue<Circle>().Color, 1);
                        break;
                    default:
                        Render.Circle.DrawCircle(
                            Game.CursorPos, Program.Config.Item("catchRadius").GetValue<Slider>().Value, Program.Config.Item("DCR").GetValue<Circle>().Color);
                        break;

                }
            }

            if (Program.Config.Item("DABR").GetValue<bool>())
                return;

            if (Program.Config.Item("DKM").GetValue<Circle>().Active)
            {
                var a =
                    MinionManager.GetMinions(
                        Player.Position, 800, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth)
                        .Where(minion => minion.Health <= RealAutoAttack(minion)).ToArray();
                for (int i = 0; i < (a.Count() < 10 ? a.Count() : 10); i++)
                {
                    Render.Circle.DrawCircle(a[i].Position, a[i].BoundingRadius + 20, Program.Config.Item("DKM").GetValue<Circle>().Color);
                }
            }


            if (Program.Config.Item("DE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, Program.spells[Spells.E].Range, Program.Config.Item("DE").GetValue<Circle>().Color);
            }

            if (Program.Config.Item("DR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, Program.spells[Spells.R].Range, Program.Config.Item("DR").GetValue<Circle>().Color);
            }

            if (Program.Config.Item("DAR").GetValue<Circle>().Active)
            {
                foreach (var axe in AxeSpots)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(axe.AxeObj.Position).X-32, Drawing.WorldToScreen(axe.AxeObj.Position).Y, Color.Aqua, (((float) (axe.EndTick - Environment.TickCount))) + " ms");
                    Render.Circle.DrawCircle(axe.AxeObj.Position, 120, InCatchRadius(axe) ? Program.Config.Item("DAR").GetValue<Circle>().Color : Color.DeepSkyBlue);
                }
            }

            if (Program.Config.Item("DCA").GetValue<Circle>().Active)
            {
                Drawing.DrawText(
                    Drawing.WorldToScreen(Player.Position).X - 70, Drawing.WorldToScreen(Player.Position).Y + 60,
                    Program.Config.Item("DCA").GetValue<Circle>().Color, "Current Axes: " + CurrentAxes);
            }

            if (Program.Config.Item("DCS").GetValue<Circle>().Active)
            {
                Drawing.DrawText(
                    Drawing.WorldToScreen(Player.Position).X - 70, Drawing.WorldToScreen(Player.Position).Y + 40,
                    Program.Config.Item("DCS").GetValue<Circle>().Color, "Catching Active:  " + Program.Config.Item("catching").GetValue<KeyBind>().Active);
            }
        }

        public static bool HasQBuff {get{ return Player.Buffs.Any(a => a.DisplayName.ToLower().Contains("spinning"));}}

        public static bool CanAa { get { return Utils.TickCount + Game.Ping / 2 + 25 >= LastAa + Player.AttackDelay * 1000; } }

        public static bool CanMove { get { return Utils.TickCount + Game.Ping / 2 >= LastAa + Player.AttackCastDelay * 1000 + Program.Config.Item("ExtraWindup").GetValue<Slider>().Value;} }

        public static bool IsCatching { get { return AxeSpots.Count > 0; } }

        public static bool InCatchRadius(Axe a)
        {
            var mode = Program.Config.Item("catchRadiusMode").GetValue<StringList>().SelectedIndex;
            switch (mode)
            {
                case 1:
                    var b = new Geometry.Sector(Player.Position.To2D(), Game.CursorPos.To2D() - Player.Position.To2D(), Program.Config.Item("sectorAngle").GetValue<Slider>().Value * (float)Math.PI / 180, Program.Config.Item("catchRadius").GetValue<Slider>().Value).ToPolygon()
                        .IsOutside(a.AxeObj.Position.Extend(Game.CursorPos, 30).To2D());
                    return !b;
                default:
                    return a.AxeObj.Position.Distance(Game.CursorPos) <
                           Program.Config.Item("catchRadius").GetValue<Slider>().Value;
            }
        }

        public static void Orbwalk(Vector3 pos, bool moveOnly = false)
        {
            if (!pos.IsValid())
                pos = Game.CursorPos;

            if (CanAa && GetTarget() != null)
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, GetTarget());
            }
            else if (CanMove && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
            {
                if (Game.CursorPos.Distance(Player.Position) < Program.Config.Item("HoldPosRadius").GetValue<Slider>().Value)
                {
                    Player.IssueOrder(GameObjectOrder.HoldPosition, Player.Position);
                    return;
                }
                Player.IssueOrder(GameObjectOrder.MoveTo, pos);
            }
        }

        public static void catchAxes()
        {
            Orbwalker.SetAttack(false);
            Orbwalker.SetMovement(false);
            var axeMinValue = int.MaxValue;
            Axe selectedAxe = null;

            foreach (var axe in AxeSpots.Where(a => a.AxeObj.IsValid))
            {
                if (axeMinValue > axe.EndTick)
                {
                    axeMinValue = axe.EndTick;
                    selectedAxe = axe;
                }
            }

            var turret = ObjectManager.Get<Obj_Turret>().FirstOrDefault(t => t.IsEnemy && t.Position.Distance(Player.Position) < 2000 && t.Health > 0);
            for (var i = 0; i < AxeSpots.Count; i++)
            {
                if (AxeSpots[i].EndTick < Environment.TickCount || (Program.Config.Item("ignoreTowerReticle").GetValue<bool>() && (turret != null && AxeSpots[i].AxeObj.Position.Distance(turret.Position) < 1000)))
                {
                    AxeSpots.RemoveAt(i);
                    return;
                }
            }

            if (GetTarget() != null && GetTarget().IsValidTarget())
            {
                Orbwalker.ForceTarget(GetTarget() as Obj_AI_Base);
            }
                
            else if(selectedAxe != null && Player.Distance(selectedAxe.AxeObj.Position) < 100 && InCatchRadius(selectedAxe))
            {
                Orbwalker.ForceTarget(null);
                Player.IssueOrder(GameObjectOrder.MoveTo, selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 95));
                return;

                 if (AxeSpots.Count == 2 && Program.Config.Item("useWCatch").GetValue<bool>() && Program.spells[Spells.W].IsReady() && !CanMakeIt((int) (((selectedAxe.EndTick / 1000) - (Environment.TickCount / 1000)) * Player.MoveSpeed)) &&
                    (selectedAxe.AxeObj.Position.Distance(Player.Position) <
                     ((selectedAxe.EndTick / 1000 - Environment.TickCount / 1000) *
                      (Player.MoveSpeed *
                       new[] { 1.40f, 1.45f, 1.50f, 1.55f, 1.60f }[Program.spells[Spells.W].Level - 1]))))
                {
                    Program.spells[Spells.W].Cast();
                    Player.IssueOrder(GameObjectOrder.MoveTo, selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 95));
                    return;
                }
            }
                  

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
            {
                if (selectedAxe == null || AxeSpots.Count == 0 || Player.Distance(selectedAxe.AxeObj.Position) <= 100 ||
                    GetTarget().IsValid<Obj_AI_Hero>() &&
                    Player.GetAutoAttackDamage(GetTarget() as Obj_AI_Base) * 2 > GetTarget().Health ||
                    !Program.Config.Item("catching").GetValue<KeyBind>().Active || !InCatchRadius(selectedAxe))
                {
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    if (selectedAxe != null && Player.Distance(selectedAxe.AxeObj.Position) < 100)
                        return;
                    Orbwalker.SetMovement(true);
                    return;
                }
                if ((Player.AttackDelay +
                     ((Player.Distance(selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 100)) / Player.MoveSpeed) *
                      1000) + Environment.TickCount < selectedAxe.EndTick &&
                     GetTarget().IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && CanAa || Player.Distance(selectedAxe.AxeObj.Position) <= 100))
                {
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    if (Player.Distance(selectedAxe.AxeObj.Position) < 100)
                        return;
                    Orbwalker.SetMovement(true);
                    return;
                }
            }

            if (selectedAxe == null) return;

            if (Player.Distance(selectedAxe.AxeObj.Position) < 100 || !InCatchRadius(selectedAxe))
                return;


            Orbwalker.SetMovement(true);
            Player.IssueOrder(GameObjectOrder.MoveTo, selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 95));
            return;

            if (AxeSpots.Count == 2 && Program.Config.Item("useWCatch").GetValue<bool>() && Program.spells[Spells.W].IsReady() &&
                    selectedAxe.AxeObj.Position.Distance(Player.Position) >
                    ((selectedAxe.EndTick / 1000 - Environment.TickCount / 1000) * (Player.MoveSpeed)) &&
                    (selectedAxe.AxeObj.Position.Distance(Player.Position) <
                     ((selectedAxe.EndTick / 1000 - Environment.TickCount / 1000) *
                      (Player.MoveSpeed *
                       new[] { 1.40f, 1.45f, 1.50f, 1.55f, 1.60f }[Program.spells[Spells.W].Level - 1]))))
                {
                    Program.spells[Spells.W].Cast();
                    Orbwalker.SetMovement(true);
                    Player.IssueOrder(GameObjectOrder.MoveTo, selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 95));
                    return;
                }
        }


        public static bool CanMakeIt(int time)
        {
            var axeMinValue = int.MaxValue;
            Axe selectedAxe = null;

            foreach (var axe in AxeSpots.Where(a => a.AxeObj.IsValid))
            {
                if (axeMinValue > axe.EndTick)
                {
                    axeMinValue = axe.EndTick;
                    selectedAxe = axe;
                }
            }

            if (selectedAxe == null) return true;

            return time +
                   ((Player.Distance(selectedAxe.AxeObj.Position.Extend(Game.CursorPos, 100))/Player.MoveSpeed)*1000) +
                   Environment.TickCount < selectedAxe.EndTick;
        }


        //Events

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.IsAutoAttack())
            {
                LastAa = Environment.TickCount;
            }
            if (args.SData.Name == "dravenspinning")
            {
                LastQ = Environment.TickCount;
            }
        }

        public static void OnCreate(GameObject sender, EventArgs args)
        {
            var Name = sender.Name;

            if ((AxesList.Contains(Name)) &&
                sender.Position.Distance(ObjectManager.Player.Position) / ObjectManager.Player.MoveSpeed <= 2)
            {
                AxeSpots.Add(new Axe(sender));
            }

            if ((QBuffList.Contains(Name)) &&
                sender.Position.Distance(ObjectManager.Player.Position) < 100)
            {
                CurrentAxes += 1;
            }
        }

        public static void OnDelete(GameObject sender, EventArgs args)
        {
            for (var i = 0; i < AxeSpots.Count; i++)
            {
                if (AxeSpots[i].AxeObj.NetworkId == sender.NetworkId)
                {
                    AxeSpots.RemoveAt(i);
                    return;
                }
            }

            if ((QBuffList.Contains(sender.Name)) &&
                sender.Position.Distance(ObjectManager.Player.Position) < 300)
            {
                if (CurrentAxes == 0)
                    CurrentAxes = 0;
                if (CurrentAxes <= 2)
                    CurrentAxes = CurrentAxes - 1;
                else CurrentAxes = CurrentAxes - 1;
            }
        }


        // Orbwalker stuff

        private static bool ShouldWait()
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                        minion =>
                            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral &&
                            Orbwalking.InAutoAttackRange(minion) &&
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int)((Player.AttackDelay * 1000) * 2), Program.Config.Item("FarmDelay").GetValue<Slider>().Value) <= RealAutoAttack(minion));
        }

        public static AttackableUnit GetTarget()
        {
            AttackableUnit result = null;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                return null;
            /*Killable Minion*/
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ||
                Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                foreach (var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.IsValidTarget() && Orbwalker.InAutoAttackRange(minion) &&
                                minion.Health <
                                2 *
                                (RealAutoAttack(minion)))
                    )
                {
                    var t = (int)(Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                            1000 * (int)Player.Distance(minion) / (int)Orbwalking.GetMyProjectileSpeed();
                    var predHealth = HealthPrediction.GetHealthPrediction(minion, t, Program.Config.Item("FarmDelay").GetValue<Slider>().Value);

                    if (minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, true))
                    {
                        if (predHealth > 0 && predHealth <= RealAutoAttack(minion))
                        {
                            return minion;
                        }
                    }
                }
            }

            /* turrets / inhibitors / nexus */
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                /* turrets */
                foreach (var turret in
                    ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && Orbwalker.InAutoAttackRange(t)))
                {
                    return turret;
                }

                /* inhibitor */
                foreach (var turret in
                    ObjectManager.Get<Obj_BarracksDampener>().Where(t => t.IsValidTarget() && Orbwalker.InAutoAttackRange(t)))
                {
                    return turret;
                }

                /* nexus */
                foreach (var nexus in
                    ObjectManager.Get<Obj_HQ>().Where(t => t.IsValidTarget() && Orbwalker.InAutoAttackRange(t)))
                {
                    return nexus;
                }
            }

            /*Champions*/
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                var target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget())
                {
                    return target;
                }
            }

            /*Jungle minions*/
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                result =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            mob =>
                                mob.IsValidTarget() && Orbwalker.InAutoAttackRange(mob) && mob.Team == GameObjectTeam.Neutral)
                        .MaxOrDefault(mob => mob.MaxHealth);
                if (result != null)
                {
                    return result;
                }
            }

            /*Lane Clear minions*/
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (!ShouldWait())
                {
                    if (_prevMinion.IsValidTarget() && Orbwalker.InAutoAttackRange(_prevMinion))
                    {
                        var predHealth = HealthPrediction.LaneClearHealthPrediction(
                            _prevMinion, (int)((Player.AttackDelay * 1000) * 2f), Program.Config.Item("FarmDelay").GetValue<Slider>().Value);
                        if (predHealth >= 2 * RealAutoAttack(_prevMinion) ||
                            Math.Abs(predHealth - _prevMinion.Health) < float.Epsilon)
                        {
                            return _prevMinion;
                        }
                    }

                    result = (from minion in
                                  ObjectManager.Get<Obj_AI_Minion>()
                                      .Where(minion => minion.IsValidTarget() && Orbwalker.InAutoAttackRange(minion))
                              let predHealth =
                                  HealthPrediction.LaneClearHealthPrediction(
                                      minion, (int)((Player.AttackDelay * 1000) * 2f), Program.Config.Item("FarmDelay").GetValue<Slider>().Value)
                              where
                                  predHealth >= 2 * RealAutoAttack(minion) ||
                                  Math.Abs(predHealth - minion.Health) < float.Epsilon
                              select minion).MaxOrDefault(m => m.Health);

                    if (result != null)
                    {
                        _prevMinion = (Obj_AI_Minion)result;
                    }
                }
            }
            return null;
        }

    }
}
