using BepInEx;
using cspotcode.SlopCrewClient.Patches;
using UnityEngine;

namespace cspotcode.SlopCrewClient;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance;

    public Plugin() {
        this.Logger.LogInfo($"{Info.Metadata.Name} plugin loaded");
    }

    private void Awake() {
        Instance = this;
        CustomAppAPIPatch.AttemptPatch();
    }
    private void Update() {
        // var installed = SlopCrewAPI.APIManager.IsSlopCrewInstalled;
        // var connected = installed ? SlopCrewAPI.APIManager.API.Connected : false;
        // Debug.Log($"installed:{installed} connected:{connected}");
    }
}
