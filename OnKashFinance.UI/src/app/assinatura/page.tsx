"use client";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { AssinaturaPainel } from "@/componentes/AssinaturaPainel";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";

export default function Assinatura() {
  const { sessao } = useAutenticacao();
  if (!sessao) return null;
  return <AreaAutenticada tipo={sessao.tipoConta === "PESSOAL" ? "pessoal" : "empresarial"}><AssinaturaPainel/></AreaAutenticada>;
}
