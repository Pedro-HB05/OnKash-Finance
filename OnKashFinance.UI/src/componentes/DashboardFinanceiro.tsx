"use client";

import Link from "next/link";

import {
  ArrowDownRight,
  ArrowUpRight,
  CalendarClock,
  CircleDollarSign,
  Landmark,
  TrendingUp,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import type {
  DashboardEmpresarial,
  DashboardPessoal,
} from "@/tipos/api";

import { moeda } from "@/utilitarios/formatadores";

type Dados =
  | DashboardPessoal
  | DashboardEmpresarial;

export type TipoPeriodoDashboard =
  | "TODO"
  | "MES_ATUAL"
  | "MES_ANTERIOR"
  | "ULTIMOS_7_DIAS"
  | "ANO_ATUAL"
  | "PERSONALIZADO";

type DashboardFinanceiroProps = {
  tipo: "pessoal" | "empresarial";

  dados: Dados;

  periodo: TipoPeriodoDashboard;

  dataInicial: string;

  dataFinal: string;

  alterarPeriodo: (
    periodo: TipoPeriodoDashboard,
  ) => void;

  alterarDataInicial: (
    valor: string,
  ) => void;

  alterarDataFinal: (
    valor: string,
  ) => void;
};

function isDashboardEmpresarial(
  dados: Dados,
  tipo: "pessoal" | "empresarial",
): dados is DashboardEmpresarial {
  return (
    tipo === "empresarial" &&
    "resultado" in dados
  );
}

function valorNumerico(
  valor: unknown,
): number {
  const numero = Number(valor);

  return Number.isFinite(numero)
    ? numero
    : 0;
}

function nomePeriodo(
  periodo: TipoPeriodoDashboard,
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
      return "Período personalizado";

    default:
      return "Todo o período";
  }
}

function comparacao(atual: number, anterior: number) {
  if (anterior === 0) return atual === 0 ? "Sem mudança no período" : "Primeiro valor registrado";
  const percentual = ((atual - anterior) / Math.abs(anterior)) * 100;
  return `${percentual >= 0 ? "+" : ""}${percentual.toFixed(1).replace(".", ",")}% vs. período anterior`;
}

