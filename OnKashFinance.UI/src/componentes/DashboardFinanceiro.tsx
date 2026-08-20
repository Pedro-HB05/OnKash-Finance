"use client";
import {
  ArrowDownRight,
  ArrowUpRight,
  CalendarClock,
  CircleDollarSign,
  Landmark,
  TrendingUp,
  WalletCards,
} from "lucide-react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import type { DashboardEmpresarial, DashboardPessoal } from "@/tipos/api";
import { moeda } from "@/utilitarios/formatadores";

type Dados = DashboardPessoal | DashboardEmpresarial;
const empresariais = (
  dados: Dados,
  tipo: "pessoal" | "empresarial",
): dados is DashboardEmpresarial => tipo === "empresarial";

export function DashboardFinanceiro({
  tipo,
  dados,
}: {
  tipo: "pessoal" | "empresarial";
  dados: Dados;
}) {
  const empresarial = empresariais(dados, tipo);
  const resultado = empresarial ? dados.resultado : dados.resultadoMes;
  const barras = [
    {
      nome: empresarial ? "Fluxo atual" : "Mês atual",
      entradas: dados.entradas,
      saidas: dados.saidas,
    },
  ];
  return (
    <section className="painel-dashboard">
      <header className="hero-financeiro">
        <div>
          <p className="sobre-titulo">Visão financeira</p>
          <h1>Seu dinheiro, com clareza.</h1>
          <p>Acompanhe os principais números do período atual.</p>
        </div>
        <div className="periodo-visual">
          <CalendarClock size={18} />
          <span>Período atual</span>
        </div>
      </header>
      <section className="saldo-principal" aria-label="Resumo de saldo">
        <div>
          <span className="rotulo-card">Saldo total</span>
          <strong>{moeda(dados.saldo)}</strong>
          <p>
            <TrendingUp size={17} /> Valor disponível nas suas contas
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
      <section className="indicadores-financeiros" aria-label="Indicadores financeiros">
        <article className="indicador receita">
          <span className="icone-indicador">
            <ArrowUpRight size={20} />
          </span>
          <div>
            <span>Entradas</span>
            <strong>{moeda(dados.entradas)}</strong>
            <small>Valores que entraram</small>
          </div>
        </article>
        <article className="indicador despesa">
          <span className="icone-indicador">
            <ArrowDownRight size={20} />
          </span>
          <div>
            <span>Saídas</span>
            <strong>{moeda(dados.saidas)}</strong>
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
              {resultado >= 0 ? "+ " : ""}
              {moeda(resultado)}
            </strong>
            <small>Resultado do período</small>
          </div>
        </article>
      </section>
      <section className="grade-analise">
        <article className="painel-grafico">
          <div className="titulo-painel">
            <div>
              <span className="sobre-titulo">Resumo</span>
              <h2>Entradas e saídas</h2>
            </div>
            <span className="legenda-grafico">
              <i className="legenda entrada" />
              Entradas <i className="legenda saida" />
              Saídas
            </span>
          </div>
          <div className="grafico">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={barras} barGap={8}>
                <CartesianGrid vertical={false} stroke="var(--borda)" strokeDasharray="4 4" />
                <XAxis
                  dataKey="nome"
                  axisLine={false}
                  tickLine={false}
                  tick={{ fill: "var(--texto2)", fontSize: 13 }}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  tickFormatter={(valor) => `R$ ${Number(valor) / 1000}k`}
                  tick={{ fill: "var(--texto2)", fontSize: 12 }}
                />
                <Tooltip
                  cursor={{ fill: "var(--suave)" }}
                  formatter={(valor) => moeda(Number(valor ?? 0))}
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
        <article className="painel-apoio">
          <div className="titulo-painel">
            <div>
              <span className="sobre-titulo">Planejamento</span>
              <h2>{empresarial ? "Compromissos" : "Análise de gastos"}</h2>
            </div>
          </div>
          {empresarial ? (
            <div className="lista-compromissos">
              <div>
                <span>A pagar</span>
                <strong>{moeda(dados.contasAPagar)}</strong>
              </div>
              <div>
                <span>A receber</span>
                <strong>{moeda(dados.contasAReceber)}</strong>
              </div>
              <div className="alerta-vencido">
                <span>Valores vencidos</span>
                <strong>{moeda(dados.valoresVencidos)}</strong>
              </div>
            </div>
          ) : (
            <div className="painel-sem-historico">
              <WalletCards size={28} />
              <strong>Detalhamento por categoria</strong>
              <p>Este gráfico aparecerá quando a API fornecer os gastos por categoria.</p>
            </div>
          )}
        </article>
      </section>
    </section>
  );
}
