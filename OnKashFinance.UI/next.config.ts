import type { NextConfig } from "next";

// Pode ser substituída por API_URL na publicação ou no ambiente local.
const apiUrl = (process.env.API_URL ?? "https://onkash-finance.onrender.com").replace(/\/$/, "");

const nextConfig: NextConfig = {
  async rewrites() {
    return apiUrl ? [{ source: "/api/:caminho*", destination: `${apiUrl}/api/:caminho*` }] : [];
  },
};

export default nextConfig;
