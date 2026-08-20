"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { CabecalhoGlobal } from "@/componentes/CabecalhoGlobal";

const pessoal = [
  ["visao-geral", "Visão geral"],
  ["lancamentos", "Lançamentos"],
  ["contas", "Contas"],
  ["cartoes", "Cartões"],
  ["faturas", "Faturas"],
  ["categorias", "Categorias"],
  ["relatorios", "Relatórios"],
];
const empresarial = [
  ["visao-geral", "Visão geral"],
  ["lancamentos", "Lançamentos"],
  ["contas", "Contas"],
  ["clientes", "Clientes"],
  ["fornecedores", "Fornecedores"],
  ["contas-a-pagar", "Contas a pagar"],
  ["contas-a-receber", "Contas a receber"],
  ["categorias", "Categorias"],
  ["relatorios", "Relatórios"],
  ["usuarios", "Usuários"],
];

export function AreaAutenticada({
  tipo,
  children,
}: {
  tipo: "pessoal" | "empresarial";
  children: React.ReactNode;
}) {
  const { sessao, carregando } = useAutenticacao();
  const [menuAberto, setMenuAberto] = useState(false);
  const router = useRouter();
  const caminhoAtual = usePathname();
  if (carregando) return <main className="tela-carregando">Carregando sua área...</main>;
  if (!sessao) {
    router.replace("/login");
    return null;
  }
  if (sessao.tipoConta !== tipo.toUpperCase()) {
    router.replace(
      sessao.tipoConta === "PESSOAL" ? "/pessoal/visao-geral" : "/empresarial/visao-geral",
    );
    return null;
  }
  const itens = tipo === "pessoal" ? pessoal : empresarial;
  return (
    <div className="area">
      <aside className={`menu ${menuAberto ? "aberto" : ""}`}>
        <div className="topo-menu">
          <Link className="marca" href={`/${tipo}/visao-geral`}>
            OnKash <span>Finance</span>
          </Link>
          <button
            className="botao-menu"
            aria-expanded={menuAberto}
            onClick={() => setMenuAberto(!menuAberto)}
          >
            Menu
          </button>
        </div>
        <nav aria-label="Menu principal">
          {itens.map(([rota, nome]) => (
            <Link
              key={rota}
              className={caminhoAtual.includes(`/${rota}`) ? "ativo" : ""}
              href={`/${tipo}/${rota}`}
              onClick={() => setMenuAberto(false)}
            >
              {nome}
            </Link>
          ))}
        </nav>
      </aside>
      <main className="conteudo">
        <CabecalhoGlobal />
        {children}
      </main>
    </div>
  );
}
