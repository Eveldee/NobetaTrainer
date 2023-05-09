using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using static ItemSystem;

namespace NobetaTrainer.Utils;

public static class ItemUtils
{
    public static IList<ItemType> HPItems { get; } = new List<ItemType>
    {
        ItemType.HPCure,
        ItemType.HPCureMiddle,
        ItemType.HPCureBig,
        ItemType.HPCureTemp,
        ItemType.HPMaxAdd
    };
    public static string[] HPItemNames { get; } = HPItems.Select(item => item.GetName()).ToArray();

    public static IList<ItemType> MPItems { get; } = new List<ItemType>
    {
        ItemType.MPCure,
        ItemType.MPCureMiddle,
        ItemType.MPCureBig,
        ItemType.MPCureTemp,
        ItemType.MPMaxAdd
    };
    public static string[] MPItemNames { get; } = MPItems.Select(item => item.GetName()).ToArray();

    public static IList<ItemType> BuffItems { get; } = new List<ItemType>
    {
        ItemType.Defense,
        ItemType.DefenseM,
        ItemType.DefenseB,
        ItemType.Mysterious,
        ItemType.MysteriousM,
        ItemType.MysteriousB,
        ItemType.Holy,
        ItemType.HolyM,
        ItemType.HolyB
    };
    public static string[] BuffItemNames { get; } = BuffItems.Select(item => item.GetName()).ToArray();

    public static IList<ItemType> OtherItems { get; } = new List<ItemType>
    {
        ItemType.MagicNull,
        ItemType.MagicIce,
        ItemType.MagicFire,
        ItemType.MagicLightning,
        ItemType.SkyJump,
        ItemType.Absorb,
        ItemType.BagMaxAdd,
        ItemType.SPMaxAdd,
        ItemType.AreaTeleport,
        ItemType.Property
    };
    public static string[] OtherItemNames { get; } = OtherItems.Select(item => item.GetName()).ToArray();
}