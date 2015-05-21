using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace RLProjectJunglePlay
{
	class AddOn
	{
        static Orbwalking.Orbwalker Orbwalker;
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static void Load()
        {
            Game.OnUpdate += OnAttack.Game_OnUpdate;
            Game.OnUpdate += Killsteal.Game_OnUpdate;
            Game.OnUpdate += Combo.Game_OnUpdate;
			CustomEvents.Game.OnGameLoad += AfterAttack.Game_OnGameLoad;
			CustomEvents.Game.OnGameLoad += Combo.Game_OnGameLoad;
            Orbwalking.AfterAttack += AfterAttack.Orbwalking_AfterAttack;
            Orbwalking.OnAttack += OnAttack.Orbwalking_OnAttack;
		}
		
		internal class OnAttack
		{
			internal static Spell RS;
			internal static SpellSlot smiteSlot = SpellSlot.Unknown;
			internal static float smrange = 700f;
            internal static void Game_OnUpdate(EventArgs args)
            {
				setRSmiteSlot();
			}
			internal static void setRSmiteSlot()
			{
				foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase))) // Red Smite
				{
					smiteSlot = spell.Slot;
					RS = new Spell(smiteSlot, smrange);
					return;
				}
			}

			internal static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
			{
				var Target = (Obj_AI_Hero)target;
					
				if (!unit.IsMe || Target == null)
						return;
						
					RS.Slot = smiteSlot;
					if(smiteSlot.IsReady())
					Player.Spellbook.CastSpell(smiteSlot, Target);

			}
		}
		
		internal static bool isKillable(Obj_AI_Base target, float damage)
        {
            return target.Health + target.HPRegenRate <= damage;
        }
		
		internal class Killsteal
		{
			internal static Spell BS;
			internal static SpellSlot smiteSlot = SpellSlot.Unknown;
			internal static float smrange = 700f;
            internal static void Game_OnUpdate(EventArgs args)
            {
				setBSmiteSlot();
				
				var ts = ObjectManager.Get<Obj_AI_Hero>().Where(f => !f.IsAlly && !f.IsDead && Player.Distance(f, false) <= smrange);
				if (ts == null)
					return;

				float dmg = BSDamage();
					foreach (var t in ts)
					{
						if (isKillable(t,dmg))
						{
							BS.Slot = smiteSlot;
							Player.Spellbook.CastSpell(smiteSlot, t);
						}
					}
			}
			
			internal static float BSDamage()
			{
				int lvl = Player.Level;
				int damage = (20 + 8 * lvl);
				return damage;
			}
			
			internal static void setBSmiteSlot()
			{
				foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteplayerganker", StringComparison.CurrentCultureIgnoreCase))) // Red Smite
				{
					smiteSlot = spell.Slot;
					BS = new Spell(smiteSlot, smrange);
					return;
				}
			}		
		}
		
        internal class AfterAttack
        {
			internal static Items.Item H, T;
			internal static List<Items.Item> itemsList = new List<Items.Item>();
			internal static void Game_OnGameLoad(EventArgs args)
			{
            H = new Items.Item(3074, 250f);
            T = new Items.Item(3077, 250f);
			}
			internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
			{
			if (!unit.IsMe || target == null || target.IsDead || unit.IsDead || target.Type != GameObjectType.obj_AI_Hero) //target.Type != GameObjectType.obj_AI_Minion
				return;
			S(H);
			S(T);
			}
		}
		
		internal class Combo
		{
			internal static Items.Item B, K, G;
			internal static List<Items.Item> itemsList = new List<Items.Item>();
			internal static void Game_OnGameLoad(EventArgs args)
			{
            B = new Items.Item(3144, 250f);
            K = new Items.Item(3153, 250f);
			G = new Items.Item(3146, 250f);
			}
			internal static void Game_OnUpdate(EventArgs args)
			{
			ST(B);
			ST(K);
			ST(G);
			}
		}
		
		internal static void S(Items.Item spell)
		{
			if(spell.IsReady())
			spell.Cast();
		}
		internal static void ST(Items.Item spell)
		{
			Obj_AI_Hero Target = TargetSelector.GetTarget(450 , TargetSelector.DamageType.Physical);
			if(spell.IsReady())
			spell.Cast(Target);
		}
	}
}