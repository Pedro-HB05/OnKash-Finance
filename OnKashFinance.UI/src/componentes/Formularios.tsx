"use client";
import { useEffect, useState } from "react";
import { Campo } from "@/componentes/Base";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { requisicao } from "@/servicos/api";
import type { Conta } from "@/tipos/api";
type Tipo = "conta" | "categoria" | "cliente" | "fornecedor" | "cartao";
type Dados = Record<string, string | number | boolean | undefined>;
const dataParaDia = (valor: string) => Number(valor.split("/")[0]);
export function FormularioSimples({
  tipo,
  rota,
  concluir,
  iniciais,
  metodo = "POST",
}: {
  tipo: Tipo;
  rota: string;
  concluir: () => void;
  iniciais?: Dados;
  metodo?: "POST" | "PUT";
}) {
  const { sessao } = useAutenticacao();
  const [dados, setDados] = useState<Dados>({
    tipo: tipo === "categoria" ? "ENTRADA" : "",
    ativo: "true",
    ...iniciais,
  });
  const [erro, setErro] = useState("");
  const [salvando, setSalvando] = useState(false);
  const handleCampoChange =
    (campo: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setDados({ ...dados, [campo]: e.target.value });
  const valor = (campo: string) => String(dados[campo] ?? "");
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSalvando(true);
    try {
      const corpo: Dados = { ...dados };
      if (tipo === "conta" && metodo === "POST") {
        corpo.saldoInicial = Number(dados.saldoInicial || 0);
      }

      if (tipo === "conta" && metodo === "PUT") {
        delete corpo.saldoInicial;
      }
      if (tipo === "cartao") {
        corpo.limite = Number(dados.limite);
        corpo.diaFechamento = dataParaDia(String(dados.dataFechamento));
        corpo.diaVencimento = dataParaDia(String(dados.dataVencimento));
        delete corpo.dataFechamento;
        delete corpo.dataVencimento;
      }
      if (typeof corpo.ativo === "string") corpo.ativo = corpo.ativo === "true";
      await requisicao(rota, { method: metodo, body: JSON.stringify(corpo) }, sessao?.token);
      concluir();
    } catch (f) {
      setErro(f instanceof Error ? f.message : "Não foi possível salvar.");
    } finally {
      setSalvando(false);
    }
  };
  return (
    <form onSubmit={handleSubmit} className="formulario">
      {(tipo === "conta" || tipo === "cartao") && (
        <Campo label="Nome" required value={valor("nome")} onChange={handleCampoChange("nome")} />
      )}{" "}
      {tipo === "conta" && (
        <>
          <Campo
            label="Tipo da conta"
            required
            value={valor("tipo")}
            onChange={handleCampoChange("tipo")}
          />
          {metodo === "POST" && (
            <Campo
              label="Saldo inicial"
              type="number"
              min="0"
              step="0.01"
              required
              value={valor("saldoInicial")}
              onChange={handleCampoChange("saldoInicial")}
            />
          )}
        </>
      )}{" "}
      {tipo === "categoria" && (
        <>
          <Campo label="Nome" required value={valor("nome")} onChange={handleCampoChange("nome")} />
          <label className="campo">
            Tipo
            <select value={valor("tipo")} onChange={handleCampoChange("tipo")}>
              <option value="ENTRADA">Entrada</option>
              <option value="SAIDA">Saída</option>
            </select>
          </label>
        </>
      )}{" "}
      {(tipo === "cliente" || tipo === "fornecedor") && (
        <>
          <Campo
            label="Nome ou razão social"
            required
            value={valor("nomeRazaoSocial")}
            onChange={handleCampoChange("nomeRazaoSocial")}
          />
          <Campo
            label="CPF/CNPJ"
            value={valor("cpfCnpj")}
            onChange={handleCampoChange("cpfCnpj")}
          />
          <Campo
            label="Telefone"
            value={valor("telefone")}
            onChange={handleCampoChange("telefone")}
          />
          <Campo
            label="E-mail"
            type="email"
            value={valor("email")}
            onChange={handleCampoChange("email")}
          />
          <Campo
            label="Observação"
            value={valor("observacao")}
            onChange={handleCampoChange("observacao")}
          />
        </>
      )}{" "}
      {tipo === "cartao" && (
        <>
          <Campo
            label="Instituição"
            required
            value={valor("instituicao")}
            onChange={handleCampoChange("instituicao")}
          />
          <Campo
            label="Limite"
            required
            type="number"
            step="0.01"
            min="0"
            value={valor("limite")}
            onChange={handleCampoChange("limite")}
          />
          <Campo
            label="Data de fechamento (dd/mm/aaaa)"
            placeholder="dd/mm/aaaa"
            inputMode="numeric"
            pattern="\d{2}/\d{2}/\d{4}"
            required
            value={valor("dataFechamento")}
            onChange={handleCampoChange("dataFechamento")}
          />
          <Campo
            label="Data de vencimento (dd/mm/aaaa)"
            placeholder="dd/mm/aaaa"
            inputMode="numeric"
            pattern="\d{2}/\d{2}/\d{4}"
            required
            value={valor("dataVencimento")}
            onChange={handleCampoChange("dataVencimento")}
          />
        </>
      )}
      {erro && <p className="mensagem erro">{erro}</p>}
      <button className="botao" disabled={salvando}>
        {salvando ? "Salvando..." : metodo === "PUT" ? "Salvar alterações" : "Salvar cadastro"}
      </button>
    </form>
  );
}
export function FormularioBaixa({
  rota,
  receber,
  concluir,
}: {
  rota: string;
  receber: boolean;
  concluir: () => void;
}) {
  const { sessao } = useAutenticacao();
  const [contas, setContas] = useState<Conta[]>([]);
  const [erro, setErro] = useState("");
  const [salvando, setSalvando] = useState(false);
  useEffect(() => {
    if (sessao)
      void requisicao<Conta[]>(
        receber ? "/api/empresarial/contas" : "/api/pessoal/contas",
        {},
        sessao.token,
      )
        .then(setContas)
        .catch(() => setErro("Não foi possível carregar as contas."));
  }, [sessao, receber]);
  const enviar = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!sessao) return;
    const f = new FormData(e.currentTarget);
    setSalvando(true);
    try {
      await requisicao(
        rota,
        {
          method: "POST",
          body: JSON.stringify({
            contaId: f.get("contaId"),
            [receber ? "dataRecebimento" : "dataPagamento"]: f.get("data"),
          }),
        },
        sessao.token,
      );
      concluir();
    } catch {
      setErro("Não foi possível concluir a operação.");
    } finally {
      setSalvando(false);
    }
  };
  return (
    <form className="formulario" onSubmit={enviar}>
      <label className="campo">
        Conta
        <select name="contaId" required>
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
      <Campo
        label={receber ? "Data do recebimento" : "Data do pagamento"}
        name="data"
        type="date"
        required
      />
      {erro && <p className="mensagem erro">{erro}</p>}
      <button className="botao" disabled={salvando}>
        {salvando ? "Salvando..." : "Confirmar"}
      </button>
    </form>
  );
}
