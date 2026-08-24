"use client";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { PortalPrivacidade } from "@/componentes/PortalPrivacidade";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
export default function MeusDados() {
  const { sessao } = useAutenticacao(); if (!sessao) return null;
  return <AreaAutenticada tipo={sessao.tipoConta === "PESSOAL" ? "pessoal" : "empresarial"}><PortalPrivacidade/></AreaAutenticada>;
}
