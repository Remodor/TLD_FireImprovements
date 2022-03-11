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
                return;
            } else if (name == "GEAR_Firestriker" && Settings.Get().worst_firestriker)
            {
                __result = Utils.GetLowestConditionGearThatMatchesName(__instance.m_Items, name);
                return;
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
    //* Don't light torch with one starter.
    [HarmonyPatch(typeof(Panel_TorchLight), "OnUseSelectedItem")]
    internal class Panel_TorchLight_OnUseSelectedItem
    {
        internal static bool Prefix(Panel_TorchLight __instance)
        {
            if (Settings.Get().no_auto_use_torch_lighter && __instance.m_ScrollList.m_ScrollObjects.Count == 1
                && Panel_TorchLight_Enable.enable)
            {
                __instance.RefreshVisuals();
                Input.ResetInputAxes();
                Panel_TorchLight_Enable.enable = false;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Panel_TorchLight), "Enable")]
    internal class Panel_TorchLight_Enable
    {
        internal static bool enable = false;
        internal static void Prefix()
        {
            enable = true;
        }
        internal static void Postfix()
        {
            enable = false;
        }
    }
    //* Use worst matches. Sort torch starters.
    [HarmonyPatch(typeof(Inventory), "GetBestMatches", new System.Type[] { typeof(MatchesType) })]
    internal class Inventory_GetBestMatchess
    {
        internal static void Postfix(Inventory __instance, MatchesType matchesType, ref GearItem __result)
        {
            if (Settings.Get().sort_torch_starter && InterfaceManager.m_Panel_TorchLight.IsEnabled())
            {
                if (matchesType == MatchesType.CardboardMatches)
                {
                    __result = Implementation.GetWorstMatches(__instance.m_Items, MatchesType.WoodMatches);
                }
                if (matchesType == MatchesType.WoodMatches)
                {
                    __result = Implementation.GetWorstMatches(__instance.m_Items, MatchesType.CardboardMatches);
                }
                return;
            }
            if (Settings.Get().worst_matches)
            {
                __result = Implementation.GetWorstMatches(__instance.m_Items, matchesType);
                return;
            }
        }
    }
    //* Turn off 0 heat fires.
    [HarmonyPatch(typeof(Panel_FeedFire), "OnTakeTorch")]
    internal class Panel_FeedFire_OnTakeTorch
    {
        internal static void Postfix(Panel_FeedFire  __instance)
        {
            if (Settings.Get().turn_off_0_heat_fires && __instance.m_Fire.m_FuelHeatIncrease <= 0.1f)
            {
                __instance.m_Fire.m_ElapsedOnTODSeconds = __instance.m_Fire.m_MaxOnTODSeconds;
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
    //* Save fire state
    [HarmonyPatch(typeof(LoadScene), "LoadLevelWhenFadeOutComplete")]
    internal class LoadScene_Load
    {
        internal static bool carriedLitTorch = false;
        internal static void Prefix()
        {
            GearItem itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;
            if (itemInHands && itemInHands.IsLitTorch())
            {
                carriedLitTorch = true;
            } else
            {
                carriedLitTorch = false;
            }
        }
    }
    [HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData", new System.Type[] { typeof(string), typeof(string) })]
    internal class LoadSceneData_Load
    {
        public static void Postfix()
        {
            GearItem itemInHands = GameManager.GetPlayerManagerComponent().m_ItemInHands;
            if (Settings.Get().restore_lit_torch && itemInHands && itemInHands.m_TorchItem 
                && LoadScene_Load.carriedLitTorch && !itemInHands.IsLitTorch())
            {
                MelonLogger.Msg("Restored lit torch!");
                itemInHands.m_TorchItem.m_State = TorchState.Burning;
            }
        }
    }
}
