using BepInEx.Bootstrap;
using System.Collections.ObjectModel;

namespace cspotcode.SlopCrewClient.SlopCrewAPI;

// Carefully written SlopCrew API wrapper which can be linked against even when
// SlopCrew.API DLL is missing.

public class APIManager {
    // This trick learned from LazyDuchess and WallPlant: https://github.com/LazyDuchess/BRC-WallPlant/blob/f4a686695c6142486cedb123aaab00c6bac75027/Plugin.cs#L84-L87
    private const string SlopCrewGUID = "SlopCrew.Plugin";
    private static bool _CheckedIfSlopCrewInstalled = false;
    private static bool _IsSlopCrewInstalled;
    public static bool IsSlopCrewInstalled {
        get {
            if (!_CheckedIfSlopCrewInstalled) {
                throw new Exception("SlopCrewClient not initialized yet");
            }
            return _IsSlopCrewInstalled;
        }
    }

    public static ISlopCrewAPI? API { get; private set; }
    internal static void Init() {
        _IsSlopCrewInstalled = Chainloader.PluginInfos.Keys.Contains(SlopCrewGUID);
        _CheckedIfSlopCrewInstalled = true;
        if (IsSlopCrewInstalled) {
            SlopCrew_Init();
        }
    }
    private static void SlopCrew_Init() {
        SlopCrew.API.APIManager.OnAPIRegistered += api => {
            if (API != null) {
                throw new Exception("SlopCrew API unexpectedly registered multiple times.");
            }
            API = new SlopCrewAPI(api);
            OnAPIRegistered?.Invoke(API);
        };
        if (SlopCrew.API.APIManager.API != null) {
            API = new SlopCrewAPI(SlopCrew.API.APIManager.API);
            OnAPIRegistered?.Invoke(API);
        }
    }
    public static event Action<ISlopCrewAPI>? OnAPIRegistered;
}

// Verbatim copy from SlopCrew.API source: https://github.com/SlopCrew/SlopCrew/blob/main/SlopCrew.API/ISlopCrewAPI.cs
public interface ISlopCrewAPI {
    public string ServerAddress { get; }

    public int PlayerCount { get; }
    public event Action<int> OnPlayerCountChanged;

    public bool Connected { get; }
    public event Action OnConnected;
    public event Action OnDisconnected;
    public ulong Latency { get; }
    public int TickRate { get; }

    public int? StageOverride { get; set; }

    public uint? PlayerId { get; }
    public string? PlayerName { get; }
    public ReadOnlyCollection<uint>? Players { get; }

    public string? GetGameObjectPathForPlayerID(uint playerId);
    public uint? GetPlayerIDForGameObjectPath(string gameObjectPath);
    public bool? PlayerIDExists(uint playerId);
    public string? GetPlayerName(uint playerId);

    public void SendCustomPacket(string id, byte[] data);
    public void SetCustomCharacterInfo(string id, byte[]? data);

    public event Action<uint, string, byte[]> OnCustomPacketReceived;
    public event Action<uint, string, byte[]> OnCustomCharacterInfoReceived;
    public event Action<ulong> OnServerTickReceived;
}

// Implement ISlopCrew by delegation
public class SlopCrewAPI : ISlopCrewAPI {
    private SlopCrew.API.ISlopCrewAPI api;

    internal SlopCrewAPI(SlopCrew.API.ISlopCrewAPI api) {
        this.api = api;
    }
    
    // Implement the entire ISlopCrewAPI by delegation

    public string ServerAddress { get => api.ServerAddress; }

    public int PlayerCount { get => api.PlayerCount; }
    
    public event Action<int> OnPlayerCountChanged
    {
        add { api.OnPlayerCountChanged += value; }
        remove { api.OnPlayerCountChanged -= value; }
    }

    public bool Connected { get => api.Connected; }

    public event Action OnConnected
    {
        add { api.OnConnected += value; }
        remove { api.OnConnected -= value; }
    }
    
    public event Action OnDisconnected
    {
        add { api.OnDisconnected += value; }
        remove { api.OnDisconnected -= value; }
    }
    
    public ulong Latency { get => api.Latency; }

    public int TickRate { get => api.TickRate; }

    public int? StageOverride { get => api.StageOverride; set => api.StageOverride = value; }

    public uint? PlayerId { get => api.PlayerId; }
    public string? PlayerName { get => api.PlayerName; }

    public ReadOnlyCollection<uint>? Players { get => api.Players; }

    public string? GetGameObjectPathForPlayerID(uint playerId) => api.GetGameObjectPathForPlayerID(playerId);

    public uint? GetPlayerIDForGameObjectPath(string gameObjectPath) => api.GetPlayerIDForGameObjectPath(gameObjectPath);

    public bool? PlayerIDExists(uint playerId) => api.PlayerIDExists(playerId);

    public string? GetPlayerName(uint playerId) => api.GetPlayerName(playerId);
    
    public void SendCustomPacket(string id, byte[] data) => api.SendCustomPacket(id, data);

    public void SetCustomCharacterInfo(string id, byte[]? data) => api.SetCustomCharacterInfo(id, data);
    
    public event Action<uint, string, byte[]> OnCustomPacketReceived {
        add { api.OnCustomPacketReceived += value; }
        remove { api.OnCustomPacketReceived -= value; }
    }

    public event Action<uint, string, byte[]> OnCustomCharacterInfoReceived {
        add { api.OnCustomCharacterInfoReceived += value; }
        remove { api.OnCustomCharacterInfoReceived -= value; }
    }

    public event Action<ulong> OnServerTickReceived {
        add { api.OnServerTickReceived += value; }
        remove { api.OnServerTickReceived -= value; }
    }
}