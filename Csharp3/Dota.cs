using MiniJsonSerializerLibrary.Attributes;

namespace DotaNamespace
{
    // атрибуты героя
    public enum PrimaryAttribute
    {
        Strength,
        Agility,
        Intelligence
    }

    // сложность героя
    public enum HeroComplexity
    {
        Simple,
        Moderate,
        Complex
    }

    // роли героя
    [Flags]
    public enum HeroRole
    {
        Carry = 1,
        Support = 2,
        Nuker = 4,
        Disabler = 8,
        Jungler = 16,
        Durable = 32,
        Escape = 64,
        Pusher = 128,
    }

    // способности героя
    public class Ability
    {
        [JsonName("ability_name")]
        public string Name { get; set; } = "";

        [JsonName("mana_cost")]
        public List<int> ManaCost { get; set; } = new List<int>();

        [JsonName("cooldown")]
        public List<double> Cooldown { get; set; } = new List<double>();

        [JsonName("description")]
        public string Description { get; set; } = "";
    }

    public class DotaItem
    {
        [JsonName("item_id")]
        private int ItemId { get; set; } = new Random().Next();

        [JsonName("item_name")]
        public string Name { get; set; } = "";

        [JsonName("cost")]
        public int Cost { get; set; }

        [JsonName("in_backpack")]
        public bool InBackpack { get; set; }

        [JsonName("charges")]
        public int? Charges { get; set; }

        [JsonName("cooldown_remaining")]
        public double CooldownRemaining { get; set; }
    }

    // статистика матча
    public class MatchStats
    {
        [JsonName("kills")]
        public int Kills { get; set; }

        [JsonName("deaths")]
        public int Deaths { get; set; }

        [JsonName("assists")]
        public int Assists { get; set; }

    }

    public class DotaHero
    {
        [JsonName("hero_id")]
        private int HeroId { get; set; } = new Random().Next();

        [JsonName("hero_name")]
        public string Name { get; set; } = "";

        [JsonName("primary_attribute")]
        public PrimaryAttribute Attribute { get; set; }

        [JsonName("complexity")]
        public HeroComplexity Complexity { get; set; }

        [JsonName("roles")]
        public HeroRole Roles { get; set; }

        [JsonName("я не придумал название")]
        public Dictionary<string, object> CustomAttributes { get; set; } = new Dictionary<string, object>();

        [JsonName("attack_range")]
        public int AttackRange { get; set; }

        [JsonName("attack_rate")]
        public double AttackRate { get; set; }

        [JsonName("move_speed")]
        public int MoveSpeed { get; set; }

        [JsonName("current_level")]
        public int CurrentLevel { get; set; }

        [JsonName("current_health")]
        public double CurrentHealth { get; set; }

        [JsonName("current_mana")]
        public double CurrentMana { get; set; }

        [JsonName("gold")]
        public int Gold { get; set; }

        [JsonName("abilities")]
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        [JsonName("ability_points_available")]
        public int AbilityPointsAvailable { get; set; }

        [JsonName("inventory")]
        public List<DotaItem> Inventory { get; set; } = new List<DotaItem>();

        [JsonName("neutral_item")]
        public DotaItem? NeutralItem { get; set; }

        [JsonName("match_stats")]
        public MatchStats MatchStats { get; set; } = new MatchStats();

        [JsonName("lore")]
        public string Lore { get; set; } = "";

    }

    public class TestHelper
    {
        public static DotaHero GetHoodwink()
        {
            return new DotaHero
            {
                Name = "Hoodwink",
                Attribute = PrimaryAttribute.Agility,
                Complexity = HeroComplexity.Moderate,
                Roles = HeroRole.Support | HeroRole.Disabler | HeroRole.Escape | HeroRole.Nuker,
                AttackRange = 475,
                AttackRate = 1.7,
                MoveSpeed = 310,
                CurrentLevel = 15,
                CurrentHealth = 980,
                CurrentMana = 650,
                Gold = 2450,
                AbilityPointsAvailable = 1,

                Abilities = new List<Ability>
            {
                new Ability
                {
                    Name = "Acorn Shot",
                    ManaCost = new List<int> { 70, 80, 90, 100 },
                    Cooldown = new List<double> { 18, 16, 14, 12 },
                    Description = "Fires an acorn at the target unit. The acorn bounces to nearby targets, dealing damage and applying a slow.",
                },
                new Ability
                {
                    Name = "Bushwhack",
                    ManaCost = new List<int> { 100, 110, 120, 130 },
                    Cooldown = new List<double> { 12, 11, 10, 9 },
                    Description = "Sets a trap that stuns enemy heroes after a short delay. Deals damage and reveals the area.",
                },
                new Ability
                {
                    Name = "Scurry",
                    ManaCost = new List<int> { 60, 50, 40, 30 },
                    Cooldown = new List<double> { 22, 20, 18, 16 },
                    Description = "Hoodwink gains bonus movement speed and phased movement. Passing near trees refreshes the duration.",
                },
                new Ability
                {
                    Name = "Sharpshooter",
                    ManaCost = new List<int> { 125, 175, 225 },
                    Cooldown = new List<double> { 40, 35, 30 },
                    Description = "Channels a powerful shot that deals heavy damage and applies a strong slow. Damage increases with channel time.",
                }
            },
                Inventory = new List<DotaItem>
            {
                new DotaItem
                {
                    Name = "Gleipnir",
                    Cost = 6150,
                    InBackpack = false,
                    CooldownRemaining = 0
                },
                new DotaItem
                {
                    Name = "Dragon Lance",
                    Cost = 1900,
                    InBackpack = false,
                    CooldownRemaining = 0
                },
                new DotaItem
                {
                    Name = "Power Treads",
                    Cost = 1400,
                    InBackpack = false,
                    CooldownRemaining = 0
                },
                new DotaItem
                {
                    Name = "Magic Wand",
                    Cost = 450,
                    InBackpack = false,
                    Charges = 17,
                    CooldownRemaining = 0
                },
                new DotaItem
                {
                    Name = "Town Portal Scroll",
                    Cost = 100,
                    InBackpack = false,
                    Charges = 1,
                    CooldownRemaining = 0
                }
            },

                NeutralItem = new DotaItem
                {
                    Name = "Quickening Charm",
                    Cost = 0,
                    InBackpack = false,
                    CooldownRemaining = 0
                },

                MatchStats = new MatchStats
                {
                    Kills = 7,
                    Deaths = 4,
                    Assists = 18,
                },

                CustomAttributes = new Dictionary<string, object>
                {
                    ["я"] = "не",
                    ["придумал"] = 15,
                    ["че"] = true,
                    ["тут"] = "2023-11-15",
                    ["написать"] = ":)"
                },

                Lore = "Белочка"
            };
        }
    }
}