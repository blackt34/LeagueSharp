using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Properties;

namespace xSaliceResurrected.Utilities
{
    class GankAlerter
    {
        private readonly List<EnemyInfo> _info = new List<EnemyInfo>();
        private readonly Menu _aleterMenu;
        private int _lastAlert;
        private bool _shouldAlert;
        private readonly Render.Sprite _warningSprite = new Render.Sprite(Resources.warnin, Vector2.Zero);

        public GankAlerter(Menu menu)
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                EnemyInfo info = new EnemyInfo
                {
                    Enemysprite =
                        new Render.Sprite(
                            (Bitmap)
                                Resources.ResourceManager.GetObject(string.Format("LP_{0}",
                                    enemy.ChampionName)) ?? Resources.LP_Aatrox, Vector2.Zero),
                    EnemyId = enemy.NetworkId,
                    LastPos = Vector3.Zero,
                    LastVisibile = 0
                };
                _info.Add(info);
            }

            _warningSprite.Scale = new Vector2(.5f, .5f);

            _aleterMenu = new Menu("Awareness", "Awareness");
            {
                var alter = new Menu("Alerter", "Alerter");
                {
                    alter.AddItem(new MenuItem("Info", "Alerter is a Big red X icon indicating new enemy in range", true).SetValue(true));
                    alter.AddItem(new MenuItem("enableAlert", "Enabled", true).SetValue(true));
                    alter.AddItem(new MenuItem("DisplayMinimumRange", "Minium Range To Alert", true).SetValue(new Slider(1200, 0, 8000)));
                    alter.AddItem(new MenuItem("DisplayEnemyEnterRange", "Alert When Enemy Enters Range of", true).SetValue(new Slider(2000, 0, 8000)));
                    alter.AddItem(new MenuItem("AlterDuration", "Duration of Alert (Seconds)", true).SetValue(new Slider(2)));
                    _aleterMenu.AddSubMenu(alter);
                }

                var locationDetector = new Menu("Enemy Location", "Enemy Location");
                {
                    locationDetector.AddItem(new MenuItem("DisplayLastSeen", "Display Last Seen", true).SetValue(true));
                    locationDetector.AddItem(new MenuItem("MiniMap", "Minimap Display broken atm cause of API(Will fix asap)", true));
                    locationDetector.AddItem(new MenuItem("IconDistanceFromCharacter", "Icon Distant From Character", true).SetValue(new Slider(400, 0, 2000)));
                    locationDetector.AddItem(new MenuItem("IconRed", "Icon Red If enemy is not on map ", true));
                    locationDetector.AddItem(new MenuItem("IconYellow", "Icon yellow if >", true).SetValue(new Slider(1200, 500, 2000)));
                    locationDetector.AddItem(new MenuItem("IconBlue", "Icon blue if >", true).SetValue(new Slider(2000, 2000, 3500)));
                    locationDetector.AddItem(new MenuItem("IconGreen", "Icon blue if >", true).SetValue(new Slider(3500, 3500, 8000)));
                    _aleterMenu.AddSubMenu(locationDetector);
                }

            }

            menu.AddSubMenu(_aleterMenu);

            AttackableUnit.OnEnterLocalVisiblityClient += OnEnterVisiblityClient;
            AttackableUnit.OnLeaveVisiblityClient += AttackableUnitOnOnLeaveVisiblityClient;
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            if (_aleterMenu.Item("enableAlert", true).GetValue<bool>())
            {
                if (_shouldAlert &&
                    Utils.TickCount - _lastAlert < _aleterMenu.Item("AlterDuration", true).GetValue<Slider>().Value * 1000)
                {
                    var vec = ObjectManager.Player.HPBarPosition;
                    vec.Y += 50;
                    _warningSprite.Position = vec;
                    _warningSprite.OnEndScene();
                }
            }


            if (!_aleterMenu.Item("DisplayLastSeen", true).GetValue<bool>())
                return;

            foreach (var enemy in HeroManager.Enemies)
            {
                var enemy1 = enemy;
                foreach (var enemyInfo in _info.Where(x => x.EnemyId == enemy1.NetworkId))
                {
                    Vector2 playerToEnemyVec;

                    if (!enemy.IsVisible)
                    {
                        playerToEnemyVec = Drawing.WorldToScreen(ObjectManager.Player.Position.Extend(enemyInfo.LastPos, _aleterMenu.Item("IconDistanceFromCharacter", true).GetValue<Slider>().Value));
                        enemyInfo.Enemysprite.Color = new ColorBGRA(255, 0, 0, 255);
                        enemyInfo.Enemysprite.Scale = new Vector2(2, 2);
                    }
                    else
                    {
                        playerToEnemyVec = Drawing.WorldToScreen(ObjectManager.Player.Position.Extend(enemy.Position, _aleterMenu.Item("IconDistanceFromCharacter", true).GetValue<Slider>().Value));

                        if (ObjectManager.Player.Distance(enemy1) > _aleterMenu.Item("IconYellow", true).GetValue<Slider>().Value
                            && ObjectManager.Player.Distance(enemy1) <= _aleterMenu.Item("IconBlue", true).GetValue<Slider>().Value)
                            enemyInfo.Enemysprite.Color = new ColorBGRA(255, 255, 0, 255);
                        else if (ObjectManager.Player.Distance(enemy1) > _aleterMenu.Item("IconBlue", true).GetValue<Slider>().Value
                            && ObjectManager.Player.Distance(enemy1) <= _aleterMenu.Item("IconGreen", true).GetValue<Slider>().Value)
                            enemyInfo.Enemysprite.Color = new ColorBGRA(0, 0, 255, 255);
                        else if (ObjectManager.Player.Distance(enemy1) > _aleterMenu.Item("IconGreen", true).GetValue<Slider>().Value)
                            enemyInfo.Enemysprite.Color = new ColorBGRA(0, 255, 0, 255);
                        else
                            playerToEnemyVec = Vector2.Zero;

                        enemyInfo.Enemysprite.Scale = new Vector2(2, 2);
                    }

                    if (playerToEnemyVec == Vector2.Zero)
                        continue;

                    enemyInfo.Enemysprite.Position = playerToEnemyVec;

                    enemyInfo.Enemysprite.OnEndScene();
                }
            }
        }

        private void AttackableUnitOnOnLeaveVisiblityClient(AttackableUnit sender, EventArgs args)
        {
            if (!sender.IsEnemy || !(sender is Obj_AI_Hero))
                return;

            foreach (var enemy in _info.Where(x => x.EnemyId == sender.NetworkId))
            {
                enemy.LastPos = sender.Position;
                enemy.LastVisibile = Utils.TickCount;
            }
        }

        private void OnEnterVisiblityClient(AttackableUnit sender, EventArgs args)
        {
            if (!sender.IsEnemy || !(sender is Obj_AI_Hero))
                return;

            if (ObjectManager.Player.Distance(sender) < _aleterMenu.Item("DisplayEnemyEnterRange", true).GetValue<Slider>().Value &&
                ObjectManager.Player.Distance(sender) > _aleterMenu.Item("IconDistanceFromCharacter", true).GetValue<Slider>().Value)
            {
                _shouldAlert = true;
                _lastAlert = Utils.TickCount;
            }
        }

        class EnemyInfo
        {
            public Render.Sprite Enemysprite;
            public int EnemyId { get; set; }
            public float LastVisibile { get; set; }
            public Vector3 LastPos { get; set; }
        }
    }
}