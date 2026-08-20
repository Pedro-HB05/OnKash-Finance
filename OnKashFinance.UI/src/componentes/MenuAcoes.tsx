"use client";

import { useEffect, useRef, useState } from "react";

export type AcaoMenu = { rotulo: string; executar: () => void; perigosa?: boolean };

export function MenuAcoes({ acoes }: { acoes: AcaoMenu[] }) {
  const [aberto, setAberto] = useState(false);
  const referencia = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const fecharFora = (evento: MouseEvent) => {
      if (!referencia.current?.contains(evento.target as Node)) setAberto(false);
    };
    const fecharEscape = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") setAberto(false);
    };
    document.addEventListener("mousedown", fecharFora);
    document.addEventListener("keydown", fecharEscape);
    return () => {
      document.removeEventListener("mousedown", fecharFora);
      document.removeEventListener("keydown", fecharEscape);
    };
  }, []);

  if (!acoes.length) return <span className="sem-acoes">—</span>;
  return <div className="menu-acoes" ref={referencia}>
    <button className="botao-mais" type="button" aria-label="Mais opções" title="Mais opções" aria-expanded={aberto} onClick={() => setAberto((valor) => !valor)}>⋮</button>
    {aberto && <div className="dropdown-acoes" role="menu">
      {acoes.map((acao) => <button key={acao.rotulo} type="button" role="menuitem" className={acao.perigosa ? "perigosa" : ""} onClick={() => { setAberto(false); acao.executar(); }}>{acao.rotulo}</button>)}
    </div>}
  </div>;
}

export function ConfirmacaoAcao({ descricao, confirmar, textoConfirmar, fechar, processando = false }: { descricao: string; confirmar: () => void; textoConfirmar: string; fechar: () => void; processando?: boolean }) {
  return <div className="confirmacao-acao">
    <p>{descricao}</p>
    <div className="acoes-confirmacao"><button type="button" className="botao secundario" onClick={fechar} disabled={processando}>Voltar</button><button type="button" className="botao perigo" onClick={confirmar} disabled={processando}>{processando ? "Processando..." : textoConfirmar}</button></div>
  </div>;
}
