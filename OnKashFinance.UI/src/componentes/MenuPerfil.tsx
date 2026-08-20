"use client";
import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { Settings, UserRound, LogOut, Sun, Moon } from "lucide-react";
import { useRouter } from "next/navigation";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";

const iniciais = (nome: string) => nome.split(" ").filter(Boolean).slice(0, 2).map(parte => parte[0]).join("").toUpperCase();
export function MenuPerfil() {
  const { sessao, sair } = useAutenticacao(); const [aberto, setAberto] = useState(false); const [escuro, setEscuro] = useState(false); const ref = useRef<HTMLDivElement>(null); const router = useRouter();
  useEffect(() => { setEscuro(document.documentElement.dataset.theme === "escuro"); const fora = (evento: MouseEvent) => { if (!ref.current?.contains(evento.target as Node)) setAberto(false); }; const tecla = (evento: KeyboardEvent) => { if (evento.key === "Escape") setAberto(false); }; document.addEventListener("mousedown", fora); document.addEventListener("keydown", tecla); return () => { document.removeEventListener("mousedown", fora); document.removeEventListener("keydown", tecla); }; }, []);
  const trocarTema = () => { const proximo = !escuro; setEscuro(proximo); localStorage.setItem("onkash.tema", proximo ? "escuro" : "claro"); document.documentElement.dataset.theme = proximo ? "escuro" : "claro"; };
  if (!sessao) return null;
  return <div className="menu-perfil" ref={ref}><button className="avatar" aria-label="Abrir menu do perfil" aria-expanded={aberto} onClick={() => setAberto(!aberto)}>{iniciais(sessao.nome)}</button>{aberto && <div className="dropdown-perfil" role="menu"><div className="perfil-resumo"><span className="avatar grande">{iniciais(sessao.nome)}</span><div><strong>{sessao.nome}</strong><small>{sessao.email}</small></div></div><Link href="/perfil" role="menuitem" onClick={() => setAberto(false)}><UserRound size={17}/>Meu perfil</Link><Link href="/configuracoes" role="menuitem" onClick={() => setAberto(false)}><Settings size={17}/>Configurações</Link><button role="menuitem" onClick={trocarTema}>{escuro ? <Sun size={17}/> : <Moon size={17}/>}{escuro ? "Usar modo claro" : "Usar modo escuro"}</button><button className="sair-menu" role="menuitem" onClick={() => { sair(); router.replace("/login"); }}><LogOut size={17}/>Sair</button></div>}</div>;
}
