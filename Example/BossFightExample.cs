using ProtoBuf;
using Reptile;
using UnityEngine;

namespace cspotcode.SlopCrewClient.Example;

// We use protobuf-net under the hood, which uses the protobuf packet format.
// https://github.com/protobuf-net/protobuf-net
// All packets for your mod must inherit from a single BasePacket.

[ProtoContract]
[ProtoInclude(1, typeof(BossFightState))]
[ProtoInclude(2, typeof(BossFireArmCannon))]
[ProtoInclude(3, typeof(BossUpdateStats))]
[ProtoInclude(4, typeof(PingLocation))]
class BasePacket {}

[ProtoContract]
class BossFightState : BasePacket
{
    [ProtoMember(1)]
    public uint HostPlayer;
}

[ProtoContract]
class BossFireArmCannon : BasePacket
{
    [ProtoMember(1)]
    public uint TargetPlayerId;
}

[ProtoContract]
class BossUpdateStats : BasePacket
{
    [ProtoMember(1)]
    public uint BossHealth;
    [ProtoMember(2)]
    public BossPhase BossPhase;
}
enum BossPhase
{
    Arrive,
    Stomping,
    MissileTime,
    Desperate,
    Dead
}

[ProtoContract]
class PingLocation : BasePacket
{
    [ProtoMember(1)]
    public Vector3 Location;
}


public class MyGameplayController
{
    // Each mod uses a unique name to differentiate its custom packets and player
    // data from other mods.
    private const string ModName = "SlopCrewClientBossFightExample";
    
    private readonly Client<BasePacket> client;
    
    // For the sake of example, imagine these are set correctly by your mod code.
    public GameObject pingVisualPrefab;
    public Material selfPingMaterial;
    public Player localPlayer;
    
    public MyGameplayController()
    {
        client = new Client<BasePacket>(ModName);
        // Subscribe to incoming packets
        client.OnPacketReceived += OnPacketReceived;
        // Connect to SlopCrew's API and start listening to incoming packets.
        client.Enable();
    }

    // For the sake of example, imagine this is called by your mod every frame
    public void Update()
    {
        // When player presses P, we want to display a visual ping at their location for all players
        if (Input.GetKeyDown(KeyCode.P))
        {
            client.Send(new PingLocation()
            {
                Location = localPlayer.transform.position
            }, true);
        }
    }

    private void OnPacketReceived(uint playerId, BasePacket packet, bool local)
    {
        if (packet is PingLocation ping)
        {
            // When players ping a location
            var pingVisual = GameObject.Instantiate(pingVisualPrefab);
            pingVisual.transform.position = ping.Location;
            
            // local means this packet came from ourselves.  SlopCrew doesn't
            // send your own packets back to you, but SlopCrewClient simulates this
            // to make gameplay logic simpler.
            if (local)
            {
                // Give my own pings a different color
                pingVisual.GetComponent<MeshRenderer>().material = selfPingMaterial;
            }
        }
        else if (packet is BossUpdateStats bossStats)
        {
            // Handle all other packet types, this example only shows PingLocation
        }
    }
}