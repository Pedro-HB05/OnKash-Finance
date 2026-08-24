"use client";

import { useEffect, useState } from "react";
import { CalendarClock, Plus, Target, Trash2 } from "lucide-react";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Badge, Campo, Modal } from "@/componentes/Base";
import { ConfirmacaoAcao, MenuAcoes } from "@/componentes/MenuAcoes";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { Categoria, Conta, OrcamentoPessoal, RecorrenciaPessoal } from "@/tipos/api";
import { moeda, textoEnum } from "@/utilitarios/formatadores";

const mesAtual = () => new Date().toISOString().slice(0, 7);

export function PlanejamentoPessoal() {
  const { sessao } = useAutenticacao();
  const [aba, setAba] = useState<"orcamentos" | "recorrencias">("orcamentos");
  const [orcamentos, setOrcamentos] = useState<OrcamentoPessoal[]>([]);
  const [recorrencias, setRecorrencias] = useState<RecorrenciaPessoal[]>([]);
  const [contas, setContas] = useState<Conta[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [mes, setMes] = useState(mesAtual());
  const [modal, setModal] = useState<"orcamento" | "recorrencia" | null>(null);
  const [excluindo, setExcluindo] = useState<{ tipo: "orcamentos" | "recorrencias"; id: string; nome: string } | null>(null);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [salvando, setSalvando] = useState(false);
  const [tipoRecorrencia, setTipoRecorrencia] = useState<"ENTRADA" | "SAIDA">("SAIDA");

  const carregar = async () => {
    if (!sessao) return;
    try {
      setErro("");
      const [o, r, c, cat] = await Promise.all([
        requisicao<OrcamentoPessoal[]>(`/api/pessoal/planejamento/orcamentos?mes=${mes}-01`, {}, sessao.token),
        requisicao<RecorrenciaPessoal[]>("/api/pessoal/planejamento/recorrencias", {}, sessao.token),
        requisicao<Conta[]>("/api/pessoal/contas", {}, sessao.token),
        requisicao<Categoria[]>("/api/pessoal/categorias", {}, sessao.token),
      ]);
      setOrcamentos(o); setRecorrencias(r); setContas(c); setCategorias(cat);
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível carregar o planejamento."); }
  };
  useEffect(() => { void carregar(); }, [sessao, mes]);

  const salvarOrcamento = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault(); if (!sessao) return; const f = new FormData(e.currentTarget); setSalvando(true);
    try {
      await requisicao("/api/pessoal/planejamento/orcamentos", { method: "POST", body: JSON.stringify({ categoriaId: f.get("categoriaId"), mes: `${f.get("mes")}-01`, limite: Number(f.get("limite")) }) }, sessao.token);
      setModal(null); setSucesso("Orçamento salvo com sucesso."); await carregar();
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível salvar o orçamento."); } finally { setSalvando(false); }
  };
  const salvarRecorrencia = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault(); if (!sessao) return; const f = new FormData(e.currentTarget); setSalvando(true);
    try {
      await requisicao("/api/pessoal/planejamento/recorrencias", { method: "POST", body: JSON.stringify({ contaId: f.get("contaId"), categoriaId: f.get("categoriaId"), tipo: f.get("tipo"), descricao: f.get("descricao"), valor: Number(f.get("valor")), frequencia: f.get("frequencia"), proximaExecucao: f.get("proximaExecucao"), ativo: true }) }, sessao.token);
      setModal(null); setSucesso("Recorrência criada com sucesso."); await carregar();
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível salvar a recorrência."); } finally { setSalvando(false); }
  };
  const excluir = async () => {
    if (!sessao || !excluindo) return;
    try { await requisicao(`/api/pessoal/planejamento/${excluindo.tipo}/${excluindo.id}`, { method: "DELETE" }, sessao.token); setExcluindo(null); setSucesso("Registro excluído."); await carregar(); }
    catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível excluir."); }
  };
  const alternarRecorrencia = async (item: RecorrenciaPessoal) => {
    if (!sessao) return;
    try {
      await requisicao(`/api/pessoal/planejamento/recorrencias/${item.id}`, {
        method: "PUT",
        body: JSON.stringify({
          contaId: item.contaId, categoriaId: item.categoriaId, tipo: item.tipo,
          descricao: item.descricao, valor: item.valor, frequencia: item.frequencia,
          proximaExecucao: item.proximaExecucao, ativo: !item.ativo,
        }),
      }, sessao.token);
      setSucesso(item.ativo ? "Recorrência pausada." : "Recorrência reativada.");
      await carregar();
    } catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível alterar a recorrência."); }
  };

  return <AreaAutenticada tipo="pessoal">
    <header className="cabecalho"><div><p className="sobre-titulo">Planejamento</p><h1>Seu dinheiro antes que ele aconteça</h1><p>Defina limites, automatize lançamentos e antecipe decisões.</p></div><button className="botao" onClick={() => setModal(aba === "orcamentos" ? "orcamento" : "recorrencia")}><Plus size={18} /> Novo {aba === "orcamentos" ? "orçamento" : "recorrente"}</button></header>
    {erro && <p className="mensagem erro">{erro}</p>}{sucesso && <p className="mensagem sucesso">{sucesso}</p>}
    <div className="abas-planejamento"><button className={aba === "orcamentos" ? "ativo" : ""} onClick={() => setAba("orcamentos")}><Target size={18}/> Orçamentos</button><button className={aba === "recorrencias" ? "ativo" : ""} onClick={() => setAba("recorrencias")}><CalendarClock size={18}/> Recorrências</button></div>
    {aba === "orcamentos" ? <>
      <label className="seletor-mes">Mês de referência <input type="month" value={mes} onChange={e => setMes(e.target.value)} /></label>
      {orcamentos.length === 0 ? <div className="estado-vazio"><Target size={28}/><h2>Nenhum orçamento definido</h2><p>Crie limites por categoria para receber alertas antes de gastar além do planejado.</p></div> : <div className="grade-orcamentos">{orcamentos.map(o => <article className="card-orcamento" key={o.id}><div><span>{o.categoria}</span><MenuAcoes acoes={[{ rotulo: "Excluir", perigosa: true, executar: () => setExcluindo({ tipo: "orcamentos", id: o.id, nome: o.categoria }) }]}/></div><strong>{moeda(o.utilizado)} <small>de {moeda(o.limite)}</small></strong><div className="barra-orcamento"><span style={{ width: `${Math.min(o.percentual, 100)}%` }}/></div><p>{o.percentual.toFixed(0)}% utilizado</p></article>)}</div>}
    </> : recorrencias.length === 0 ? <div className="estado-vazio"><CalendarClock size={28}/><h2>Nenhum lançamento recorrente</h2><p>Automatize salários, assinaturas, aluguel e outras movimentações frequentes.</p></div> : <div className="tabela"><table><thead><tr><th>Descrição</th><th>Tipo</th><th>Conta</th><th>Valor</th><th>Frequência</th><th>Próxima</th><th>Status</th><th/></tr></thead><tbody>{recorrencias.map(r => <tr key={r.id}><td>{r.descricao}</td><td><Badge valor={r.tipo}/></td><td>{r.conta}</td><td>{moeda(r.valor)}</td><td>{textoEnum(r.frequencia)}</td><td>{new Date(`${r.proximaExecucao}T12:00:00`).toLocaleDateString("pt-BR")}</td><td><Badge valor={r.ativo ? "ATIVO" : "PAUSADO"}/></td><td><MenuAcoes acoes={[{ rotulo: r.ativo ? "Pausar" : "Ativar", executar: () => void alternarRecorrencia(r) }, { rotulo: "Excluir", perigosa: true, executar: () => setExcluindo({ tipo: "recorrencias", id: r.id, nome: r.descricao }) }]}/></td></tr>)}</tbody></table></div>}
    {modal === "orcamento" && <Modal titulo="Novo orçamento" fechar={() => setModal(null)}><form className="formulario" onSubmit={salvarOrcamento}><label className="campo">Categoria<select name="categoriaId" required><option value="">Selecione</option>{categorias.filter(c => c.ativo && c.tipo === "SAIDA").map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}</select></label><Campo label="Mês" name="mes" type="month" defaultValue={mes} required/><Campo label="Limite mensal" name="limite" type="number" min="0.01" step="0.01" required/><button className="botao" disabled={salvando}>Salvar orçamento</button></form></Modal>}
    {modal === "recorrencia" && <Modal titulo="Novo lançamento recorrente" fechar={() => setModal(null)}><form className="formulario" onSubmit={salvarRecorrencia}><label className="campo">Tipo<select name="tipo" value={tipoRecorrencia} onChange={e => setTipoRecorrencia(e.target.value as typeof tipoRecorrencia)}><option value="ENTRADA">Entrada</option><option value="SAIDA">Saída</option></select></label><label className="campo">Conta<select name="contaId" required><option value="">Selecione</option>{contas.filter(c => c.ativo).map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}</select></label><label className="campo">Categoria<select name="categoriaId" required><option value="">Selecione</option>{categorias.filter(c => c.ativo && c.tipo === tipoRecorrencia).map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}</select></label><Campo label="Descrição" name="descricao" required/><Campo label="Valor" name="valor" type="number" min="0.01" step="0.01" required/><label className="campo">Frequência<select name="frequencia"><option value="SEMANAL">Semanal</option><option value="MENSAL">Mensal</option><option value="ANUAL">Anual</option></select></label><Campo label="Próxima execução" name="proximaExecucao" type="date" required/><button className="botao" disabled={salvando}>Criar recorrência</button></form></Modal>}
    {excluindo && <Modal titulo="Excluir planejamento" fechar={() => setExcluindo(null)}><ConfirmacaoAcao descricao={`Excluir “${excluindo.nome}”?`} textoConfirmar="Excluir" confirmar={() => void excluir()} fechar={() => setExcluindo(null)}/></Modal>}
  </AreaAutenticada>;
}
