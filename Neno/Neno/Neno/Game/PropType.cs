using System;

namespace Neno
{
    public enum PropType
    {
        //Entity Properties
        X, Y, MaxHp, Hp, MaxStamina, Stamina, Owner,
        HairR, HairG, HairB,
        SkinR, SkinG, SkinB,
        EyeR, EyeG, EyeB,
        ShirtR, ShirtG, ShirtB,
        PantsR, PantsG, PantsB,

        //Items
        Value,

        //Attributes
        IsRandomGen,
        Unbreakable,

        //Weapon
        Weight, DmgMin, DmgMax, DmgCap,
        DmgSharp, DmgBlunt, DmgMagic,

        //Consumable
        EffectTime, AddFood, AddHp,

        //Entity Place
        Count,

        //Equip
        Armor, ArmorBlunt, ArmorSharp,
    }
}
