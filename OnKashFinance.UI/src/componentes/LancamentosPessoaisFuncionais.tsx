"use client";
import { useEffect, useState } from "react";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { Badge, Campo, Modal } from "@/componentes/Base";
import { ConfirmacaoAcao, MenuAcoes } from "@/componentes/MenuAcoes";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { Categoria, Conta, LancamentoPessoal } from "@/tipos/api";
import { data, moeda, textoEnum } from "@/utilitarios/formatadores";

export function LancamentosPessoaisFuncionais() {
  const { sessao } = useAutenticacao();
  const [lancamentos, setLancamentos] = useState<LancamentoPessoal[]>([]);
  const [contas, setContas] = useState<Conta[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [lancamentoEmEdicao, setLancamentoEmEdicao] = useState<LancamentoPessoal | null>(null);
  const [lancamentoEmCancelamento, setLancamentoEmCancelamento] =
    useState<LancamentoPessoal | null>(null);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [salvando, setSalvando] = useState(false);
  const carregarDados = async () => {
    if (!sessao) return;
    try {
      const [l, c, categorias] = await Promise.all([
        requisicao<LancamentoPessoal[]>("/api/pessoal/lancamentos", {}, sessao.token),
        requisicao<Conta[]>("/api/pessoal/contas", {}, sessao.token),
        requisicao<Categoria[]>("/api/pessoal/categorias", {}, sessao.token),
      ]);
      setLancamentos(l);
      setContas(c);
      setCategorias(categorias);
    } catch {
      setErro("Não foi possível carregar os lançamentos.");
    }
  };
  useEffect(() => {
    void carregarDados();
  }, [sessao]);
  const handleSalvar = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!sessao || !lancamentoEmEdicao) return;
    const f = new FormData(e.currentTarget),
      novo = lancamentoEmEdicao.id === "novo";
    setSalvando(true);
    try {
      await requisicao(
        novo ? "/api/pessoal/lancamentos" : `/api/pessoal/lancamentos/${lancamentoEmEdicao.id}`,
        {
          method: novo ? "POST" : "PUT",
          body: JSON.stringify({
            contaId: f.get("contaId"),
            categoriaId: f.get("categoriaId") || null,
            tipo: f.get("tipo"),
            descricao: f.get("descricao"),
            valor: Number(f.get("valor")),
            data: f.get("data"),
            observacao: f.get("observacao") || null,
          }),
        },
        sessao.token,
      );
      setLancamentoEmEdicao(null);
      setSucesso(novo ? "Lançamento cadastrado com sucesso." : "Alterações salvas com sucesso.");
      await carregarDados();
    } catch {
      setErro("Não foi possível salvar as alterações.");
    } finally {
      setSalvando(false);
    }
  };
  const handleCancelar = async () => {
    if (!sessao || !lancamentoEmCancelamento) return;
    try {
      await requisicao(
        `/api/pessoal/lancamentos/${lancamentoEmCancelamento.id}`,
        { method: "DELETE" },
        sessao.token,
      );
      setLancamentoEmCancelamento(null);
      setSucesso("Lançamento cancelado com sucesso.");
      await carregarDados();
    } catch {
      setErro("Não foi possível cancelar o lançamento.");
    }
  };
  return (
    <AreaAutenticada tipo="pessoal">
      <header className="cabecalho">
        <div>
          <h1>Lançamentos</h1>
          <p>Consulte suas entradas e saídas.</p>
        </div>
        <button
          className="botao"
          onClick={() =>
            setLancamentoEmEdicao({
              id: "novo",
              contaId: "",
              conta: "",
              tipo: "ENTRADA",
              descricao: "",
              valor: 0,
              data: "",
              cancelado: false,
            })
          }
        >
          + Novo lançamento
        </button>
      </header>
      {sucesso && <p className="mensagem sucesso">{sucesso}</p>}
      {erro ? (
        <p className="mensagem erro">{erro}</p>
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
              {lancamentos.map((lancamento) => (
                <tr key={lancamento.id}>
                  <td data-label="Data">{data(lancamento.data)}</td>
                  <td data-label="Descrição">{lancamento.descricao}</td>
                  <td data-label="Tipo">{textoEnum(lancamento.tipo)}</td>
                  <td data-label="Conta">{lancamento.conta}</td>
                  <td data-label="Valor">{moeda(lancamento.valor)}</td>
                  <td data-label="Status">
                    <Badge valor={lancamento.cancelado ? "CANCELADO" : "Ativo"} />
                  </td>
                  <td data-label="Mais opções">
                    {!lancamento.cancelado && (
                      <MenuAcoes
                        acoes={[
                          {
                            rotulo: "Editar",
                            executar: () => setLancamentoEmEdicao(lancamento),
                          },
                          {
                            rotulo: "Cancelar",
                            perigosa: true,
                            executar: () => setLancamentoEmCancelamento(lancamento),
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
      {lancamentoEmEdicao && (
        <Modal
          titulo={lancamentoEmEdicao.id === "novo" ? "Novo lançamento" : "Editar lançamento"}
          fechar={() => setLancamentoEmEdicao(null)}
        >
          <form className="formulario" onSubmit={handleSalvar}>
            <label className="campo">
              Conta
              <select name="contaId" defaultValue={lancamentoEmEdicao.contaId} required>
                <option value="">Selecione</option>
                {contas
                  .filter((x) => x.ativo)
                  .map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.nome}
                    </option>
                  ))}
              </select>
            </label>
            <label className="campo">
              Categoria
              <select name="categoriaId" defaultValue={lancamentoEmEdicao.categoriaId ?? ""}>
                <option value="">Não informado</option>
                {categorias
                  .filter((x) => x.ativo)
                  .map((x) => (
                    <option key={x.id} value={x.id}>
                      {x.nome}
                    </option>
                  ))}
              </select>
            </label>
            <label className="campo">
              Tipo
              <select name="tipo" defaultValue={lancamentoEmEdicao.tipo}>
                <option value="ENTRADA">Entrada</option>
                <option value="SAIDA">Saída</option>
              </select>
            </label>
            <Campo
              label="Descrição"
              name="descricao"
              defaultValue={lancamentoEmEdicao.descricao}
              required
            />
            <Campo
              label="Valor"
              name="valor"
              type="number"
              min="0.01"
              step="0.01"
              defaultValue={lancamentoEmEdicao.valor || ""}
              required
            />
            <Campo
              label="Data"
              name="data"
              type="date"
              defaultValue={lancamentoEmEdicao.data}
              required
            />
            <Campo
              label="Observação"
              name="observacao"
              defaultValue={lancamentoEmEdicao.observacao ?? ""}
            />
            <button className="botao" disabled={salvando}>
              {salvando
                ? "Salvando..."
                : lancamentoEmEdicao.id === "novo"
                  ? "Salvar lançamento"
                  : "Salvar alterações"}
            </button>
          </form>
        </Modal>
      )}
      {lancamentoEmCancelamento && (
        <Modal titulo="Cancelar lançamento" fechar={() => setLancamentoEmCancelamento(null)}>
          <ConfirmacaoAcao
            descricao="Esta ação preservará o histórico financeiro."
            textoConfirmar="Confirmar cancelamento"
            confirmar={() => void handleCancelar()}
            fechar={() => setLancamentoEmCancelamento(null)}
          />
        </Modal>
      )}
    </AreaAutenticada>
  );
}
