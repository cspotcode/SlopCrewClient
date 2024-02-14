using cspotcode.SlopCrewClient.SlopCrewAPI;
using ProtoBuf;
using UnityEngine;

namespace cspotcode.SlopCrewClient;

public class Client<T>
{
    /// <summary>
    /// SlopCrew never tells us our own player ID.
    /// So when referring to ourselves, referring to packets from the local player, we use this ID.
    /// </summary>
    public const uint LocalPlayerId = uint.MaxValue - 1;
    // no strong reason to do -1 except
    // Winterland already used MaxValue for
    // packets from the server. (even though
    // that code's since been removed from the
    // SlopCrew server)
    
    // Queue packets during temporary disconnection? Not today.  Silently drop packets if offline
    // Deduplicate packets? Not today
    // Self-imposed rate-limit?
    // Include player ID in the returned packet struct?

    // TODO ensure sync of CustomCharacterData

    private ISlopCrewAPI api;
    public ISlopCrewAPI SlopCrewAPI => api;
    private readonly string modName;
    private bool enabled = false;
    
    /// <summary>
    /// If smoothed servertick is more than this many seconds away from received servertick,
    /// then smoothed will abruptly reset to the receive servertick.
    /// Drift smaller than this will be smoothed out.
    /// </summary>
    public float MaxAllowedClockDrift = 1;
    /// <summary>
    /// When local clock needs to run fast or slow to catch up to/wait for server,
    /// what percentage speed change to use?
    /// NOTE be careful adjusting this number, the implementation is naive, and bad values here may break things.
    /// </summary>
    public float MaxClockScaling = 0.05f;
    /// <summary>
    /// Locally-maintained ServerTick approximation which mimics real ServerTick but using extrapolation
    /// and smoothing.
    /// </summary>
    public ulong CurrentTickSmoothed { get; private set; } = 0;
    public ulong CurrentTick { get; private set; } = 0;
    private bool firstTickReceived = false;
    private float tickTimeAccumulator = 0;
    private float smoothedTickTimeAccumulator = 0;
    // This is not hardcoded!  We default to 10 ticks per second, and simulate that when disconnected from SlopCrew.
    public float TickDuration { get; private set; } = 0f/10;
    private bool receivedTickRateFromRealServer = false;

    public bool Enabled => enabled;

    /// <summary>
    /// True if SlopCrew API is available -- SlopCrew mod is running -- even if disconnected.
    /// </summary>
    public bool ApiAvailable => api != null;

#if CUSTOM_CHARACTER_INFO
    /// <summary>
    /// Client maintains a dictionary of the character info received for all players
    /// </summary>
    public Dictionary<uint, T> CharacterInfo = new();
#endif
    
    public Client(string modName)
    {
        this.modName = modName;
        APIManager.OnAPIRegistered += onSlopCrewAPIRegistered;
        if (APIManager.API != null)
        {
            this.api = APIManager.API;
        }

        UpdateEmitter.EnsureInstance().OnUpdate += Update;
    }

    /// <summary>
    /// Start listening to SlopCrew's API events.
    /// </summary>
#if CUSTOM_CHARACTER_INFO
    public void Enable(bool InvokeCharacterInfoHandlers = true)
#else
    public void Enable()
#endif
    {
        if (enabled == true) return;
        enabled = true;
        if (api != null)
        {
            EnableForApi(api);
        }
    }

    public void Disable()
    {
        if (enabled == false) return;
        enabled = false;
        RemoveListeners();
    }

    public void Send(T packet, bool receiveLocally)
    {
        var data = SerializePacket(packet);
        api?.SendCustomPacket(modName, data);
        if (receiveLocally)
        {
            // TODO delay till next frame, make the sequencing feel more networked
            // Or simulate approximated round-trip latency
            OnPacketReceived?.Invoke(LocalPlayerId, packet, true);
        }
    }

    public event PacketReceivedHandler OnPacketReceived;

