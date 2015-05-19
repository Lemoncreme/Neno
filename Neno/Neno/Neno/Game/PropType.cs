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

        //Item Properties

        //Basic
        Weight, DmgMin, DmgMax, DmgCap,

        //Damage Types
        DmgSharp, DmgBlunt, DmgMagic,

        //Attributes
        IsRandomGen,
        Unbreakable
    }
}
