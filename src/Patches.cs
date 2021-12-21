using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace FireImprovements
{
    using Settings = Fire_Settings;
    //* Use lowest torch. Use worst fire striker.
    [HarmonyPatch(typeof(Inventory), "GetHighestConditionGearThatMatchesName")]
    internal class Inventory_GetHighestConditionGearThatMatchesName
    {
        internal static void Postfix(Inventory __instance, string name, ref GearItem __result)
        {
            if (name == "GEAR_Torch" && Settings.Get().worst_torch_in_radial)
            {
                __result = Utils.GetLowestConditionGearThatMatchesName(__instance.m_Items, name);
            } else if (name == "GEAR_Firestriker" && Settings.Get().worst_firestriker)
            {
                __result = Utils.GetLowestConditionGearThatMatchesName(__instance.m_Items, name);
            }
        }
    }
    //* Use lowest torch in quick access.
    [HarmonyPatch(typeof(Utils), "GetBestTorch")]
    internal class Utils_GetBestTorch
    {
        internal static void Postfix(Inventory __instance, List<GearItemObject> items, ref GearItem __result)
        {
            if (Settings.Get().worst_torch_quick_access)
            {
                __result = Implementation.GetWorstTorch(items);
            }
        }
    }
    //* Use worst matches.
    [HarmonyPatch(typeof(Inventory), "GetBestMatches", new System.Type[] { typeof(MatchesType) })]
    internal class Inventory_GetBestMatchess
    {
        internal static void Postfix(Inventory __instance, MatchesType matchesType, ref GearItem __result)
        {
            if (Settings.Get().worst_matches)
            {
                __result = Implementation.GetWorstMatches(__instance.m_Items, matchesType);
            }
        }
    }
    //* Sort fire resource lists
    [HarmonyPatch(typeof(Panel_FireStart), "RefreshList")]
    internal class Panel_FireStart_RefreshList
    {
        internal static void Postfix(Panel_FireStart __instance, ref List<GearItem> gearList, FireStartMaterialType type)
        {
            switch (type)
            {
                case FireStartMaterialType.FireStarter:
                    Implementation.SortFireStarter(ref gearList);
                    break;
                case FireStartMaterialType.Tinder:
                    Implementation.SortTinder(ref gearList);
                    break;
                case FireStartMaterialType.FuelSource:
                    Implementation.SortFuel(ref gearList);
                    break;
            }
        }
    }
    //* Load last used fire resource
    [HarmonyPatch(typeof(Panel_FireStart), "Enable")]
    internal class Panel_FireStart_Enable
    {
        internal static void Postfix(Panel_FireStart __instance)
        {
            Implementation.SelectFireStarter(__instance);
            Implementation.SelectTinder(__instance);
            Implementation.SelectFuel(__instance);
            Implementation.SelectAccelerant(__instance);
        }
    }
    //* Save last used fire ressource
    [HarmonyPatch(typeof(Panel_FireStart), "OnStartFire")]
    internal class Panel_FireStart_OnStartFire
    {
        internal static void Prefix(Panel_FireStart __instance)
        {
            Implementation.SaveLastUsedFireRessources(__instance);
        }
    }
    //* Tinder level.
    [HarmonyPatch(typeof(Panel_FireStart), "CanStartFire")]
    internal class Panel_FireStart_CanStartFire
    {
        internal static void Prefix()
        {
            GameManager.GetSkillFireStarting().m_LevelWhereTinderNotRequired = Settings.Get().tinder_not_required_level;
        }
    }
    //* No tinder penalty.
    [HarmonyPatch(typeof(FireManager), "CalclateFireStartSuccess")]
    internal class FireManager_CalclateFireStartSuccess
    {
        internal static void Postfix(ref float __result)
        {
            if (InterfaceManager.m_Panel_FireStart.GetSelectedTinder() == null)
            {
                float penalty = Settings.Get().no_tinder_penalty;
                if (Settings.Get().tinder_penalty_level_multiplier != 0)
                {
                    penalty *= Mathf.Max(Settings.Get().tinder_penalty_level_multiplier - GameManager.GetSkillFireStarting().GetCurrentTierNumber() - 1, 0);
                }
                __result -= penalty;
            }
        }
    }
    //* No more fire boost.
    [HarmonyPatch(typeof(Fire), "GetWeatherAdjustedElapsedDuration")]
    internal class Fire_GetWeatherAdjustedElapsedDuration
    {
        internal static bool Prefix(ref float __result)
        {
            if (Settings.Get().no_more_fire_boost)
            {
                __result = GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);
                return false;
            }
            return true;
        }
    }
}
