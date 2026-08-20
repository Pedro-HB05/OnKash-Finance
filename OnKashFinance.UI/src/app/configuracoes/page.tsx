"use client";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { AlternadorTema } from "@/componentes/AlternadorTema";
export default function Configuracoes() {
  const { sessao } = useAutenticacao();
  if (!sessao) return null;
  return (
    <AreaAutenticada tipo={sessao.tipoConta === "PESSOAL" ? "pessoal" : "empresarial"}>
      <section className="pagina-config">
        <p className="sobre-titulo">Preferências</p>
        <h1>Configurações</h1>
        <section className="bloco-config">
          <h2>Aparência</h2>
          <div className="linha-config">
            <div>
              <strong>Tema</strong>
              <p>Escolha a aparência mais confortável para você.</p>
            </div>
            <AlternadorTema />
          </div>
        </section>
        <section className="bloco-config">
          <h2>Preferências</h2>
          <dl>
            <div>
              <dt>Idioma</dt>
              <dd>Português (Brasil)</dd>
            </div>
            <div>
              <dt>Moeda</dt>
              <dd>Real brasileiro (BRL)</dd>
            </div>
          </dl>
        </section>
      </section>
    </AreaAutenticada>
  );
}
