using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSaliceResurrected.Utilities;

namespace xSaliceResurrected.Managers
{
    static class OrbwalkManager
    {
        public static int CurrentOrbwalker
        {
            get
            {
                if (ObjectManager.Player.ChampionName.ToLower() == "azir")
                    return 1;

                return Champion.menu.Item("OrbwalkingMode").GetValue<StringList>().SelectedIndex;
            }
        }

        public static void Orbwalk(Obj_AI_Base target, Vector3 pos)
        {
            if(CurrentOrbwalker == 1)
                Orbwalking.Orbwalk(target, pos);
            else
            {
                xSaliceWalker.Orbwalk(target, pos);
            }
        }

        public static bool InAutoAttackRange(Obj_AI_Base target)
        {
            if (CurrentOrbwalker == 1)
                return Champion.Orbwalker.InAutoAttackRange(target);

            return xSaliceWalker.InAutoAttackRange(target);
        }

        public static void ResetAutoAttackTimer()
        {
            xSaliceWalker.ResetAutoAttackTimer();
            Orbwalking.ResetAutoAttackTimer();
        }

        public static void SetAttack(bool f)
        {
            if(CurrentOrbwalker == 1)
                Champion.Orbwalker.SetAttack(f);
            else
            {
                xSaliceWalker.SetAttack(f);
            }
        }

        public static void SetMovement(bool f)
        {
            if (CurrentOrbwalker == 1)
                Champion.Orbwalker.SetMovement(f);
            else
            {
                xSaliceWalker.SetMovement(f);
            }
        }

        public static bool CanMove(float delay)
        {
            if (CurrentOrbwalker == 1)
                return Orbwalking.CanMove(delay);

            return xSaliceWalker.CanMove(delay);
        }

        public static bool IsAutoAttack(string name)
        {
            if (CurrentOrbwalker == 1)
                return Orbwalking.IsAutoAttack(name);

            return xSaliceWalker.IsAutoAttack(name);
        }
    }
}
