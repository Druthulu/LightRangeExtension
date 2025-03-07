using System;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.Xml;

public class LightRanger : IModApi
{
    public static string modsFolderPath;
    public static float LightIntensity = 2f; // 0.75f
    public static float LightViewDistanceScale = 25f; // 1.5f default values
    public static float LightRange = 0.75f; // 0.5f
    public static float SunIntensityScale = 1.5f; // 1.1f
    public static float MoonAmbientScale = 0.7f; // 1f
    public static float TileEntityLightRange = 150f; // 10f

    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + base.GetType().ToString());
        modsFolderPath = _modInstance.Path;
        ReadXML();
        Harmony harmony = new Harmony(base.GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    void ReadXML()
    {
        using (XmlReader xmlReader = XmlReader.Create(modsFolderPath + "\\settings.xml"))
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.ToString() == "LightIntensity")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out LightIntensity))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {LightIntensity}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                    if (xmlReader.Name.ToString() == "LightViewDistanceScale")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out LightViewDistanceScale))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {LightViewDistanceScale}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                    if (xmlReader.Name.ToString() == "LightRange")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out LightRange))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {LightRange}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                    if (xmlReader.Name.ToString() == "SunIntensityScale")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out SunIntensityScale))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {SunIntensityScale}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                    if (xmlReader.Name.ToString() == "MoonAmbientScale")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out MoonAmbientScale))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {MoonAmbientScale}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                    if (xmlReader.Name.ToString() == "TileEntityLightRange")
                    {
                        string temp = xmlReader.ReadElementContentAsString().ToLower();
                        if (!float.TryParse(temp, out TileEntityLightRange))
                        {
                            Log.Warning($"[Majoras Mask Moon] Failed to read settings.xml setting scaleMoon. Using default of {TileEntityLightRange}");
                            Log.Warning($"[Majoras Mask Moon] Failed settings.xml read should be reviewed and mod reinstalled if needed");
                        }
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(ControllerGUI), "Start")]
public class Patch_ControllerGUIStart
{
    public static void Postix(ControllerGUI __instance, ref float __lightIntensity)
    {
        __lightIntensity = LightRanger.LightIntensity;
    }
}



[HarmonyPatch(typeof(LightLOD), "CalcViewDistance")]
public class Patch_LightLODCalcViewDistance
{
    public static bool Prefix(LightLOD __instance)
    {
        __instance.lightViewDistance = Utils.FastMax(__instance.MaxDistance, __instance.lightRangeMaster * LightRanger.LightViewDistanceScale);
        return false;
    }
}

[HarmonyPatch(typeof(LightLOD), "FrameUpdate")]
public class Patch_LightLODFrameUpdate
{
    public static bool Prefix(Vector3 cameraPos, LightLOD __instance)
    {
        __instance.priority = 0f;
        if (__instance.bRenderingOff || __instance.lightStateEnabled)
        {
            return false;
        }
        __instance.CheckInitialBlock();
        if (!__instance.bSwitchedOn)
        {
            return false;
        }
        Light light = __instance.myLight;
        if (light)
        {
            float num = (__instance.selfT.position - cameraPos).sqrMagnitude * __instance.DistanceScale;
            float num2 = Mathf.Sqrt(num) - __instance.lightRange;
            if (num2 < 0f)
            {
                num2 = 0f;
            }
            float num3 = __instance.lightViewDistance;
            if (__instance.bPlayerPlacedLight)
            {
                num3 *= LightRanger.LightViewDistanceScale; // ORIG 1.2f
            }
            if (LightLOD.DebugViewDistance > 0f)
            {
                num3 = Utils.FastMax(LightLOD.DebugViewDistance, __instance.lightRange + 0.01f);
            }
            float num4 = num3 * num3;
            __instance.distSqRatio = num / num4;
            if (__instance.bToggleable)
            {
                __instance.LightStateCheck();
            }
            float num5 = num3 - __instance.lightRange;
            if (num2 < num5)
            {
                __instance.priority = 1f;
                if (__instance.bPlayerPlacedLight)
                {
                    if (__instance.distSqRatio >= 0.640000045f)
                    {
                        light.shadows = LightShadows.None;
                    }
                    else if (__instance.distSqRatio >= 0.0625f)
                    {
                        if (__instance.shadowStateMaster == LightShadows.Soft)
                        {
                            light.shadows = LightShadows.Hard;
                        }
                        light.shadowStrength = (1f - Utils.FastClamp01((__instance.distSqRatio - 0.36f) / 0.280000031f)) * __instance.shadowStrengthMaster;
                    }
                    else
                    {
                        light.shadows = __instance.shadowStateMaster;
                        light.shadowStrength = __instance.shadowStrengthMaster;
                    }
                }
                float num6 = num2 / num5;
                float num7 = 1f - num6 * num6;
                light.intensity = __instance.lightIntensity * num7;
                light.range = __instance.lightRange * LightRanger.LightRange + __instance.lightRange * LightRanger.LightRange * num7;
                light.enabled = true;
            }
            else
            {
                light.enabled = false;
            }
            if (__instance.lensFlare != null)
            {
                if (num < 10f * num4)
                {
                    float num8 = (1f - num / (num4 * 10f)) * __instance.lightIntensity * 0.33f * __instance.FlareBrightnessFactor;
                    if (num8 > 1f)
                    {
                        num8 = 1f;
                    }
                    if (__instance.lightRange < 4f)
                    {
                        num8 *= __instance.lightRange * 0.25f;
                    }
                    __instance.lensFlare.brightness = num8;
                    __instance.lensFlare.color = light.color;
                    __instance.lensFlare.enabled = true;
                    return false;
                }
                __instance.lensFlare.enabled = false;
            }
        }
        return false;
    }
}



[HarmonyPatch(typeof(SkyManager), "SetSunIntensity")]
public class Patch_SkyManagerSetSunIntensity
{
    public static bool Prefix(float i, SkyManager __instance)
    {
        SkyManager.sunIntensity = i;
        float sunAngle = SkyManager.GetSunAngle();
        if (sunAngle >= -SkyManager.sSunFadeHeight)
        {
            SkyManager.sunIntensity = -sunAngle * 10f * SkyManager.sunIntensity * (float)((sunAngle < 0f) ? 1 : 0);
        }
        SkyManager.sunIntensity = Mathf.Clamp(SkyManager.sunIntensity, 0f, SkyManager.sMaxSunIntensity);
        SkyManager.sunIntensity *= LightRanger.SunIntensityScale;
        if (SkyManager.sunLight != null)
        {
            SkyManager.sunLight.intensity = SkyManager.sunIntensity * SkyManager.fogLightScale;
        }
        return false;
    }
}

[HarmonyPatch(typeof(SkyManager), "GetMoonAmbientScale")]
public class Patch_SkyManagerGetMoonAmbientScale
{
    public static float Postfix(float add, float mpy, SkyManager __instance)
    {
        return Utils.FastLerp(add + SkyManager.moonBright * mpy, LightRanger.MoonAmbientScale, SkyManager.dayPercent * 3.030303f);
    }
}


[HarmonyPatch(typeof(TileEntityLight), MethodType.Constructor)]
[HarmonyPatch("TileEntityLight")]
[HarmonyPatch(new Type[]
{
    typeof(Chunk)
})]
public static class Patch_TileEntityLightCtor
{
    private static void Postfix(TileEntityLight __instance, ref float ___LightRange)
    {
        ___LightRange = LightRanger.TileEntityLightRange;
    }
}