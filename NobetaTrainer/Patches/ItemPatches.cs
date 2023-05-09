using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Patches;

public static class ItemPatches
{
    public static int SelectedHPItemIndex;
    public static int SelectedMPItemIndex;
    public static int SelectedBuffItemIndex;
    public static int SelectedOtherItemIndex;
    public static int ItemSlots;

    public static void GiveHPItem()
    {
        var itemType = ItemUtils.HPItems[SelectedHPItemIndex];

        GiveItem(itemType);
    }

    public static void GiveMPItem()
    {
        var itemType = ItemUtils.MPItems[SelectedMPItemIndex];

        GiveItem(itemType);
    }

    public static void GiveBuffItem()
    {
        var itemType = ItemUtils.BuffItems[SelectedBuffItemIndex];

        GiveItem(itemType);
    }

    public static void SpawnOther()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var pool = Singletons.ItemSystem.itemPoolMap[ItemUtils.OtherItems[SelectedOtherItemIndex]];

            pool.NewUse(Singletons.WizardGirl.GetCenter(), Quaternion.identity, false);
        });
    }

    public static void UpdateSlots()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            Singletons.WizardGirl.g_PlayerItem.g_iItemSize = ItemSlots;
            Singletons.StageUi.itemBar.UpdateItemSize(ItemSlots);
        });
    }

    private static void GiveItem(ItemSystem.ItemType itemType)
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var wizardGirl = Singletons.WizardGirl;
            var items = wizardGirl.g_PlayerItem;

            // Find first empty slot if there's any
            for (int i = 0; i < items.g_iItemSize; i++)
            {
                if (items.g_HoldItem[i] == ItemSystem.ItemType.Null)
                {
                    items.g_HoldItem[i] = itemType;
                    Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);

                    return;
                }
            }

            // Replace actual slot
            items.g_HoldItem[Singletons.StageUi.itemBar.itemSelectPos] = itemType;
            Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
        });
    }

    // Update ItemSlots on load
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    private static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
    {
        ItemSlots = __instance.g_PlayerItem.g_iItemSize;
    }
}