"use client";

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

import type { DashboardEmpresarial, DashboardPessoal } from "@/tipos/api";
import { moeda } from "@/utilitarios/formatadores";

type Dados = DashboardPessoal | DashboardEmpresarial;

type DashboardFinanceiroProps = {
  tipo: "pessoal" | "empresarial";
  dados: Dados;
};

function isDashboardEmpresarial(
  dados: Dados,
  tipo: "pessoal" | "empresarial",
): dados is DashboardEmpresarial {
  return tipo === "empresarial" && "resultado" in dados;
}

function valorNumerico(valor: unknown): number {
  const numero = Number(valor);

  return Number.isFinite(numero) ? numero : 0;
}

export function DashboardFinanceiro({
  tipo,
  dados,
}: DashboardFinanceiroProps) {
  const empresarial = isDashboardEmpresarial(dados, tipo);

  const saldo = valorNumerico(dados.saldo);
  const entradas = valorNumerico(dados.entradas);
  const saidas = valorNumerico(dados.saidas);

  const resultado = valorNumerico(
    empresarial ? dados.resultado : dados.resultadoMes,
  );

  const barras = [
    {
      nome: empresarial ? "Fluxo atual" : "Mês atual",
      entradas,
      saidas,
    },
  ];

  return (
    <section className="painel-dashboard">
      <header className="hero-financeiro">
        <div>
          <p className="sobre-titulo">Visão financeira</p>
          <h1>Seu dinheiro, com clareza.</h1>
          <p>Acompanhe os principais números de todos os lançamentos.</p>
        </div>

        <div className="periodo-visual">
          <CalendarClock size={18} />
          <span>Todo o período</span>
        </div>
      </header>

      <section className="saldo-principal" aria-label="Resumo de saldo">
        <div>
          <span className="rotulo-card">Saldo total</span>

          <strong>{moeda(saldo)}</strong>

          <p>
            <TrendingUp size={17} />
            Valor disponível nas suas contas
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
            <strong>{moeda(entradas)}</strong>
            <small>Valores que entraram</small>
          </div>
        </article>

        <article className="indicador despesa">
          <span className="icone-indicador">
            <ArrowDownRight size={20} />
          </span>

          <div>
            <span>Saídas</span>
            <strong>{moeda(saidas)}</strong>
            <small>Valores que saíram</small>
          </div>
        </article>

        <article className="indicador resultado">
          <span className="icone-indicador">
            <CircleDollarSign size={20} />
          </span>

          <div>
            <span>Resultado</span>

            <strong>
              {resultado > 0 ? "+ " : ""}
              {moeda(resultado)}
            </strong>

            <small>Resultado do período</small>
          </div>
        </article>
      </section>

      <section className="grade-analise">
        <article
          className="painel-grafico"
          style={{ gridColumn: "1 / -1" }}
        >
          <div className="titulo-painel">
            <div>
              <span className="sobre-titulo">Resumo</span>
              <h2>Entradas e saídas</h2>
            </div>

            <span className="legenda-grafico">
              <i className="legenda entrada" />
              Entradas

              <i className="legenda saida" />
              Saídas
            </span>
          </div>

          <div className="grafico">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={barras} barGap={8}>
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
                  tickFormatter={(valor) => moeda(Number(valor))}
                  tick={{
                    fill: "var(--texto2)",
                    fontSize: 12,
                  }}
                />

                <Tooltip
                  cursor={{ fill: "var(--suave)" }}
                  formatter={(valor) => moeda(valorNumerico(valor))}
                  contentStyle={{
                    background: "var(--superficie)",
                    border: "1px solid var(--borda)",
                    borderRadius: 12,
                    color: "var(--texto)",
                  }}
                />

                <Bar
                  dataKey="entradas"
                  name="Entradas"
                  fill="var(--receita)"
                  radius={[7, 7, 0, 0]}
                  maxBarSize={62}
                />

                <Bar
                  dataKey="saidas"
                  name="Saídas"
                  fill="var(--despesa)"
                  radius={[7, 7, 0, 0]}
                  maxBarSize={62}
                />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </article>
      </section>
    </section>
  );
}