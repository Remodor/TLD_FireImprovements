using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace FireImprovements
{
    using Settings = Fire_Settings;
    public class Implementation : MelonMod
    {
        public override void OnApplicationStart()
        {
            Debug.Log($"[{Info.Name}] version {Info.Version} loaded!");
            Settings.OnLoad();
        }
        //* Same as games GetBestMatches only reverse.
        internal static GearItem GetWorstMatches(List<GearItemObject> items)
        {
            GearItem matches = null;
            for (int i = 0; i < items.Count; i++)
            {
                GearItem currentMatches = items[i];
                if (currentMatches && currentMatches.m_MatchesItem)
                {
                    if (!Utils.IsZero(currentMatches.m_CurrentHP))
                    {
                        if (!matches)
                        {
                            matches = currentMatches;
                        }
                        // If burntime lower, or burntime the same, but hp lower.
                        else if (currentMatches.m_MatchesItem.m_MaxBurnTimeGametimeSeconds < matches.m_MatchesItem.m_MaxBurnTimeGametimeSeconds ||
                        (currentMatches.m_MatchesItem.m_MaxBurnTimeGametimeSeconds == matches.m_MatchesItem.m_MaxBurnTimeGametimeSeconds &&
                        currentMatches.m_CurrentHP < matches.m_CurrentHP))
                        {
                            matches = currentMatches;
                        }
                    }
                }
            }
            return matches;
        }
        //* Fire Starters.
        internal static Comparison<GearItem> FireStarterIgniteChanceComparison = new System.Func<GearItem, GearItem, int>(CompareFireStartersIgniteChance);
        internal static Comparison<GearItem> FireStarterTorchesFlaresFirstComparison = new System.Func<GearItem, GearItem, int>(CompareFireStartersTorchesFlaresFirst);
        internal static Predicate<GearItem> TorchesOrFlaresPredicate = new System.Func<GearItem, bool>(IsTorchesOrFlares);
        internal static Predicate<GearItem> FireStarterPredicate = new System.Func<GearItem, bool>(IsLastFireStarter);
        internal static Predicate<GearItem> EqualFireStarterPredicate = new System.Func<GearItem, bool>(EqualLastFireStarter);
        internal static void SortFireStarter(ref List<GearItem> items)
        {
            if (Settings.Get().sort_fire_starter_by_ignite_chance) { items.Sort(FireStarterIgniteChanceComparison); }
            if (Settings.Get().sort_torches_flares_first) { items.Sort(FireStarterTorchesFlaresFirstComparison); }
        }
        internal static void SelectFireStarter(Panel_FireStart instance)
        {
            int index = -1;
            if (Settings.Get().select_torches_flares_first) { index = instance.m_StarterList.FindIndex(TorchesOrFlaresPredicate); }
            if (Settings.Get().remember_fire_starter && index == -1)
            {
                index = instance.m_StarterList.FindIndex(FireStarterPredicate); // Find the last fire starter.
                if (index == -1) { index = instance.m_StarterList.FindIndex(EqualFireStarterPredicate); } // If not, find equal.
            }
            if (index == -1) { index = 0; }
            instance.m_SelectedStarterIndex = index;
        }
        //* Tinder.
        internal static Comparison<GearItem> TinderWeightComparison = new System.Func<GearItem, GearItem, int>(CompareTinderWeight);
        internal static Predicate<GearItem> TinderPredicate = new System.Func<GearItem, bool>(EqualLastTinder);
        internal static void SortTinder(ref List<GearItem> items)
        {
            if (Settings.Get().sort_tinder_by_weight) { items.Sort(TinderWeightComparison); }
        }
        internal static void SelectTinder(Panel_FireStart instance)
        {
            int index = -1;
            if (Settings.Get().remember_tinder) { index = instance.m_TinderList.FindIndex(TinderPredicate); }
            if (index == -1) { index = 0; }
            instance.m_SelectedTinderIndex = index;
        }
        //* Fuel.
        internal static Comparison<GearItem> FuelBurnDurationComparison = new System.Func<GearItem, GearItem, int>(CompareFuelBurnDuration);
        internal static Comparison<GearItem> FuelBurnDurationReverseComparison = new System.Func<GearItem, GearItem, int>(CompareFuelBurnDurationReverse);
        internal static Comparison<GearItem> FuelIgniteChanceComparison = new System.Func<GearItem, GearItem, int>(CompareFuelIgniteChance);
        internal static Comparison<GearItem> FuelBurnDurationAndIgniteChanceComparison = new System.Func<GearItem, GearItem, int>(CompareFuelBurnDurationAndIgniteChance);

        internal static Predicate<GearItem> FuelPredicate = new System.Func<GearItem, bool>(EqualLastFuel);
        internal static void SortFuel(ref List<GearItem> items)
        {
            if (Settings.Get().sort_fuel_by_burn_duration && Settings.Get().sort_fuel_by_ignite_chance)
            {
                items.Sort(FuelBurnDurationAndIgniteChanceComparison);
            }
            else if (Settings.Get().sort_fuel_by_burn_duration) { items.Sort(FuelBurnDurationComparison); }
            else if (Settings.Get().sort_fuel_by_ignite_chance) { items.Sort(FuelIgniteChanceComparison); }
        }
        internal static void SelectFuel(Panel_FireStart instance)
        {
            int index = -1;
            if (Settings.Get().remember_fuel) { index = instance.m_FuelList.FindIndex(FuelPredicate); }
            if (index == -1) { index = 0; }
            instance.m_SelectedFuelIndex = index;
        }
        //* Accelerant.
        internal static Predicate<GearItem> AccelerantPredicate = new System.Func<GearItem, bool>(EqualLastAccelerant);
        internal static void SelectAccelerant(Panel_FireStart instance)
        {
            int index = -1;
            if (Settings.Get().remember_accelerant) { index = instance.m_AccelerantList.FindIndex(AccelerantPredicate); }
            if (index == -1) { index = 0; }
            instance.m_SelectedAccelerantIndex = index;
        }
        //* Save fire ressources.
        internal static int LastFireStarterID = -1;
        internal static String LastFireStarterName = "";
        internal static String LastTinderName = "";
        internal static String LastFuelName = "";
        internal static String LastAccelerantName = "";
        internal static void SaveLastUsedFireRessources(Panel_FireStart instance)

        {
            FireStarterItem fireStarter = instance.GetSelectedFireStarter();
            FuelSourceItem tinder = instance.GetSelectedTinder();
            FuelSourceItem fuel = instance.GetSelectedFuelSource();
            FireStarterItem accelerant = instance.GetSelectedAccelerant();
            if (fireStarter)
            {
                var fireStarterGi = fireStarter.GetComponent<GearItem>();
                if (!fireStarterGi.m_TorchItem && !fireStarterGi.m_FlareItem)
                {
                    LastFireStarterID = fireStarterGi.m_InstanceID;
                    LastFireStarterName = fireStarterGi.m_GearName;
                }
            }
            else
            {
                LastFireStarterID = -1;
                LastFireStarterName = "";
            }
            if (tinder)
            {
                LastTinderName = tinder.GetComponent<GearItem>().m_GearName;
            }
            else { LastTinderName = ""; }
            if (fuel)
            {
                LastFuelName = fuel.GetComponent<GearItem>().m_GearName;
            }
            else { LastFuelName = ""; }
            if (accelerant)
            {
                LastAccelerantName = accelerant.GetComponent<GearItem>().m_GearName;
            }
            else { LastAccelerantName = ""; }
        }
        //* Predicate and comparison methods.
        public static bool IsTorchesOrFlares(GearItem gearItem)
        {
            if (gearItem.m_TorchItem || gearItem.m_FlareItem) { return true; }
            return false;
        }
        public static bool IsLastFireStarter(GearItem gearItem)
        {
            if (gearItem && gearItem.m_InstanceID == LastFireStarterID) { return true; }
            return false;
        }
        public static bool EqualLastFireStarter(GearItem gearItem)
        {
            if (gearItem && gearItem.m_GearName == LastFireStarterName) { return true; }
            return false;
        }
        public static bool EqualLastTinder(GearItem gearItem)
        {
            if (gearItem && gearItem.m_GearName == LastTinderName) { return true; }
            return false;
        }
        public static bool EqualLastFuel(GearItem gearItem)
        {
            if (gearItem && gearItem.m_GearName == LastFuelName) { return true; }
            return false;
        }
        public static bool EqualLastAccelerant(GearItem gearItem)
        {
            if (gearItem && gearItem.m_GearName == LastAccelerantName) { return true; }
            return false;
        }
        public static int CompareFireStartersIgniteChance(GearItem g1, GearItem g2)
        {
            FireStarterItem fireStarterItem1 = g1.GetComponent<FireStarterItem>();
            FireStarterItem fireStarterItem2 = g2.GetComponent<FireStarterItem>();

            if (fireStarterItem1.m_RequiresSunLight && !InterfaceManager.m_Panel_FireStart.HasDirectSunlight())
            {
                return 1; //g1 to the back
            }
            if (fireStarterItem2.m_RequiresSunLight && !InterfaceManager.m_Panel_FireStart.HasDirectSunlight())
            {
                return -1; //g1 to the front
            }
            if (fireStarterItem1.m_FireStartSkillModifier < fireStarterItem2.m_FireStartSkillModifier)
            {
                return 1; //g1 to the back
            }
            else if (fireStarterItem1.m_FireStartSkillModifier > fireStarterItem2.m_FireStartSkillModifier)
            {
                return -1; //g1 to the front
            }
            return 0;
        }
        public static int CompareFireStartersTorchesFlaresFirst(GearItem g1, GearItem g2)
        {
            if (g1.m_TorchItem && !g2.m_TorchItem)
            {
                return -1;
            }
            if (!g1.m_TorchItem && g2.m_TorchItem)
            {
                return 1;
            }
            return 0;
        }
        public static int CompareTinderWeight(GearItem g1, GearItem g2)
        {
            float g1_weight = g1.m_WeightKG;
            float g2_weight = g2.m_WeightKG;
            if (g1_weight < g2_weight)
            {
                return -1; //g1 to the front
            }
            else if (g1_weight > g2_weight)
            {
                return 1; //g1 to the back
            }
            return 0;
        }
        public static int CompareFuelIgniteChance(GearItem g1, GearItem g2)
        {
            float g1_igniteChance = g1.GetComponent<FuelSourceItem>().m_FireStartSkillModifier;
            float g2_igniteChance = g2.GetComponent<FuelSourceItem>().m_FireStartSkillModifier;
            if (g1_igniteChance < g2_igniteChance)
            {
                return 1; //g1 to the back
            }
            else if (g1_igniteChance > g2_igniteChance)
            {
                return -1; //g1 to the front
            }
            return 0;
        }
        public static int CompareFuelBurnDuration(GearItem g1, GearItem g2)
        {
            float g1_burnDuration = g1.GetComponent<FuelSourceItem>().m_BurnDurationHours;
            float g2_burnDuration = g2.GetComponent<FuelSourceItem>().m_BurnDurationHours;
            if (g1_burnDuration < g2_burnDuration)
            {
                return -1; //g1 to the front
            }
            else if (g1_burnDuration > g2_burnDuration)
            {
                return 1; //g1 to the back
            }
            return 0;
        }
        public static int CompareFuelBurnDurationReverse(GearItem g1, GearItem g2)
        {
            float g1_burnDuration = g1.GetComponent<FuelSourceItem>().m_BurnDurationHours;
            float g2_burnDuration = g2.GetComponent<FuelSourceItem>().m_BurnDurationHours;
            if (g1_burnDuration < g2_burnDuration)
            {
                return 1; //g1 to the front
            }
            else if (g1_burnDuration > g2_burnDuration)
            {
                return -1; //g1 to the back
            }
            return 0;
        }
        public static int CompareFuelBurnDurationAndIgniteChance(GearItem g1, GearItem g2)
        {
            int compFuelIgnite = CompareFuelIgniteChance(g1, g2);
            if (compFuelIgnite == 0)
            {
                return CompareFuelBurnDuration(g1, g2);
            }
            return compFuelIgnite;
        }
    }
}
