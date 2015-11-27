using SharpDX;

namespace AutoJungle.Data
{
    class Camps
    {
        
        public static readonly string[] BigMobs =
        {
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak",
            "SRU_Red", "SRU_Krug", "SRU_Dragon", "SRU_BaronSpawn", "Sru_Crab"
        };

        #region Camps

        public static MonsterInfo Baron = new MonsterInfo
        {
            ID = "Baron",
            Position = new Vector3(4910f, 10268f, -71.24f),
            name = "SRU_BaronSpawn",
            Index = 100
        };

        public static MonsterInfo Dragon = new MonsterInfo
        {
            ID = "Dragon",
            Position = new Vector3(9836f, 4408f, -71.24f),
            name = "SRU_Dragon"
        };

        public static MonsterInfo top_crab = new MonsterInfo
        {
            ID = "top_crab",
            Position = new Vector3(4266f, 9634f, -67.87f),
            name = "Sru_Crab"
        };

        public static MonsterInfo BLUE_MID = new MonsterInfo
        {
            ID = "blue_MID",
            Position = new Vector3(5294.531f, 5537.924f, 50.46155f),
            name = "noneuses"
        };

        public static MonsterInfo PURPLE_MID = new MonsterInfo
        {
            ID = "purple_MID",
            Position = new Vector3(9443.35f, 9339.06f, 53.30994f),
            name = "noneuses"
        };

        public static MonsterInfo down_crab = new MonsterInfo
        {
            ID = "down_crab",
            Position = new Vector3(10524f, 5116f, -62.81f),
            name = "Sru_Crab"
        };

        public static MonsterInfo bteam_Razorbeak = new MonsterInfo
        {
            ID = "bteam_Razorbeak",
            Position = new Vector3(6974f, 5460f, 54f),
            name = "SRU_Razorbeak"
        };

        public static MonsterInfo bteam_Red = new MonsterInfo
        {
            ID = "bteam_Red",
            Position = new Vector3(7796f, 4028f, 54f),
            name = "SRU_Red"
        };

        public static MonsterInfo bteam_Krug = new MonsterInfo
        {
            ID = "bteam_Krug",
            Position = new Vector3(8394f, 2750f, 50f),
            name = "SRU_Krug"
        };

        public static MonsterInfo bteam_Blue = new MonsterInfo
        {
            ID = "bteam_Blue",
            Position = new Vector3(3832f, 7996f, 52f),
            name = "SRU_Blue"
        };

        public static MonsterInfo bteam_Gromp = new MonsterInfo
        {
            ID = "bteam_Gromp",
            Position = new Vector3(2112f, 8372f, 51.7f),
            name = "SRU_Gromp"
        };

        public static MonsterInfo bteam_Wolf = new MonsterInfo
        {
            ID = "bteam_Wolf",
            Position = new Vector3(3844f, 6474f, 52.46f),
            name = "SRU_Murkwolf"
        };

        public static MonsterInfo pteam_Razorbeak = new MonsterInfo
        {
            ID = "pteam_Razorbeak",
            Position = new Vector3(7856f, 9492f, 52.33f),
            name = "SRU_Razorbeak"
        };

        public static MonsterInfo pteam_Red = new MonsterInfo
        {
            ID = "pteam_Red",
            Position = new Vector3(7124f, 10856f, 56.34f),
            name = "SRU_Red",
        };

        public static MonsterInfo pteam_Krug = new MonsterInfo
        {
            ID = "pteam_Krug",
            Position = new Vector3(6495f, 12227f, 56.47f),
            name = "SRU_Krug"
        };

        public static MonsterInfo pteam_Blue = new MonsterInfo
        {
            ID = "pteam_Blue",
            Position = new Vector3(10850f, 6938f, 51.72f),
            name = "SRU_Blue",
        };

        public static MonsterInfo pteam_Gromp = new MonsterInfo
        {
            ID = "pteam_Gromp",
            Position = new Vector3(12766f, 6464f, 51.66f),
            name = "SRU_Gromp"
        };

        public static MonsterInfo pteam_Wolf = new MonsterInfo
        {
            ID = "pteam_Wolf",
            Position = new Vector3(10958f, 8286f, 62.46f),
            name = "SRU_Murkwolf"
        };

        #endregion

    }
    public class MonsterInfo
    {
        public Vector3 Position;
        public string ID;
        public string name;
        public int Team;
        public int Index;

        public MonsterInfo(MonsterInfo baseInfo, int index)
        {
            Position = baseInfo.Position;
            ID = baseInfo.ID;
            name = baseInfo.name;
            Team = baseInfo.Team;
            Index = index;
        }
        public MonsterInfo()
        {          
        }
    }
}
