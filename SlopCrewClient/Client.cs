using System.Text.Json;
using ProtoBuf;
using SlopCrew.API;

namespace cspotcode.SlopCrewClient;

public class Client<T>
{
    /// <summary>
    /// SlopCrew never tells us our own player ID.
    /// So when referring to ourselves, referring to packets from the local player, we use this ID.
    /// </summary>
    public const uint LocalPlayerId = uint.MaxValue - 1; // no strong reason to do -1 except
    // Winterland already used MaxValue for
    // packets from the server. (even though
    // that code's since been removed from the
    // SlopCrew server)
    public Client(string modName)
    {
        this.modName = modName;
        APIManager.OnAPIRegistered += onSlopCrewAPIRegistered;
        if (APIManager.API != null)
        {
            this.api = APIManager.API;
        }
    }

    // Queue packets during temporary disconnection? Not today.  Silently drop packets if offline
    // Deduplicate packets? Not today
    // Self-imposed rate-limit?
    // Include player ID in the returned packet struct?

    // TODO ensure sync of CustomCharacterData

    private ISlopCrewAPI api;
    public ISlopCrewAPI SlopCrewAPI => api;
    private readonly string modName;
    private bool enabled = false;

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
    }

    private void RemoveListeners()
    {
        api.OnCustomPacketReceived -= onSlopCrewCustomPacketReceived;
#if CUSTOM_CHARACTER_INFO
        api.OnCustomCharacterInfoReceived -= onSlopCrewCustomCharacterInfoReceived;
#endif
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

}
