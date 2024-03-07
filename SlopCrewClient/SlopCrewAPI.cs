using BepInEx.Bootstrap;
using System.Collections.ObjectModel;
using RealAPI = SlopCrew.API.ISlopCrewAPI;

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
    // Intentionally avoid any reference to SlopCrew.API.ISlopCrewAPI
    // in interface, so that reflection never throws errors when loading this type.
    private readonly object api;

    internal SlopCrewAPI(object api) {
        // Casting unnecessary, but ensures an error is thrown if the passed value is wrong type
        this.api = (RealAPI)api;
    }

    public string ServerAddress { get => ((RealAPI)api).ServerAddress; }

    public int PlayerCount { get => ((RealAPI)api).PlayerCount; }

    public event Action<int> OnPlayerCountChanged
    {
        add { ((RealAPI) api).OnPlayerCountChanged += value; }
        remove { ((RealAPI)api).OnPlayerCountChanged -= value; }
    }

    public bool Connected { get => ((RealAPI)api).Connected; }

    public event Action OnConnected
    {
        add { ((RealAPI)api).OnConnected += value; }
        remove { ((RealAPI)api).OnConnected -= value; }
    }
    
    public event Action OnDisconnected
    {
        add { ((RealAPI)api).OnDisconnected += value; }
        remove { ((RealAPI)api).OnDisconnected -= value; }
    }
    
    public ulong Latency { get => ((RealAPI)api).Latency; }

    public int TickRate { get => ((RealAPI)api).TickRate; }

    public int? StageOverride { get => ((RealAPI)api).StageOverride; set => ((RealAPI)api).StageOverride = value; }

    public uint? PlayerId { get => ((RealAPI)api).PlayerId; }
    public string? PlayerName { get => ((RealAPI)api).PlayerName; }

    public ReadOnlyCollection<uint>? Players { get => ((RealAPI)api).Players; }

    public string? GetGameObjectPathForPlayerID(uint playerId) => ((RealAPI)api).GetGameObjectPathForPlayerID(playerId);

    public uint? GetPlayerIDForGameObjectPath(string gameObjectPath) => ((RealAPI)api).GetPlayerIDForGameObjectPath(gameObjectPath);

    public bool? PlayerIDExists(uint playerId) => ((RealAPI)api).PlayerIDExists(playerId);

    public string? GetPlayerName(uint playerId) => ((RealAPI)api).GetPlayerName(playerId);

    public void SendCustomPacket(string id, byte[] data) => ((RealAPI)api).SendCustomPacket(id, data);

    public void SetCustomCharacterInfo(string id, byte[]? data) => ((RealAPI)api).SetCustomCharacterInfo(id, data);
    
    public event Action<uint, string, byte[]> OnCustomPacketReceived {
        add { ((RealAPI)api).OnCustomPacketReceived += value; }
        remove { ((RealAPI)api).OnCustomPacketReceived -= value; }
    }

    public event Action<uint, string, byte[]> OnCustomCharacterInfoReceived {
        add { ((RealAPI)api).OnCustomCharacterInfoReceived += value; }
        remove { ((RealAPI)api).OnCustomCharacterInfoReceived -= value; }
    }

    public event Action<ulong> OnServerTickReceived {
        add { ((RealAPI)api).OnServerTickReceived += value; }
        remove { ((RealAPI)api).OnServerTickReceived -= value; }
    }
}
