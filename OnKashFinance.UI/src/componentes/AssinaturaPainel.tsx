"use client";

import { useEffect, useState } from "react";
import { Check, Crown, Gauge, Rocket, ShieldCheck, Sparkles } from "lucide-react";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { ErroApi, requisicao } from "@/servicos/api";
import type { AssinaturaResumo } from "@/tipos/api";

export function AssinaturaPainel() {
  const { sessao } = useAutenticacao();
  const empresarial = sessao?.tipoConta === "EMPRESARIAL";
  const [resumo, setResumo] = useState<AssinaturaResumo | null>(null);
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState<string>();
  const [mensagem, setMensagem] = useState("");

  useEffect(() => {
    if (!sessao) return;
    requisicao<AssinaturaResumo>("/api/assinatura", {}, sessao.token)
      .then(setResumo)
      .catch((e: unknown) => setErro(e instanceof ErroApi ? e.message : "Não foi possível consultar o plano."));
  }, [sessao]);

  const solicitar = async (plano: string) => {
    if (!sessao) return;
    setEnviando(plano); setErro(""); setMensagem("");
    try {
      const resposta = await requisicao<{ mensagem: string }>("/api/assinatura/solicitar-upgrade", {
        method: "POST", body: JSON.stringify({ plano }),
      }, sessao.token);
      setMensagem(resposta.mensagem);
      setResumo(atual => atual ? { ...atual, possuiSolicitacaoPendente: true } : atual);
    } catch (e) {
      setErro(e instanceof ErroApi ? e.message : "Não foi possível registrar seu interesse.");
    } finally { setEnviando(undefined); }
  };

  if (erro && !resumo) return <p className="mensagem erro">{erro}</p>;
  if (!resumo) return <p className="estado">Carregando informações do plano...</p>;

  return <section className="pagina-assinatura">
    <header className="hero-assinatura">
      <div>
        <p className="sobre-titulo">{empresarial ? "Planos para empresas" : "Planos para você"}</p>
        <h1>{empresarial ? "Plano empresarial OnKash" : "Plano pessoal OnKash"}</h1>
        <p>{empresarial
          ? "Recursos pensados para operação, equipe, controle e crescimento da empresa."
          : "Recursos pensados para organização, planejamento e evolução da sua vida financeira."}</p>
      </div>
      <div className="plano-atual-selo"><Crown size={20}/><span><small>Plano atual</small><strong>{resumo.nomePlano}</strong></span></div>
    </header>

    {mensagem && <p className="mensagem sucesso"><ShieldCheck size={18}/>{mensagem}</p>}
    {erro && <p className="mensagem erro">{erro}</p>}

    <section className="painel-uso-plano">
      <header><div><p className="sobre-titulo">Uso do ciclo atual</p><h2>Consumo da conta</h2></div><span><Gauge size={16}/> Atualizado agora</span></header>
      <div className="grade-uso-plano">{resumo.uso.map(item => {
        const percentual = item.limite ? Math.min(100, Math.round(item.utilizado / item.limite * 100)) : 0;
        return <article key={item.chave}>
          <div><strong>{item.nome}</strong><span>{item.utilizado} {item.limite ? `de ${item.limite}` : "sem limite"} {item.unidade}</span></div>
          <div className="trilha-uso" aria-label={`${percentual}% utilizado`}><i style={{ width: `${item.limite ? percentual : 8}%` }}/></div>
        </article>;
      })}</div>
    </section>

    <div className="titulo-planos"><div><p className="sobre-titulo">{empresarial ? "Soluções empresariais" : "Soluções pessoais"}</p><h2>{empresarial ? "Planos para cada fase do negócio" : "Planos para cada fase da sua vida financeira"}</h2></div><span className="selo-em-breve"><Rocket size={15}/> Pagamentos em breve</span></div>
    <section className="grade-planos">{resumo.planos.map(plano => <article key={plano.codigo} className={`${plano.destaque ? "destaque" : ""} ${plano.atual ? "atual" : ""}`}>
      {plano.destaque && <span className="mais-escolhido"><Sparkles size={13}/> Recomendado</span>}
      <header><h3>{plano.nome}</h3><p>{plano.descricao}</p></header>
      <ul>{plano.recursos.map(recurso => <li key={recurso}><Check size={17}/>{recurso}</li>)}</ul>
      {plano.atual
        ? <button className="botao-plano atual" disabled>Seu plano atual</button>
        : <button className="botao-plano" disabled={!!enviando || resumo.possuiSolicitacaoPendente} onClick={() => void solicitar(plano.codigo)}>
            {enviando === plano.codigo ? "Registrando..." : resumo.possuiSolicitacaoPendente ? "Interesse registrado" : "Quero ser avisado"}
          </button>}
    </article>)}</section>
  </section>;
}
