using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace OverNinetyNine;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony _harmony;
    private void Awake()
    {
        _harmony = Harmony.CreateAndPatchAll(typeof(OptionAmountPatch), "fururikawa.OverNinetyNine");

        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}

[HarmonyPatch]
public static class OptionAmountPatch
{
    [HarmonyPatch(typeof(OptionAmount), "selectedAmountUp")]
    [HarmonyPatch(typeof(OptionAmount), "selectedAmountDown")]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)99))
            .Repeat(matcher => matcher.SetAndAdvance(OpCodes.Ldc_I4, 9999))
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(OptionAmount), "holdUpOrDown", MethodType.Enumerator)]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> holdUpOrDownPatch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        FieldInfo dif = typeof(OptionAmount)
            .GetNestedType("<holdUpOrDown>d__10", BindingFlags.NonPublic)?.GetField("dif", BindingFlags.Public | BindingFlags.Instance);

        var _instructions = new CodeMatcher(instructions)
        .MatchForward(false,
            new CodeMatch(OpCodes.Ldc_R4, 0.15f))
        .SetOperandAndAdvance(0.3f)
        .MatchForward(true,
            new CodeMatch(OpCodes.Ldstr, "n0"),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(int), "ToString", new Type[] { typeof(String) })),
            new CodeMatch(OpCodes.Call, AccessTools.Method(
                typeof(String),
                nameof(String.Concat),
                new Type[] { typeof(String), typeof(String) })),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt))
        .Advance(1)
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, dif))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_2))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Mul))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)-100))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)100))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(
            typeof(Mathf),
            nameof(Mathf.Clamp),
            new Type[] { typeof(int), typeof(int), typeof(int) })))
        .InsertAndAdvance(new CodeInstruction(OpCodes.Stfld, dif))
        .MatchForward(true,
            new CodeMatch(OpCodes.Add),
            new CodeMatch(OpCodes.Ldc_R4, 0f),
            new CodeMatch(OpCodes.Ldc_R4, 0.14f))
        .SetOperandAndAdvance(0.29f)
        .MatchBack(false,
            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)99))
        .Repeat(matcher => matcher.SetAndAdvance(OpCodes.Ldc_I4, 9999))
        .InstructionEnumeration();
        return _instructions;
    }
}
