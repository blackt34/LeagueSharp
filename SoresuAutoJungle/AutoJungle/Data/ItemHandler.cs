using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoJungle.Data
{
    internal class ItemHandler
    {
        public static List<BuyingOrder> ItemList = new List<BuyingOrder>();

        public ItemHandler(BuildType type)
        {
            switch (type)
            {
                case BuildType.AD:
                    SetListAD();
                    break;
                case BuildType.AS:
                    SetListAS();
                    break;
            }
        }

        private void SetListAD()
        {
            //ItemList.Add(new BuyingOrder());
        }

        private void SetListAS()
        {
            ItemList.Add(new BuyingOrder(1, 1041, "Hunter's Machete", 350)); //狩獵者彎刀
            ItemList.Add(new BuyingOrder(2, 2031, "Refillable Potion", 150)); //藥水
            ItemList.Add(new BuyingOrder(3, 3340, "Trinket", 0)); //飾品眼
            ItemList.Add(new BuyingOrder(4, 1039, "Hunter's Talisman", 350)); //狩獵者護符
            ItemList.Add(new BuyingOrder(5, 3715, "Skirmisher's Sabre", 700)); //紅打野刀
            ItemList.Add(new BuyingOrder(6, 1042, "Dagger", 350)); //短劍
            ItemList.Add(new BuyingOrder(7, 1415, "Devourer", 1100)); //附魔噬魂
            ItemList.Add(new BuyingOrder(8, (int)ItemId.Boots_of_Speed, "Boots of Speed", 300)); //速度之靴
            ItemList.Add(new BuyingOrder(9, (int)ItemId.Boots_of_Swiftness, "Boots of Swoftness", 500)); //輕靈之靴
            ItemList.Add(new BuyingOrder(10, (int)ItemId.Bilgewater_Cutlass, "Bilgewater Cutlass", 1650)); //吸血彎刀
            ItemList.Add(new BuyingOrder(11, (int)ItemId.Blade_of_the_Ruined_King, "BOTRK", 1750)); //殞落王者之劍
            ItemList.Add(new BuyingOrder(12, (int)ItemId.Recurve_Bow, "Recurve Bow", 1000)); //反曲弓
            ItemList.Add(new BuyingOrder(13, (int)ItemId.Wits_End, "Wits End", 1800)); //智慧末刃 
            ItemList.Add(new BuyingOrder(14, (int)ItemId.Tiamat_Melee_Only, "Tiamat", 1250)); //海神之斧 
            ItemList.Add(new BuyingOrder(15, (int)ItemId.Vampiric_Scepter, "Vampiric_Screpter", 900)); //吸血鬼權杖
            ItemList.Add(new BuyingOrder(16, (int)ItemId.Ravenous_Hydra_Melee_Only, "Ravenous Hydra Melee Only", 1450)); //狂怒九頭蛇 
            ItemList.Add(new BuyingOrder(17, (int)ItemId.Zeal, "Zeal", 1200)); //狂熱雙刀 
            ItemList.Add(new BuyingOrder(18, (int)ItemId.Phantom_Dancer, "Phantom Dancer", 1500)); //幻影之舞
        }

        public static void UseItemsJungle()
        {
            if (Items.HasItem((int) ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int) ItemId.Tiamat_Melee_Only) &&
                Helpers.getMobs(Program.player.Position, 400).Count > 2)
            {
                Items.UseItem((int) ItemId.Tiamat_Melee_Only);
            }
            if (Items.HasItem((int) ItemId.Ravenous_Hydra_Melee_Only) &&
                Items.CanUseItem((int) ItemId.Ravenous_Hydra_Melee_Only) &&
                Helpers.getMobs(Program.player.Position, 400).Count > 2)
            {
                Items.UseItem((int) ItemId.Ravenous_Hydra_Melee_Only);
            }
        }

        public static void UseItemsCombo(Obj_AI_Base target, bool use)
        {
            if (Items.HasItem((int) ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int) ItemId.Tiamat_Melee_Only) &&
                target.Distance(Program.player) < 400)
            {
                Items.UseItem((int) ItemId.Tiamat_Melee_Only);
            }
            if (Items.HasItem((int) ItemId.Ravenous_Hydra_Melee_Only) &&
                Items.CanUseItem((int) ItemId.Ravenous_Hydra_Melee_Only) && target.Distance(Program.player) < 400)
            {
                Items.UseItem((int) ItemId.Ravenous_Hydra_Melee_Only);
            }
            if (!use)
            {
                return;
            }
            if (Items.HasItem((int) ItemId.Bilgewater_Cutlass) && Items.CanUseItem((int) ItemId.Bilgewater_Cutlass) &&
                (target.Distance(Program.player) > Orbwalking.GetRealAutoAttackRange(target) || (target.HealthPercent<35 && Program.player.HealthPercent<35)))
            {
                Items.UseItem((int)ItemId.Bilgewater_Cutlass, target);
            }
            if (Items.HasItem((int) ItemId.Blade_of_the_Ruined_King) &&
                Items.CanUseItem((int) ItemId.Blade_of_the_Ruined_King) &&
                (target.Distance(Program.player) > Orbwalking.GetRealAutoAttackRange(target) || (target.HealthPercent < 35 && Program.player.HealthPercent < 35)))
            {
                Items.UseItem((int) ItemId.Blade_of_the_Ruined_King, target);
            }
        }
    }

    internal class BuyingOrder
    {
        public int Index;
        public int ItemId;
        public string Name;
        public int Price;

        public BuyingOrder(int idx, int itemid, string name, int price)
        {
            Index = idx;
            ItemId = itemid;
            Name = name;
            Price = price;
        }
    }

    internal enum BuildType
    {
        AS,
        AD
    }
}