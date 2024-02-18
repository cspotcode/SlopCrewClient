Messy Notes
---

MaxCustomPacketSize = 512  
We can auto-split large packets to get around this.  
Each packet is sent with 3 byte header:  
packet ID  
chunk index  
total chunk count

Ability to sync "entities":  
They re-broadcast their state on an interval

Each entity has owner client that broadcasts state to
everyone else.

Entities can RPC to the owner
