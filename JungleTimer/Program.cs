#region
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace JungleTimer
{
	internal class Program
	{
		private class JungleCamp
		{
			public string Name;
			public int NextRespawnTime;
			public int RespawnTime;
			public bool IsDead;
			public bool Visibled;
			public Vector3 Position;
			public string[] Names;
			public readonly int Id;
			public JungleCamp(string name, int respawnTime, Vector3 position, string[] names, int id)
			{
				Name = name;
				RespawnTime = respawnTime;
				Position = position;
				Names = names;
				IsDead = false;
				Visibled = false;
				Id = id;
			}
		}
		
		private class DrawText
		{
			private static int _layer;
			public Render.Text Text { get; set; }
			public JungleCamp JungleCamp;
			public DrawText(JungleCamp camp)
			{
				Text = new Render.Text(Drawing.WorldToMinimap(camp.Position),"",15,SharpDX.Color.White)
				{
					VisibleCondition = sender => (camp.NextRespawnTime > 0 ),
					TextUpdate = () => (camp.NextRespawnTime - (int)Game.ClockTime).ToString(CultureInfo.InvariantCulture),
				};
				JungleCamp = camp;
				Text.Add(_layer);
				_layer++;
			}
		}
		
		private static readonly List<JungleCamp> _jungleCamps = new List<JungleCamp>();
		private static readonly List<DrawText> _DrawText = new List<DrawText>();
		private static int _nextTime;

		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}
		
		private static void Initialize()
		{
			if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift)
			{
				// Blue: Blue Buff
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Blue", 300, new Vector3(3388.2f, 8400f, 55.2f),
						new[] { "SRU_Blue1.1.1", "SRU_BlueMini1.1.2", "SRU_BlueMini21.1.3" },1));
				
				// Blue: Wolves
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Murkwolf", 100, new Vector3(3415.8f, 6950f, 55.6f),
						new[] { "SRU_Murkwolf2.1.1", "SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini2.1.3" },2));
				
				// Blue: Chicken
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Razorbeak", 100, new Vector3(6500f, 5900f, 60f),
						new[]
						{
							"SRU_Razorbeak3.1.1", "SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini3.1.3", "SRU_RazorbeakMini3.1.4"
						},3));
				
				// Blue: Red Buff
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Red", 300, new Vector3(7300.4f, 4600.1f, 56.9f),
						new[] { "SRU_Red4.1.1", "SRU_RedMini4.1.2", "SRU_RedMini4.1.3" },4));
				
				// Blue: Krug
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Krug", 100, new Vector3(7700.2f, 3200f, 54.3f),
						new[] { "SRU_Krug5.1.2", "SRU_KrugMini5.1.1" },5));
				
				// Blue: Gromp
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Gromp", 100, new Vector3(1900.1f, 9200f, 54.9f), new[] { "SRU_Gromp13.1.1" },6));
				
				// Red: Blue Buff
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Blue", 300, new Vector3(10440f, 7500f, 54.9f),
						new[] { "SRU_Blue7.1.1", "SRU_BlueMini7.1.2", "SRU_BlueMini27.1.3" },7));
				
				// Red: Wolves
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Murkwolf", 100, new Vector3(10350f, 9000f, 65.5f),
						new[] { "SRU_Murkwolf8.1.1", "SRU_MurkwolfMini8.1.2", "SRU_MurkwolfMini8.1.3" },8));
				
				// Red: Chicken
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Razorbeak", 100, new Vector3(7100f, 10000f, 55.5f),
						new[]
						{
							"SRU_Razorbeak9.1.1", "SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini9.1.3", "SRU_RazorbeakMini9.1.4"
						},9));
				
				// Red: Red Buff
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Red", 300, new Vector3(6450.2f, 11400f, 54.6f),
						new[] { "SRU_Red10.1.1", "SRU_RedMini10.1.2", "SRU_RedMini10.1.3" },10));
				
				// Red: Krug
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Krug", 100, new Vector3(6005f, 13000f, 39.6f),
						new[] { "SRU_Krug11.1.2", "SRU_KrugMini11.1.1" },11));
				
				// Red: Gromp
				_jungleCamps.Add(
					new JungleCamp("SRU_Gromp", 100, new Vector3(12000f, 7000f, 54.8f), new[] { "SRU_Gromp14.1.1" },12));
				
				// Neutral: Dragon
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Dragon", 360, new Vector3(9300.8f, 4200.5f, -60.3f), new[] { "SRU_Dragon6.1.1" },13));
				
				// Neutral: Baron
				_jungleCamps.Add(
					new JungleCamp(
						"SRU_Baron", 420, new Vector3(4300.1f, 11600.7f, -63.1f), new[] { "SRU_Baron12.1.1" },14));
				
				// Dragon: Crab
				_jungleCamps.Add(
					new JungleCamp("Sru_Crab", 180, new Vector3(10600f, 5600.5f, -60.3f), new[] { "Sru_Crab15.1.1" },15));
				
				// Baron: Crab
				_jungleCamps.Add(
					new JungleCamp("Sru_Crab", 180, new Vector3(4200.1f, 9900.7f, -63.1f), new[] { "Sru_Crab16.1.1" },16));

			}
			else if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
			{
				// Blue: Wraiths
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NWraith", 50, new Vector3(3550f, 6250f, 60f),
						new[] { "TT_NWraith1.1.1", "TT_NWraith21.1.2", "TT_NWraith21.1.3" },1));
				
				// Blue: Golems
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NGolem", 50, new Vector3(4500f, 8550f, 60f),
						new[] { "TT_NGolem2.1.1", "TT_NGolem22.1.2" },2));
				
				// Blue: Wolves
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NWolf", 50, new Vector3(5600f, 6400f, 60f),
						new[] { "TT_NWolf3.1.1", "TT_NWolf23.1.2", "TT_NWolf23.1.3" },3));
				
				// Red: Wraiths
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NWraith", 50, new Vector3(10300f, 6250f, 60f),
						new[] { "TT_NWraith4.1.1", "TT_NWraith24.1.2", "TT_NWraith24.1.3" },4));
				
				// Red: Golems
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NGolem", 50, new Vector3(9800f, 8550f, 60f),
						new[] { "TT_NGolem5.1.1", "TT_NGolem25.1.2" },5));
				
				// Red: Wolves
				_jungleCamps.Add(
					new JungleCamp(
						"TT_NWolf", 50, new Vector3(8600f, 6400f, 60f),
						new[] { "TT_NWolf6.1.1", "TT_NWolf26.1.2", "TT_NWolf26.1.3" },6));
				
				// Neutral: Vilemaw
				_jungleCamps.Add(
					new JungleCamp(
						"TT_Spiderboss", 300, new Vector3(7150f, 11100f, 60f),
						new[] { "TT_Spiderboss8.1.1" },7));
			}
			if (_jungleCamps.Count > 0)
			{
				foreach (var camp in _jungleCamps)
				{
					DrawText pos = new DrawText(camp);
					_DrawText.Add(pos);
				}
				Game.OnGameUpdate += Game_OnGameUpdate;
				Drawing.OnEndScene += Drawing_OnEndScene;
				Game.PrintChat("JungleTimer loaded!");
			}
			else Game.PrintChat("Jungle Timer only supports SummonersRift and TwistedTreeline maps.");
		}
		
		private static void Game_OnGameLoad(EventArgs args)
		{
			Utility.DelayAction.Add(4000, () => Initialize());
		}
		
		private static void Game_OnGameUpdate(EventArgs args)
		{
			if ((int) Game.ClockTime - _nextTime >= 0)
			{
				_nextTime = (int) Game.ClockTime + 1;
				var minions =
					ObjectManager.Get<Obj_AI_Base>()
					.Where(minion => !minion.IsDead && minion.IsValid && minion.Name.ToUpper().StartsWith("SRU"));
				var junglesAlive =
					_jungleCamps.Where(
						jungle =>
						!jungle.IsDead &&
						jungle.Names.Any(
							s =>
							minions.Where(minion => minion.Name == s)
							.Select(minion => minion.Name)
							.FirstOrDefault() != null));
				foreach (var jungle in junglesAlive)
				{
					jungle.Visibled = true;
				}
				var junglesDead =
					_jungleCamps.Where(
						jungle =>
						!jungle.IsDead && jungle.Visibled &&
						jungle.Names.All(
							s =>
							minions.Where(minion => minion.Name == s)
							.Select(minion => minion.Name)
							.FirstOrDefault() == null));
				foreach (var jungle in junglesDead)
				{
					jungle.IsDead = true;
					jungle.Visibled = false;
					jungle.NextRespawnTime = (int) Game.ClockTime + jungle.RespawnTime;
				}
				foreach (JungleCamp jungleCamp in
				         _jungleCamps.Where(jungleCamp => (jungleCamp.NextRespawnTime - (int) Game.ClockTime) <= 0))
				{
					jungleCamp.IsDead = false;
					jungleCamp.NextRespawnTime = 0;
				}
			}
		}
		
		private static void Drawing_OnEndScene(EventArgs args)
		{
			foreach (var Texts in _DrawText)
			{
				foreach (JungleCamp jungleCamp in _jungleCamps.Where(camp => camp.NextRespawnTime > 0 && Texts.JungleCamp.Id == camp.Id))
				{
					Texts.Text.OnEndScene();
				}
			}
		}
	}
}