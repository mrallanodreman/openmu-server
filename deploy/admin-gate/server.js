"use strict";

const http = require("node:http");
const crypto = require("node:crypto");
const httpProxy = require("http-proxy");

const port = Number(process.env.PORT || 8080);
const targetHost = process.env.TARGET_HOST || "openmu-server.railway.internal";
const targetPort = Number(process.env.TARGET_PORT || 8080);
const username = process.env.ADMIN_USER;
const password = process.env.ADMIN_PASSWORD;

if (!username || !password) {
  console.error("ADMIN_USER and ADMIN_PASSWORD are required");
  process.exit(1);
}

const target = `http://${targetHost}:${targetPort}`;
const proxy = httpProxy.createProxyServer({ target, changeOrigin: true, ws: true });

function safeEqual(a, b) {
  const aa = Buffer.from(a);
  const bb = Buffer.from(b);
  return aa.length === bb.length && crypto.timingSafeEqual(aa, bb);
}

function authorized(request) {
  const header = request.headers.authorization || "";
  if (!header.startsWith("Basic ")) return false;
  let decoded;
  try {
    decoded = Buffer.from(header.slice(6), "base64").toString("utf8");
  } catch {
    return false;
  }
  const separator = decoded.indexOf(":");
  if (separator < 0) return false;
  return safeEqual(decoded.slice(0, separator), username)
    && safeEqual(decoded.slice(separator + 1), password);
}

function reject(response) {
  response.writeHead(401, { "WWW-Authenticate": 'Basic realm="OpenMU AdminPanel"' });
  response.end("Authentication required\n");
}

const server = http.createServer((request, response) => {
  if (!authorized(request)) return reject(response);
  proxy.web(request, response);
});

server.on("upgrade", (request, socket, head) => {
  if (!authorized(request)) {
    socket.write("HTTP/1.1 401 Unauthorized\r\nWWW-Authenticate: Basic realm=\"OpenMU AdminPanel\"\r\n\r\n");
    socket.destroy();
    return;
  }
  proxy.ws(request, socket, head);
});

proxy.on("error", (error, request, response) => {
  console.error(JSON.stringify({ event: "proxy-error", error: error.message }));
  if (response && !response.headersSent) response.writeHead(502);
  if (response) response.end("AdminPanel upstream unavailable\n");
});

server.listen(port, "0.0.0.0", () => {
  console.log(JSON.stringify({ event: "admin-gate-started", port, target }));
});
