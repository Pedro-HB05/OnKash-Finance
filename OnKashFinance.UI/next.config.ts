import type { NextConfig } from "next";

// Pode ser substituída por API_URL na publicação ou no ambiente local.
const apiUrl = (process.env.API_URL ?? "https://onkash-finance.onrender.com").replace(/\/$/, "");
const isDev = process.env.NODE_ENV === "development";
const apiPublica = process.env.NEXT_PUBLIC_API_URL ?? apiUrl;
let origemApi = "";
try { origemApi = new URL(apiPublica).origin; } catch { origemApi = ""; }
const politicaConteudo = `default-src 'self'; script-src 'self' 'unsafe-inline'${isDev ? " 'unsafe-eval'" : ""}; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self' data:; connect-src 'self' ${origemApi}; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; upgrade-insecure-requests`;

const nextConfig: NextConfig = {
  async headers() {
    return [{ source: "/(.*)", headers: [
      { key: "X-Content-Type-Options", value: "nosniff" },
      { key: "X-Frame-Options", value: "DENY" },
      { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
      { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=(), payment=()" },
      { key: "Content-Security-Policy", value: politicaConteudo },
      { key: "Strict-Transport-Security", value: "max-age=31536000; includeSubDomains" },
    ] }];
  },
  async rewrites() {
    return apiUrl ? [{ source: "/api/:caminho*", destination: `${apiUrl}/api/:caminho*` }] : [];
  },
};

export default nextConfig;
