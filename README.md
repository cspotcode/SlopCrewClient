# SlopCrewClient

FOR MOD AUTHORS USING SLOPCREW.API

For Bomb Rush Cyberfunk mod authors.  It is a client that wraps SlopCrew's API and makes it easier to
send and receive custom packets and custom character data. Use it to implement new game modes.

Even when not connected to SlopCrew, will silently skip sending packets and can still emit them locally.
This means you can write networked logic that still behaves offline.

In lieu of docs, check the Example subdirectory.