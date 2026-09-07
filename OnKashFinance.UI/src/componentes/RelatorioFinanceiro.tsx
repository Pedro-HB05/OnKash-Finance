"use client";
import { useEffect, useState } from "react";
import { CalendarClock, FileDown } from "lucide-react";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { TipoPeriodoDashboard } from "@/componentes/DashboardFinanceiro";
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

function formatarDataLocal(valor: Date) {
  const ano = valor.getFullYear();
  const mes = String(valor.getMonth() + 1).padStart(2, "0");
  const dia = String(valor.getDate()).padStart(2, "0");

  return `${ano}-${mes}-${dia}`;
}

function obterIntervaloPeriodo(
  periodo: TipoPeriodoDashboard,
  dataInicial: string,
  dataFinal: string,
) {
  const hoje = new Date();

  if (periodo === "TODO") {
    return {
      inicio: undefined,
      fim: undefined,
    };
  }

  if (periodo === "ULTIMOS_7_DIAS") {
    const seteDiasAtras = new Date();
    seteDiasAtras.setDate(hoje.getDate() - 6);

    return {
      inicio: formatarDataLocal(seteDiasAtras),
      fim: formatarDataLocal(hoje),
    };
  }

  if (periodo === "MES_ATUAL") {
    const primeiroDia = new Date(
      hoje.getFullYear(),
      hoje.getMonth(),
      1,
    );
    const ultimoDia = new Date(
      hoje.getFullYear(),
      hoje.getMonth() + 1,
      0,
    );

    return {
      inicio: formatarDataLocal(primeiroDia),
      fim: formatarDataLocal(ultimoDia),
    };
  }

  if (periodo === "MES_ANTERIOR") {
    const primeiroDia = new Date(
      hoje.getFullYear(),
      hoje.getMonth() - 1,
      1,
    );
    const ultimoDia = new Date(
      hoje.getFullYear(),
      hoje.getMonth(),
      0,
    );

    return {
      inicio: formatarDataLocal(primeiroDia),
      fim: formatarDataLocal(ultimoDia),
    };
  }

  if (periodo === "ANO_ATUAL") {
    const primeiroDia = new Date(
      hoje.getFullYear(),
      0,
      1,
    );
    const ultimoDia = new Date(
      hoje.getFullYear(),
      11,
      31,
    );

    return {
      inicio: formatarDataLocal(primeiroDia),
      fim: formatarDataLocal(ultimoDia),
    };
  }

  return {
    inicio: dataInicial || undefined,
    fim: dataFinal || undefined,
  };
}

function rotuloPeriodo(
  periodo: TipoPeriodoDashboard,
  dataInicial?: string,
  dataFinal?: string,
) {
  switch (periodo) {
    case "MES_ATUAL":
      return "Este mês";
    case "MES_ANTERIOR":
      return "Mês anterior";
    case "ULTIMOS_7_DIAS":
      return "Últimos 7 dias";
    case "ANO_ATUAL":
      return "Este ano";
    case "PERSONALIZADO":
      if (dataInicial && dataFinal) {
        return `${data(dataInicial)} até ${data(dataFinal)}`;
      }
      return "Período personalizado";
    default:
      return "Todo o período";
  }
}

