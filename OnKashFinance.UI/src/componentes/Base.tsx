"use client";

import { useEffect, useState } from "react";
import { ErroApi, requisicao } from "@/servicos/api";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { data, moeda, textoEnum } from "@/utilitarios/formatadores";

export function Cabecalho({ titulo, descricao, acao }: { titulo: string; descricao: string; acao?: React.ReactNode }) { return <header className="cabecalho"><div><h1>{titulo}</h1><p>{descricao}</p></div>{acao}</header>; }
export function Badge({ valor }: { valor: string | boolean }) { const texto = typeof valor === "boolean" ? (valor ? "Ativo" : "Inativo") : textoEnum(valor); return <span className={`badge ${String(valor).toLowerCase()}`}>{texto}</span>; }
export function Modal({ titulo, children, fechar }: { titulo: string; children: React.ReactNode; fechar: () => void }) { return <div className="fundo-modal" role="presentation"><section className="modal" role="dialog" aria-modal="true" aria-labelledby="titulo-modal"><button className="fechar" onClick={fechar} aria-label="Fechar">×</button><h2 id="titulo-modal">{titulo}</h2>{children}</section></div>; }
export type Coluna<T> = { titulo: string; valor: (item: T) => React.ReactNode };
export function Lista<T extends { id?: string; empresaUsuarioId?: string }>({ titulo, descricao, rota, colunas, renderFormulario, renderAcoes }: { titulo: string; descricao: string; rota: string; colunas: Coluna<T>[]; renderFormulario: (concluir: () => void) => React.ReactNode; renderAcoes?: (item: T, recarregar: () => void) => React.ReactNode }) {
  const { sessao, sair } = useAutenticacao(); const [itens, setItens] = useState<T[]>([]); const [erro, setErro] = useState(""); const [carregando, setCarregando] = useState(true); const [abrir, setAbrir] = useState(false);
  const carregar = async () => { if (!sessao) return; setCarregando(true); setErro(""); try { setItens(await requisicao<T[]>(rota, {}, sessao.token)); } catch (erroApi) { const erroResposta = erroApi as ErroApi; if (erroResposta.status === 401) sair(); setErro(erroResposta.message); } finally { setCarregando(false); } };
  useEffect(() => { void carregar(); }, [sessao]);
  return <><Cabecalho titulo={titulo} descricao={descricao} acao={<button className="botao" onClick={() => setAbrir(true)}>Novo cadastro</button>} />{erro ? <p className="mensagem erro">{erro}</p> : carregando ? <p className="estado">Carregando informações...</p> : itens.length === 0 ? <p className="estado">Você ainda não possui nenhum cadastro.</p> : <div className="tabela"><table><thead><tr>{colunas.map((coluna) => <th key={coluna.titulo}>{coluna.titulo}</th>)}{renderAcoes && <th aria-label="Mais opções" />}</tr></thead><tbody>{itens.map((item) => <tr key={item.id ?? item.empresaUsuarioId}>{colunas.map((coluna) => <td key={coluna.titulo} data-label={coluna.titulo}>{coluna.valor(item)}</td>)}{renderAcoes && <td data-label="Mais opções">{renderAcoes(item, carregar)}</td>}</tr>)}</tbody></table></div>}{abrir && <Modal titulo={`Novo cadastro: ${titulo}`} fechar={() => setAbrir(false)}>{renderFormulario(() => { setAbrir(false); void carregar(); })}</Modal>}</>;
}
export function Campo({ label, ...props }: { label: string } & React.InputHTMLAttributes<HTMLInputElement>) { const id = props.id ?? label.toLowerCase().replaceAll(" ", "-"); return <label className="campo" htmlFor={id}>{label}<input id={id} {...props} /></label>; }
export function formatarData(valor?: string) { return data(valor); } export function formatarMoeda(valor?: number) { return moeda(valor); }
