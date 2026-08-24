"use client";

import { useEffect, useState } from "react";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { Conta, DreSimplificada, MovimentoImportacao, ProjecaoCaixa, ResultadoImportacao } from "@/tipos/api";
import { moeda } from "@/utilitarios/formatadores";
import { BarChart3, CheckCircle2, FileSpreadsheet, LineChart as LineIcon, ShieldCheck, UploadCloud } from "lucide-react";
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type Aba = "importar" | "projecao" | "dre";
const hoje = () => new Date().toISOString().slice(0, 10);
const inicioAno = () => `${new Date().getFullYear()}-01-01`;

function numero(valor: string) {
  const limpo = valor.replace(/[R$\s]/g, "");
  const normalizado = limpo.includes(",") ? limpo.replaceAll(".", "").replace(",", ".") : limpo;
  return Number(normalizado);
}
function dataIso(valor: string) {
  const texto = valor.trim().slice(0, 10);
  if (/^\d{4}-\d{2}-\d{2}$/.test(texto)) return texto;
  if (/^\d{8}/.test(texto)) return `${texto.slice(0, 4)}-${texto.slice(4, 6)}-${texto.slice(6, 8)}`;
  const partes = texto.split(/[\/.-]/);
  if (partes.length === 3) return `${partes[2].padStart(4, "20")}-${partes[1].padStart(2, "0")}-${partes[0].padStart(2, "0")}`;
  return "";
}
function colunasCsv(linha: string, separador: string) {
  const itens: string[] = []; let atual = ""; let aspas = false;
  for (let i = 0; i < linha.length; i++) {
    const c = linha[i];
    if (c === '"' && linha[i + 1] === '"') { atual += '"'; i++; }
    else if (c === '"') aspas = !aspas;
    else if (c === separador && !aspas) { itens.push(atual.trim()); atual = ""; }
    else atual += c;
  }
  itens.push(atual.trim()); return itens;
}
function lerCsv(conteudo: string): MovimentoImportacao[] {
  const linhas = conteudo.replace(/^\uFEFF/, "").split(/\r?\n/).filter(Boolean);
  if (linhas.length < 2) throw new Error("O CSV não possui movimentos.");
  const separador = (linhas[0].match(/;/g)?.length ?? 0) >= (linhas[0].match(/,/g)?.length ?? 0) ? ";" : ",";
  const cabecalho = colunasCsv(linhas[0], separador).map(x => x.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, ""));
  const indice = (nomes: string[]) => cabecalho.findIndex(x => nomes.some(n => x.includes(n)));
  const iData = indice(["data", "date"]), iDescricao = indice(["descricao", "historico", "memo", "description"]), iValor = indice(["valor", "amount", "trnamt"]), iCredito = indice(["credito"]), iDebito = indice(["debito"]);
  if (iData < 0 || iDescricao < 0 || (iValor < 0 && iCredito < 0 && iDebito < 0)) throw new Error("Use colunas Data, Descrição e Valor (ou Crédito/Débito).");
  return linhas.slice(1).map(linha => {
    const c = colunasCsv(linha, separador); const credito = iCredito >= 0 ? numero(c[iCredito] ?? "0") : 0; const debito = iDebito >= 0 ? numero(c[iDebito] ?? "0") : 0;
    return { data: dataIso(c[iData] ?? ""), descricao: c[iDescricao]?.trim() ?? "", valor: iValor >= 0 ? numero(c[iValor] ?? "0") : credito - debito };
  }).filter(x => x.data && x.descricao && Number.isFinite(x.valor) && x.valor !== 0);
}
function lerOfx(conteudo: string): MovimentoImportacao[] {
  const blocos = conteudo.match(/<STMTTRN>[\s\S]*?(?=<STMTTRN>|<\/BANKTRANLIST>|$)/gi) ?? [];
  const tag = (bloco: string, nome: string) => bloco.match(new RegExp(`<${nome}>([^<\\r\\n]+)`, "i"))?.[1]?.trim() ?? "";
  const itens = blocos.map(bloco => ({ data: dataIso(tag(bloco, "DTPOSTED")), descricao: tag(bloco, "MEMO") || tag(bloco, "NAME") || "Movimento bancário", valor: numero(tag(bloco, "TRNAMT")) })).filter(x => x.data && Number.isFinite(x.valor) && x.valor !== 0);
  if (!itens.length) throw new Error("Nenhum movimento bancário foi encontrado no OFX.");
  return itens;
}

