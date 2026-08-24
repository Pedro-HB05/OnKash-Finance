"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Search } from "lucide-react";

import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Badge, Campo, Modal } from "@/componentes/Base";
import { ConfirmacaoAcao, MenuAcoes } from "@/componentes/MenuAcoes";
import { AnexosLancamento } from "@/componentes/AnexosLancamento";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type {
  Categoria,
  Conta,
  LancamentoPessoal,
} from "@/tipos/api";
import {
  data,
  moeda,
  textoEnum,
} from "@/utilitarios/formatadores";

type TipoLancamento = "ENTRADA" | "SAIDA";

type FormaPagamento = "CONTA" | "CARTAO";

type CartaoLancamento = {
  id: string;
  nome: string;
  instituicao: string;
  ativo: boolean;
};

function hojeLocal() {
  const agora = new Date();

  const ano = agora.getFullYear();
  const mes = String(
    agora.getMonth() + 1,
  ).padStart(2, "0");

  const dia = String(
    agora.getDate(),
  ).padStart(2, "0");

  return `${ano}-${mes}-${dia}`;
}

function mensagemErro(
  falha: unknown,
  padrao: string,
) {
  return falha instanceof Error
    ? falha.message
    : padrao;
}

export function LancamentosPessoaisFuncionais() {
  const { sessao } = useAutenticacao();

  const [
    lancamentos,
    setLancamentos,
  ] = useState<LancamentoPessoal[]>([]);

  const [contas, setContas] = useState<
    Conta[]
  >([]);

  const [
    categorias,
    setCategorias,
  ] = useState<Categoria[]>([]);

  const [
    cartoes,
    setCartoes,
  ] = useState<CartaoLancamento[]>([]);

  const [
    lancamentoEmEdicao,
    setLancamentoEmEdicao,
  ] = useState<LancamentoPessoal | null>(
    null,
  );

  const [
    lancamentoEmCancelamento,
    setLancamentoEmCancelamento,
  ] = useState<LancamentoPessoal | null>(
    null,
  );

  const [
    tipoSelecionado,
    setTipoSelecionado,
  ] = useState<TipoLancamento>("ENTRADA");

  const [
    formaPagamento,
    setFormaPagamento,
  ] = useState<FormaPagamento>("CONTA");

  const [
    contaSelecionada,
    setContaSelecionada,
  ] = useState("");

  const [
    categoriaSelecionada,
    setCategoriaSelecionada,
  ] = useState("");

  const [
    cartaoSelecionado,
    setCartaoSelecionado,
  ] = useState("");

  const [
    numeroParcelas,
    setNumeroParcelas,
  ] = useState(1);

  const [erro, setErro] = useState("");

  const [sucesso, setSucesso] =
    useState("");

  const [salvando, setSalvando] =
    useState(false);
  const [busca, setBusca] = useState("");
  const [filtroTipo, setFiltroTipo] = useState<"TODOS" | TipoLancamento>("TODOS");
  const [anexando, setAnexando] = useState<LancamentoPessoal | null>(null);

  const carregarDados = async () => {
    if (!sessao) {
      return;
    }

    try {
      setErro("");

      const [
        lancamentosResposta,
        contasResposta,
        categoriasResposta,
        cartoesResposta,
      ] = await Promise.all([
        requisicao<LancamentoPessoal[]>(
          "/api/pessoal/lancamentos",
          {},
          sessao.token,
        ),

        requisicao<Conta[]>(
          "/api/pessoal/contas",
          {},
          sessao.token,
        ),

        requisicao<Categoria[]>(
          "/api/pessoal/categorias",
          {},
          sessao.token,
        ),

        requisicao<CartaoLancamento[]>(
          "/api/pessoal/cartoes",
          {},
          sessao.token,
        ),
      ]);

      setLancamentos(
        lancamentosResposta,
      );

      setContas(contasResposta);

      setCategorias(
        categoriasResposta,
      );

      setCartoes(cartoesResposta);
    } catch (falha) {
      setErro(
        mensagemErro(
          falha,
          "Não foi possível carregar os dados.",
        ),
      );
    }
  };

  useEffect(() => {
    void carregarDados();
  }, [sessao]);

  const abrirNovoLancamento = () => {
    setErro("");
    setSucesso("");

    setTipoSelecionado("ENTRADA");
    setFormaPagamento("CONTA");

    setContaSelecionada("");
    setCategoriaSelecionada("");
    setCartaoSelecionado("");

    setNumeroParcelas(1);

    setLancamentoEmEdicao({
      id: "novo",
      contaId: "",
      conta: "",
      tipo: "ENTRADA",
      descricao: "",
      valor: 0,
      data: hojeLocal(),
      cancelado: false,
    });
  };

  const abrirEdicao = (
    lancamento: LancamentoPessoal,
  ) => {
    setErro("");
    setSucesso("");

    setTipoSelecionado(
      lancamento.tipo as TipoLancamento,
    );

    setFormaPagamento("CONTA");

    setContaSelecionada(
      lancamento.contaId ?? "",
    );

    setCategoriaSelecionada(
      lancamento.categoriaId ?? "",
    );

    setCartaoSelecionado("");
    setNumeroParcelas(1);

    setLancamentoEmEdicao(
      lancamento,
    );
  };

  const alterarTipo = (
    novoTipo: TipoLancamento,
  ) => {
    setTipoSelecionado(novoTipo);

    setCategoriaSelecionada("");

    if (novoTipo === "ENTRADA") {
      setFormaPagamento("CONTA");
      setCartaoSelecionado("");
      setNumeroParcelas(1);
    }
  };

  const alterarFormaPagamento = (
    novaForma: FormaPagamento,
  ) => {
    setFormaPagamento(novaForma);

    if (novaForma === "CONTA") {
      setCartaoSelecionado("");
      setNumeroParcelas(1);
    } else {
      setContaSelecionada("");
    }
  };

  const handleSalvar = async (
    evento: React.FormEvent<HTMLFormElement>,
  ) => {
    evento.preventDefault();

    if (
      !sessao ||
      !lancamentoEmEdicao
    ) {
      return;
    }

    const formulario = new FormData(
      evento.currentTarget,
    );

    const novo =
      lancamentoEmEdicao.id ===
      "novo";

    const descricao = String(
      formulario.get("descricao") ?? "",
    ).trim();

    const valor = Number(
      formulario.get("valor"),
    );

    const dataLancamento = String(
      formulario.get("data") ?? "",
    );

    const observacao =
      String(
        formulario.get(
          "observacao",
        ) ?? "",
      ).trim() || null;

    if (!descricao) {
      setErro(
        "Informe a descrição.",
      );
      return;
    }

    if (
      !Number.isFinite(valor) ||
      valor <= 0
    ) {
      setErro(
        "Informe um valor maior que zero.",
      );
      return;
    }

    if (!dataLancamento) {
      setErro("Informe a data.");
      return;
    }

    if (!categoriaSelecionada) {
      setErro(
        "Selecione uma categoria.",
      );
      return;
    }

    const compraNoCartao =
      novo &&
      tipoSelecionado ===
        "SAIDA" &&
      formaPagamento ===
        "CARTAO";

    if (
      compraNoCartao &&
      !cartaoSelecionado
    ) {
      setErro(
        "Selecione o cartão.",
      );
      return;
    }

    if (
      !compraNoCartao &&
      !contaSelecionada
    ) {
      setErro(
        "Selecione a conta.",
      );
      return;
    }

    if (
      compraNoCartao &&
      (!Number.isInteger(
        numeroParcelas,
      ) ||
        numeroParcelas < 1)
    ) {
      setErro(
        "O número de parcelas deve ser maior que zero.",
      );
      return;
    }

    setErro("");
    setSucesso("");
    setSalvando(true);

    try {
      if (compraNoCartao) {
        await requisicao(
          "/api/pessoal/cartoes/compras",
          {
            method: "POST",

            body: JSON.stringify({
              cartaoId:
                cartaoSelecionado,

              categoriaId:
                categoriaSelecionada,

              descricao,

              valorTotal: valor,

              dataCompra:
                dataLancamento,

              numeroParcelas,

              observacao,
            }),
          },
          sessao.token,
        );

        setLancamentoEmEdicao(
          null,
        );

        setSucesso(
          numeroParcelas > 1
            ? `Compra no cartão registrada em ${numeroParcelas} parcelas. As faturas correspondentes foram atualizadas.`
            : "Compra no cartão registrada com sucesso. A fatura correspondente foi atualizada.",
        );

        await carregarDados();

        return;
      }

      await requisicao(
        novo
          ? "/api/pessoal/lancamentos"
          : `/api/pessoal/lancamentos/${lancamentoEmEdicao.id}`,
        {
          method: novo
            ? "POST"
            : "PUT",

          body: JSON.stringify({
            contaId:
              contaSelecionada,

            categoriaId:
              categoriaSelecionada,

            tipo:
              tipoSelecionado,

            descricao,

            valor,

            data: dataLancamento,

            observacao,
          }),
        },
        sessao.token,
      );

      setLancamentoEmEdicao(null);

      setSucesso(
        novo
          ? "Lançamento cadastrado com sucesso."
          : "Alterações salvas com sucesso.",
      );

      await carregarDados();
    } catch (falha) {
      setErro(
        mensagemErro(
          falha,
          compraNoCartao
            ? "Não foi possível registrar a compra no cartão."
            : "Não foi possível salvar o lançamento.",
        ),
      );
    } finally {
      setSalvando(false);
    }
  };

  const handleCancelar =
    async () => {
      if (
        !sessao ||
        !lancamentoEmCancelamento
      ) {
        return;
      }

      try {
        setErro("");
        setSucesso("");

        await requisicao(
          `/api/pessoal/lancamentos/${lancamentoEmCancelamento.id}`,
          {
            method: "DELETE",
          },
          sessao.token,
        );

        setLancamentoEmCancelamento(
          null,
        );

        setSucesso(
          "Lançamento cancelado com sucesso.",
        );

        await carregarDados();
      } catch (falha) {
        setErro(
          mensagemErro(
            falha,
            "Não foi possível cancelar o lançamento.",
          ),
        );
      }
    };

  const categoriasFiltradas =
    categorias.filter(
      (categoria) =>
        categoria.ativo &&
        categoria.tipo ===
          tipoSelecionado,
    );

  const novoLancamento =
    lancamentoEmEdicao?.id ===
    "novo";

  const compraNoCartao =
    novoLancamento &&
    tipoSelecionado === "SAIDA" &&
    formaPagamento === "CARTAO";

  const possuiContaAtiva = contas.some((conta) => conta.ativo);
  const possuiCategoriaAtiva = categorias.some((categoria) => categoria.ativo);
  const podeCriarLancamento = possuiContaAtiva && possuiCategoriaAtiva;
  const lancamentosFiltrados = lancamentos.filter(l => (filtroTipo === "TODOS" || l.tipo === filtroTipo) && `${l.descricao} ${l.conta} ${l.categoria ?? ""}`.toLowerCase().includes(busca.toLowerCase()));

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

        <button
          className="botao"
          onClick={
            abrirNovoLancamento
          }
          disabled={!podeCriarLancamento}
          title={!podeCriarLancamento ? "Cadastre uma conta e uma categoria ativa primeiro" : undefined}
        >
          + Novo lançamento
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

      {lancamentos.length > 0 && <div className="barra-filtros"><label><Search size={17}/><input value={busca} onChange={e => setBusca(e.target.value)} placeholder="Buscar por descrição, conta ou categoria..." aria-label="Buscar lançamentos"/></label><select value={filtroTipo} onChange={e => setFiltroTipo(e.target.value as typeof filtroTipo)} aria-label="Filtrar por tipo"><option value="TODOS">Todos os tipos</option><option value="ENTRADA">Entradas</option><option value="SAIDA">Saídas</option></select><span>{lancamentosFiltrados.length} resultado(s)</span></div>}
      {lancamentos.length === 0 ? (
        <div className="estado-vazio estado-vazio-onboarding">
          <p className="sobre-titulo">Primeiros passos</p>
          <h2>Registre sua primeira movimentação</h2>
          <p>Prepare a estrutura básica e comece a acompanhar seu dinheiro.</p>
          <ol className="passos-onboarding">
            <li className={possuiContaAtiva ? "concluido" : ""}><span>1</span><div><strong>Cadastre uma conta</strong><small>Onde seu saldo será acompanhado.</small></div>{!possuiContaAtiva && <Link href="/pessoal/contas">Cadastrar</Link>}</li>
            <li className={possuiCategoriaAtiva ? "concluido" : ""}><span>2</span><div><strong>Organize as categorias</strong><small>Classifique entradas e saídas.</small></div>{!possuiCategoriaAtiva && <Link href="/pessoal/categorias">Cadastrar</Link>}</li>
            <li><span>3</span><div><strong>Adicione um lançamento</strong><small>Registre sua primeira entrada ou saída.</small></div>{podeCriarLancamento && <button type="button" onClick={abrirNovoLancamento}>Começar</button>}</li>
          </ol>
        </div>
      ) : <div className="tabela">
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
            {lancamentosFiltrados.map(
              (lancamento) => (
                <tr
                  key={
                    lancamento.id
                  }
                >
                  <td data-label="Data">
                    {data(
                      lancamento.data,
                    )}
                  </td>

                  <td data-label="Descrição">
                    {
                      lancamento.descricao
                    }
                  </td>

                  <td data-label="Tipo">
                    {textoEnum(
                      lancamento.tipo,
                    )}
                  </td>

                  <td data-label="Conta">
                    {
                      lancamento.conta
                    }
                  </td>

                  <td data-label="Valor">
                    {moeda(
                      lancamento.valor,
                    )}
                  </td>

                  <td data-label="Status">
                    <Badge
                      valor={
                        lancamento.cancelado
                          ? "CANCELADO"
                          : "Ativo"
                      }
                    />
                  </td>

                  <td data-label="Mais opções">
                    <MenuAcoes
                        acoes={[
                          { rotulo: "Comprovantes", executar: () => setAnexando(lancamento) },
                          ...(!lancamento.cancelado ? [
                          {
                            rotulo:
                              "Editar",

                            executar:
                              () =>
                                abrirEdicao(
                                  lancamento,
                                ),
                          },

                          {
                            rotulo:
                              "Cancelar",

                            perigosa:
                              true,

                            executar:
                              () =>
                                setLancamentoEmCancelamento(
                                  lancamento,
                                ),
                          },
                          ] : []),
                        ]}
                      />
                  </td>
                </tr>
              ),
            )}
          </tbody>
        </table>
      </div>}

      {lancamentoEmEdicao && (
        <Modal
          titulo={
            novoLancamento
              ? "Novo lançamento"
              : "Editar lançamento"
          }
          fechar={() =>
            setLancamentoEmEdicao(
              null,
            )
          }
        >
          <form
            className="formulario"
            onSubmit={handleSalvar}
          >
            <label className="campo">
              Tipo

              <select
                name="tipo"
                value={
                  tipoSelecionado
                }
                onChange={(evento) =>
                  alterarTipo(
                    evento.target
                      .value as TipoLancamento,
                  )
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

            {novoLancamento &&
              tipoSelecionado ===
                "SAIDA" && (
                <label className="campo">
                  Forma de pagamento

                  <select
                    value={
                      formaPagamento
                    }
                    onChange={(
                      evento,
                    ) =>
                      alterarFormaPagamento(
                        evento.target
                          .value as FormaPagamento,
                      )
                    }
                  >
                    <option value="CONTA">
                      Conta
                    </option>

                    <option value="CARTAO">
                      Cartão de
                      crédito
                    </option>
                  </select>
                </label>
              )}

            {!compraNoCartao && (
              <label className="campo">
                Conta

                <select
                  name="contaId"
                  value={
                    contaSelecionada
                  }
                  onChange={(evento) =>
                    setContaSelecionada(
                      evento.target
                        .value,
                    )
                  }
                  required
                >
                  <option value="">
                    Selecione
                  </option>

                  {contas
                    .filter(
                      (conta) =>
                        conta.ativo,
                    )
                    .map((conta) => (
                      <option
                        key={
                          conta.id
                        }
                        value={
                          conta.id
                        }
                      >
                        {
                          conta.nome
                        }
                      </option>
                    ))}
                </select>
              </label>
            )}

            {compraNoCartao && (
              <>
                <label className="campo">
                  Cartão

                  <select
                    name="cartaoId"
                    value={
                      cartaoSelecionado
                    }
                    onChange={(
                      evento,
                    ) =>
                      setCartaoSelecionado(
                        evento.target
                          .value,
                      )
                    }
                    required
                  >
                    <option value="">
                      Selecione
                    </option>

                    {cartoes
                      .filter(
                        (cartao) =>
                          cartao.ativo,
                      )
                      .map(
                        (cartao) => (
                          <option
                            key={
                              cartao.id
                            }
                            value={
                              cartao.id
                            }
                          >
                            {
                              cartao.nome
                            }
                            {cartao.instituicao
                              ? ` - ${cartao.instituicao}`
                              : ""}
                          </option>
                        ),
                      )}
                  </select>
                </label>

                <Campo
                  label="Número de parcelas"
                  name="numeroParcelas"
                  type="number"
                  min="1"
                  step="1"
                  value={
                    numeroParcelas
                  }
                  onChange={(
                    evento,
                  ) =>
                    setNumeroParcelas(
                      Number(
                        evento
                          .target
                          .value,
                      ),
                    )
                  }
                  required
                />
              </>
            )}

            <label className="campo">
              Categoria

              <select
                name="categoriaId"
                value={
                  categoriaSelecionada
                }
                onChange={(evento) =>
                  setCategoriaSelecionada(
                    evento.target
                      .value,
                  )
                }
                required
              >
                <option value="">
                  Selecione
                </option>

                {categoriasFiltradas.map(
                  (categoria) => (
                    <option
                      key={
                        categoria.id
                      }
                      value={
                        categoria.id
                      }
                    >
                      {
                        categoria.nome
                      }
                    </option>
                  ),
                )}
              </select>
            </label>

            <Campo
              label="Descrição"
              name="descricao"
              defaultValue={
                lancamentoEmEdicao.descricao
              }
              required
            />

            <Campo
              label="Valor"
              name="valor"
              type="number"
              min="0.01"
              step="0.01"
              defaultValue={
                lancamentoEmEdicao.valor ||
                ""
              }
              required
            />

            <Campo
              label={
                compraNoCartao
                  ? "Data da compra"
                  : "Data"
              }
              name="data"
              type="date"
              defaultValue={
                lancamentoEmEdicao.data
              }
              required
            />

            <Campo
              label="Observação"
              name="observacao"
              defaultValue={
                lancamentoEmEdicao.observacao ??
                ""
              }
            />

            {compraNoCartao && (
              <p className="texto-apoio">
                A compra será adicionada
                à fatura do cartão e não
                reduzirá o saldo de uma
                conta agora. A saída da
                conta acontecerá quando
                a fatura for paga.
              </p>
            )}

            <button
              className="botao"
              disabled={salvando}
            >
              {salvando
                ? "Salvando..."
                : compraNoCartao
                  ? "Registrar compra no cartão"
                  : novoLancamento
                    ? "Salvar lançamento"
                    : "Salvar alterações"}
            </button>
          </form>
        </Modal>
      )}

      {lancamentoEmCancelamento && (
        <Modal
          titulo="Cancelar lançamento"
          fechar={() =>
            setLancamentoEmCancelamento(
              null,
            )
          }
        >
          <ConfirmacaoAcao
            descricao="Esta ação preservará o histórico financeiro."
            textoConfirmar="Confirmar cancelamento"
            confirmar={() =>
              void handleCancelar()
            }
            fechar={() =>
              setLancamentoEmCancelamento(
                null,
              )
            }
          />
        </Modal>
      )}
      {anexando && <AnexosLancamento tipo="pessoal" lancamentoId={anexando.id} descricao={anexando.descricao} fechar={() => setAnexando(null)}/>}
    </AreaAutenticada>
  );
}
