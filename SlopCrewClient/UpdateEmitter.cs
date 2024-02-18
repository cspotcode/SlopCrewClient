#pragma warning disable CS8625
#pragma warning disable CS8618
using UnityEngine;
using Object = UnityEngine.Object;

namespace cspotcode.SlopCrewClient;

internal class UpdateEmitter : MonoBehaviour {

    internal static UpdateEmitter Instance { get; private set; }
    internal static UpdateEmitter EnsureInstance() {
        if (Instance == null) {
            var go = new GameObject("SlopCrewClient");
            Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<UpdateEmitter>();
        }
        return Instance;
    }
    
    public event Action OnUpdate;
    public event Action OnLateUpdate;
    
    private void Update() {
        OnUpdate?.Invoke();
    }
    private void LateUpdate() {
        OnLateUpdate?.Invoke();
    }
}