"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { cadastrar } from "@/servicos/api";
export default function Cadastro() {
  const [tipo, setTipo] = useState<"PESSOAL" | "EMPRESARIAL" | null>(null),
    [nome, setNome] = useState(""),
    [email, setEmail] = useState(""),
    [senha, setSenha] = useState(""),
    [confirmacao, setConfirmacao] = useState(""),
    [empresa, setEmpresa] = useState(""),
    [erro, setErro] = useState(""),
    [sucesso, setSucesso] = useState(""),
    [enviando, setEnviando] = useState(false);
  const router = useRouter();
  const enviar = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    if (!tipo) {
      setErro("Escolha como deseja usar o OnKash.");
      return;
    }
    if (senha !== confirmacao) {
      setErro("As senhas não coincidem.");
      return;
    }
    setEnviando(true);
    try {
      await cadastrar({
        nome,
        email,
        senha,
        tipoConta: tipo,
        nomeEmpresa: tipo === "EMPRESARIAL" ? empresa : undefined,
      });
      setSucesso("Conta criada com sucesso.");
      setTimeout(() => router.replace("/login"), 700);
    } catch {
      setErro("Não foi possível criar a conta. Verifique os dados informados.");
    } finally {
      setEnviando(false);
    }
  };
  return (
    <main className="login">
      <form className="cartao-login cadastro" onSubmit={enviar}>
        <div className="marca">
          OnKash <span>Finance</span>
        </div>
        <h1>Crie sua conta</h1>
        <p>Escolha como você deseja usar o OnKash.</p>
        <div className="opcoes-cadastro" role="radiogroup" aria-label="Tipo de conta">
          <button
            type="button"
            className={tipo === "PESSOAL" ? "opcao selecionada" : "opcao"}
            onClick={() => setTipo("PESSOAL")}
            role="radio"
            aria-checked={tipo === "PESSOAL"}
          >
            <strong>👤 Uso pessoal</strong>
            <small>Organize contas, lançamentos, cartões e finanças pessoais.</small>
          </button>
          <button
            type="button"
            className={tipo === "EMPRESARIAL" ? "opcao selecionada" : "opcao"}
            onClick={() => setTipo("EMPRESARIAL")}
            role="radio"
            aria-checked={tipo === "EMPRESARIAL"}
          >
            <strong>🏢 Uso empresarial</strong>
            <small>Gerencie receitas, despesas e a rotina da empresa.</small>
          </button>
        </div>
        {tipo && (
          <>
            <Campo label="Nome" value={nome} onChange={setNome} />
            <Campo label="E-mail" type="email" value={email} onChange={setEmail} />
            {tipo === "EMPRESARIAL" && (
              <Campo label="Nome da empresa" value={empresa} onChange={setEmpresa} />
            )}
            <Campo label="Senha" type="password" value={senha} onChange={setSenha} />
            <Campo
              label="Confirmar senha"
              type="password"
              value={confirmacao}
              onChange={setConfirmacao}
            />
          </>
        )}
        {erro && <p className="mensagem erro">{erro}</p>}
        {sucesso && <p className="mensagem sucesso">{sucesso}</p>}
        <button className="botao" disabled={!tipo || enviando}>
          {enviando ? "Criando conta..." : "Criar minha conta"}
        </button>
        <p>
          Já possui uma conta? <Link href="/login">Entrar</Link>
        </p>
      </form>
    </main>
  );
}
function Campo({
  label,
  type = "text",
  value,
  onChange,
}: {
  label: string;
  type?: string;
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <label className="campo">
      {label}
      <input type={type} value={value} onChange={(e) => onChange(e.target.value)} required />
    </label>
  );
}
