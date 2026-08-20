"use client";
import { createContext, useContext, useEffect, useState } from "react";
import type { Sessao } from "@/tipos/api";
interface Contexto {
  sessao: Sessao | null;
  carregando: boolean;
  iniciar: (s: Sessao) => void;
  sair: () => void;
}
const ContextoAutenticacao = createContext<Contexto | undefined>(undefined);
const chave = "onkash.sessao";
export function ProvedorAutenticacao({ children }: { children: React.ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(null);
  const [carregando, setCarregando] = useState(true);
  useEffect(() => {
    try {
      const salva = localStorage.getItem(chave);
      if (salva) setSessao(JSON.parse(salva) as Sessao);
    } finally {
      setCarregando(false);
    }
  }, []);
  const iniciar = (nova: Sessao) => {
    localStorage.setItem(chave, JSON.stringify(nova));
    setSessao(nova);
  };
  const sair = () => {
    localStorage.removeItem(chave);
    setSessao(null);
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
