"use client";

import { useEffect, useState } from "react";
import { Search } from "lucide-react";

import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Badge, Campo, Modal } from "@/componentes/Base";
import { ConfirmacaoAcao, MenuAcoes } from "@/componentes/MenuAcoes";
import { FormularioSimples } from "@/componentes/Formularios";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type {
  Cartao,
  Categoria,
  Conta,
  LancamentoPessoal,
} from "@/tipos/api";
import { data, moeda, textoEnum } from "@/utilitarios/formatadores";

type CartaoComDatas = Omit<
  Cartao,
  "diaFechamento" | "diaVencimento"
> & {
  dataFechamento: string;
  dataVencimento: string;
};

type Cadastro = Conta | CartaoComDatas | Categoria;

type TipoCadastro =
  | "conta"
  | "cartao"
  | "categoria";

const configuracao = {
  conta: {
    rota: "/api/pessoal/contas",
    titulo: "Contas",
    singular: "conta",
    descricao: "Acompanhe onde seu dinheiro está e mantenha os saldos organizados.",
    vazio: "Cadastre sua primeira conta para começar a registrar entradas e saídas.",
    colunas: [
      "Nome",
      "Tipo",
      "Saldo atual",
      "Situação",
    ],
  },

  cartao: {
    rota: "/api/pessoal/cartoes",
    titulo: "Cartões",
    singular: "cartão",
    descricao: "Organize limites, vencimentos e compras dos seus cartões.",
    vazio: "Cadastre um cartão para acompanhar compras e faturas.",
    colunas: [
      "Nome",
      "Instituição",
      "Limite",
      "Fechamento",
      "Vencimento",
      "Situação",
    ],
  },

  categoria: {
    rota: "/api/pessoal/categorias",
    titulo: "Categorias",
    singular: "categoria",
    descricao: "Classifique suas entradas e saídas para entender melhor seus gastos.",
    vazio: "Crie categorias personalizadas para organizar seus lançamentos.",
    colunas: [
      "Nome",
      "Tipo",
      "Padrão",
      "Situação",
    ],
  },
} as const;

function mensagemErro(
  falha: unknown,
  mensagemPadrao: string,
) {
  return falha instanceof Error
    ? falha.message
    : mensagemPadrao;
}

function dataParaInput(valor: string | undefined) {
  if (!valor) {
    return "";
  }

  return valor.slice(0, 10);
}

function DadosCadastro({
  tipo,
  item,
  salvar,
}: {
  tipo: TipoCadastro;
  item: Cadastro;
  salvar: (
    dados: Record<string, unknown>,
  ) => Promise<void>;
}) {
  const [salvando, setSalvando] =
    useState(false);

  const enviar = async (
    evento: React.FormEvent<HTMLFormElement>,
  ) => {
    evento.preventDefault();

    const formulario = new FormData(
      evento.currentTarget,
    );

    setSalvando(true);

    try {
      const dados: Record<string, unknown> = {
        nome: formulario.get("nome"),
        ativo: item.ativo,
      };

      if (tipo === "conta") {
        dados.tipo = formulario.get("tipo");
      }

      if (tipo === "categoria") {
        dados.tipo = formulario.get("tipo");
      }

      if (tipo === "cartao") {
        dados.instituicao =
          formulario.get("instituicao");

        dados.limite = Number(
          formulario.get("limite"),
        );

        dados.dataFechamento =
          formulario.get("dataFechamento");

        dados.dataVencimento =
          formulario.get("dataVencimento");
      }

      await salvar(dados);
    } finally {
      setSalvando(false);
    }
  };

  return (
    <form
      className="formulario"
      onSubmit={enviar}
    >
      <Campo
        label="Nome"
        name="nome"
        defaultValue={item.nome}
        required
      />

      {tipo === "conta" && (
        <Campo
          label="Tipo da conta"
          name="tipo"
          defaultValue={(item as Conta).tipo}
          required
        />
      )}

      {tipo === "categoria" && (
        <label className="campo">
          Tipo

          <select
            name="tipo"
            defaultValue={
              (item as Categoria).tipo
            }
          >
            <option value="ENTRADA">
              Entrada
            </option>

            <option value="SAIDA">
              Saída
            </option>
          </select>
        </label>
      )}

      {tipo === "cartao" && (
        <>
          <Campo
            label="Instituição"
            name="instituicao"
            defaultValue={
              (item as CartaoComDatas)
                .instituicao
            }
            required
          />

          <Campo
            label="Limite"
            name="limite"
            type="number"
            min="0"
            step="0.01"
            defaultValue={
              (item as CartaoComDatas).limite
            }
            required
          />

          <Campo
            label="Data de fechamento"
            name="dataFechamento"
            type="date"
            defaultValue={dataParaInput(
              (item as CartaoComDatas)
                .dataFechamento,
            )}
            required
          />

          <Campo
            label="Data de vencimento"
            name="dataVencimento"
            type="date"
            defaultValue={dataParaInput(
              (item as CartaoComDatas)
                .dataVencimento,
            )}
            required
          />
        </>
      )}

      <button
        className="botao"
        disabled={salvando}
      >
        {salvando
          ? "Salvando..."
          : "Salvar alterações"}
      </button>
    </form>
  );
}

