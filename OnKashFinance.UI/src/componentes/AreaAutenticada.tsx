"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { CabecalhoGlobal } from "@/componentes/CabecalhoGlobal";
import { BarChart3, Building2, CreditCard, FileText, FolderTree, Gauge, Landmark, Menu as MenuIcon, ReceiptText, Sparkles, Tags, Target, UsersRound, UserRound, WalletCards, X } from "lucide-react";

const pessoal = [
  { grupo: "Principal", itens: [{ rota: "visao-geral", nome: "Visão geral", icone: Gauge }] },
  { grupo: "Financeiro", itens: [
    { rota: "lancamentos", nome: "Lançamentos", icone: ReceiptText },
    { rota: "contas", nome: "Contas", icone: Landmark },
    { rota: "cartoes", nome: "Cartões", icone: CreditCard },
    { rota: "faturas", nome: "Faturas", icone: FileText },
  ] },
  { grupo: "Organização", itens: [
    { rota: "planejamento", nome: "Planejamento", icone: Target },
    { rota: "inteligencia", nome: "Inteligência", icone: Sparkles },
    { rota: "categorias", nome: "Categorias", icone: Tags },
    { rota: "relatorios", nome: "Relatórios", icone: BarChart3 },
  ] },
];
const empresarial = [
  { grupo: "Principal", itens: [{ rota: "visao-geral", nome: "Visão geral", icone: Gauge }] },
  { grupo: "Financeiro", itens: [
    { rota: "lancamentos", nome: "Lançamentos", icone: ReceiptText },
    { rota: "contas", nome: "Contas", icone: Landmark },
    { rota: "contas-a-pagar", nome: "Contas a pagar", icone: WalletCards },
    { rota: "contas-a-receber", nome: "Contas a receber", icone: FileText },
    { rota: "inteligencia", nome: "Inteligência", icone: Sparkles },
  ] },
  { grupo: "Cadastros", itens: [
    { rota: "clientes", nome: "Clientes", icone: UserRound },
    { rota: "fornecedores", nome: "Fornecedores", icone: Building2 },
    { rota: "categorias", nome: "Categorias", icone: FolderTree },
    { rota: "usuarios", nome: "Usuários", icone: UsersRound },
    { rota: "relatorios", nome: "Relatórios", icone: BarChart3 },
  ] },
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
  const grupos = tipo === "pessoal" ? pessoal : empresarial;
  return (
    <div className="area">
      <aside className={`menu ${menuAberto ? "aberto" : ""}`}>
        <div className="topo-menu">
          <Link className="marca" href={`/${tipo}/visao-geral`}>
            <span className="simbolo-marca"><WalletCards size={19} /></span>
            <span className="nome-marca">OnKash <em>Finance</em></span>
          </Link>
          <button
            className="botao-menu"
            aria-expanded={menuAberto}
            onClick={() => setMenuAberto(!menuAberto)}
          >
            {menuAberto ? <X size={20} /> : <MenuIcon size={20} />}
            <span className="sr-only">Menu</span>
          </button>
        </div>
        <nav aria-label="Menu principal">
          {grupos.map((grupo) => <div className="grupo-menu" key={grupo.grupo}>
            <span className="titulo-grupo-menu">{grupo.grupo}</span>
            {grupo.itens.map(({ rota, nome, icone: Icone }) => {
              const ativo = caminhoAtual.includes(`/${rota}`);
              return <Link key={rota} className={ativo ? "ativo" : ""} aria-current={ativo ? "page" : undefined} href={`/${tipo}/${rota}`} onClick={() => setMenuAberto(false)}>
                <Icone size={18} strokeWidth={2} /><span>{nome}</span>
              </Link>;
            })}
          </div>)}
        </nav>
        <div className="tipo-conta-menu"><span>{tipo === "pessoal" ? "P" : "E"}</span><div><strong>Espaço {tipo}</strong><small>Ambiente protegido</small></div></div>
      </aside>
      <main className="conteudo">
        <CabecalhoGlobal />
        {children}
      </main>
    </div>
  );
}