export function RelatorioFinanceiro({ tipo }: { tipo: "pessoal" | "empresarial" }) {
  const { sessao } = useAutenticacao();
  const [periodo, setPeriodo] = useState<TipoPeriodoDashboard>("MES_ATUAL");
  const [dataInicial, setDataInicial] = useState("");
  const [dataFinal, setDataFinal] = useState("");
  const [resumo, setResumo] = useState<Resumo | null>(null);
  const [lancamentos, setLancamentos] = useState<(LancamentoPessoal | LancamentoEmpresarial)[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");

  const alterarPeriodo = (novoPeriodo: TipoPeriodoDashboard) => {
    setPeriodo(novoPeriodo);

    if (novoPeriodo !== "PERSONALIZADO") {
      setDataInicial("");
      setDataFinal("");
    } else if (!dataInicial || !dataFinal) {
      const hoje = new Date();
      const primeiroDia = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
      setDataInicial(formatarDataLocal(primeiroDia));
      setDataFinal(formatarDataLocal(hoje));
    }
  };

  useEffect(() => {
    if (!sessao) return;

    const intervalo = obterIntervaloPeriodo(periodo, dataInicial, dataFinal);

    if (periodo === "PERSONALIZADO" && (!intervalo.inicio || !intervalo.fim)) {
      return;
    }

    if (intervalo.inicio && intervalo.fim && intervalo.inicio > intervalo.fim) {
      setErro("A data inicial não pode ser maior que a data final.");
      return;
    }

    setCarregando(true);
    setErro("");

    const paramsDashboard = new URLSearchParams();
    if (intervalo.inicio) paramsDashboard.set("inicio", intervalo.inicio);
    if (intervalo.fim) paramsDashboard.set("fim", intervalo.fim);
    const queryDashboard = paramsDashboard.toString();
    const rotaDashboard = `/api/dashboard/${tipo}${queryDashboard ? `?${queryDashboard}` : ""}`;

    const paramsLancamentos = new URLSearchParams();
    if (tipo === "pessoal") {
      if (intervalo.inicio) paramsLancamentos.set("inicio", intervalo.inicio);
      if (intervalo.fim) paramsLancamentos.set("fim", intervalo.fim);
    } else {
      if (intervalo.inicio) paramsLancamentos.set("dataInicial", intervalo.inicio);
      if (intervalo.fim) paramsLancamentos.set("dataFinal", intervalo.fim);
    }
    const queryLancamentos = paramsLancamentos.toString();
    const rotaLancamentos = tipo === "pessoal"
      ? `/api/pessoal/lancamentos${queryLancamentos ? `?${queryLancamentos}` : ""}`
      : `/api/empresarial/lancamentos${queryLancamentos ? `?${queryLancamentos}` : ""}`;

    Promise.all([
      requisicao<Resumo>(rotaDashboard, {}, sessao.token),
      requisicao<(LancamentoPessoal | LancamentoEmpresarial)[]>(rotaLancamentos, {}, sessao.token),
    ])
      .then(([dadosResumo, dadosLancamentos]) => {
        setResumo(dadosResumo);
        setLancamentos(dadosLancamentos);
      })
      .catch((falha) => {
        setErro(falha instanceof Error ? falha.message : "Não foi possível carregar os dados do relatório.");
      })
      .finally(() => {
        setCarregando(false);
      });
  }, [sessao, tipo, periodo, dataInicial, dataFinal]);

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

  const intervaloAtual = obterIntervaloPeriodo(periodo, dataInicial, dataFinal);
  const textoPeriodoAtual = rotuloPeriodo(periodo, intervaloAtual.inicio, intervaloAtual.fim);

  const exportarCsv = () => {
    const conteudo = [
      `Relatório financeiro - OnKash Finance`,
      `Período: ${textoPeriodoAtual}`,
      `Emitido em: ${new Intl.DateTimeFormat("pt-BR").format(new Date())}`,
      "",
      "Data;Descrição;Categoria;Conta;Tipo;Valor;Status",
      ...linhas.map((l) => l.map((v) => `"${String(v).replaceAll('"', '""')}"`).join(";")),
    ].join("\n");

    const url = URL.createObjectURL(
      new Blob(["\ufeff", conteudo], { type: "text/csv;charset=utf-8" }),
    );
    const link = document.createElement("a");
    link.href = url;
    link.download = `relatorio-onkash-${periodo.toLowerCase().replace(/_/g, "-")}.csv`;
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
    pdf.text(`Período: ${textoPeriodoAtual}`, 14, 34);
    pdf.text(`Emitido em ${new Intl.DateTimeFormat("pt-BR").format(new Date())}`, 14, 40);

    autoTable(pdf, {
      startY: 46,
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
        startY: 74,
        head: [["Data", "Descrição", "Categoria", "Conta", "Tipo", "Valor", "Status"]],
        body: linhas,
        styles: { fontSize: 7 },
        headStyles: { fillColor: [17, 106, 113] },
      });

    pdf.save(`relatorio-onkash-${periodo.toLowerCase().replace(/_/g, "-")}.pdf`);
  };

  return (
    <section className="relatorio">
      <header className="cabecalho cabecalho-relatorio">
        <div>
          <p className="sobre-titulo">Relatórios</p>
          <h1>Visão financeira detalhada</h1>
          <p>Exportação e análise · {textoPeriodoAtual}</p>
        </div>

        <div className="acoes-relatorio">
          <div className="periodo-visual" title="Filtrar período do relatório">
            <CalendarClock size={18} />
            <select
              value={periodo}
              onChange={(e) => alterarPeriodo(e.target.value as TipoPeriodoDashboard)}
              aria-label="Selecionar período do relatório"
            >
              <option value="MES_ATUAL">Este mês</option>
              <option value="MES_ANTERIOR">Mês anterior</option>
              <option value="ULTIMOS_7_DIAS">Últimos 7 dias</option>
              <option value="ANO_ATUAL">Este ano</option>
              <option value="TODO">Todo o período</option>
              <option value="PERSONALIZADO">Personalizado</option>
            </select>
          </div>

          <button
            className="botao secundario"
            onClick={exportarCsv}
            disabled={!lancamentos.length || carregando}
            title={!lancamentos.length ? "Adicione lançamentos para exportar" : undefined}
          >
            <FileDown size={18} /> CSV
          </button>

          <button
            className="botao"
            onClick={exportarPdf}
            disabled={!lancamentos.length || carregando}
            title={!lancamentos.length ? "Adicione lançamentos para exportar" : undefined}
          >
            <FileDown size={18} /> Exportar PDF
          </button>
        </div>
      </header>

      {periodo === "PERSONALIZADO" && (
        <section
          className="filtro-periodo-personalizado"
          aria-label="Período personalizado"
        >
          <div className="filtro-periodo-titulo">
            <CalendarClock size={16} />
            <span>Definir intervalo:</span>
          </div>

          <label className="campo">
            <span>Data inicial</span>
            <input
              type="date"
              value={dataInicial}
              onChange={(evento) => setDataInicial(evento.target.value)}
            />
          </label>

          <label className="campo">
            <span>Data final</span>
            <input
              type="date"
              value={dataFinal}
              onChange={(evento) => setDataFinal(evento.target.value)}
            />
          </label>
        </section>
      )}

      {erro ? (
        <p className="mensagem erro">{erro}</p>
      ) : carregando && !resumo ? (
        <p className="estado">Carregando relatório financeiro...</p>
      ) : !resumo ? (
        <p className="estado">Nenhum dado encontrado para o período selecionado.</p>
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
              <h2>Nenhum lançamento no período</h2>
              <p>Nenhuma movimentação registrada em {textoPeriodoAtual.toLowerCase()}. Altere o filtro acima para ver outros períodos.</p>
            </div>
          ) : (
            <section className="tabela relatorio-lista">
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
            </section>
          )}
        </>
      )}
    </section>
  );
}
