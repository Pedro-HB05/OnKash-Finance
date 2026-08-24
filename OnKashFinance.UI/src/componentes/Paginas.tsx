"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Badge, Cabecalho, Lista, Modal } from "@/componentes/Base";
import { FormularioBaixa, FormularioSimples } from "@/componentes/Formularios";
import { DashboardFinanceiro, type TipoPeriodoDashboard } from "@/componentes/DashboardFinanceiro";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type {
  Cartao,
  Categoria,
  Conta,
  ContaPagar,
  ContaReceber,
  DashboardEmpresarial,
  DashboardPessoal,
  Fatura,
  LancamentoPessoal,
  PessoaCadastro,
  UsuarioEmpresa,
} from "@/tipos/api";
import { data, moeda, textoEnum } from "@/utilitarios/formatadores";

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
      inicio: "",
      fim: "",
    };
  }

  if (periodo === "ULTIMOS_7_DIAS") {
    const inicio = new Date(hoje);
    inicio.setDate(hoje.getDate() - 6);

    return {
      inicio: formatarDataLocal(inicio),
      fim: formatarDataLocal(hoje),
    };
  }

  if (periodo === "MES_ATUAL") {
    const inicio = new Date(
      hoje.getFullYear(),
      hoje.getMonth(),
      1,
    );

    const fim = new Date(
      hoje.getFullYear(),
      hoje.getMonth() + 1,
      0,
    );

    return {
      inicio: formatarDataLocal(inicio),
      fim: formatarDataLocal(fim),
    };
  }

  if (periodo === "MES_ANTERIOR") {
    const inicio = new Date(
      hoje.getFullYear(),
      hoje.getMonth() - 1,
      1,
    );

    const fim = new Date(
      hoje.getFullYear(),
      hoje.getMonth(),
      0,
    );

    return {
      inicio: formatarDataLocal(inicio),
      fim: formatarDataLocal(fim),
    };
  }

  if (periodo === "ANO_ATUAL") {
    return {
      inicio: `${hoje.getFullYear()}-01-01`,
      fim: `${hoje.getFullYear()}-12-31`,
    };
  }

  return {
    inicio: dataInicial,
    fim: dataFinal,
  };
}

export function PaginaDashboard({
  tipo,
}: {
  tipo: "pessoal" | "empresarial";
}) {
  const { sessao } = useAutenticacao();

  const [dashboard, setDashboard] = useState<
    DashboardPessoal | DashboardEmpresarial | null
  >(null);

  const [periodo, setPeriodo] =
    useState<TipoPeriodoDashboard>("MES_ATUAL");

  const [dataInicial, setDataInicial] = useState("");

  const [dataFinal, setDataFinal] = useState("");

  const [erro, setErro] = useState("");

  useEffect(() => {
    if (!sessao) {
      return;
    }

    const intervalo = obterIntervaloPeriodo(
      periodo,
      dataInicial,
      dataFinal,
    );

    if (
      periodo === "PERSONALIZADO" &&
      (!intervalo.inicio || !intervalo.fim)
    ) {
      return;
    }

    if (
      intervalo.inicio &&
      intervalo.fim &&
      intervalo.inicio > intervalo.fim
    ) {
      setErro(
        "A data inicial não pode ser maior que a data final.",
      );
      return;
    }

    const carregarDashboard = async () => {
      try {
        setErro("");

        const parametros = new URLSearchParams();

        if (intervalo.inicio) {
          parametros.set(
            "inicio",
            intervalo.inicio,
          );
        }

        if (intervalo.fim) {
          parametros.set(
            "fim",
            intervalo.fim,
          );
        }

        const query =
          parametros.toString();

        const rota =
          `/api/dashboard/${tipo}` +
          (query ? `?${query}` : "");

        const resultado =
          await requisicao<
            DashboardPessoal | DashboardEmpresarial
          >(
            rota,
            {},
            sessao.token,
          );

        setDashboard(resultado);
      } catch (falha) {
        setErro(
          falha instanceof Error
            ? falha.message
            : "Não foi possível carregar o dashboard.",
        );
      }
    };

    void carregarDashboard();
  }, [
    sessao,
    tipo,
    periodo,
    dataInicial,
    dataFinal,
  ]);

  const alterarPeriodo = (
    novoPeriodo: TipoPeriodoDashboard,
  ) => {
    setPeriodo(novoPeriodo);

    if (
      novoPeriodo !== "PERSONALIZADO"
    ) {
      setDataInicial("");
      setDataFinal("");
    }
  };

  return (
    <AreaAutenticada tipo={tipo}>
      {erro ? (
        <p className="mensagem erro">
          {erro}
        </p>
      ) : !dashboard ? (
        <p className="estado">
          Carregando seu resumo financeiro...
        </p>
      ) : (
        <DashboardFinanceiro
          tipo={tipo}
          dados={dashboard}
          periodo={periodo}
          dataInicial={dataInicial}
          dataFinal={dataFinal}
          alterarPeriodo={alterarPeriodo}
          alterarDataInicial={setDataInicial}
          alterarDataFinal={setDataFinal}
        />
      )}
    </AreaAutenticada>
  );
}

