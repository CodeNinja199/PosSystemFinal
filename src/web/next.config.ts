import type { NextConfig } from "next";

// "standalone" makes next build write .next/standalone: a folder with server.js and only the node_modules it needs,
// which is what the Dockerfile copies into the runtime image (Next.js docs, "output: standalone").
const nextConfig: NextConfig = { output: "standalone" };

export default nextConfig;
