"use client";
import Link from "next/link";
import { useEffect, useState } from "react";
import { usePathname } from "next/navigation";
import { Bell, CalendarDays, ShieldCheck } from "lucide-react";
import { MenuPerfil } from "@/componentes/MenuPerfil";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { AlertaFinanceiro } from "@/tipos/api";

export function CabecalhoGlobal() {
  const caminho = usePathname();
  const { sessao } = useAutenticacao();
  const [alertas, setAlertas] = useState<AlertaFinanceiro[]>([]);
  const [abrirAlertas, setAbrirAlertas] = useState(false);
  const segmento = caminho.split("/").filter(Boolean).at(-1) ?? "visao-geral";
  const nomes: Record<string, string> = {
    "visao-geral": "Visão geral", lancamentos: "Lançamentos", contas: "Contas",
    cartoes: "Cartões", faturas: "Faturas", categorias: "Categorias",
    relatorios: "Relatórios", clientes: "Clientes", fornecedores: "Fornecedores",
    "contas-a-pagar": "Contas a pagar", "contas-a-receber": "Contas a receber",
    usuarios: "Usuários",
    planejamento: "Planejamento",
  };
  const pagina = nomes[segmento] ?? segmento.replaceAll("-", " ");
  const data = new Intl.DateTimeFormat("pt-BR", { weekday: "long", day: "2-digit", month: "long" }).format(new Date());
  useEffect(() => {
    if (sessao?.tipoConta !== "PESSOAL") return;
    requisicao<AlertaFinanceiro[]>("/api/pessoal/planejamento/alertas", {}, sessao.token).then(setAlertas).catch(() => setAlertas([]));
  }, [sessao]);
  return (
    <header className="topbar-global">
      <div className="contexto-topbar">
        <span className="pagina-topbar">{pagina}</span>
        <span className="data-topbar"><CalendarDays size={14} /> {data}</span>
      </div>
      <div className="acoes-topbar">
        <span className="sessao-segura"><ShieldCheck size={15} /> Sessão segura</span>
        <div className="central-alertas">
          <button className="botao-alertas" aria-label={`Alertas financeiros: ${alertas.length}`} aria-expanded={abrirAlertas} onClick={() => setAbrirAlertas(v => !v)}><Bell size={18}/>{alertas.length > 0 && <span>{alertas.length}</span>}</button>
          {abrirAlertas && <div className="painel-alertas"><header><strong>Alertas financeiros</strong><small>{alertas.length} aviso(s)</small></header>{alertas.length === 0 ? <p>Está tudo em ordem por aqui.</p> : alertas.map((a, i) => <Link href={a.link ?? "#"} key={`${a.tipo}-${i}`} onClick={() => setAbrirAlertas(false)}><i className={a.severidade.toLowerCase()}/><span><strong>{a.titulo}</strong><small>{a.descricao}</small></span></Link>)}</div>}
        </div>
        <span className="saudacao-topbar">Olá, <strong>{sessao?.nome.split(" ")[0]}</strong></span>
        <MenuPerfil />
      </div>
    </header>
  );
}