export function PaginaContas({
  tipo,
}: {
  tipo: "pessoal" | "empresarial";
}) {
  const rota =
    `/api/${tipo}/contas`;

  return (
    <AreaAutenticada tipo={tipo}>
      <Lista<Conta>
        titulo="Contas"
        descricao="Veja os saldos de cada conta cadastrada."
        rota={rota}
        colunas={[
          {
            titulo: "Nome",
            valor: (i) => i.nome,
          },
          {
            titulo: "Tipo",
            valor: (i) => i.tipo,
          },
          {
            titulo: "Saldo atual",
            valor: (i) =>
              moeda(i.saldoAtual),
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge valor={i.ativo} />
            ),
          },
        ]}
        renderFormulario={(c) => (
          <FormularioSimples
            tipo="conta"
            rota={rota}
            concluir={c}
          />
        )}
      />
    </AreaAutenticada>
  );
}

export function PaginaCategorias({
  tipo,
}: {
  tipo: "pessoal" | "empresarial";
}) {
  const rota =
    `/api/${tipo}/categorias`;

  return (
    <AreaAutenticada tipo={tipo}>
      <Lista<Categoria>
        titulo="Categorias"
        descricao="Organize entradas e saídas por categoria."
        rota={rota}
        colunas={[
          {
            titulo: "Nome",
            valor: (i) => i.nome,
          },
          {
            titulo: "Tipo",
            valor: (i) =>
              textoEnum(i.tipo),
          },
          {
            titulo: "Categoria padrão",
            valor: (i) =>
              i.padrao ? "Sim" : "Não",
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge valor={i.ativo} />
            ),
          },
        ]}
        renderFormulario={(c) => (
          <FormularioSimples
            tipo="categoria"
            rota={rota}
            concluir={c}
          />
        )}
      />
    </AreaAutenticada>
  );
}

export function PaginaPessoas({
  tipo,
}: {
  tipo: "clientes" | "fornecedores";
}) {
  const rota =
    `/api/empresarial/${tipo}`;

  const singular =
    tipo === "clientes"
      ? "cliente"
      : "fornecedor";

  return (
    <AreaAutenticada tipo="empresarial">
      <Lista<PessoaCadastro>
        titulo={
          tipo === "clientes"
            ? "Clientes"
            : "Fornecedores"
        }
        descricao={`Cadastre e acompanhe seus ${tipo}.`}
        rota={rota}
        colunas={[
          {
            titulo: "Nome / Razão social",
            valor: (i) =>
              i.nomeRazaoSocial,
          },
          {
            titulo: "CPF/CNPJ",
            valor: (i) =>
              i.cpfCnpj ?? "—",
          },
          {
            titulo: "Telefone",
            valor: (i) =>
              i.telefone ?? "—",
          },
          {
            titulo: "E-mail",
            valor: (i) =>
              i.email ?? "—",
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge valor={i.ativo} />
            ),
          },
        ]}
        renderFormulario={(c) => (
          <FormularioSimples
            tipo={singular}
            rota={rota}
            concluir={c}
          />
        )}
      />
    </AreaAutenticada>
  );
}

export function PaginaLancamentosPessoais() {
  return (
    <AreaAutenticada tipo="pessoal">
      <Lista<LancamentoPessoal>
        titulo="Lançamentos"
        descricao="Consulte suas entradas e saídas."
        rota="/api/pessoal/lancamentos"
        colunas={[
          {
            titulo: "Data",
            valor: (i) =>
              data(i.data),
          },
          {
            titulo: "Descrição",
            valor: (i) =>
              i.descricao,
          },
          {
            titulo: "Tipo",
            valor: (i) =>
              textoEnum(i.tipo),
          },
          {
            titulo: "Conta",
            valor: (i) =>
              i.conta,
          },
          {
            titulo: "Valor",
            valor: (i) =>
              moeda(i.valor),
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge
                valor={
                  i.cancelado
                    ? "CANCELADO"
                    : "Ativo"
                }
              />
            ),
          },
        ]}
        renderFormulario={() => (
          <p>
            O formulário de inclusão
            utiliza contas e categorias
            cadastradas. Este fluxo pode
            ser concluído pelo backend
            atual, mas a seleção assistida
            será adicionada junto ao
            endpoint de pesquisa de dados.
          </p>
        )}
      />
    </AreaAutenticada>
  );
}

