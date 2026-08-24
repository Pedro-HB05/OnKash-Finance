"use client";

import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";

export type AcaoMenu = { rotulo: string; executar: () => void; perigosa?: boolean };

export function MenuAcoes({ acoes }: { acoes: AcaoMenu[] }) {
  const [aberto, setAberto] = useState(false);
  const referencia = useRef<HTMLDivElement>(null);
  const referenciaBotao = useRef<HTMLButtonElement>(null);
  const referenciaMenu = useRef<HTMLDivElement>(null);
  const [posicao, setPosicao] = useState({ top: 0, right: 0, acima: false });

  useEffect(() => {
    const fecharFora = (evento: MouseEvent) => {
      const alvo = evento.target as Node;
      if (!referencia.current?.contains(alvo) && !referenciaMenu.current?.contains(alvo)) setAberto(false);
    };
    const fecharEscape = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") setAberto(false);
    };
    const fecharAoMoverPagina = () => setAberto(false);
    document.addEventListener("mousedown", fecharFora);
    document.addEventListener("keydown", fecharEscape);
    window.addEventListener("resize", fecharAoMoverPagina);
    window.addEventListener("scroll", fecharAoMoverPagina, true);
    return () => {
      document.removeEventListener("mousedown", fecharFora);
      document.removeEventListener("keydown", fecharEscape);
      window.removeEventListener("resize", fecharAoMoverPagina);
      window.removeEventListener("scroll", fecharAoMoverPagina, true);
    };
  }, []);

  if (!acoes.length) return <span className="sem-acoes">—</span>;
  return (
    <div className="menu-acoes" ref={referencia}>
      <button
        ref={referenciaBotao}
        className="botao-mais"
        type="button"
        aria-label="Mais opções"
        title="Mais opções"
        aria-expanded={aberto}
        onClick={() => {
          if (!aberto && referenciaBotao.current) {
            const caixa = referenciaBotao.current.getBoundingClientRect();
            const acima = window.innerHeight - caixa.bottom < 180;
            setPosicao({ top: acima ? caixa.top - 8 : caixa.bottom + 6, right: window.innerWidth - caixa.right, acima });
          }
          setAberto((valor) => !valor);
        }}
      >
        ⋮
      </button>
      {aberto && createPortal(
        <div ref={referenciaMenu} className="dropdown-acoes dropdown-acoes-flutuante" role="menu" style={{ top: posicao.top, right: posicao.right, transform: posicao.acima ? "translateY(-100%)" : undefined }}>
          {acoes.map((acao) => (
            <button
              key={acao.rotulo}
              type="button"
              role="menuitem"
              className={acao.perigosa ? "perigosa" : ""}
              onClick={() => {
                setAberto(false);
                acao.executar();
              }}
            >
              {acao.rotulo}
            </button>
          ))}
        </div>, document.body
      )}
    </div>
  );
}

export function ConfirmacaoAcao({
  descricao,
  confirmar,
  textoConfirmar,
  fechar,
  processando = false,
  perigosa = true,
}: {
  descricao: string;
  confirmar: () => void;
  textoConfirmar: string;
  fechar: () => void;
  processando?: boolean;
  perigosa?: boolean;
}) {
  return (
    <div className="confirmacao-acao">
      <p>{descricao}</p>
      <div className="acoes-confirmacao">
        <button type="button" className="botao secundario" onClick={fechar} disabled={processando}>
          Voltar
        </button>
        <button type="button" className={perigosa ? "botao perigo" : "botao"} onClick={confirmar} disabled={processando}>
          {processando ? "Processando..." : textoConfirmar}
        </button>
      </div>
    </div>
  );
}
