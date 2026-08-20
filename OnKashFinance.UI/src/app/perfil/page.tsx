"use client";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { AreaAutenticada } from "@/componentes/AreaAutenticada";
export default function Perfil() {
  const { sessao } = useAutenticacao();
  if (!sessao) return null;
  return (
    <AreaAutenticada tipo={sessao.tipoConta === "PESSOAL" ? "pessoal" : "empresarial"}>
      <section className="pagina-config">
        <p className="sobre-titulo">Conta</p>
        <h1>Meu perfil</h1>
        <div className="perfil-card">
          <span className="avatar grande">
            {sessao.nome
              .split(" ")
              .slice(0, 2)
              .map((x) => x[0])
              .join("")
              .toUpperCase()}
          </span>
          <div>
            <h2>{sessao.nome}</h2>
            <p>{sessao.email}</p>
          </div>
        </div>
        <section className="bloco-config">
          <h2>Informações pessoais</h2>
          <dl>
            <div>
              <dt>Nome</dt>
              <dd>{sessao.nome}</dd>
            </div>
            <div>
              <dt>E-mail</dt>
              <dd>{sessao.email}</dd>
            </div>
            <div>
              <dt>Tipo de conta</dt>
              <dd>{sessao.tipoConta === "PESSOAL" ? "Pessoal" : "Empresarial"}</dd>
            </div>
            {sessao.perfil && (
              <div>
                <dt>Perfil</dt>
                <dd>{sessao.perfil === "ADMINISTRADOR" ? "Administrador" : "Funcionário"}</dd>
              </div>
            )}
          </dl>
        </section>
      </section>
    </AreaAutenticada>
  );
}