function FormularioNovoCartao({
  rota,
  token,
  concluir,
  falhar,
}: {
  rota: string;
  token: string;
  concluir: () => void;
  falhar: (mensagem: string) => void;
}) {
  const [salvando, setSalvando] =
    useState(false);

  const enviar = async (
    evento: React.FormEvent<HTMLFormElement>,
  ) => {
    evento.preventDefault();

    const formulario = new FormData(
      evento.currentTarget,
    );

    const dataFechamento = String(
      formulario.get("dataFechamento") ?? "",
    );

    const dataVencimento = String(
      formulario.get("dataVencimento") ?? "",
    );

    if (!dataFechamento) {
      falhar(
        "Informe a data de fechamento.",
      );
      return;
    }

    if (!dataVencimento) {
      falhar(
        "Informe a data de vencimento.",
      );
      return;
    }

    if (
      new Date(dataVencimento) <=
      new Date(dataFechamento)
    ) {
      falhar(
        "A data de vencimento deve ser posterior à data de fechamento.",
      );
      return;
    }

    const dados = {
      nome: formulario.get("nome"),
      instituicao:
        formulario.get("instituicao"),
      limite: Number(
        formulario.get("limite"),
      ),
      dataFechamento,
      dataVencimento,
    };

    setSalvando(true);
    falhar("");

    try {
      await requisicao(
        rota,
        {
          method: "POST",
          body: JSON.stringify(dados),
        },
        token,
      );

      concluir();
    } catch (falha) {
      falhar(
        mensagemErro(
          falha,
          "Não foi possível cadastrar o cartão.",
        ),
      );
    } finally {
      setSalvando(false);
    }
  };

  return (
    <form
      className="formulario"
      onSubmit={enviar}
    >
      <Campo
        label="Nome"
        name="nome"
        required
      />

      <Campo
        label="Instituição"
        name="instituicao"
        required
      />

      <Campo
        label="Limite"
        name="limite"
        type="number"
        min="0"
        step="0.01"
        required
      />

      <Campo
        label="Data de fechamento"
        name="dataFechamento"
        type="date"
        required
      />

      <Campo
        label="Data de vencimento"
        name="dataVencimento"
        type="date"
        required
      />

      <button
        className="botao"
        disabled={salvando}
      >
        {salvando
          ? "Cadastrando..."
          : "Cadastrar cartão"}
      </button>
    </form>
  );
}

