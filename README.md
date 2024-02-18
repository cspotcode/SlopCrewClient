# SlopCrewClient

FOR MOD AUTHORS USING SLOPCREW.API

For Bomb Rush Cyberfunk mod authors.  It is a client that wraps SlopCrew's API and makes it easier to
send and receive custom packets and custom character data. Use it to implement new game modes.

Even when not connected to SlopCrew, will silently skip sending packets and can still emit them locally.
This means you can write networked logic that still behaves offline.

Take care to exclude SlopCrewClient DLLs from your mod. You can reference it like this in your `.csproj`. The import bit is "Private" and "IncludeAssets".

```
    <!-- Adjust path as necessary -->
    <ProjectReference Include="./libs/SlopCrewClient/SlopCrewClient/SlopCrewClient.csproj" Private="false" IncludeAssets="compile" />
```

In lieu of docs, check the Example subdirectory.