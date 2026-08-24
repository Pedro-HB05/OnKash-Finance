import Link from "next/link";
import { ShieldCheck, WalletCards } from "lucide-react";

export function PaginaLegal({ titulo, subtitulo, children }: { titulo: string; subtitulo: string; children: React.ReactNode }) {
  return <main className="pagina-legal">
    <header className="topo-legal"><Link className="marca" href="/"><span className="simbolo-marca"><WalletCards size={19}/></span><span className="nome-marca">OnKash <em>Finance</em></span></Link><nav><Link href="/privacidade">Privacidade</Link><Link href="/termos">Termos</Link><Link href="/login">Entrar</Link></nav></header>
    <section className="hero-legal"><ShieldCheck size={34}/><p className="sobre-titulo">Transparência e proteção de dados</p><h1>{titulo}</h1><p>{subtitulo}</p><small>Versão 2026-08-24 · Atualizada em 24 de agosto de 2026</small></section>
    <article className="documento-legal">{children}</article>
    <footer className="rodape-legal"><span>© 2026 OnKash Finance</span><span>Controlador: Pedro Henrique Benvento · Curitiba/PR</span><a href="mailto:onkashfinance@gmail.com">onkashfinance@gmail.com</a></footer>
  </main>;
}
