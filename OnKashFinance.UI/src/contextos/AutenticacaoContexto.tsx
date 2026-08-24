"use client";
import { createContext, useCallback, useContext, useEffect, useState } from "react";
import type { Sessao } from "@/tipos/api";
interface Contexto {
  sessao: Sessao | null;
  carregando: boolean;
  iniciar: (s: Sessao) => void;
  sair: () => void;
}
const ContextoAutenticacao = createContext<Contexto | undefined>(undefined);
const chave = "onkash.sessao";
function expiraEm(token: string) {
  try {
    const parte = token.split(".")[1];
    if (!parte) return 0;
    const base64 = parte.replaceAll("-", "+").replaceAll("_", "/").padEnd(Math.ceil(parte.length / 4) * 4, "=");
    const payload = JSON.parse(atob(base64)) as { exp?: number };
    return typeof payload.exp === "number" ? payload.exp * 1000 : 0;
  } catch { return 0; }
}
export function ProvedorAutenticacao({ children }: { children: React.ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(null);
  const [carregando, setCarregando] = useState(true);
  const sair = useCallback(() => {
    localStorage.removeItem(chave);
    setSessao(null);
  }, []);
  const expirar = useCallback(() => {
    sair();
    if (window.location.pathname !== "/login") window.location.replace("/login?motivo=sessao-expirada");
  }, [sair]);
  useEffect(() => {
    try {
      const salva = localStorage.getItem(chave);
      if (salva) {
        const armazenada = JSON.parse(salva) as Sessao;
        const expiracao = expiraEm(armazenada.token);
        if (!expiracao || expiracao <= Date.now()) expirar();
        else setSessao(armazenada);
      }
    } finally {
      setCarregando(false);
    }
  }, [expirar]);
  useEffect(() => {
    if (!sessao) return;
    const verificar = () => { if (expiraEm(sessao.token) <= Date.now()) expirar(); };
    const restante = Math.max(0, expiraEm(sessao.token) - Date.now());
    const temporizador = window.setTimeout(verificar, Math.min(restante, 2_147_000_000));
    const monitor = window.setInterval(verificar, 60_000);
    const eventoExpiracao = () => expirar();
    const sincronizar = (evento: StorageEvent) => {
      if (evento.key === chave && !evento.newValue) {
        sair();
        if (window.location.pathname !== "/login") window.location.replace("/login");
      }
    };
    window.addEventListener("onkash:sessao-expirada", eventoExpiracao);
    window.addEventListener("storage", sincronizar);
    window.addEventListener("focus", verificar);
    document.addEventListener("visibilitychange", verificar);
    return () => {
      window.clearTimeout(temporizador);
      window.clearInterval(monitor);
      window.removeEventListener("onkash:sessao-expirada", eventoExpiracao);
      window.removeEventListener("storage", sincronizar);
      window.removeEventListener("focus", verificar);
      document.removeEventListener("visibilitychange", verificar);
    };
  }, [sessao, expirar, sair]);
  const iniciar = (nova: Sessao) => {
    localStorage.setItem(chave, JSON.stringify(nova));
    setSessao(nova);
  };
  return (
    <ContextoAutenticacao.Provider value={{ sessao, carregando, iniciar, sair }}>
      {children}
    </ContextoAutenticacao.Provider>
  );
}
export function useAutenticacao() {
  const c = useContext(ContextoAutenticacao);
  if (!c) throw new Error("Contexto de autenticação indisponível.");
  return c;
}