export function DashboardFinanceiro({
  tipo,
  dados,
  periodo,
  dataInicial,
  dataFinal,
  alterarPeriodo,
  alterarDataInicial,
  alterarDataFinal,
}: DashboardFinanceiroProps) {
  const empresarial =
    isDashboardEmpresarial(
      dados,
      tipo,
    );

  const saldo =
    valorNumerico(dados.saldo);

  const entradas =
    valorNumerico(dados.entradas);

  const saidas =
    valorNumerico(dados.saidas);

  const resultado =
    valorNumerico(
      empresarial
        ? dados.resultado
        : dados.resultadoMes,
    );

  const barras = [
    {
      nome: nomePeriodo(periodo),
      entradas,
      saidas,
    },
  ];

  const semMovimento = entradas === 0 && saidas === 0;

  return (
    <section className="painel-dashboard">
      <header className="hero-financeiro">
        <div>
          <p className="sobre-titulo">
            Visão financeira
          </p>

          <h1>
            Seu dinheiro, com clareza.
          </h1>

          <p>
            Acompanhe os principais
            números dos seus lançamentos.
          </p>
        </div>

        <div className="periodo-visual">
          <CalendarClock size={18} />

          <select
            value={periodo}
            onChange={(evento) =>
              alterarPeriodo(
                evento.target
                  .value as TipoPeriodoDashboard,
              )
            }
            aria-label="Selecionar período"
          >
            <option value="TODO">
              Todo o período
            </option>

            <option value="MES_ATUAL">
              Este mês
            </option>

            <option value="MES_ANTERIOR">
              Mês anterior
            </option>

            <option value="ULTIMOS_7_DIAS">
              Últimos 7 dias
            </option>

            <option value="ANO_ATUAL">
              Este ano
            </option>

            <option value="PERSONALIZADO">
              Personalizado
            </option>
          </select>
        </div>
      </header>

      {periodo === "PERSONALIZADO" && (
        <section
          className="filtro-periodo-personalizado"
          aria-label="Período personalizado"
        >
          <label className="campo">
            Data inicial

            <input
              type="date"
              value={dataInicial}
              onChange={(evento) =>
                alterarDataInicial(
                  evento.target.value,
                )
              }
            />
          </label>

          <label className="campo">
            Data final

            <input
              type="date"
              value={dataFinal}
              onChange={(evento) =>
                alterarDataFinal(
                  evento.target.value,
                )
              }
            />
          </label>
        </section>
      )}

      <section
        className="saldo-principal"
        aria-label="Resumo de saldo"
      >
        <div>
          <span className="rotulo-card">
            {periodo === "TODO"
              ? "Saldo atual"
              : "Saldo no fim do período"}
          </span>

          <strong>
            {moeda(saldo)}
          </strong>

          <p>
            <TrendingUp size={17} />

            {periodo === "TODO"
              ? "Valor disponível nas suas contas"
              : "Saldo calculado até a data final"}
          </p>
        </div>

        <div className="selo-saldo">
          <Landmark size={30} />

          <span>
            OnKash
            <br />
            Finance
          </span>
        </div>
      </section>

      <section
        className="indicadores-financeiros"
        aria-label="Indicadores financeiros"
      >
        <article className="indicador receita">
          <span className="icone-indicador">
            <ArrowUpRight size={20} />
          </span>

          <div>
            <span>Entradas</span>

            <strong>
              {moeda(entradas)}
            </strong>

            <small>
              {comparacao(entradas, valorNumerico(dados.entradasAnteriores))}
            </small>
          </div>
        </article>

        <article className="indicador despesa">
          <span className="icone-indicador">
            <ArrowDownRight size={20} />
          </span>

          <div>
            <span>Saídas</span>

            <strong>
              {moeda(saidas)}
            </strong>

            <small>
              {comparacao(saidas, valorNumerico(dados.saidasAnteriores))}
            </small>
          </div>
        </article>

        <article className="indicador resultado">
          <span className="icone-indicador">
            <CircleDollarSign size={20} />
          </span>

          <div>
            <span>
              Sobra do período
            </span>

            <strong>
              {resultado > 0
                ? "+ "
                : ""}

              {moeda(resultado)}
            </strong>

            <small>
              {comparacao(resultado, valorNumerico(dados.resultadoAnterior))}
            </small>
          </div>
        </article>
      </section>

      <section className="grade-analise">
        <article
          className="painel-grafico"
          style={{
            gridColumn: "1 / -1",
          }}
        >
          <div className="titulo-painel">
            <div>
              <span className="sobre-titulo">
                {nomePeriodo(periodo)}
              </span>

              <h2>
                Entradas e saídas
              </h2>
            </div>

            <span className="legenda-grafico">
              <i className="legenda entrada" />
              Entradas

              <i className="legenda saida" />
              Saídas
            </span>
          </div>

          {semMovimento ? (
            <div className="estado-vazio estado-vazio-grafico">
              <span className="icone-estado-vazio"><TrendingUp size={25} /></span>
              <h3>Ainda não há movimentações neste período</h3>
              <p>Registre uma entrada ou saída para acompanhar a evolução das suas finanças aqui.</p>
              <Link className="botao" href={`/${tipo}/lancamentos`}>
                Ir para lançamentos
              </Link>
            </div>
          ) : <div className="grafico">
            <ResponsiveContainer
              width="100%"
              height="100%"
            >
              <BarChart
                data={barras}
                barGap={8}
              >
                <CartesianGrid
                  vertical={false}
                  stroke="var(--borda)"
                  strokeDasharray="4 4"
                />

                <XAxis
                  dataKey="nome"
                  axisLine={false}
                  tickLine={false}
                  tick={{
                    fill: "var(--texto2)",
                    fontSize: 13,
                  }}
                />

                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tickFormatter={(valor) =>
                    moeda(Number(valor))
                  }
                  tick={{
                    fill: "var(--texto2)",
                    fontSize: 12,
                  }}
                />

                <Tooltip
                  cursor={{
                    fill: "var(--suave)",
                  }}
                  formatter={(valor) =>
                    moeda(
                      valorNumerico(
                        valor,
                      ),
                    )
                  }
                  contentStyle={{
                    background:
                      "var(--superficie)",
                    border:
                      "1px solid var(--borda)",
                    borderRadius: 12,
                    color:
                      "var(--texto)",
                  }}
                />

                <Bar
                  dataKey="entradas"
                  name="Entradas"
                  fill="var(--receita)"
                  radius={[
                    7,
                    7,
                    0,
                    0,
                  ]}
                  maxBarSize={62}
                />

                <Bar
                  dataKey="saidas"
                  name="Saídas"
                  fill="var(--despesa)"
                  radius={[
                    7,
                    7,
                    0,
                    0,
                  ]}
                  maxBarSize={62}
                />
              </BarChart>
            </ResponsiveContainer>
          </div>}
        </article>
      </section>
    </section>
  );
}
