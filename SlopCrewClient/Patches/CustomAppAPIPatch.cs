using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace cspotcode.SlopCrewClient.Patches;
[HarmonyPatch()]
internal class CustomAppAPIPatch {
    internal static void AttemptPatch() {
        // Patch conditionally using reflection so that we don't have to link against it
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "CustomAppAPI").FirstOrDefault();
        if (assembly == null) return;
        var CustomAppModType = assembly.GetTypes().Where(t => t.Name == "CustomAppMod").FirstOrDefault();
        if (CustomAppModType == null) return;
        var original = CustomAppModType.GetMethod("FindDerivedTypes", BindingFlags.Static | BindingFlags.NonPublic);
        if (original == null) return;
        var prefix = typeof(CustomAppAPIPatch).GetMethod(nameof(FindDerivedTypes_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
    
        var harmony = new Harmony(PluginInfo.PLUGIN_NAME);
        harmony.Patch(original, new HarmonyMethod(prefix));
    }
    
    // Target being patched:
    // CustomAppAPI.CustomAppMod {
    //   private static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType) {
    //     return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType);
    private static bool FindDerivedTypes_Prefix(Assembly assembly, Type baseType, ref IEnumerable<Type> __result) {
        __result = assembly.GetTypes().Where(t => {
            try {
                return baseType.IsAssignableFrom(t) && t != baseType;
            } catch(Exception e) {
                if (e is TypeLoadException || e is ReflectionTypeLoadException) {
                    // Swallow it
                    return false;
                }
                else
                    throw;
            }
        });
        return false;
    }
}