function CadastroPessoal({
  tipo,
}: {
  tipo: TipoCadastro;
}) {
  const { sessao } = useAutenticacao();

  const info = configuracao[tipo];

  const [itens, setItens] = useState<
    Cadastro[]
  >([]);

  const [
    abrirCadastro,
    setAbrirCadastro,
  ] = useState(false);

  const [editando, setEditando] =
    useState<Cadastro | null>(null);

  const [alterandoStatus, setAlterandoStatus] = useState<Cadastro | null>(null);
  const [excluindo, setExcluindo] = useState<Cadastro | null>(null);
  const [processando, setProcessando] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [busca, setBusca] = useState("");
  const [situacao, setSituacao] = useState<"TODOS" | "ATIVOS" | "INATIVOS">("TODOS");

  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] =
    useState("");

  const carregar = async () => {
    if (!sessao) {
      return;
    }

    try {
      setErro("");

      const registros =
        await requisicao<Cadastro[]>(
          info.rota,
          {},
          sessao.token,
        );

      setItens(registros);
    } catch (falha) {
      setErro(
        mensagemErro(
          falha,
          "Não foi possível carregar os registros.",
        ),
      );
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => {
    void carregar();
  }, [sessao]);

  const atualizar = async (
    item: Cadastro,
    dados: Record<string, unknown>,
  ) => {
    if (!sessao) {
      return;
    }

    try {
      setErro("");
      setSucesso("");

      await requisicao(
        `${info.rota}/${item.id}`,
        {
          method: "PUT",
          body: JSON.stringify(dados),
        },
        sessao.token,
      );

      setEditando(null);

      setSucesso(
        "Alterações salvas com sucesso.",
      );

      await carregar();
    } catch (falha) {
      setErro(
        mensagemErro(
          falha,
          "Não foi possível salvar as alterações.",
        ),
      );
    }
  };

  const dadosComStatus = (item: Cadastro, ativo: boolean) => {
    if (tipo === "conta") {
      const conta = item as Conta;
      return { nome: conta.nome, tipo: conta.tipo, ativo };
    }
    if (tipo === "categoria") {
      const categoria = item as Categoria;
      return { nome: categoria.nome, tipo: categoria.tipo, ativo };
    }
    const cartao = item as CartaoComDatas;
    return { nome: cartao.nome, instituicao: cartao.instituicao, limite: cartao.limite, dataFechamento: cartao.dataFechamento, dataVencimento: cartao.dataVencimento, ativo };
  };

  const alterarSituacao = async () => {
    if (!sessao || !alterandoStatus) {
      return;
    }
    const item = alterandoStatus;
    const novoStatus = !item.ativo;

    try {
      setProcessando(true);
      setErro("");
      setSucesso("");

      await requisicao(
        `${info.rota}/${item.id}`,
        {
          method: "PUT",
          body: JSON.stringify(dadosComStatus(item, novoStatus)),
        },
        sessao.token,
      );

      setAlterandoStatus(null);

      setSucesso(
        `Registro ${novoStatus ? "ativado" : "desativado"} com sucesso.`,
      );

      await carregar();
    } catch (falha) {
      setErro(
        mensagemErro(
          falha,
          `Não foi possível ${novoStatus ? "ativar" : "desativar"} o registro.`,
        ),
      );
    } finally {
      setProcessando(false);
    }
  };

  const excluir = async () => {
    if (!sessao || !excluindo) return;
    try {
      setProcessando(true);
      setErro("");
      await requisicao(`${info.rota}/${excluindo.id}`, { method: "DELETE" }, sessao.token);
      setExcluindo(null);
      setSucesso("Registro excluído com sucesso.");
      await carregar();
    } catch (falha) {
      setErro(mensagemErro(falha, "Não foi possível excluir o registro."));
    } finally {
      setProcessando(false);
    }
  };

  const valor = (
    item: Cadastro,
    coluna: string,
  ) => {
    if (coluna === "Nome") {
      return item.nome;
    }

    if (coluna === "Tipo") {
      return textoEnum(
        (item as Conta | Categoria).tipo,
      );
    }

    if (coluna === "Saldo atual") {
      return moeda(
        (item as Conta).saldoAtual,
      );
    }

    if (coluna === "Instituição") {
      return (item as CartaoComDatas)
        .instituicao;
    }

    if (coluna === "Limite") {
      return moeda(
        (item as CartaoComDatas).limite,
      );
    }

    if (coluna === "Fechamento") {
      return data(
        (item as CartaoComDatas)
          .dataFechamento,
      );
    }

    if (coluna === "Vencimento") {
      return data(
        (item as CartaoComDatas)
          .dataVencimento,
      );
    }

    if (coluna === "Padrão") {
      return (item as Categoria).padrao
        ? "Sim"
        : "Não";
    }

    return <Badge valor={item.ativo} />;
  };

  const itensFiltrados = itens.filter((item) => {
    const correspondeBusca = JSON.stringify(item).toLowerCase().includes(busca.trim().toLowerCase());
    const correspondeSituacao = situacao === "TODOS" || (situacao === "ATIVOS" ? item.ativo : !item.ativo);
    return correspondeBusca && correspondeSituacao;
  });

  return (
    <AreaAutenticada tipo="pessoal">
      <header className="cabecalho">
        <div>
          <h1>{info.titulo}</h1>

          <p>{info.descricao}</p>
        </div>

        <button
          className="botao"
          onClick={() => {
            setErro("");
            setSucesso("");
            setAbrirCadastro(true);
          }}
        >
          + Nova {info.singular}
        </button>
      </header>

      {sucesso && (
        <p className="mensagem sucesso">
          {sucesso}
        </p>
      )}

      {erro && (
        <p className="mensagem erro">
          {erro}
        </p>
      )}

      {itens.length > 0 && <div className="barra-filtros"><label><Search size={17}/><input value={busca} onChange={e => setBusca(e.target.value)} placeholder={`Buscar ${info.titulo.toLowerCase()}...`} aria-label={`Buscar ${info.titulo.toLowerCase()}`}/></label><select value={situacao} onChange={e => setSituacao(e.target.value as typeof situacao)} aria-label="Filtrar por situação"><option value="TODOS">Todas as situações</option><option value="ATIVOS">Ativos</option><option value="INATIVOS">Inativos</option></select><span>{itensFiltrados.length} resultado(s)</span></div>}
      {carregando ? <p className="estado">Carregando informações...</p> : itens.length === 0 ? (
        <div className="estado-vazio">
          <h2>Nenhuma {info.singular} cadastrada</h2>
          <p>{info.vazio}</p>
          <button className="botao" onClick={() => setAbrirCadastro(true)}>Cadastrar {info.singular}</button>
        </div>
      ) : <div className="tabela">
        <table>
          <thead>
            <tr>
              {info.colunas.map(
                (coluna) => (
                  <th key={coluna}>
                    {coluna}
                  </th>
                ),
              )}

              <th aria-label="Mais opções" />
            </tr>
          </thead>

          <tbody>
            {itensFiltrados.map((item) => (
              <tr key={item.id}>
                {info.colunas.map(
                  (coluna) => (
                    <td
                      key={coluna}
                      data-label={coluna}
                    >
                      {valor(
                        item,
                        coluna,
                      )}
                    </td>
                  ),
                )}

                <td data-label="Mais opções">
                  <MenuAcoes
                    acoes={[
                      {
                        rotulo: "Editar",
                        executar: () => {
                          setErro("");
                          setSucesso("");
                          setEditando(
                            item,
                          );
                        },
                      },

                      ...(tipo !==
                        "categoria" ||
                      !(item as Categoria)
                        .padrao
                        ? [
                            {
                              rotulo: item.ativo ? "Desativar" : "Ativar",
                              perigosa:
                                item.ativo,
                              executar:
                                () =>
                                  setAlterandoStatus(
                                    item,
                                  ),
                            },
                            ...(!item.ativo ? [{
                              rotulo: "Excluir",
                              perigosa: true,
                              executar: () => setExcluindo(item),
                            }] : []),
                          ]
                        : []),
                    ]}
                  />
                </td>
              </tr>
            ))}
            {itensFiltrados.length === 0 && <tr><td colSpan={info.colunas.length + 1}><div className="resultado-vazio">Nenhum registro corresponde aos filtros.</div></td></tr>}
          </tbody>
        </table>
      </div>}

      {abrirCadastro && (
        <Modal
          titulo={`Novo ${info.titulo
            .slice(0, -1)
            .toLowerCase()}`}
          fechar={() =>
            setAbrirCadastro(false)
          }
        >
          {tipo === "cartao" &&
          sessao ? (
            <FormularioNovoCartao
              rota={info.rota}
              token={sessao.token}
              falhar={setErro}
              concluir={() => {
                setAbrirCadastro(
                  false,
                );

                setSucesso(
                  "Cartão cadastrado com sucesso.",
                );

                void carregar();
              }}
            />
          ) : (
            <FormularioSimples
              tipo={tipo}
              rota={info.rota}
              concluir={() => {
                setAbrirCadastro(
                  false,
                );

                setSucesso(
                  "Cadastro realizado com sucesso.",
                );

                void carregar();
              }}
            />
          )}
        </Modal>
      )}

      {editando && (
        <Modal
          titulo={`Editar ${info.titulo
            .slice(0, -1)
            .toLowerCase()}`}
          fechar={() =>
            setEditando(null)
          }
        >
          <DadosCadastro
            tipo={tipo}
            item={editando}
            salvar={(dados) =>
              atualizar(
                editando,
                dados,
              )
            }
          />
        </Modal>
      )}

      {alterandoStatus && (
        <Modal
          titulo={`${alterandoStatus.ativo ? "Desativar" : "Ativar"} registro`}
          fechar={() =>
            setAlterandoStatus(null)
          }
        >
          <ConfirmacaoAcao
            descricao={alterandoStatus.ativo ? "O registro será desativado e o histórico será preservado." : "O registro voltará a aparecer nas opções disponíveis para novos lançamentos."}
            textoConfirmar={alterandoStatus.ativo ? "Desativar" : "Ativar"}
            confirmar={() =>
              void alterarSituacao()
            }
            fechar={() =>
              setAlterandoStatus(null)
            }
            processando={processando}
            perigosa={alterandoStatus.ativo}
          />
        </Modal>
      )}
      {excluindo && (
        <Modal titulo="Excluir registro" fechar={() => setExcluindo(null)}>
          <ConfirmacaoAcao
            descricao={`Excluir “${excluindo.nome}” permanentemente? O registro e todo o histórico financeiro diretamente vinculado a ele serão removidos. Esta ação não pode ser desfeita.`}
            textoConfirmar="Excluir definitivamente"
            confirmar={() => void excluir()}
            fechar={() => setExcluindo(null)}
            processando={processando}
          />
        </Modal>
      )}
    </AreaAutenticada>
  );
}

