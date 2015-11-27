﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutoJungle.Data;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoJungle
{
    internal class Program
    {
        public static GameInfo _GameInfo = new GameInfo();

        public static Menu menu;

        public static float UpdateLimiter, ResetTimer, GameStateChanging;

        public static readonly Obj_AI_Hero player = ObjectManager.Player;

        public static Random Random = new Random(Environment.TickCount);

        public static ItemHandler ItemHandler;

        #region Main

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_GameInfo.SmiteableMob != null)
            {
                Jungle.CastSmite(_GameInfo.SmiteableMob);
            }
            if (ShouldSkipUpdate())
            {
                return;
            }
            SetGameInfo();
            if (_GameInfo.WaitOnFountain)
            {
                return;
            }
            //Checking Afk
            if (CheckAfk())
            {
                return;
            }
            if (Debug)
            {
                Console.WriteLine("Items: ");
                foreach (var i in player.InventoryItems)
                {
                    Console.WriteLine("\t Name: {0}, ID: {1}({2})", i.IData.TranslatedDisplayName, i.Id, (int) i.Id);
                }
                _GameInfo.Show();
            }
            //Check the camp, maybe its cleared
            CheckCamp();
            //Shopping
            if (Shopping())
            {
                return;
            }

            //Recalling
            if (RecallHander())
            {
                return;
            }
            if (menu.Item("UseTrinket").GetValue<bool>())
            {
                PlaceWard();
            }
            MoveToPos();

            CastSpells();
        }

        private static void PlaceWard()
        {
            if (_GameInfo.ClosestWardPos.IsValid() && Items.CanUseItem(3340))
            {
                Items.UseItem(3340, _GameInfo.ClosestWardPos);
            }
        }

        private static bool CheckAfk()
        {
            if (player.IsMoving || player.IsWindingUp || player.IsRecalling() || player.Level == 1)
            {
                _GameInfo.afk = 0;
            }
            else
            {
                _GameInfo.afk++;
            }
            if (_GameInfo.afk > 15 && !player.InFountain())
            {
                player.Spellbook.CastSpell(SpellSlot.Recall);
                return true;
            }
            return false;
        }

        private static void CheckCamp()
        {
            if (_GameInfo.GameState == State.Positioning)
            {
                if (Helpers.GetRealDistance(player, _GameInfo.MoveTo) < 500 && _GameInfo.MinionsAround == 0 &&
                    player.Level > 1)
                {
                    _GameInfo.CurrentMonster++;
                    if (Debug)
                    {
                        Console.WriteLine("MoveTo: CurrentMonster++1");
                    }
                }

                var probablySkippedMob = Helpers.GetNearest(player.Position, 1000);
                if (probablySkippedMob != null && probablySkippedMob.Distance(_GameInfo.MoveTo) > 200)
                {
                    var monster = _GameInfo.MonsterList.FirstOrDefault(m => probablySkippedMob.Name.Contains(m.name));
                    if (monster != null && monster.Index < 13)
                    {
                        _GameInfo.MoveTo = probablySkippedMob.Position;
                    }
                }
            }
        }

        private static void SetGameInfo()
        {
            ResetDamageTakenTimer();
            AutoLevel.Enable();
            _GameInfo.WaitOnFountain = WaitOnFountain();
            _GameInfo.ShouldRecall = ShouldRecall();
            _GameInfo.GameState = SetGameState();
            _GameInfo.MoveTo = GetMovePosition();
            _GameInfo.Target = GetTarget();
            _GameInfo.MinionsAround = Helpers.getMobs(player.Position, 700).Count;
            _GameInfo.SmiteableMob = Helpers.GetNearest(player.Position);
            _GameInfo.AllyStructures = GetStructures(true, _GameInfo.SpawnPointEnemy);
            _GameInfo.EnemyStructures = GetStructures(false, _GameInfo.SpawnPoint);
            _GameInfo.ClosestWardPos = Helpers.GetClosestWard();
        }

        private static IEnumerable<Vector3> GetStructures(bool ally, Vector3 basePos)
        {
            var turrets =
                ObjectManager.Get<Obj_Turret>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0 && t.Health < t.MaxHealth)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);
            var inhibs =
                ObjectManager.Get<Obj_BarracksDampener>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0 && !t.IsDead && t.Health < t.MaxHealth)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);
            var nexus =
                ObjectManager.Get<Obj_HQ>()
                    .Where(t => t.IsAlly == ally && t.IsValid && t.Health > 0 && !t.IsDead && t.Health < t.MaxHealth)
                    .OrderBy(t => t.Position.Distance(basePos))
                    .Select(t => t.Position);

            return turrets.Concat(inhibs).Concat(nexus);
        }

        #region MainFunctions

        private static bool RecallHander()
        {
            if ((_GameInfo.GameState != State.Positioning && _GameInfo.GameState != State.Retreat) ||
                !_GameInfo.MonsterList.Any(m => m.Position.Distance(player.Position) < 800))
            {
                return false;
            }
            if (Helpers.getMobs(player.Position, 1300).Count > 0)
            {
                return false;
            }
            if (player.InFountain() || player.ServerPosition.Distance(_GameInfo.SpawnPoint) < 1000 && _GameInfo.afk > 2)
            {
                return false;
            }
            if ((_GameInfo.ShouldRecall && !player.IsRecalling() && !player.InFountain()) &&
                (_GameInfo.GameState == State.Positioning ||
                 (_GameInfo.GameState == State.Retreat && _GameInfo.afk > 15)))
            {
                player.Spellbook.CastSpell(SpellSlot.Recall);
                return true;
            }

            if (player.IsRecalling())
            {
                return true;
            }

            return false;
        }

        private static void CastSpells()
        {
            if (_GameInfo.Target == null)
            {
                return;
            }
            switch (_GameInfo.GameState)
            {
                case State.FightIng:
                    _GameInfo.Champdata.Combo();
                    break;
                case State.Ganking:
                    break;
                case State.Jungling:
                    _GameInfo.Champdata.JungleClear();
                    UsePotions();
                    break;
                case State.LaneClear:
                    _GameInfo.Champdata.JungleClear();
                    UsePotions();
                    break;
                case State.Objective:
                    if (_GameInfo.Target is Obj_AI_Hero)
                    {
                        _GameInfo.Champdata.Combo();
                    }
                    else
                    {
                        _GameInfo.Champdata.JungleClear();
                    }
                    break;
                default:
                    break;
            }
        }

        private static void UsePotions()
        {
            if (Items.HasItem(2031) && Items.CanUseItem(2031) && player.HealthPercent < 80 &&
                !player.Buffs.Any(b => b.Name.Equals("ItemCrystalFlask")))
            {
                Items.UseItem(2031);
            }
        }

        private static void MoveToPos()
        {
            if ((_GameInfo.GameState != State.Positioning && _GameInfo.GameState != State.Ganking &&
                 _GameInfo.GameState != State.Retreat && _GameInfo.GameState != State.Grouping) ||
                !_GameInfo.MoveTo.IsValid())
            {
                return;
            }
            if (!Helpers.CheckPath(player.GetPath(_GameInfo.MoveTo)))
            {
                _GameInfo.CurrentMonster++;
                if (Debug)
                {
                    Console.WriteLine("MoveTo: CurrentMonster++2");
                }
            }
            if (_GameInfo.GameState == State.Retreat && _GameInfo.MoveTo.Distance(player.Position) < 100)
            {
                return;
            }
            if (_GameInfo.MoveTo.IsValid() &&
                (_GameInfo.MoveTo.Distance(_GameInfo.LastClick) > 150 || (!player.IsMoving && _GameInfo.afk > 10)))
            {
                player.IssueOrder(GameObjectOrder.MoveTo, _GameInfo.MoveTo);
            }
        }

        private static bool Shopping()
        {
            if (!player.InFountain())
            {
                if (Debug)
                {
                    Console.WriteLine("Shopping: Not in shop - false");
                }
                return false;
            }
            var current =
                ItemHandler.ItemList.Where(i => Items.HasItem(i.ItemId))
                    .OrderByDescending(i => i.Index)
                    .FirstOrDefault();

            if (current != null)
            {
                var currentIndex = current.Index;
                var orderedList =
                    ItemHandler.ItemList.Where(i => !Items.HasItem(i.ItemId) && i.Index > currentIndex)
                        .OrderBy(i => i.Index);
                var itemToBuy = orderedList.FirstOrDefault();
                if (itemToBuy == null)
                {
                    if (Debug)
                    {
                        Console.WriteLine("Shopping: No next Item - false");
                    }
                    return false;
                }
                if (itemToBuy.Price <= player.Gold)
                {
                    player.BuyItem((ItemId) itemToBuy.ItemId);
                    if (itemToBuy.Index > 9 && Items.HasItem(2031))
                    {
                        player.SellItem(player.InventoryItems.First(i => i.Id == (ItemId) 2031).Slot);
                    }
                    var nextItem = orderedList.FirstOrDefault(i => i.Index == itemToBuy.Index + 1);
                    if (nextItem != null)
                    {
                        _GameInfo.NextItemPrice = nextItem.Price;
                    }
                    if (Debug)
                    {
                        Console.WriteLine("Shopping: Shopping- " + itemToBuy.Name + " - true");
                    }
                    return true;
                }
            }
            else
            {
                player.BuyItem((ItemId) ItemHandler.ItemList.FirstOrDefault(i => i.Index == 1).ItemId);
                var nextItem = ItemHandler.ItemList.FirstOrDefault(i => i.Index == 2);
                if (nextItem != null)
                {
                    _GameInfo.NextItemPrice = nextItem.Price;
                }
                return true;
            }


            if (Debug)
            {
                Console.WriteLine("Shopping: End - false");
            }
            return false;
        }

        private static Obj_AI_Base GetTarget()
        {
            switch (_GameInfo.GameState)
            {
                case State.Objective:
                    var obj = Helpers.GetNearest(player.Position, GameInfo.ChampionRange);
                    if (obj != null && (obj.Name.Contains("Dragon") || obj.Name.Contains("Baron")) &&
                        (HealthPrediction.GetHealthPrediction(obj, 3000) + 500 < Jungle.smiteDamage(obj) ||
                         _GameInfo.EnemiesAround == 0))
                    {
                        return obj;
                    }
                    else
                    {
                        return _GameInfo.EnemiesAround > 0 ? Helpers.GetTargetEnemy() : null;
                    }
                    break;
                case State.FightIng:
                    return Helpers.GetTargetEnemy();
                    break;
                case State.Ganking:
                    return null;
                    break;
                case State.Jungling:
                    return Helpers.getMobs(player.Position, 1000).OrderByDescending(m => m.MaxHealth).FirstOrDefault();
                    break;
                case State.LaneClear:
                    return
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .Where(m => !m.UnderTurret(true))
                            .OrderByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.Distance(player))
                            .FirstOrDefault();
                    break;
                case State.Pushing:
                    var enemy = Helpers.GetTargetEnemy();
                    if (enemy != null)
                    {
                        _GameInfo.Target = enemy;
                        _GameInfo.Champdata.Combo();
                        return enemy;
                    }
                    var enemyTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(
                                t =>
                                    t.IsEnemy && !t.IsDead && t.Distance(player) < 2000 &&
                                    Helpers.getAllyMobs(t.Position, 500).Count > 0);
                    if (enemyTurret != null)
                    {
                        _GameInfo.Champdata.JungleClear();
                        return enemyTurret;
                    }
                    var mob =
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .OrderBy(m => m.UnderTurret(true))
                            .ThenByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.Distance(player))
                            .FirstOrDefault();
                    if (mob != null)
                    {
                        _GameInfo.Target = mob;
                        _GameInfo.Champdata.JungleClear();
                        return mob;
                    }
                    break;
                case State.Defending:
                    var enemyDef = Helpers.GetTargetEnemy();
                    if (enemyDef != null && !_GameInfo.InDanger)
                    {
                        _GameInfo.Target = enemyDef;
                        _GameInfo.Champdata.Combo();
                        return enemyDef;
                    }
                    var mobDef =
                        Helpers.getMobs(player.Position, GameInfo.ChampionRange)
                            .OrderByDescending(m => m.CountEnemiesInRange(500) == 0)
                            .ThenByDescending(m => player.GetAutoAttackDamage(m, true) > m.Health)
                            .ThenBy(m => m.CountEnemiesInRange(500))
                            .FirstOrDefault();
                    if (mobDef != null)
                    {
                        _GameInfo.Target = mobDef;
                        _GameInfo.Champdata.JungleClear();
                        return mobDef;
                    }
                    break;
                default:
                    break;
            }

            if (Debug)
            {
                Console.WriteLine("GetTarget: Cant get target");
            }
            return null;
        }


        private static bool CheckObjective(Vector3 pos)
        {
            if ((pos.CountEnemiesInRange(800) > 0 || pos.CountAlliesInRange(800) > 0) && !CheckForRetreat(null, pos))
            {
                var obj = Helpers.GetNearest(pos);
                if (obj != null && obj.Health < obj.MaxHealth - 300)
                {
                    if (player.Distance(pos) > Jungle.smiteRange)
                    {
                        _GameInfo.MoveTo = pos;
                        return true;
                    }
                }
            }
            if (Jungle.SmiteReady() && player.Level >= 9 &&
                player.Distance(Camps.Dragon.Position) < GameInfo.ChampionRange)
            {
                var drake = Helpers.GetNearest(player.Position, GameInfo.ChampionRange);
                if (drake != null && drake.Name.Contains("Dragon"))
                {
                    _GameInfo.CurrentMonster = 13;
                    _GameInfo.MoveTo = drake.Position;
                    return true;
                }
            }
            return false;
        }

        private static bool CheckGanking()
        {
            Obj_AI_Hero gankTarget = null;
            if (player.Level >= menu.Item("GankLevel").GetValue<Slider>().Value &&
                ((player.Mana > _GameInfo.Champdata.R.ManaCost && player.MaxMana > 100) || player.MaxMana <= 100))
            {
                var heroes =
                    HeroManager.Enemies.Where(
                        e =>
                            e.Distance(player) < menu.Item("GankRange").GetValue<Slider>().Value && e.IsValidTarget() &&
                            !e.UnderTurret(true) && !CheckForRetreat(e, e.Position))
                        .OrderBy(
                            e =>
                                _GameInfo.MoveTo.IsValid()
                                    ? _GameInfo.MoveTo.Distance(e.Position)
                                    : e.CountEnemiesInRange(GameInfo.ChampionRange));
                foreach (var possibleTarget in heroes)
                {
                    var myDmg = Helpers.GetComboDMG(player, possibleTarget);
                    if (player.Level + 1 <= possibleTarget.Level)
                    {
                        continue;
                    }
                    if (
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(t => t.IsEnemy && t.IsValidTarget() && t.Distance(possibleTarget) < 1200) !=
                        null)
                    {
                        continue;
                    }
                    if (Helpers.AlliesThere(possibleTarget.Position) + 1 <
                        possibleTarget.Position.CountEnemiesInRange(GameInfo.ChampionRange))
                    {
                        continue;
                    }
                    if (Helpers.GetComboDMG(possibleTarget, player) > player.Health)
                    {
                        continue;
                    }
                    var ally =
                        HeroManager.Allies.Where(a => !a.IsDead && a.Distance(possibleTarget) < 2000)
                            .OrderBy(a => a.Distance(possibleTarget))
                            .FirstOrDefault();
                    var hp = possibleTarget.Health - myDmg * menu.Item("GankFrequency").GetValue<Slider>().Value / 100;
                    if (ally != null)
                    {
                        hp -= Helpers.GetComboDMG(ally, possibleTarget) *
                              menu.Item("GankFrequency").GetValue<Slider>().Value / 100;
                    }
                    if (hp < 0)
                    {
                        gankTarget = possibleTarget;
                        break;
                    }
                }
            }
            if (gankTarget != null)
            {
                var gankPosition =
                    Helpers.GankPos.Where(p => p.Distance(gankTarget.Position) < 2000)
                        .OrderBy(p => player.Distance(gankTarget.Position))
                        .FirstOrDefault();
                if (gankTarget.Distance(player) > 2000 && gankPosition.IsValid() &&
                    gankPosition.Distance(gankTarget.Position) < 2000 &&
                    player.Distance(gankTarget) > gankPosition.Distance(gankTarget.Position))
                {
                    _GameInfo.MoveTo = gankPosition;
                    return true;
                }
                else if (gankTarget.Distance(player) <= 2000)
                {
                    _GameInfo.MoveTo = gankTarget.Position;
                    return true;
                }
                else if (!gankPosition.IsValid())
                {
                    _GameInfo.MoveTo = gankTarget.Position;
                    return true;
                }
            }
            return false;
        }

        private static State SetGameState()
        {
            var enemy = Helpers.GetTargetEnemy();
            State tempstate = State.Null;
            if (CheckForRetreat(enemy, player.Position))
            {
                tempstate = State.Retreat;
            }
            if (tempstate == State.Null && _GameInfo.EnemiesAround == 0 &&
                (CheckObjective(Camps.Baron.Position) || CheckObjective(Camps.Dragon.Position)))
            {
                tempstate = State.Objective;
            }
            if (tempstate == State.Null && player.Level >= 6 && CheckForGrouping())
            {
                if (_GameInfo.MoveTo.Distance(player.Position) <= GameInfo.ChampionRange)
                {
                    if (
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(t => t.Distance(_GameInfo.MoveTo) < GameInfo.ChampionRange && t.IsAlly) !=
                        null && (_GameInfo.GameState == State.Grouping || _GameInfo.GameState == State.Defending))
                    {
                        tempstate = State.Defending;
                    }
                    else if (_GameInfo.GameState == State.Grouping || _GameInfo.GameState == State.Pushing)
                    {
                        tempstate = State.Pushing;
                    }
                }
                if (_GameInfo.MoveTo.Distance(player.Position) > GameInfo.ChampionRange &&
                    (_GameInfo.GameState == State.Positioning || _GameInfo.GameState == State.Grouping))
                {
                    tempstate = State.Grouping;
                }
            }
            if (tempstate == State.Null && enemy != null && _GameInfo.GameState != State.Retreat &&
                _GameInfo.GameState != State.Pushing && _GameInfo.GameState != State.Defending &&
                !CheckForRetreat(enemy, enemy.Position))
            {
                tempstate = State.FightIng;
            }
            if (tempstate == State.Null && _GameInfo.EnemiesAround == 0 &&
                (_GameInfo.GameState == State.Ganking || _GameInfo.GameState == State.Positioning) && CheckGanking())
            {
                tempstate = State.Ganking;
            }
            if (tempstate == State.Null && _GameInfo.MinionsAround > 0 &&
                (_GameInfo.MonsterList.Any(m => m.Position.Distance(player.Position) < 700) ||
                 _GameInfo.SmiteableMob != null) && _GameInfo.GameState != State.Retreat)
            {
                tempstate = State.Jungling;
            }
            if (tempstate == State.Null && CheckLaneClear(player.Position))
            {
                tempstate = State.LaneClear;
            }
            if (tempstate == State.Null)
            {
                tempstate = State.Positioning;
            }
            if (tempstate == _GameInfo.GameState)
            {
                return tempstate;
            }
            else if (Environment.TickCount - GameStateChanging > 3000)
            {
                GameStateChanging = Environment.TickCount;
                return tempstate;
            }
            else
            {
                return _GameInfo.GameState;
            }
        }

        private static bool CheckLaneClear(Vector3 pos)
        {
            return (Helpers.AlliesThere(pos) == 0 || Helpers.AlliesThere(pos) >= 2 ||
                    player.Distance(_GameInfo.SpawnPoint) < 6000 || player.Distance(_GameInfo.SpawnPointEnemy) < 6000) &&
                   pos.CountEnemiesInRange(GameInfo.ChampionRange) == 0 &&
                   Helpers.getMobs(pos, GameInfo.ChampionRange).Count > 0 &&
                   !_GameInfo.MonsterList.Any(m => m.Position.Distance(pos) < 600) && _GameInfo.SmiteableMob == null &&
                   _GameInfo.GameState != State.Retreat;
        }

        private static bool CheckForRetreat(Obj_AI_Base enemy, Vector3 pos)
        {
            if (_GameInfo.GameState == State.Jungling)
            {
                return false;
            }
            var indanger = ((Helpers.GetHealth(true, pos) +
                             ((player.Distance(pos) < GameInfo.ChampionRange) ? 0 : player.Health)) * 1.3f <
                            Helpers.GetHealth(false, pos) && pos.CountEnemiesInRange(GameInfo.ChampionRange) > 0 &&
                            Helpers.AlliesThere(pos, 500) == 0) ||
                           player.HealthPercent < menu.Item("HealtToBack").GetValue<Slider>().Value;
            if (indanger || _GameInfo.AttackedByTurret)
            {
                if ((enemy != null &&
                     enemy.CountEnemiesInRange(GameInfo.ChampionRange) >=
                     enemy.CountAlliesInRange(GameInfo.ChampionRange) + 1 && Helpers.AlliesThere(pos, 500) == 0) ||
                    indanger)
                {
                    return true;
                }
                if (_GameInfo.AttackedByTurret)
                {
                    if ((enemy != null &&
                         (enemy.Health > player.GetAutoAttackDamage(enemy, true) * 2 ||
                          enemy.Distance(player) > Orbwalking.GetRealAutoAttackRange(enemy)) || enemy == null))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool CheckForGrouping()
        {
            //Checking grouping allies
            var ally =
                HeroManager.Allies.FirstOrDefault(
                    a =>
                        a.CountAlliesInRange(GameInfo.ChampionRange) >= 2 &&
                        a.Distance(_GameInfo.SpawnPointEnemy) < 7000);
            if (ally != null && Helpers.CheckPath(player.GetPath(ally.Position), true) &&
                !CheckForRetreat(null, ally.Position))
            {
                _GameInfo.MoveTo = ally.Position.Extend(player.Position, 200);
                return true;
            }
            //Checknig base after recall
            if (player.Distance(_GameInfo.SpawnPoint) < 5000)
            {
                var mobs =
                    MinionManager.GetBestCircularFarmLocation(
                        Helpers.getMobs(_GameInfo.SpawnPoint, 5000).Select(m => m.Position.To2D()).ToList(), 500, 5000);
                if (Helpers.CheckPath(player.GetPath(mobs.Position.To3D())) &&
                    !CheckForRetreat(null, mobs.Position.To3D()))
                {
                    _GameInfo.MoveTo = mobs.Position.To3D();
                }
            }
            //Checknig enemy turrets
            foreach (var vector in
                _GameInfo.EnemyStructures.Where(
                    s => s.Distance(player.Position) < menu.Item("GankRange").GetValue<Slider>().Value))
            {
                var aMinis = Helpers.getAllyMobs(vector, GameInfo.ChampionRange);
                if (!CheckLaneClear(vector))
                {
                    continue;
                }
                if (vector.CountAlliesInRange(GameInfo.ChampionRange) + 1 >
                    vector.CountEnemiesInRange(GameInfo.ChampionRange) && aMinis.Count > 3)
                {
                    var eMinis = Helpers.getMobs(vector, GameInfo.ChampionRange).Select(e => e.Position.To2D()).ToList();
                    if (eMinis.Any())
                    {
                        var pos =
                            (Vector3)
                                MinionManager.GetBestCircularFarmLocation(
                                    eMinis, 500, menu.Item("GankRange").GetValue<Slider>().Value).Position;
                        if (Helpers.CheckPath(player.GetPath(pos)) && !CheckForRetreat(null, pos))
                        {
                            _GameInfo.MoveTo = pos;
                            return true;
                        }
                    }
                    else
                    {
                        if (Helpers.CheckPath(player.GetPath(vector)) && !CheckForRetreat(null, vector))
                        {
                            _GameInfo.MoveTo = vector;
                            return true;
                        }
                    }
                }
            }
            //Checknig ally turrets
            foreach (var vector in
                _GameInfo.AllyStructures.Where(
                    s => s.Distance(player.Position) < menu.Item("GankRange").GetValue<Slider>().Value))
            {
                var eMinis = Helpers.getMobs(vector, GameInfo.ChampionRange);
                if (!CheckLaneClear(vector))
                {
                    continue;
                }
                if (vector.CountAlliesInRange(GameInfo.ChampionRange) + 1 >
                    vector.CountEnemiesInRange(GameInfo.ChampionRange) && eMinis.Count > 3)
                {
                    var temp = eMinis.Select(e => e.Position.To2D()).ToList();
                    if (temp.Any())
                    {
                        var pos =
                            (Vector3)
                                MinionManager.GetBestCircularFarmLocation(
                                    temp, 500, menu.Item("GankRange").GetValue<Slider>().Value).Position;
                        if (Helpers.CheckPath(player.GetPath(pos)) && !CheckForRetreat(null, pos))
                        {
                            _GameInfo.MoveTo = pos;
                            return true;
                        }
                    }
                    else
                    {
                        if (Helpers.CheckPath(player.GetPath(vector)) && !CheckForRetreat(null, vector))
                        {
                            _GameInfo.MoveTo = vector;
                            return true;
                        }
                    }
                }
            }
            //follow minis
            var minis = Helpers.getAllyMobs(player.Position, 1000);
            if (minis.Count >= 6 && player.Level >= 8)
            {
                var objAiBase = minis.OrderBy(m => m.Distance(_GameInfo.SpawnPointEnemy)).FirstOrDefault();
                if (objAiBase != null &&
                    (objAiBase.CountAlliesInRange(GameInfo.ChampionRange) == 0 ||
                     objAiBase.CountAlliesInRange(GameInfo.ChampionRange) >= 2) &&
                    Helpers.getMobs(objAiBase.Position, 1000).Count == 0)
                {
                    _GameInfo.MoveTo = player.Position.Extend(objAiBase.Position, GameInfo.ChampionRange + 100);
                    return true;
                }
            }
            //Checking free enemy minionwaves
            if (player.Level > 8)
            {
                var miniwave =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                ((m.CountEnemiesInRange(GameInfo.ChampionRange) == 0 ||
                                  (m.CountAlliesInRange(GameInfo.ChampionRange) + 1 >=
                                   m.CountEnemiesInRange(GameInfo.ChampionRange))) ||
                                 m.Distance(_GameInfo.SpawnPoint) < 7000) &&
                                Helpers.getMobs(m.Position, 1200).Count >= 6)
                        .OrderByDescending(m => m.Distance(_GameInfo.SpawnPoint) < 7000)
                        .ThenBy(m => m.Distance(player))
                        .FirstOrDefault();
                if (miniwave != null && Helpers.CheckPath(player.GetPath(miniwave.Position)) &&
                    !CheckForRetreat(null, miniwave.Position) && CheckLaneClear(miniwave.Position))
                {
                    _GameInfo.MoveTo = miniwave.Position.Extend(player.Position, 200);
                    return true;
                }
            }
            //Checking ally mobs, pushing
            if (player.Level > 8)
            {
                var miniwave =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            m =>
                                m.Distance(_GameInfo.SpawnPointEnemy) < 7000 &&
                                Helpers.getAllyMobs(m.Position, 1200).Count >= 7)
                        .OrderByDescending(m => m.Distance(_GameInfo.SpawnPointEnemy) < 7000)
                        .ThenBy(m => m.Distance(player))
                        .FirstOrDefault();
                if (miniwave != null && Helpers.CheckPath(player.GetPath(miniwave.Position)) &&
                    !CheckForRetreat(null, miniwave.Position) && CheckLaneClear(miniwave.Position))
                {
                    _GameInfo.MoveTo = miniwave.Position.Extend(player.Position, 200);
                    return true;
                }
            }
            return false;
        }

        private static Vector3 GetMovePosition()
        {
            switch (_GameInfo.GameState)
            {
                case State.Retreat:
                    var enemyTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(t => t.IsEnemy && !t.IsDead && t.Distance(player) < 2000);
                    var allyTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .OrderBy(t => t.Distance(player))
                            .FirstOrDefault(
                                t =>
                                    t.IsAlly && !t.IsDead && t.Distance(player) < 4000 &&
                                    t.CountEnemiesInRange(1200) == 0);
                    var enemy = Helpers.GetTargetEnemy();
                    if (_GameInfo.AttackedByTurret && enemyTurret != null)
                    {
                        if (allyTurret != null)
                        {
                            return allyTurret.Position;
                        }
                        var nextPost = Prediction.GetPrediction(player, 1);
                        if (!nextPost.UnitPosition.UnderTurret(true))
                        {
                            return nextPost.UnitPosition;
                        }
                        else
                        {
                            return _GameInfo.SpawnPoint;
                        }
                    }
                    if (allyTurret != null && player.Distance(_GameInfo.SpawnPoint) > player.Distance(allyTurret))
                    {
                        return allyTurret.Position.Extend(_GameInfo.SpawnPoint, 300);
                    }
                    return _GameInfo.SpawnPoint;
                    break;
                case State.Objective:
                    return _GameInfo.MoveTo;
                    break;
                case State.Grouping:
                    return _GameInfo.MoveTo;
                    break;
                case State.Defending:
                    return Vector3.Zero;
                    break;
                case State.Pushing:
                    return Vector3.Zero;
                    break;
                case State.Warding:
                    return _GameInfo.MoveTo;
                    break;
                case State.FightIng:
                    return Vector3.Zero;
                    break;
                case State.Ganking:
                    return _GameInfo.MoveTo;
                    break;
                case State.Jungling:
                    return Vector3.Zero;
                    break;
                case State.LaneClear:
                    return Vector3.Zero;
                    break;
                default:
                    var nextMob =
                        _GameInfo.MonsterList.OrderBy(m => m.Index)
                            .FirstOrDefault(m => m.Index == _GameInfo.CurrentMonster);
                    if (nextMob != null)
                    {
                        return nextMob.Position;
                    }
                    var firstOrDefault = _GameInfo.MonsterList.FirstOrDefault(m => m.Index == 1);
                    if (firstOrDefault != null)
                    {
                        return firstOrDefault.Position;
                    }
                    break;
            }

            if (Debug)
            {
                Console.WriteLine("GetMovePosition: Can't get Position");
            }
            return Vector3.Zero;
        }

        private static void ResetDamageTakenTimer()
        {
            if (Environment.TickCount - ResetTimer > 1200)
            {
                ResetTimer = Environment.TickCount;
                _GameInfo.DamageTaken = 0f;
                _GameInfo.DamageCount = 0;
            }
            if (_GameInfo.CurrentMonster == 13 && player.Level <= 9)
            {
                _GameInfo.CurrentMonster++;
            }
            if (_GameInfo.CurrentMonster > 16)
            {
                _GameInfo.CurrentMonster = 1;
            }
        }

        private static bool ShouldRecall()
        {
            if (player.HealthPercent <= menu.Item("HealtToBack").GetValue<Slider>().Value)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Low Health - true");
                }
                return true;
            }
            if (_GameInfo.CanBuyItem())
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Can buy item - true");
                }
                return true;
            }
            if (Helpers.getMobs(_GameInfo.SpawnPoint, 5000).Count > 6)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: Def base - true");
                }
                return true;
            }
            if (_GameInfo.GameState == State.Retreat && player.CountEnemiesInRange(GameInfo.ChampionRange) == 0)
            {
                if (Debug)
                {
                    Console.WriteLine("ShouldRecall: After retreat - true");
                }
                return true;
            }
            if (Debug)
            {
                Console.WriteLine("ShouldRecall: End - false");
            }
            return false;
        }

        private static bool WaitOnFountain()
        {
            if (!player.InFountain())
            {
                return false;
            }
            if (player.InFountain() && player.IsRecalling())
            {
                return false;
            }
            if (player.HealthPercent < 90 || (player.ManaPercent < 90 && player.MaxMana > 100))
            {
                if (player.IsMoving)
                {
                    player.IssueOrder(GameObjectOrder.HoldPosition, player.Position);
                }
                return true;
            }
            return false;
        }

        private static bool ShouldSkipUpdate()
        {
            if (!menu.Item("Enabled").GetValue<Boolean>())
            {
                return true;
            }
            if (Environment.TickCount - UpdateLimiter <= 400)
            {
                return true;
            }
            if (player.IsDead)
            {
                return true;
            }
            if (player.IsRecalling() && !player.InFountain())
            {
                return true;
            }
            UpdateLimiter = Environment.TickCount - Random.Next(0, 100);
            return false;
        }

        public static bool Debug
        {
            get { return menu.Item("debug").GetValue<KeyBind>().Active; }
        }

        #endregion

        #endregion

        #region Events

        private static void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            Obj_AI_Hero target = args.Target as Obj_AI_Hero;
            if (target != null)
            {
                if (target.IsMe && sender.IsValid && !sender.IsDead && sender.IsEnemy && target.IsValid)
                {
                    if (Orbwalking.IsAutoAttack(args.SData.Name))
                    {
                        _GameInfo.DamageTaken += (float) sender.GetAutoAttackDamage(player, true);
                        _GameInfo.DamageCount++;
                    }
                    if (sender is Obj_AI_Turret && !_GameInfo.AttackedByTurret)
                    {
                        _GameInfo.AttackedByTurret = true;
                        Utility.DelayAction.Add(4000, () => _GameInfo.AttackedByTurret = false);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Debug)
            {
                foreach (var m in Helpers.mod)
                {
                    Render.Circle.DrawCircle(m, 50, Color.Crimson, 7);
                }
                if (_GameInfo.LastClick.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.LastClick, 70, Color.Blue, 7);
                }
                if (_GameInfo.MoveTo.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.MoveTo, 77, Color.BlueViolet, 7);
                }
                foreach (var e in _GameInfo.EnemyStructures)
                {
                    Render.Circle.DrawCircle(e, 300, Color.Red, 7);
                }
                foreach (var a in _GameInfo.AllyStructures)
                {
                    Render.Circle.DrawCircle(a, 300, Color.DarkGreen, 7);
                }
                if (_GameInfo.ClosestWardPos.IsValid())
                {
                    Render.Circle.DrawCircle(_GameInfo.ClosestWardPos, 70, Color.LawnGreen, 7);
                }
            }
            if (menu.Item("State").GetValue<Boolean>())
            {
                Drawing.DrawText(150f, 200f, Color.Aqua, _GameInfo.GameState.ToString());
            }
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (sender.IsMe)
            {
                _GameInfo.LastClick = args.Path.Last();
            }
        }

        #endregion

        #region Init

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (Game.MapId != GameMapId.SummonersRift)
            {
                Game.PrintChat("The map is not supported!");
                return;
            }
            _GameInfo.Champdata = new Champdata();
            if (_GameInfo.Champdata.Hero == null)
            {
                Game.PrintChat("The champion is not supported!");
                return;
            }
            Jungle.setSmiteSlot();
            if (Jungle.smiteSlot == SpellSlot.Unknown)
            {
                Console.WriteLine("Items: ");
                foreach (var i in player.InventoryItems)
                {
                    Console.WriteLine("\t Name: {0}, ID: {1}({2})", i.IData.TranslatedDisplayName, i.Id, (int) i.Id);
                }
                Game.PrintChat("You don't have smite!");
                return;
            }

            ItemHandler = new ItemHandler(_GameInfo.Champdata.Type);
            CreateMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
            Game.OnEnd += Game_OnEnd;
        }

        private static void Game_OnEnd(GameEndEventArgs args)
        {
            if (menu.Item("AutoClose").GetValue<Boolean>())
            {
                Console.WriteLine("END");
                Thread.Sleep(Random.Next(10000, 13000));
                Game.Quit();
            }
        }

        private static void CreateMenu()
        {
            menu = new Menu("AutoJungle", "AutoJungle", true);

            Menu menuD = new Menu("Debug ", "dsettings");
            menuD.AddItem(new MenuItem("debug", "Print to console"))
                .SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))
                .SetFontStyle(FontStyle.Bold, SharpDX.Color.Orange);
            menuD.AddItem(new MenuItem("State", "Show GameState")).SetValue(false);
            menu.AddSubMenu(menuD);
            Menu menuJ = new Menu("Jungle settings", "jsettings");
            menuJ.AddItem(new MenuItem("HealtToBack", "Recall on HP(%)").SetValue(new Slider(30, 0, 100)));
            menuJ.AddItem(new MenuItem("UseTrinket", "Use Trinket")).SetValue(true);
            menu.AddSubMenu(menuJ);
            Menu menuG = new Menu("Gank settings", "gsettings");
            menuG.AddItem(new MenuItem("GankLevel", "Min level to gank").SetValue(new Slider(5, 1, 18)));
            menuG.AddItem(new MenuItem("GankFrequency", "Ganking frequency").SetValue(new Slider(100, 0, 100)));
            menuG.AddItem(new MenuItem("GankRange", "Searching range").SetValue(new Slider(7000, 0, 20000)));
            menuG.AddItem(new MenuItem("ComboSmite", "Use Smite")).SetValue(true);
            menu.AddSubMenu(menuG);
            menu.AddItem(new MenuItem("Enabled", "Enabled")).SetValue(true);
            menu.AddItem(new MenuItem("AutoClose", "Close at the end")).SetValue(true);
            menu.AddItem(
                new MenuItem(
                    "AutoJungle",
                    "by Soresu v" + Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(",", ".")));
            menu.AddToMainMenu();
        }

        #endregion
    }
}