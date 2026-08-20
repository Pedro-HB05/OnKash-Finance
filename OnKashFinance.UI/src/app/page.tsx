"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
export default function Inicio() {
  const { sessao, carregando } = useAutenticacao();
  const router = useRouter();
  useEffect(() => {
    if (!carregando)
      router.replace(
        !sessao
          ? "/login"
          : sessao.tipoConta === "PESSOAL"
            ? "/pessoal/visao-geral"
            : "/empresarial/visao-geral",
      );
  }, [carregando, router, sessao]);
  return <main className="tela-carregando">Verificando seu acesso...</main>;
}
