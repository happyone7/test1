import { createInterface } from "readline";
import http from "http";

const MCP_URL = new URL(process.env.MCP_HTTP_URL || "http://127.0.0.1:8080/mcp");
let sessionId = null;

const rl = createInterface({ input: process.stdin, terminal: false });

rl.on("line", (line) => {
  let request;
  try {
    request = JSON.parse(line);
  } catch {
    return;
  }

  const headers = {
    "Content-Type": "application/json",
    "Accept": "application/json, text/event-stream",
  };
  if (sessionId) headers["mcp-session-id"] = sessionId;

  const req = http.request(
    {
      hostname: MCP_URL.hostname,
      port: MCP_URL.port,
      path: MCP_URL.pathname,
      method: "POST",
      headers,
    },
    (res) => {
      const sid = res.headers["mcp-session-id"];
      if (sid) sessionId = sid;

      let data = "";
      res.on("data", (chunk) => {
        data += chunk;
      });
      res.on("end", () => {
        const lines = data.split("\n");
        for (const l of lines) {
          if (l.startsWith("data: ")) {
            try {
              const parsed = JSON.parse(l.slice(6));
              process.stdout.write(JSON.stringify(parsed) + "\n");
            } catch {}
            return;
          }
        }
        // fallback: try direct JSON parse
        try {
          const parsed = JSON.parse(data);
          process.stdout.write(JSON.stringify(parsed) + "\n");
        } catch {}
      });
    }
  );

  req.on("error", (err) => {
    if (request.id) {
      process.stdout.write(
        JSON.stringify({
          jsonrpc: "2.0",
          id: request.id,
          error: { code: -32603, message: err.message },
        }) + "\n"
      );
    }
  });

  req.setTimeout(120000, () => {
    req.destroy();
  });

  req.write(JSON.stringify(request));
  req.end();
});

process.stdin.on("end", () => process.exit(0));
