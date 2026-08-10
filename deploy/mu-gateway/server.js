"use strict";

const net = require("node:net");
const dns = require("node:dns").promises;

const listenPort = Number(process.env.PORT || 3000);
const targetHost = process.env.TARGET_HOST || "openmu-server.railway.internal";
const connectPort = Number(process.env.CONNECT_PORT || 44405);
const gamePort = Number(process.env.GAME_PORT || 55901);
const chatPort = Number(process.env.CHAT_PORT || 55980);
const inspectTimeoutMs = Number(process.env.INSPECT_TIMEOUT_MS || 3000);
const publicHost = process.env.PUBLIC_HOST || "interchange.proxy.rlwy.net";
const publicPort = Number(process.env.PUBLIC_PORT || listenPort);
let publicAddress = publicHost;

function packetLength(buffer) {
  if (buffer.length < 2) return null;
  const kind = buffer[0];
  if (kind === 0xc1 || kind === 0xc3) return buffer[1];
  if (kind === 0xc2 || kind === 0xc4) {
    if (buffer.length < 3) return null;
    return buffer.readUInt16BE(1);
  }
  return 0;
}

function classify(buffer) {
  const kind = buffer[0];
  const opcode = buffer[2];

  // ConnectServer requests use C1/C2 F4 packets (server list and connection info).
  if ((kind === 0xc1 || kind === 0xc2) && opcode === 0xf4) return "connect";

  // Chat connections are negotiated with CA packets. Keep this explicit so an
  // unknown future client packet is not accidentally sent to the chat listener.
  if ((kind === 0xc3 || kind === 0xc4) && opcode === 0xca) return "chat";

  // The normal post-login game connection starts with C3/C4 packets. This is
  // also the safe fallback for compatible MU clients after the connect request.
  return "game";
}

function destination(route) {
  if (route === "connect") return connectPort;
  if (route === "chat") return chatPort;
  return gamePort;
}

function rewriteConnectionInfo(chunk) {
  // C1 F4 03: 16-byte ASCII address at offset 4 and little-endian port at 20.
  if (chunk.length < 22 || chunk[0] !== 0xc1 || chunk[1] !== 0x16 || chunk[2] !== 0xf4 || chunk[3] !== 0x03) {
    return chunk;
  }
  const output = Buffer.from(chunk);
  output.fill(0, 4, 20);
  Buffer.from(publicAddress, "ascii").subarray(0, 16).copy(output, 4);
  output.writeUInt16LE(publicPort, 20);
  console.log(JSON.stringify({ event: "rewrite-connection-info", publicAddress, publicPort }));
  return output;
}

function bridge(client, buffered, route) {
  const port = destination(route);
  const upstream = net.createConnection({ host: targetHost, port }, () => {
    // The inspection timeout only protects the initial upstream connect. MU
    // sessions are intentionally long-lived and may be idle between packets.
    upstream.setTimeout(0);
    console.log(JSON.stringify({ event: "route", route, targetHost, port, remote: client.remoteAddress }));
    if (buffered.length) upstream.write(buffered);
    client.pipe(upstream);
    if (route === "connect") {
      upstream.on("data", (chunk) => client.write(rewriteConnectionInfo(chunk)));
    } else {
      upstream.pipe(client);
    }
  });

  upstream.setTimeout(inspectTimeoutMs, () => upstream.destroy(new Error("upstream timeout")));
  upstream.on("error", (error) => {
    console.error(JSON.stringify({ event: "upstream-error", route, port, error: error.message }));
    client.destroy();
  });
  upstream.on("close", () => client.destroy());
  client.on("error", () => upstream.destroy());
  client.on("close", () => upstream.destroy());
}

const server = net.createServer((client) => {
  let buffered = Buffer.alloc(0);
  let inspected = false;
  const timer = setTimeout(() => {
    if (!inspected) {
      inspected = true;
      bridge(client, buffered, "game");
    }
  }, inspectTimeoutMs);

  client.on("data", (chunk) => {
    if (inspected) return;
    buffered = Buffer.concat([buffered, chunk]);
    const expected = packetLength(buffered);
    if (expected === null || (expected > 0 && buffered.length < expected)) return;
    inspected = true;
    clearTimeout(timer);
    bridge(client, buffered, classify(buffered));
  });

  client.on("error", () => {
    clearTimeout(timer);
    client.destroy();
  });
  client.on("close", () => clearTimeout(timer));
});

server.on("error", (error) => {
  console.error(JSON.stringify({ event: "gateway-error", error: error.message }));
  process.exitCode = 1;
});

dns.lookup(publicHost, { family: 4 }).then(({ address }) => {
  publicAddress = address;
}).catch(() => {}).finally(() => {
  server.listen(listenPort, "0.0.0.0", () => {
    console.log(JSON.stringify({ event: "gateway-started", listenPort, targetHost, connectPort, gamePort, chatPort, publicAddress, publicPort }));
  });
});
