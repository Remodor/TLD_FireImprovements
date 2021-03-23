using Harmony;
using Il2CppSystem.Collections.Generic;

namespace FireImprovements
{
    //* Use lowest torch.
    [HarmonyPatch(typeof(Inventory), "GetHighestConditionGearThatMatchesName")]
    internal class Inventory_GetHighestConditionGearThatMatchesName
    {
        internal static void Postfix(Inventory __instance, string name, ref GearItem __result)
        {
            if (name == "GEAR_Torch" && Fire_Settings.Get().worst_torch_in_radial)
            {
                __result = Utils.GetLowestConditionGearThatMatchesName(__instance.m_Items, name);
            }
        }
    }
    //* Use worst matches to light torch.
    [HarmonyPatch(typeof(Panel_TorchLight), "SetupScrollList")]
    internal class Panel_TorchLight_SetupScrollList
    {
        internal static bool SetupScrollList = false;
        internal static void Prefix()
        {
            SetupScrollList = true;
        }
        internal static void Postfix()
        {
            SetupScrollList = false;
        }
    }
    [HarmonyPatch(typeof(Inventory), "GetBestMatches")]
    internal class Inventory_GetBestMatches
    {
        internal static void Postfix(Inventory __instance, ref GearItem __result)
        {
            if (Panel_TorchLight_SetupScrollList.SetupScrollList && Fire_Settings.Get().worst_matches_for_torch)
            {
                __result = Implementation.GetWorstMatches(__instance.m_Items);
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
    //* Load last used fire ressource
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
}