    public delegate void PacketReceivedHandler(uint playerId, T packet, bool local);

#if CUSTOM_CHARACTER_INFO
    public event CharacterInfoReceivedHandler OnCharacterInfoReceived;
    public delegate void CharacterInfoReceivedHandler(uint playerId, T packet, bool local);
#endif

    private void EnableForApi(ISlopCrewAPI api)
    {
        this.api = api;
        RemoveListeners();
        api.OnCustomPacketReceived += onSlopCrewCustomPacketReceived;
#if CUSTOM_CHARACTER_INFO
        api.OnCustomCharacterInfoReceived += onSlopCrewCustomCharacterInfoReceived;
#endif
        api.OnServerTickReceived += onSlopCrewServerTickReceived;
    }

    private void RemoveListeners()
    {
        api.OnCustomPacketReceived -= onSlopCrewCustomPacketReceived;
#if CUSTOM_CHARACTER_INFO
        api.OnCustomCharacterInfoReceived -= onSlopCrewCustomCharacterInfoReceived;
#endif
        api.OnServerTickReceived -= onSlopCrewServerTickReceived;
    }

    private void onSlopCrewAPIRegistered(ISlopCrewAPI api)
    {
        if (this.api != null)
        {
            throw new Exception("SlopCrew API unexpectedly registered multiple times.");
        }

        this.api = api;
        if (enabled)
        {
            EnableForApi(api);
        }
    }

#if CUSTOM_CHARACTER_INFO
    private void onSlopCrewCustomCharacterInfoReceived(uint playerId, string infoName, byte[] data)
    {
        if (infoName == modName)
        {
            var packet = Serializer.Deserialize<T>(data);
            // ?.Invoke(playerId, packet);
        }
    }
#endif

    private void onSlopCrewCustomPacketReceived(uint playerId, string packetName, byte[] data)
    {
        if (packetName == modName)
        {
            var packet = DeserializePacket(data);
            OnPacketReceived?.Invoke(playerId, packet, false);
        }
    }

    private void onSlopCrewServerTickReceived(ulong tick) {
        CurrentTick = tick;
        firstTickReceived = true;
    }

    private static T DeserializePacket(byte[] data)
    {
        T packet;
        using(var stream = new MemoryStream(data))
        {
            packet = Serializer.Deserialize<T>(stream);
        }
        return packet;
    }

    private static byte[] SerializePacket(T packet)
    {
        byte[] data;
        using(var stream = new MemoryStream())
        {
            Serializer.Serialize(stream, packet);
            data = stream.ToArray();
        }
        return data;
    }

    private void Update() {
        if (!receivedTickRateFromRealServer && api != null && api.Connected && api.TickRate > 0) {
            receivedTickRateFromRealServer = true;
            TickDuration = 1f / api.TickRate;
        }
        
        // Even when disconnected or SlopCrew is not installed, we maintain a local tick.
        
        // If max drift exceeded, snap smoothed tick back to raw
        var clockScale = 1f;
        var serverIsAhead = CurrentTick > CurrentTickSmoothed;
        var localIsAhead = CurrentTickSmoothed > CurrentTick;
        var distance = serverIsAhead ? CurrentTick - CurrentTickSmoothed : CurrentTickSmoothed - CurrentTick;
        if (TickDuration * distance > MaxAllowedClockDrift) {
            CurrentTickSmoothed = CurrentTick;
        }
        else {
            // Speed up local to catch server
            if (serverIsAhead) clockScale = 1 + MaxClockScaling;
            // Slow down local to fall back to server
            if (localIsAhead) clockScale = 1 - MaxClockScaling;
            // Run local clock fast or slow to stay in sync w/server
        }
        
        tickTimeAccumulator += Time.deltaTime;
        while (tickTimeAccumulator > TickDuration) {
            tickTimeAccumulator -= TickDuration;
            CurrentTick++;
        }
        smoothedTickTimeAccumulator += Time.deltaTime * clockScale;
        while (smoothedTickTimeAccumulator > TickDuration) {
            smoothedTickTimeAccumulator -= TickDuration;
            CurrentTickSmoothed++;
        }
    }
}
