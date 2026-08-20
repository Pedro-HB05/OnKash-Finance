"use client";

import { useEffect, useState } from "react";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Campo, Modal } from "@/componentes/Base";
import { ConfirmacaoAcao, MenuAcoes } from "@/componentes/MenuAcoes";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { Categoria, Conta, ContaPagar, ContaReceber, PessoaCadastro } from "@/tipos/api";
import { data, moeda, textoEnum } from "@/utilitarios/formatadores";

type ItemFinanceiro = ContaPagar | ContaReceber;

export function ContasFinanceiras({ receber }: { receber: boolean }) {
  const { sessao } = useAutenticacao();
  const rota = receber ? "/api/empresarial/contas-receber" : "/api/empresarial/contas-pagar";
  const titulo = receber ? "Contas a receber" : "Contas a pagar";
  const [itens, setItens] = useState<ItemFinanceiro[]>([]);
  const [pessoas, setPessoas] = useState<PessoaCadastro[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [contas, setContas] = useState<Conta[]>([]);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [carregando, setCarregando] = useState(true);
  const [abrirCadastro, setAbrirCadastro] = useState(false);
  const [selecionada, setSelecionada] = useState<ItemFinanceiro | null>(null);
  const [editando, setEditando] = useState<ItemFinanceiro | null>(null);
  const [cancelando, setCancelando] = useState<ItemFinanceiro | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [efetivando, setEfetivando] = useState(false);

  const carregar = async () => {
    if (!sessao) return;
    setCarregando(true);
    setErro("");
    try {
      const [lista, pes, cats, listaContas] = await Promise.all([
        requisicao<ItemFinanceiro[]>(rota, {}, sessao.token),
        requisicao<PessoaCadastro[]>(
          receber ? "/api/empresarial/clientes" : "/api/empresarial/fornecedores",
          {},
          sessao.token,
        ),
        requisicao<Categoria[]>("/api/empresarial/categorias", {}, sessao.token),
        requisicao<Conta[]>("/api/empresarial/contas", {}, sessao.token),
      ]);
      setItens(lista);
      setPessoas(pes);
      setCategorias(cats);
      setContas(listaContas);
    } catch {
      setErro(`Não foi possível carregar ${titulo.toLowerCase()}.`);
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => {
    void carregar();
  }, [sessao]);

  const enviar = async (evento: React.FormEvent<HTMLFormElement>) => {
    evento.preventDefault();
    if (!sessao) return;
    const formulario = new FormData(evento.currentTarget);
    setSalvando(true);
    setErro("");
    try {
      await requisicao(
        rota,
        {
          method: "POST",
          body: JSON.stringify({
            [receber ? "clienteId" : "fornecedorId"]: formulario.get("pessoaId") || null,
            descricao: formulario.get("descricao"),
            valor: Number(formulario.get("valor")),
            vencimento: formulario.get("vencimento"),
            categoriaId: formulario.get("categoriaId"),
            observacao: formulario.get("observacao") || null,
          }),
        },
        sessao.token,
      );
      setAbrirCadastro(false);
      setSucesso(
        receber
          ? "Conta a receber cadastrada com sucesso."
          : "Conta a pagar cadastrada com sucesso.",
      );
      await carregar();
    } catch {
      setErro("Não foi possível salvar. Verifique os dados informados.");
    } finally {
      setSalvando(false);
    }
  };

  const efetivar = async (evento: React.FormEvent<HTMLFormElement>) => {
    evento.preventDefault();
    if (!sessao || !selecionada) return;
    const formulario = new FormData(evento.currentTarget);
    setEfetivando(true);
    setErro("");
    try {
      await requisicao(
        `${rota}/${selecionada.id}/${receber ? "receber" : "pagar"}`,
        {
          method: "POST",
          body: JSON.stringify({
            contaId: formulario.get("contaId"),
            [receber ? "dataRecebimento" : "dataPagamento"]: formulario.get("data"),
          }),
        },
        sessao.token,
      );
      setSelecionada(null);
      setSucesso(
        receber ? "Recebimento registrado com sucesso." : "Pagamento registrado com sucesso.",
      );
      await carregar();
    } catch {
      setErro(
        receber
          ? "Não foi possível registrar o recebimento."
          : "Não foi possível registrar o pagamento.",
      );
    } finally {
      setEfetivando(false);
    }
  };

  const atualizar = async (evento: React.FormEvent<HTMLFormElement>) => {
    evento.preventDefault();
    if (!sessao || !editando) return;
    const formulario = new FormData(evento.currentTarget);
    setSalvando(true);
    setErro("");
    try {
      await requisicao(
        `${rota}/${editando.id}`,
        {
          method: "PUT",
          body: JSON.stringify({
            [receber ? "clienteId" : "fornecedorId"]: formulario.get("pessoaId") || null,
            descricao: formulario.get("descricao"),
            valor: Number(formulario.get("valor")),
            vencimento: formulario.get("vencimento"),
            categoriaId: formulario.get("categoriaId"),
            observacao: formulario.get("observacao") || null,
          }),
        },
        sessao.token,
      );
      setEditando(null);
      setSucesso("Alterações salvas com sucesso.");
      await carregar();
    } catch {
      setErro("Não foi possível salvar as alterações.");
    } finally {
      setSalvando(false);
    }
  };

  const cancelar = async () => {
    if (!sessao || !cancelando) return;
    setSalvando(true);
    setErro("");
    try {
      await requisicao(`${rota}/${cancelando.id}`, { method: "DELETE" }, sessao.token);
      setCancelando(null);
      setSucesso(
        receber ? "Conta a receber cancelada com sucesso." : "Conta a pagar cancelada com sucesso.",
      );
      await carregar();
    } catch {
      setErro(
        receber
          ? "Não foi possível cancelar a conta a receber."
          : "Não foi possível cancelar a conta a pagar.",
      );
    } finally {
      setSalvando(false);
    }
  };

  return (
    <AreaAutenticada tipo="empresarial">
      <header className="cabecalho">
        <div>
          <p className="sobre-titulo">Financeiro</p>
          <h1>{titulo}</h1>
          <p>
            {receber
              ? "Acompanhe os valores que devem entrar."
              : "Acompanhe os pagamentos da empresa."}
          </p>
        </div>
        <button className="botao" onClick={() => setAbrirCadastro(true)}>
          {receber ? "+ Nova conta a receber" : "+ Nova conta a pagar"}
        </button>
      </header>
      {sucesso && <p className="mensagem sucesso">{sucesso}</p>}
      {erro ? (
        <p className="mensagem erro">{erro}</p>
      ) : carregando ? (
        <p className="estado">Carregando informações...</p>
      ) : itens.length === 0 ? (
        <p className="estado">Nenhuma conta encontrada.</p>
      ) : (
        <div className="tabela">
          <table>
            <thead>
              <tr>
                <th>{receber ? "Cliente" : "Fornecedor"}</th>
                <th>Descrição</th>
                <th>Valor</th>
                <th>Vencimento</th>
                <th>Status</th>
                <th>{receber ? "Recebimento" : "Pagamento"}</th>
                <th>Ação</th>
                <th aria-label="Mais opções" />
              </tr>
            </thead>
            <tbody>
              {itens.map((item) => {
                const podeEfetivar = item.status === "PENDENTE" || item.status === "ATRASADO";
                const dataEfetivacao = receber
                  ? (item as ContaReceber).dataRecebimento
                  : (item as ContaPagar).dataPagamento;
                return (
                  <tr key={item.id}>
                    <td data-label={receber ? "Cliente" : "Fornecedor"}>
                      {receber
                        ? ((item as ContaReceber).cliente ?? "—")
                        : ((item as ContaPagar).fornecedor ?? "—")}
                    </td>
                    <td data-label="Descrição">{item.descricao}</td>
                    <td data-label="Valor">{moeda(item.valor)}</td>
                    <td data-label="Vencimento">{data(item.vencimento)}</td>
                    <td data-label="Status">{textoEnum(item.status)}</td>
                    <td data-label={receber ? "Recebimento" : "Pagamento"}>
                      {dataEfetivacao ? data(dataEfetivacao) : "—"}
                    </td>
                    <td data-label="Ação">
                      {podeEfetivar && (
                        <button className="botao secundario" onClick={() => setSelecionada(item)}>
                          {receber ? "Marcar como recebida" : "Marcar como paga"}
                        </button>
                      )}
                    </td>
                    <td data-label="Mais opções">
                      {podeEfetivar && (
                        <MenuAcoes
                          acoes={[
                            { rotulo: "Editar", executar: () => setEditando(item) },
                            {
                              rotulo: "Cancelar",
                              perigosa: true,
                              executar: () => setCancelando(item),
                            },
                          ]}
                        />
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
      {abrirCadastro && (
        <Modal
          titulo={receber ? "Nova conta a receber" : "Nova conta a pagar"}
          fechar={() => setAbrirCadastro(false)}
        >
          <form className="formulario" onSubmit={enviar}>
            <label className="campo">
              {receber ? "Cliente" : "Fornecedor"}
              <select name="pessoaId">
                <option value="">Não informado</option>
                {pessoas
                  .filter((pessoa) => pessoa.ativo)
                  .map((pessoa) => (
                    <option key={pessoa.id} value={pessoa.id}>
                      {pessoa.nomeRazaoSocial}
                    </option>
                  ))}
              </select>
            </label>
            <Campo label="Descrição" name="descricao" required />
            <Campo label="Valor" name="valor" type="number" min="0.01" step="0.01" required />
            <Campo label="Vencimento" name="vencimento" type="date" required />
            <label className="campo">
              Categoria
              <select name="categoriaId" required>
                <option value="">Selecione</option>
                {categorias
                  .filter((categoria) => categoria.ativo)
                  .map((categoria) => (
                    <option key={categoria.id} value={categoria.id}>
                      {categoria.nome}
                    </option>
                  ))}
              </select>
            </label>
            <Campo label="Observação" name="observacao" />
            <button className="botao" disabled={salvando}>
              {salvando ? "Salvando..." : "Salvar"}
            </button>
          </form>
        </Modal>
      )}
      {editando && (
        <Modal
          titulo={receber ? "Editar conta a receber" : "Editar conta a pagar"}
          fechar={() => setEditando(null)}
        >
          <form className="formulario" onSubmit={atualizar}>
            <label className="campo">
              {receber ? "Cliente" : "Fornecedor"}
              <select
                name="pessoaId"
                defaultValue={
                  receber
                    ? ((editando as ContaReceber).clienteId ?? "")
                    : ((editando as ContaPagar).fornecedorId ?? "")
                }
              >
                <option value="">Não informado</option>
                {pessoas
                  .filter((pessoa) => pessoa.ativo)
                  .map((pessoa) => (
                    <option key={pessoa.id} value={pessoa.id}>
                      {pessoa.nomeRazaoSocial}
                    </option>
                  ))}
              </select>
            </label>
            <Campo label="Descrição" name="descricao" defaultValue={editando.descricao} required />
            <Campo
              label="Valor"
              name="valor"
              type="number"
              min="0.01"
              step="0.01"
              defaultValue={editando.valor}
              required
            />
            <Campo
              label="Vencimento"
              name="vencimento"
              type="date"
              defaultValue={editando.vencimento}
              required
            />
            <label className="campo">
              Categoria
              <select name="categoriaId" defaultValue={editando.categoriaId} required>
                <option value="">Selecione</option>
                {categorias
                  .filter((categoria) => categoria.ativo)
                  .map((categoria) => (
                    <option key={categoria.id} value={categoria.id}>
                      {categoria.nome}
                    </option>
                  ))}
              </select>
            </label>
            <Campo label="Observação" name="observacao" defaultValue={editando.observacao ?? ""} />
            <button className="botao" disabled={salvando}>
              {salvando ? "Salvando..." : "Salvar alterações"}
            </button>
          </form>
        </Modal>
      )}
      {cancelando && (
        <Modal
          titulo={receber ? "Cancelar conta a receber" : "Cancelar conta a pagar"}
          fechar={() => setCancelando(null)}
        >
          <ConfirmacaoAcao
            descricao="Esta conta deixará de aparecer como pendente. O histórico será preservado."
            textoConfirmar="Cancelar conta"
            confirmar={() => void cancelar()}
            fechar={() => setCancelando(null)}
            processando={salvando}
          />
        </Modal>
      )}
      {selecionada && (
        <Modal
          titulo={receber ? "Registrar recebimento" : "Registrar pagamento"}
          fechar={() => setSelecionada(null)}
        >
          <form className="formulario" onSubmit={efetivar}>
            <label className="campo">
              {receber ? "Conta onde o valor foi recebido" : "Conta utilizada para o pagamento"}
              <select name="contaId" required>
                <option value="">Selecione</option>
                {contas
                  .filter((conta) => conta.ativo)
                  .map((conta) => (
                    <option key={conta.id} value={conta.id}>
                      {conta.nome}
                    </option>
                  ))}
              </select>
            </label>
            <Campo
              label={receber ? "Data do recebimento" : "Data do pagamento"}
              name="data"
              type="date"
              required
            />
            <button className="botao" disabled={efetivando}>
              {efetivando
                ? "Registrando..."
                : receber
                  ? "Confirmar recebimento"
                  : "Confirmar pagamento"}
            </button>
          </form>
        </Modal>
      )}
    </AreaAutenticada>
  );
}