export function PaginaCartoes() {
  return (
    <AreaAutenticada tipo="pessoal">
      <Lista<Cartao>
        titulo="Cartões"
        descricao="Acompanhe seus cartões e seus limites."
        rota="/api/pessoal/cartoes"
        colunas={[
          {
            titulo: "Nome",
            valor: (i) =>
              i.nome,
          },
          {
            titulo: "Instituição",
            valor: (i) =>
              i.instituicao,
          },
          {
            titulo: "Limite",
            valor: (i) =>
              moeda(i.limite),
          },
          {
            titulo: "Fechamento",
            valor: (i) =>
              `Dia ${i.diaFechamento}`,
          },
          {
            titulo: "Vencimento",
            valor: (i) =>
              `Dia ${i.diaVencimento}`,
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge
                valor={i.ativo}
              />
            ),
          },
        ]}
        renderFormulario={(c) => (
          <FormularioSimples
            tipo="cartao"
            rota="/api/pessoal/cartoes"
            concluir={c}
          />
        )}
      />
    </AreaAutenticada>
  );
}

export function PaginaFaturas() {
  const { sessao } =
    useAutenticacao();

  const [itens, setItens] =
    useState<Fatura[]>([]);

  const [erro, setErro] =
    useState("");

  const [selecionada, setSelecionada] =
    useState<Fatura | null>(null);

  const carregar = () => {
    if (sessao) {
      requisicao<Fatura[]>(
        "/api/pessoal/cartoes/faturas",
        {},
        sessao.token,
      )
        .then(setItens)
        .catch((e: Error) =>
          setErro(e.message),
        );
    }
  };

  useEffect(
    carregar,
    [sessao],
  );

  return (
    <AreaAutenticada tipo="pessoal">
      <Cabecalho
        titulo="Faturas"
        descricao="Consulte e pague suas faturas."
      />

      {erro ? (
        <p className="mensagem erro">
          {erro}
        </p>
      ) : (
        itens.length === 0 ? (
          <div className="estado-vazio">
            <h2>Nenhuma fatura disponível</h2>
            <p>Cadastre um cartão e registre compras para acompanhar vencimentos e pagamentos.</p>
            <Link className="botao" href="/pessoal/cartoes">Ir para cartões</Link>
          </div>
        ) : <div className="tabela">
          <table>
            <thead>
              <tr>
                <th>Cartão</th>
                <th>Competência</th>
                <th>Vencimento</th>
                <th>Valor</th>
                <th>Status</th>
                <th>Ação</th>
              </tr>
            </thead>

            <tbody>
              {itens.map((i) => (
                <tr key={i.id}>
                  <td data-label="Cartão">
                    {i.cartao}
                  </td>

                  <td data-label="Competência">
                    {data(i.competencia)}
                  </td>

                  <td data-label="Vencimento">
                    {data(i.dataVencimento)}
                  </td>

                  <td data-label="Valor">
                    {moeda(i.valorTotal)}
                  </td>

                  <td data-label="Status">
                    <Badge
                      valor={i.status}
                    />
                  </td>

                  <td data-label="Ação">
                    {i.status === "PAGA" ? (
                      "—"
                    ) : (
                      <button
                        className="botao secundario"
                        onClick={() =>
                          setSelecionada(i)
                        }
                      >
                        Pagar fatura
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selecionada && (
        <Modal
          titulo="Pagar fatura"
          fechar={() =>
            setSelecionada(null)
          }
        >
          <FormularioBaixa
            rota={`/api/pessoal/cartoes/faturas/${selecionada.id}/pagar`}
            receber={false}
            concluir={() => {
              setSelecionada(null);
              carregar();
            }}
          />
        </Modal>
      )}
    </AreaAutenticada>
  );
}

function PaginaFinanceira({
  receber,
}: {
  receber: boolean;
}) {
  const { sessao } =
    useAutenticacao();

  const rota = receber
    ? "/api/empresarial/contas-receber"
    : "/api/empresarial/contas-pagar";

  const [titulo] = [
    receber
      ? "Contas a receber"
      : "Contas a pagar",
  ];

  const [itens, setItens] =
    useState<
      (ContaReceber | ContaPagar)[]
    >([]);

  const [erro, setErro] =
    useState("");

  const [selecionada, setSelecionada] =
    useState<
      (ContaReceber | ContaPagar) | null
    >(null);

  const carregar = () => {
    if (sessao) {
      requisicao<
        (ContaReceber | ContaPagar)[]
      >(
        rota,
        {},
        sessao.token,
      )
        .then(setItens)
        .catch((e: Error) =>
          setErro(e.message),
        );
    }
  };

  useEffect(
    carregar,
    [sessao],
  );

  return (
    <AreaAutenticada tipo="empresarial">
      <Cabecalho
        titulo={titulo}
        descricao={
          receber
            ? "Controle os valores que sua empresa receberá."
            : "Controle os pagamentos da sua empresa."
        }
      />

      {erro ? (
        <p className="mensagem erro">
          {erro}
        </p>
      ) : (
        <div className="tabela">
          <table>
            <thead>
              <tr>
                <th>
                  {receber
                    ? "Cliente"
                    : "Fornecedor"}
                </th>

                <th>Descrição</th>
                <th>Valor</th>
                <th>Vencimento</th>
                <th>Status</th>
                <th>Ação</th>
              </tr>
            </thead>

            <tbody>
              {itens.map((i) => (
                <tr key={i.id}>
                  <td
                    data-label={
                      receber
                        ? "Cliente"
                        : "Fornecedor"
                    }
                  >
                    {receber
                      ? (
                          i as ContaReceber
                        ).cliente ??
                        "—"
                      : (
                          i as ContaPagar
                        ).fornecedor ??
                        "—"}
                  </td>

                  <td data-label="Descrição">
                    {i.descricao}
                  </td>

                  <td data-label="Valor">
                    {moeda(i.valor)}
                  </td>

                  <td data-label="Vencimento">
                    {data(i.vencimento)}
                  </td>

                  <td data-label="Status">
                    <Badge
                      valor={i.status}
                    />
                  </td>

                  <td data-label="Ação">
                    {i.status ===
                      "PENDENTE" ||
                    i.status ===
                      "ATRASADO" ? (
                      <button
                        className="botao secundario"
                        onClick={() =>
                          setSelecionada(i)
                        }
                      >
                        {receber
                          ? "Marcar como recebida"
                          : "Marcar como paga"}
                      </button>
                    ) : (
                      "—"
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selecionada && (
        <Modal
          titulo={
            receber
              ? "Registrar recebimento"
              : "Registrar pagamento"
          }
          fechar={() =>
            setSelecionada(null)
          }
        >
          <FormularioBaixa
            rota={`${rota}/${selecionada.id}/${receber ? "receber" : "pagar"}`}
            receber={receber}
            concluir={() => {
              setSelecionada(null);
              carregar();
            }}
          />
        </Modal>
      )}
    </AreaAutenticada>
  );
}

export const PaginaContasReceber =
  () => (
    <PaginaFinanceira receber />
  );

export const PaginaContasPagar =
  () => (
    <PaginaFinanceira
      receber={false}
    />
  );

export function PaginaIndisponivel({
  tipo,
  titulo,
}: {
  tipo: "pessoal" | "empresarial";
  titulo: string;
}) {
  return (
    <AreaAutenticada tipo={tipo}>
      <Cabecalho
        titulo={titulo}
        descricao="Este espaço está preparado para este recurso."
      />

      <section className="estado">
        <h2>
          Informações indisponíveis
          no momento
        </h2>

        <p>
          Não há dados disponíveis
          para esta consulta.
        </p>
      </section>
    </AreaAutenticada>
  );
}

export function PaginaUsuarios() {
  return (
    <AreaAutenticada tipo="empresarial">
      <Lista<UsuarioEmpresa>
        titulo="Usuários"
        descricao="Gerencie as pessoas que têm acesso à empresa."
        rota="/api/empresarial/usuarios"
        colunas={[
          {
            titulo: "Nome",
            valor: (i) => i.nome,
          },
          {
            titulo: "E-mail",
            valor: (i) => i.email,
          },
          {
            titulo: "Perfil",
            valor: (i) =>
              textoEnum(i.perfil),
          },
          {
            titulo: "Situação",
            valor: (i) => (
              <Badge
                valor={i.ativo}
              />
            ),
          },
        ]}
        renderFormulario={() => (
          <p>
            O backend permite adicionar
            usuários por identificador.
            Para evitar exibir
            identificadores técnicos ao
            usuário, este cadastro aguarda
            um endpoint de busca de
            usuários por e-mail.
          </p>
        )}
      />
    </AreaAutenticada>
  );
}
