"use client";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { CentralAjuda } from "@/componentes/CentralAjuda";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";

export default function Ajuda() {
  const { sessao } = useAutenticacao();
  if (!sessao) return null;
  return <AreaAutenticada tipo={sessao.tipoConta === "PESSOAL" ? "pessoal" : "empresarial"}><CentralAjuda/></AreaAutenticada>;
}
