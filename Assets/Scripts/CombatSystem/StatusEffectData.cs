using System;
using UnityEngine;

[Flags]
public enum StatusFlags
{
    None         = 0,         // 0000 0000
    Electric     = 1 << 0,    // 0000 0001 (감전)
    Burn         = 1 << 1,    // 0000 0010 (화상)
    Frostbite    = 1 << 2,    // 0000 0100 (동상)
    Bleed        = 1 << 3,    // 0000 1000 (출혈)
    Poison       = 1 << 4,    // 0001 0000 (중독)

    ElementalGroup = Electric | Burn | Frostbite 
}

public class StatusEffectData
{
    public float duration;
    public float tickDamage;
    public float tickTimer;

    public StatusEffectData(float duration, float tickDamage)
    {
        this.duration = duration;
        this.tickDamage = tickDamage;
        tickTimer = 0f;
    }
}