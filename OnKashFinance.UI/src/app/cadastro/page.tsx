"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { cadastrar } from "@/servicos/api";
import { ArrowRight, Building2, Check, UserRound } from "lucide-react";
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
    <main className="autenticacao">
      <section className="autenticacao-apresentacao cadastro-apresentacao" aria-label="Benefícios do OnKash Finance">
        <div className="marca marca-autenticacao">OnKash <span>Finance</span></div>
        <div className="autenticacao-chamada">
          <span className="selo-autenticacao"><Check size={16} /> Comece em poucos minutos</span>
          <h1>Seu dinheiro merece uma visão mais simples.</h1>
          <p>Crie sua conta e transforme movimentações financeiras em informações úteis para o seu dia a dia.</p>
          <ul className="lista-beneficios">
            <li><Check size={17} /> Organização pessoal ou empresarial</li>
            <li><Check size={17} /> Acompanhamento claro de receitas e despesas</li>
            <li><Check size={17} /> Decisões baseadas em dados reais</li>
          </ul>
        </div>
        <p className="autenticacao-rodape">Simples de começar. Fácil de acompanhar.</p>
      </section>
      <section className="autenticacao-conteudo">
      <form className="cartao-login cadastro" onSubmit={enviar}>
        <div className="marca marca-mobile">OnKash <span>Finance</span></div>
        <div className="cabecalho-autenticacao">
          <span className="sobre-titulo">Comece agora</span>
          <h2>Crie sua conta</h2>
          <p>Escolha como você deseja usar o OnKash.</p>
        </div>
        <div className="opcoes-cadastro" role="radiogroup" aria-label="Tipo de conta">
          <button
            type="button"
            className={tipo === "PESSOAL" ? "opcao selecionada" : "opcao"}
            onClick={() => setTipo("PESSOAL")}
            role="radio"
            aria-checked={tipo === "PESSOAL"}
          >
            <span className="icone-opcao"><UserRound size={23} /></span>
            <strong>Uso pessoal</strong>
            <small>Organize contas, lançamentos, cartões e finanças pessoais.</small>
            <span className="acao-opcao">Selecionar <ArrowRight size={16} /></span>
          </button>
          <button
            type="button"
            className={tipo === "EMPRESARIAL" ? "opcao selecionada" : "opcao"}
            onClick={() => setTipo("EMPRESARIAL")}
            role="radio"
            aria-checked={tipo === "EMPRESARIAL"}
          >
            <span className="icone-opcao"><Building2 size={23} /></span>
            <strong>Uso empresarial</strong>
            <small>Gerencie receitas, despesas e a rotina da empresa.</small>
            <span className="acao-opcao">Selecionar <ArrowRight size={16} /></span>
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
        <button className="botao botao-autenticacao" disabled={!tipo || enviando}>
          <span>{enviando ? "Criando conta..." : "Criar minha conta"}</span>
          {!enviando && <ArrowRight size={19} />}
        </button>
        <p>
          Já possui uma conta? <Link href="/login">Entrar</Link>
        </p>
      </form>
      </section>
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
