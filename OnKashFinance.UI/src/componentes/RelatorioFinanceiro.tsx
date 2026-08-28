"use client";
import { useEffect, useState } from "react";
import { FileDown } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type {
  DashboardEmpresarial,
  DashboardPessoal,
  LancamentoEmpresarial,
  LancamentoPessoal,
} from "@/tipos/api";
import { data, moeda } from "@/utilitarios/formatadores";

type Resumo = DashboardPessoal | DashboardEmpresarial;
const nomeTipo = (tipo: LancamentoPessoal["tipo"] | LancamentoEmpresarial["tipo"]) => ({
  ENTRADA: "Entrada",
  SAIDA: "Saída",
  RECEITA: "Receita",
  DESPESA: "Despesa",
  TRANSFERENCIA: "Transferência",
})[tipo];

export function RelatorioFinanceiro({ tipo }: { tipo: "pessoal" | "empresarial" }) {
  const { sessao } = useAutenticacao();
  const [resumo, setResumo] = useState<Resumo | null>(null);
  const [lancamentos, setLancamentos] = useState<(LancamentoPessoal | LancamentoEmpresarial)[]>([]);
  const [erro, setErro] = useState("");
  useEffect(() => {
    if (!sessao) return;
    requisicao<Resumo>(`/api/dashboard/${tipo}`, {}, sessao.token)
      .then(setResumo)
      .catch(() => setErro("Não foi possível carregar o resumo financeiro."));
    const rota = tipo === "pessoal" ? "/api/pessoal/lancamentos" : "/api/empresarial/lancamentos";
    requisicao<(LancamentoPessoal | LancamentoEmpresarial)[]>(rota, {}, sessao.token)
      .then(setLancamentos)
      .catch(() => setErro("Não foi possível carregar os lançamentos."));
  }, [sessao, tipo]);
  const linhas = lancamentos.map((item) => [
    data(item.data),
    item.descricao,
    item.categoria ?? "—",
    item.tipo === "TRANSFERENCIA" && item.contaDestino
      ? `${item.conta} → ${item.contaDestino}`
      : item.conta,
    nomeTipo(item.tipo),
    moeda(item.valor),
    item.cancelado ? "Cancelado" : "Ativo",
  ]);
  const exportarCsv = () => {
    const conteudo = [
      "Data;Descrição;Categoria;Conta;Tipo;Valor;Status",
      ...linhas.map((l) => l.map((v) => `"${String(v).replaceAll('"', '""')}"`).join(";")),
    ].join("\n");
    const url = URL.createObjectURL(
      new Blob(["\ufeff", conteudo], { type: "text/csv;charset=utf-8" }),
    );
    const link = document.createElement("a");
    link.href = url;
    link.download = "relatorio-onkash.csv";
    link.click();
    URL.revokeObjectURL(url);
  };
  const exportarPdf = () => {
    if (!resumo) return;
    const pdf = new jsPDF();
    pdf.setFontSize(19);
    pdf.text("OnKash Finance", 14, 18);
    pdf.setFontSize(14);
    pdf.text("Relatório financeiro", 14, 27);
    pdf.setFontSize(10);
    pdf.text(`Emitido em ${new Intl.DateTimeFormat("pt-BR").format(new Date())}`, 14, 34);
    autoTable(pdf, {
      startY: 41,
      head: [["Saldo", "Entradas", "Saídas", "Resultado"]],
      body: [
        [
          moeda(resumo.saldo),
          moeda(resumo.entradas),
          moeda(resumo.saidas),
          moeda("resultadoMes" in resumo ? resumo.resultadoMes : resumo.resultado),
        ],
      ],
      theme: "grid",
    });
    if (linhas.length)
      autoTable(pdf, {
        startY: 70,
        head: [["Data", "Descrição", "Categoria", "Conta", "Tipo", "Valor", "Status"]],
        body: linhas,
        styles: { fontSize: 7 },
        headStyles: { fillColor: [17, 106, 113] },
      });
    pdf.save("relatorio-onkash.pdf");
  };
  return (
    <section className="relatorio">
      <header className="cabecalho">
        <div>
          <p className="sobre-titulo">Relatórios</p>
          <h1>Visão financeira detalhada</h1>
          <p>Dados reais do período atual.</p>
        </div>
        <div className="acoes-relatorio">
          <button className="botao secundario" onClick={exportarCsv} disabled={!lancamentos.length} title={!lancamentos.length ? "Adicione lançamentos para exportar" : undefined}>
            <FileDown size={18} /> CSV
          </button>
          <button className="botao" onClick={exportarPdf} disabled={!lancamentos.length} title={!lancamentos.length ? "Adicione lançamentos para exportar" : undefined}>
            <FileDown size={18} /> Exportar PDF
          </button>
        </div>
      </header>
      {erro ? (
        <p className="mensagem erro">{erro}</p>
      ) : !resumo ? (
        <p className="estado">Carregando relatório financeiro...</p>
      ) : (
        <>
          <section className="resumos">
            <article className="resumo">
              <span>Saldo</span>
              <strong>{moeda(resumo.saldo)}</strong>
            </article>
            <article className="resumo">
              <span>Entradas</span>
              <strong>{moeda(resumo.entradas)}</strong>
            </article>
            <article className="resumo">
              <span>Saídas</span>
              <strong>{moeda(resumo.saidas)}</strong>
            </article>
            <article className="resumo">
              <span>Resultado</span>
              <strong>
                {moeda("resultadoMes" in resumo ? resumo.resultadoMes : resumo.resultado)}
              </strong>
            </article>
          </section>
          {lancamentos.length === 0 ? (
            <div className="estado-vazio">
              <span className="icone-estado-vazio"><FileDown size={24} /></span>
              <h2>Nenhum dado para exportar</h2>
              <p>Os lançamentos registrados aparecerão aqui e poderão ser exportados em CSV ou PDF.</p>
            </div>
          ) : <section className="tabela relatorio-lista">
            <table>
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Descrição</th>
                  <th>Categoria</th>
                  <th>Conta</th>
                  <th>Tipo</th>
                  <th>Valor</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {lancamentos.map((item) => (
                  <tr key={item.id}>
                    <td>{data(item.data)}</td>
                    <td>{item.descricao}</td>
                    <td>{item.categoria ?? "—"}</td>
                    <td>{item.tipo === "TRANSFERENCIA" && item.contaDestino ? `${item.conta} → ${item.contaDestino}` : item.conta}</td>
                    <td>{nomeTipo(item.tipo)}</td>
                    <td>{moeda(item.valor)}</td>
                    <td>{item.cancelado ? "Cancelado" : "Ativo"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>}
        </>
      )}
    </section>
  );
}
