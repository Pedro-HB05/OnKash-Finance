"use client";

import { FormEvent, useEffect, useState } from "react";
import Link from "next/link";
import { CheckCircle2, Download, FileCheck2, LockKeyhole, Send, ShieldCheck, UserRound } from "lucide-react";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { baixarArquivo, ErroApi, requisicao } from "@/servicos/api";
import type { PrivacidadeResumo, SolicitacaoPrivacidade } from "@/tipos/api";

const tipos = [
  ["ACESSO", "Confirmação e acesso"], ["CORRECAO", "Correção adicional"], ["EXCLUSAO", "Exclusão da conta"],
  ["ANONIMIZACAO", "Anonimização"], ["BLOQUEIO", "Bloqueio de dados"], ["PORTABILIDADE", "Portabilidade"],
  ["REVOGACAO", "Revogação de consentimento"], ["INFORMACOES", "Informações sobre o tratamento"],
];

export function PortalPrivacidade() {
  const { sessao, atualizarSessao } = useAutenticacao();
  const [resumo, setResumo] = useState<PrivacidadeResumo>();
  const [nome, setNome] = useState(sessao?.nome ?? "");
  const [tipo, setTipo] = useState("ACESSO");
  const [detalhes, setDetalhes] = useState("");
  const [processando, setProcessando] = useState("");
  const [mensagem, setMensagem] = useState("");
  const [erro, setErro] = useState("");

  const carregar = () => sessao && requisicao<PrivacidadeResumo>("/api/privacidade", {}, sessao.token).then(setResumo).catch(e => setErro(e instanceof Error ? e.message : "Não foi possível carregar seus dados."));
  useEffect(() => { carregar(); }, [sessao]);
  const executar = async (chave: string, acao: () => Promise<void>) => {
    setProcessando(chave); setErro(""); setMensagem("");
    try { await acao(); } catch (e) { setErro(e instanceof ErroApi ? e.message : "Não foi possível concluir a operação."); }
    finally { setProcessando(""); }
  };
  const aceitar = () => executar("aceite", async () => { await requisicao("/api/privacidade/aceites", { method: "POST" }, sessao!.token); setMensagem("Seu aceite da versão atual foi registrado."); carregar(); });
  const exportar = () => executar("exportar", async () => {
    const arquivo = await baixarArquivo("/api/privacidade/exportacao", sessao!.token);
    const url = URL.createObjectURL(arquivo); const link = document.createElement("a"); link.href = url; link.download = `dados-onkash-${new Date().toISOString().slice(0, 10)}.json`; link.click(); URL.revokeObjectURL(url);
    setMensagem("Sua cópia de dados foi gerada.");
  });
  const corrigir = (e: FormEvent) => { e.preventDefault(); void executar("nome", async () => {
    await requisicao("/api/privacidade/perfil", { method: "PUT", body: JSON.stringify({ nome }) }, sessao!.token);
    atualizarSessao({ nome: nome.trim() }); setMensagem("Seu nome foi atualizado.");
  }); };
  const solicitar = (e: FormEvent) => { e.preventDefault(); void executar("solicitar", async () => {
    const nova = await requisicao<SolicitacaoPrivacidade>("/api/privacidade/solicitacoes", { method: "POST", body: JSON.stringify({ tipo, detalhes }) }, sessao!.token);
    setMensagem(`Solicitação registrada. Guarde o protocolo ${nova.protocolo}.`); setDetalhes(""); carregar();
  }); };

  if (!resumo) return erro ? <p className="mensagem erro">{erro}</p> : <p className="estado">Carregando sua área de privacidade...</p>;
  return <section className="portal-privacidade">
    <header className="hero-privacidade"><div><p className="sobre-titulo">LGPD · Seus direitos</p><h1>Privacidade e dados</h1><p>Consulte informações, baixe uma cópia e faça solicitações com protocolo.</p></div><ShieldCheck size={55}/></header>
    {mensagem && <p className="mensagem sucesso"><CheckCircle2 size={18}/>{mensagem}</p>}{erro && <p className="mensagem erro">{erro}</p>}
    <section className="resumo-privacidade"><article><LockKeyhole size={22}/><div><small>Controlador</small><strong>{resumo.controlador}</strong><span>{resumo.marca} · {resumo.localizacao}</span></div></article><article><FileCheck2 size={22}/><div><small>Documentos atuais</small><strong>Versão {resumo.versaoAtual}</strong><span>{resumo.aceiteAtual ? `Aceita em ${new Date(resumo.aceitoEm!).toLocaleDateString("pt-BR")}` : "Aceite pendente"}</span></div></article><article><Send size={22}/><div><small>Canal de atendimento</small><strong>{resumo.canal}</strong><span>Solicitações sobre dados pessoais</span></div></article></section>
    {!resumo.aceiteAtual && <section className="aviso-aceite"><div><strong>Os documentos de privacidade foram atualizados</strong><p>Leia a <Link href="/privacidade" target="_blank">Política de Privacidade</Link> e os <Link href="/termos" target="_blank">Termos de Uso</Link>.</p></div><button className="botao" onClick={() => void aceitar()} disabled={!!processando}>{processando === "aceite" ? "Registrando..." : "Li e aceito a versão atual"}</button></section>}
    <div className="grade-direitos">
      <section className="card-direito"><Download size={23}/><h2>Acessar e portar</h2><p>Baixe uma cópia estruturada dos seus dados de cadastro, finanças pessoais ou vínculo empresarial.</p><button className="botao" onClick={() => void exportar()} disabled={!!processando}>{processando === "exportar" ? "Preparando..." : "Baixar meus dados"}</button></section>
      <section className="card-direito"><UserRound size={23}/><h2>Corrigir cadastro</h2><p>Atualize seu nome. Para alterar e-mail ou outros dados, abra uma solicitação.</p><form onSubmit={corrigir}><label className="campo">Nome<input value={nome} onChange={e => setNome(e.target.value)} minLength={2} maxLength={150} required/></label><button className="botao secundario" disabled={!!processando}>{processando === "nome" ? "Salvando..." : "Atualizar nome"}</button></form></section>
    </div>
    <section className="solicitar-direito"><div><p className="sobre-titulo">Exerça outro direito</p><h2>Abrir solicitação LGPD</h2><p>Você receberá um protocolo para acompanhar o pedido. Exclusão e anonimização passam por análise das obrigações de retenção e direitos de terceiros.</p></div><form onSubmit={solicitar}><label className="campo">Tipo de solicitação<select value={tipo} onChange={e => setTipo(e.target.value)}>{tipos.map(([valor, nomeTipo]) => <option value={valor} key={valor}>{nomeTipo}</option>)}</select></label><label className="campo">Detalhes<textarea value={detalhes} onChange={e => setDetalhes(e.target.value)} maxLength={2000} placeholder="Explique o que você precisa para agilizar o atendimento."/></label><button className="botao" disabled={!!processando}>{processando === "solicitar" ? "Registrando..." : "Gerar protocolo"}</button></form></section>
    <section className="historico-privacidade"><header><div><p className="sobre-titulo">Acompanhamento</p><h2>Solicitações anteriores</h2></div><span>{resumo.solicitacoes.length} protocolo(s)</span></header>{resumo.solicitacoes.length === 0 ? <div className="estado-vazio"><FileCheck2 size={27}/><h3>Nenhuma solicitação</h3><p>Seus pedidos aparecerão aqui.</p></div> : <div className="tabela"><table><thead><tr><th>Protocolo</th><th>Tipo</th><th>Data</th><th>Status</th></tr></thead><tbody>{resumo.solicitacoes.map(item => <tr key={item.protocolo}><td>{item.protocolo}</td><td>{item.tipo.replaceAll("_", " ")}</td><td>{new Date(item.criadoEm).toLocaleDateString("pt-BR")}</td><td><span className="badge">{item.status}</span></td></tr>)}</tbody></table></div>}</section>
    <p className="nota-privacidade">Pedidos também podem ser enviados para <a href={`mailto:${resumo.canal}`}>{resumo.canal}</a>. A confirmação e o acesso simplificado são providenciados imediatamente quando disponíveis; pedidos detalhados seguem os prazos legais aplicáveis.</p>
  </section>;
}
