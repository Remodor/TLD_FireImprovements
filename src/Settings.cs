using ModSettings;

namespace FireImprovements
{
    internal class FireImprovements_Settings : JsonModSettings
    {
        //* ----Torches----
        [Section("Torches")]
        [Name("Worst Torch in Radial")]
        [Description("If you want to show the worst torch in the radial menu.\n(Vanilla = false [Best torch])")]
        public bool worst_torch_in_radial = false;

        [Name("Worst Matches to Light Torch")]
        [Description("If you want to use the worst matches to light a torch.\n(Vanilla = false [Best matches])")]
        public bool worst_matches_for_torch = false;

        //* ----Fire Starters----
        [Section("Fire Starters")]
        [Name("Sort by Igniting Chance")]
        [Description("If you want to sort torches by the igniting chance. Highest first.\n(Vanilla = false [Sorted by naming])")]
        public bool sort_fire_starter_by_ignite_chance = false;

        [Name("Remember Last Fire Starter")]
        [Description("If you want the game to automatically preselect the last used fire starter.\n(Vanilla = false [Select the first item in the list])")]
        public bool remember_fire_starter = false;

        [Name("Always Sort Torches/ Flares To First")]
        [Description("If you want to always sort burning torches/ flares to the first position regardless of the sorting order. This means torches/ flares are no longer in the usual sorting order for fire starters(e.g. sort after ignite chance).\n(Vanilla = false [Torches/ flares are sorted as usual items])")]
        public bool sort_torches_flares_first = false;
        [Name("Always Select Torches/ Flares First")]
        [Description("If you want to always select burning torches/ flares first regardless of the sorting position or the last selected fire starter.\n(Vanilla = false)")]
        public bool select_torches_flares_first = false;

        //* ----Tinder----
        [Section("Tinder")]
        [Name("Sort by Weight")]
        [Description("If you want to sort tinder by weight. Lowest first.\n(Vanilla = false)")]
        public bool sort_tinder_by_weight = false;

        [Name("Remember Last Tinder")]
        [Description("If you want the game to automatically preselect the last used tinder.\n(Vanilla = false [Select the first item in the list])")]
        public bool remember_tinder = false;

        [Name("Tinder Not Required Level")]
        [Description("The level where tinder isn't required anymore. Won't get updated in the skill log page.\n(Vanilla = 3, 6 = Allways required)")]
        [Slider(0, 6, 7)]
        public int tinder_not_required_level = 3;

        [Name("No Tinder Penalty")]
        [Description("The fire start success penalty for using no tinder.\n(Vanilla = 0%)")]
        public int no_tinder_penalty = 5;

        [Name("        Penalty Level")]
        [Description("This setting allows you to set a penalty multiplier per level below selected. E.g. if your current level is 3 and you chose level 5 your penalty will be (5 - 3) x selected penalty. If you are level 5 and select level 5 your penalty becomes 0.\n0 = Constant penalty on all levels,\n5 = No penalty at 5,\n6 = 1 x penalty at 5, etc.")]
        [Slider(0, 10, 11)]
        public int tinder_penalty_level_multiplier = 6;
        //* ----Fuel----
        [Section("Fuel")]
        [Name("Sort by Burn Duration")]
        [Description("If you want to sort fuel by burn duration. Highest first.\n(Vanilla = false [Sorted by naming])")]
        public bool sort_fuel_by_burn_duration = false;

        [Name("Sort by Igniting Chance")]
        [Description("If you want to sort fuel by igniting chance. Highest first.\n(Vanilla = false)\n\nWhen burn duration sorting is also set to true fuel is sorted by igniting chance and by burn duration. Igniting chance has the higher priority.")]
        public bool sort_fuel_by_ignite_chance = false;

        [Name("Remember Last Fuel")]
        [Description("If you want the game to automatically preselect the last used fuel.\n(Vanilla = false [Select the first item in the list])")]
        public bool remember_fuel = false;

        //* ----Accelerant----
        [Section("Accelerant")]
        [Name("Remember Last Accelerant")]
        [Description("If you want the game to automatically preselect the last used accelerant.\n(Vanilla = false [Select the first item in the list])")]
        public bool remember_accelerant = false;

        //* ----Fire----
        [Section("Fire")]
        [Name("No More Fire Duration Boost")]
        [Description("Fire burns longer when the player is outside in the cold. It doesn't matter where the fire is. This disables this mechanic.\n(Vanilla = false)")]
        public bool no_more_fire_boost = false;
    }

    internal static class Fire_Settings
    {
        private static FireImprovements_Settings settings = new FireImprovements_Settings();

        public static void OnLoad()
        {
            settings.AddToModSettings("Fire Improvements");
        }

        internal static FireImprovements_Settings Get()
        {
            return settings;
        }
    }
}
