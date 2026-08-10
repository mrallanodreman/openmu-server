# OpenMU Railway TCP Gateway

Railway exposes one public TCP proxy per service. This gateway lets the MU
client use that one endpoint while OpenMU keeps its normal internal listeners.

The first MU packet is inspected and routed to the private OpenMU service:

- `C1/C2 F4` → ConnectServer (`CONNECT_PORT`)
- `C3/C4 CA` → ChatServer (`CHAT_PORT`)
- everything else → GameServer (`GAME_PORT`)

This is intentionally a small protocol-aware transport proxy, not an HTTP
proxy. It does not decrypt or modify MU packets.
