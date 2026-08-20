"use client";
import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { entrar } from "@/servicos/api";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";

export default function Login() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState(false);
  const { iniciar } = useAutenticacao();
  const router = useRouter();
  const enviar = async (evento: React.FormEvent) => {
    evento.preventDefault();
    setEnviando(true);
    setErro("");
    try {
      const sessao = await entrar(email, senha);
      iniciar(sessao);
      router.replace(
        sessao.tipoConta === "PESSOAL" ? "/pessoal/visao-geral" : "/empresarial/visao-geral",
      );
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : "Não foi possível entrar.");
    } finally {
      setEnviando(false);
    }
  };
  return (
    <main className="login">
      <form className="cartao-login" onSubmit={enviar}>
        <div className="marca">
          OnKash <span>Finance</span>
        </div>
        <h1>Bem-vindo</h1>
        <p>Cuide das suas finanças de forma simples.</p>
        <label className="campo">
          E-mail
          <input
            type="email"
            value={email}
            onChange={(evento) => setEmail(evento.target.value)}
            required
            autoComplete="email"
          />
        </label>
        <label className="campo">
          Senha
          <input
            type="password"
            value={senha}
            onChange={(evento) => setSenha(evento.target.value)}
            required
            autoComplete="current-password"
          />
        </label>
        {erro && (
          <p className="mensagem erro" role="alert">
            {erro}
          </p>
        )}
        <button className="botao" disabled={enviando}>
          {enviando ? "Entrando..." : "Entrar"}
        </button>
        <p className="link-cadastro">
          Não tem conta? <Link href="/cadastro">Crie agora</Link>
        </p>
      </form>
    </main>
  );
}