export const ContasPessoais = () => (
  <CadastroPessoal tipo="conta" />
);

export const CartoesPessoais = () => (
  <CadastroPessoal tipo="cartao" />
);

export const CategoriasPessoais = () => (
  <CadastroPessoal tipo="categoria" />
);

export function LancamentosPessoais() {
  const { sessao } = useAutenticacao();

  const [itens, setItens] = useState<
    LancamentoPessoal[]
  >([]);

  const [
    cancelando,
    setCancelando,
  ] = useState<LancamentoPessoal | null>(
    null,
  );

  const [erro, setErro] = useState("");

  const carregar = async () => {
    if (!sessao) {
      return;
    }

    try {
      setErro("");

      const lancamentos =
        await requisicao<
          LancamentoPessoal[]
        >(
          "/api/pessoal/lancamentos",
          {},
          sessao.token,
        );

      setItens(lancamentos);
    } catch {
      setErro(
        "Não foi possível carregar os lançamentos.",
      );
    }
  };

  useEffect(() => {
    void carregar();
  }, [sessao]);

  const cancelar = async () => {
    if (!sessao || !cancelando) {
      return;
    }

    try {
      setErro("");

      await requisicao(
        `/api/pessoal/lancamentos/${cancelando.id}`,
        {
          method: "DELETE",
        },
        sessao.token,
      );

      setCancelando(null);

      await carregar();
    } catch {
      setErro(
        "Não foi possível cancelar o lançamento.",
      );
    }
  };

  return (
    <AreaAutenticada tipo="pessoal">
      <header className="cabecalho">
        <div>
          <h1>Lançamentos</h1>

          <p>
            Consulte suas entradas e
            saídas.
          </p>
        </div>
      </header>

      {erro ? (
        <p className="mensagem erro">
          {erro}
        </p>
      ) : (
        <div className="tabela">
          <table>
            <thead>
              <tr>
                <th>Data</th>
                <th>Descrição</th>
                <th>Tipo</th>
                <th>Conta</th>
                <th>Valor</th>
                <th>Status</th>
                <th aria-label="Mais opções" />
              </tr>
            </thead>

            <tbody>
              {itens.map((item) => (
                <tr key={item.id}>
                  <td data-label="Data">
                    {data(item.data)}
                  </td>

                  <td data-label="Descrição">
                    {item.descricao}
                  </td>

                  <td data-label="Tipo">
                    {textoEnum(
                      item.tipo,
                    )}
                  </td>

                  <td data-label="Conta">
                    {item.conta}
                  </td>

                  <td data-label="Valor">
                    {moeda(item.valor)}
                  </td>

                  <td data-label="Status">
                    <Badge
                      valor={
                        item.cancelado
                          ? "CANCELADO"
                          : "Ativo"
                      }
                    />
                  </td>

                  <td data-label="Mais opções">
                    {!item.cancelado && (
                      <MenuAcoes
                        acoes={[
                          {
                            rotulo:
                              "Editar",
                            executar:
                              () =>
                                setErro(
                                  "A edição utiliza o formulário de lançamento existente.",
                                ),
                          },

                          {
                            rotulo:
                              "Cancelar",
                            perigosa:
                              true,
                            executar:
                              () =>
                                setCancelando(
                                  item,
                                ),
                          },
                        ]}
                      />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {cancelando && (
        <Modal
          titulo="Cancelar lançamento"
          fechar={() =>
            setCancelando(null)
          }
        >
          <ConfirmacaoAcao
            descricao="Esta ação preservará o histórico financeiro."
            textoConfirmar="Confirmar cancelamento"
            confirmar={() =>
              void cancelar()
            }
            fechar={() =>
              setCancelando(null)
            }
          />
        </Modal>
      )}
    </AreaAutenticada>
  );
}
