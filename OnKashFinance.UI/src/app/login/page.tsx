"use client";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { entrar } from "@/servicos/api";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { ArrowRight, BarChart3, LockKeyhole, ShieldCheck } from "lucide-react";

export default function Login() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [aviso, setAviso] = useState("");
  const [precisaVerificar, setPrecisaVerificar] = useState(false);
  const { iniciar } = useAutenticacao();
  const router = useRouter();
  useEffect(() => {
    const motivo = new URLSearchParams(window.location.search).get("motivo");
    if (motivo === "sessao-expirada")
      setAviso("Sua sessão expirou por segurança. Entre novamente para continuar.");
    if (motivo === "email-verificado")
      setAviso("E-mail confirmado com sucesso. Você já pode entrar.");
  }, []);
  const enviar = async (evento: React.FormEvent) => {
    evento.preventDefault();
    setEnviando(true);
    setErro("");
    setPrecisaVerificar(false);
    try {
      const sessao = await entrar(email, senha);
      iniciar(sessao);
      router.replace(
        sessao.tipoConta === "PESSOAL" ? "/pessoal/visao-geral" : "/empresarial/visao-geral",
      );
    } catch (falha) {
      const mensagem = falha instanceof Error ? falha.message : "Não foi possível entrar.";
      setErro(mensagem);
      setPrecisaVerificar(mensagem.toLowerCase().includes("não verificado"));
    } finally {
      setEnviando(false);
    }
  };
  return (
    <main className="autenticacao">
      <section className="autenticacao-apresentacao" aria-label="Sobre o OnKash Finance">
        <div className="marca marca-autenticacao">OnKash <span>Finance</span></div>
        <div className="autenticacao-chamada">
          <span className="selo-autenticacao"><ShieldCheck size={16} /> Finanças sob controle</span>
          <h1>Clareza para decidir.<br />Controle para crescer.</h1>
          <p>Organize sua vida financeira em um só lugar, acompanhe cada movimento e tome decisões com mais tranquilidade.</p>
          <div className="beneficios-autenticacao">
            <div><BarChart3 size={20} /><span><strong>Visão completa</strong><small>Seus números de forma simples e visual.</small></span></div>
            <div><LockKeyhole size={20} /><span><strong>Acesso seguro</strong><small>Seus dados protegidos e sempre disponíveis.</small></span></div>
          </div>
        </div>
        <p className="autenticacao-rodape">Gestão financeira pessoal e empresarial</p>
      </section>
      <section className="autenticacao-conteudo">
      <form className="cartao-login" onSubmit={enviar}>
        <div className="marca marca-mobile">OnKash <span>Finance</span></div>
        <div className="cabecalho-autenticacao">
          <span className="sobre-titulo">Acesse sua conta</span>
          <h2>Bem-vindo de volta</h2>
          <p>Entre para continuar cuidando das suas finanças.</p>
        </div>
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
        {aviso && !erro && <p className="mensagem aviso" role="status">{aviso}</p>}
        {precisaVerificar && <Link className="link-verificacao" href={`/verificar-email?email=${encodeURIComponent(email)}`}>Validar meu e-mail agora</Link>}
        <button className="botao botao-autenticacao" disabled={enviando}>
          <span>{enviando ? "Entrando..." : "Entrar"}</span>{!enviando && <ArrowRight size={19} />}
        </button>
        <p className="link-cadastro">
          Não tem conta? <Link href="/cadastro">Crie agora</Link>
        </p>
        <p className="links-legais-auth"><Link href="/privacidade">Privacidade</Link><span>·</span><Link href="/termos">Termos de Uso</Link></p>
      </form>
      </section>
    </main>
  );
}
