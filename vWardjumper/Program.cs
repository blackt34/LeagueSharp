using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace vWardjumper
{
    static class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static Spell Jmpspell;
        private static Menu menu;
        private static int lastPlaced = 0;
        private static Vector3 lastWardPos = new Vector3();

        static void Game_OnGameLoad(EventArgs args)
        {
            if (!ObjectManager.Player.IsJumpHero()) return;
            
            menu = new Menu("vWardjumper", "vWardjumper", true);
            menu.AddItem(new MenuItem("Wardjump", "Wardjump").SetValue(new KeyBind(71, KeyBindType.Press)));

            menu.AddToMainMenu();

            Jmpspell = GetJumpSpell();
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat(">> WardJumper loaded <<");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward => menu.Item("Wardjump").GetValue<KeyBind>().Active && Jmpspell != null && ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 130 && ward.Distance(ObjectManager.Player) < Jmpspell.Range))
            {
                Jmpspell.Cast(ward);
            }

            if (!menu.Item("Wardjump").GetValue<KeyBind>().Active || Jmpspell == null || Environment.TickCount <= lastPlaced + 3000 || !IsJumpReady()) return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = ObjectManager.Player.Position;

            Vector3 delta = cursorPos - myPos;
            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);
            //Vector3 wardPosition = cursorPos;

            int invSlot = FindBestWardItem();
            if (invSlot == -1) return;

            //invSlot.UseItem(wardPosition);
            Items.UseItem(invSlot, wardPosition);
            lastWardPos = wardPosition;
            lastPlaced = Environment.TickCount;
        }

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }

        private static int FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return -1;

            SpellDataInst sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return Convert.ToInt32(slot.Id);
            }
            return -1;
        }

        private static Spell GetJumpSpell()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Jax":
                    return new Spell(SpellSlot.Q, 700);
                case "Katarina":
                    return new Spell(SpellSlot.E, 700);
                case "LeeSin":
                    return new Spell(SpellSlot.W, 700);
            }
            return null;
        }

        private static bool IsJumpReady()
        {
            if (ObjectManager.Player.ChampionName != "LeeSin") // Kata + Jax
            {
                return Jmpspell.IsReady();
            }
            return Jmpspell.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne"; // Lee
        }

        private static bool IsJumpHero(this Obj_AI_Hero hero)
        {
            return (hero.ChampionName == "Jax" || hero.ChampionName == "Katarina" || hero.ChampionName == "LeeSin");
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (Environment.TickCount < lastPlaced + 300)
            {
                Obj_AI_Minion ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(lastWardPos) < 500)
                {
                    Jmpspell.Cast(ward);
                }
            }
        }
    }
}