export function InteligenciaFinanceira({ tipo }: { tipo: "pessoal" | "empresarial" }) {
  const { sessao } = useAutenticacao();
  const [aba, setAba] = useState<Aba>("importar");
  const [contas, setContas] = useState<Conta[]>([]); const [contaId, setContaId] = useState("");
  const [arquivo, setArquivo] = useState(""); const [movimentos, setMovimentos] = useState<MovimentoImportacao[]>([]);
  const [resultado, setResultado] = useState<ResultadoImportacao | null>(null); const [processando, setProcessando] = useState(false);
  const [projecao, setProjecao] = useState<ProjecaoCaixa | null>(null); const [dias, setDias] = useState(90);
  const [dre, setDre] = useState<DreSimplificada | null>(null); const [inicio, setInicio] = useState(inicioAno()); const [fim, setFim] = useState(hoje());
  const [erro, setErro] = useState("");
  const base = `/api/${tipo}/inteligencia`;

  useEffect(() => { if (!sessao) return; requisicao<Conta[]>(`/api/${tipo}/contas`, {}, sessao.token).then(x => { setContas(x); setContaId(x.find(c => c.ativo)?.id ?? ""); }).catch(() => setErro("Não foi possível carregar as contas.")); }, [sessao, tipo]);
  useEffect(() => { if (!sessao || aba !== "projecao") return; requisicao<ProjecaoCaixa>(`${base}/projecao?dias=${dias}`, {}, sessao.token).then(setProjecao).catch(f => setErro(f instanceof Error ? f.message : "Não foi possível calcular a projeção.")); }, [sessao, aba, dias, base]);
  useEffect(() => { if (!sessao || aba !== "dre" || tipo !== "empresarial") return; requisicao<DreSimplificada>(`${base}/dre?inicio=${inicio}&fim=${fim}`, {}, sessao.token).then(setDre).catch(f => setErro(f instanceof Error ? f.message : "Não foi possível calcular a DRE.")); }, [sessao, aba, inicio, fim, tipo, base]);

  const selecionarArquivo = async (evento: React.ChangeEvent<HTMLInputElement>) => {
    const item = evento.target.files?.[0]; if (!item) return; setErro(""); setResultado(null);
    try { const conteudo = await item.text(); const lista = item.name.toLowerCase().endsWith(".ofx") ? lerOfx(conteudo) : lerCsv(conteudo); if (!lista.length) throw new Error("Nenhum movimento válido foi encontrado."); setArquivo(item.name); setMovimentos(lista); }
    catch (falha) { setArquivo(""); setMovimentos([]); setErro(falha instanceof Error ? falha.message : "Não foi possível ler o arquivo."); }
  };
  const importar = async () => {
    if (!sessao || !contaId || !movimentos.length) return; setProcessando(true); setErro("");
    try { const r = await requisicao<ResultadoImportacao>(`${base}/importacoes`, { method: "POST", body: JSON.stringify({ contaId, arquivoOrigem: arquivo, movimentos }) }, sessao.token); setResultado(r); setMovimentos([]); setArquivo(""); }
    catch (falha) { setErro(falha instanceof Error ? falha.message : "Não foi possível importar o extrato."); } finally { setProcessando(false); }
  };
  const dadosGrafico = projecao?.pontos.map(p => ({ ...p, dataLabel: new Date(`${p.data}T12:00:00`).toLocaleDateString("pt-BR", { day: "2-digit", month: "short" }) })) ?? [];

  return <AreaAutenticada tipo={tipo}>
    <header className="cabecalho"><div><p className="sobre-titulo">Fase 2</p><h1>Inteligência financeira</h1><p>Concilie extratos, antecipe o caixa e transforme movimentações em decisões.</p></div><span className="selo-inteligencia"><ShieldCheck size={18}/> Dados protegidos</span></header>
    {erro && <p className="mensagem erro">{erro}</p>}
    <nav className="abas-planejamento abas-inteligencia">
      <button className={aba === "importar" ? "ativo" : ""} onClick={() => { setAba("importar"); setErro(""); }}><UploadCloud size={18}/> Importar e conciliar</button>
      <button className={aba === "projecao" ? "ativo" : ""} onClick={() => { setAba("projecao"); setErro(""); }}><LineIcon size={18}/> Projeção de caixa</button>
      {tipo === "empresarial" && <button className={aba === "dre" ? "ativo" : ""} onClick={() => { setAba("dre"); setErro(""); }}><BarChart3 size={18}/> DRE simplificada</button>}
    </nav>
    {aba === "importar" && <section className="painel-inteligencia">
      <div className="passo-importacao"><span>1</span><div><strong>Escolha a conta</strong><small>Os movimentos serão conciliados nesta conta.</small></div><select value={contaId} onChange={e => setContaId(e.target.value)}><option value="">Selecione</option>{contas.filter(c => c.ativo).map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}</select></div>
      <label className="drop-arquivo"><FileSpreadsheet size={30}/><strong>{arquivo || "Selecione um extrato OFX ou CSV"}</strong><small>CSV com Data, Descrição e Valor. Até 1.000 movimentos por arquivo.</small><input type="file" accept=".ofx,.csv,text/csv,application/x-ofx" onChange={selecionarArquivo}/><span className="botao secundario">Escolher arquivo</span></label>
      {movimentos.length > 0 && <><div className="resumo-importacao"><div><strong>{movimentos.length}</strong><span>movimentos identificados</span></div><div><strong>{moeda(movimentos.filter(x => x.valor > 0).reduce((s, x) => s + x.valor, 0))}</strong><span>créditos</span></div><div><strong>{moeda(Math.abs(movimentos.filter(x => x.valor < 0).reduce((s, x) => s + x.valor, 0)))}</strong><span>débitos</span></div></div><div className="tabela previa-importacao"><table><thead><tr><th>Data</th><th>Descrição</th><th>Valor</th></tr></thead><tbody>{movimentos.slice(0, 20).map((m, i) => <tr key={`${m.data}-${i}`}><td>{new Date(`${m.data}T12:00:00`).toLocaleDateString("pt-BR")}</td><td>{m.descricao}</td><td className={m.valor >= 0 ? "valor-positivo" : "valor-negativo"}>{m.valor >= 0 ? "+ " : "- "}{moeda(Math.abs(m.valor))}</td></tr>)}</tbody></table>{movimentos.length > 20 && <p>Prévia dos primeiros 20 movimentos.</p>}</div><button className="botao" onClick={() => void importar()} disabled={!contaId || processando}>{processando ? "Conciliando..." : "Confirmar importação"}</button></>}
      {resultado && <div className="resultado-importacao"><CheckCircle2 size={28}/><div><strong>Extrato processado com sucesso</strong><p>{resultado.importados} novo(s), {resultado.conciliados} conciliado(s) e {resultado.duplicados} duplicado(s) ignorado(s).</p></div></div>}
    </section>}
    {aba === "projecao" && <section className="painel-inteligencia"><div className="cabecalho-painel"><div><p className="sobre-titulo">Previsibilidade</p><h2>Saldo projetado</h2></div><select value={dias} onChange={e => setDias(Number(e.target.value))}><option value={30}>Próximos 30 dias</option><option value={60}>Próximos 60 dias</option><option value={90}>Próximos 90 dias</option><option value={180}>Próximos 6 meses</option><option value={365}>Próximo ano</option></select></div>{projecao ? <><div className="resumo-importacao"><div><span>Saldo atual</span><strong>{moeda(projecao.saldoAtual)}</strong></div><div><span>Saldo ao fim</span><strong className={projecao.saldoProjetado < 0 ? "valor-negativo" : "valor-positivo"}>{moeda(projecao.saldoProjetado)}</strong></div><div><span>Variação prevista</span><strong>{moeda(projecao.saldoProjetado - projecao.saldoAtual)}</strong></div></div><div className="grafico grafico-projecao"><ResponsiveContainer width="100%" height="100%"><LineChart data={dadosGrafico}><CartesianGrid vertical={false} stroke="var(--borda)" strokeDasharray="4 4"/><XAxis dataKey="dataLabel" tick={{ fill: "var(--texto2)", fontSize: 12 }}/><YAxis tickFormatter={v => moeda(Number(v))} tick={{ fill: "var(--texto2)", fontSize: 11 }}/><Tooltip formatter={v => moeda(Number(v))} contentStyle={{ background: "var(--superficie)", border: "1px solid var(--borda)", borderRadius: 12 }}/><Line type="monotone" dataKey="saldoProjetado" name="Saldo projetado" stroke="var(--marca)" strokeWidth={3} dot={{ r: 3 }}/></LineChart></ResponsiveContainer></div>{projecao.pontos.length === 1 && <p className="estado">Nenhum compromisso futuro encontrado nesse período.</p>}</> : <p className="estado">Calculando projeção...</p>}</section>}
    {aba === "dre" && tipo === "empresarial" && <section className="painel-inteligencia"><div className="cabecalho-painel"><div><p className="sobre-titulo">Resultado empresarial</p><h2>DRE simplificada</h2></div><div className="periodo-dre"><input type="date" value={inicio} onChange={e => setInicio(e.target.value)}/><span>até</span><input type="date" value={fim} onChange={e => setFim(e.target.value)}/></div></div>{dre ? <><div className="resumo-importacao"><div><span>Receita bruta</span><strong className="valor-positivo">{moeda(dre.receitaBruta)}</strong></div><div><span>Despesas</span><strong className="valor-negativo">{moeda(dre.despesas)}</strong></div><div><span>Resultado</span><strong>{moeda(dre.resultado)}</strong></div><div><span>Margem líquida</span><strong>{dre.margem.toFixed(1).replace(".", ",")}%</strong></div></div><div className="grade-dre"><div><h3>Receitas por categoria</h3>{dre.receitasPorCategoria.map(x => <p key={x.categoria}><span>{x.categoria}</span><strong>{moeda(x.valor)}</strong></p>)}</div><div><h3>Despesas por categoria</h3>{dre.despesasPorCategoria.map(x => <p key={x.categoria}><span>{x.categoria}</span><strong>{moeda(x.valor)}</strong></p>)}</div></div></> : <p className="estado">Calculando DRE...</p>}</section>}
  </AreaAutenticada>;
}
