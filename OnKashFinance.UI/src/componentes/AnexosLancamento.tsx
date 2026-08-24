"use client";
import { useEffect, useState } from "react";
import { Download, FileText, Paperclip, Trash2, UploadCloud } from "lucide-react";
import { Modal } from "@/componentes/Base";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";
import { baixarArquivo, requisicao } from "@/servicos/api";
import type { AnexoFinanceiro } from "@/tipos/api";

export function AnexosLancamento({ tipo, lancamentoId, descricao, fechar }: { tipo: "pessoal" | "empresarial"; lancamentoId: string; descricao: string; fechar: () => void }) {
  const { sessao } = useAutenticacao(); const [itens, setItens] = useState<AnexoFinanceiro[]>([]); const [erro, setErro] = useState(""); const [enviando, setEnviando] = useState(false);
  const base = `/api/${tipo}/inteligencia`;
  const carregar = async () => { if (!sessao) return; try { setItens(await requisicao<AnexoFinanceiro[]>(`${base}/lancamentos/${lancamentoId}/anexos`, {}, sessao.token)); } catch (f) { setErro(f instanceof Error ? f.message : "Não foi possível carregar os anexos."); } };
  useEffect(() => { void carregar(); }, [sessao, lancamentoId]);
  const enviar = async (evento: React.ChangeEvent<HTMLInputElement>) => { const arquivo = evento.target.files?.[0]; if (!arquivo || !sessao) return; setEnviando(true); setErro(""); try { const formulario = new FormData(); formulario.append("arquivo", arquivo); await requisicao(`${base}/lancamentos/${lancamentoId}/anexos`, { method: "POST", body: formulario }, sessao.token); await carregar(); } catch (f) { setErro(f instanceof Error ? f.message : "Não foi possível enviar o arquivo."); } finally { setEnviando(false); evento.target.value = ""; } };
  const baixar = async (item: AnexoFinanceiro) => { if (!sessao) return; try { const blob = await baixarArquivo(`${base}/anexos/${item.id}/arquivo`, sessao.token); const url = URL.createObjectURL(blob); const link = document.createElement("a"); link.href = url; link.download = item.nomeArquivo; link.click(); URL.revokeObjectURL(url); } catch { setErro("Não foi possível baixar o arquivo."); } };
  const excluir = async (id: string) => { if (!sessao) return; try { await requisicao(`${base}/anexos/${id}`, { method: "DELETE" }, sessao.token); await carregar(); } catch { setErro("Não foi possível excluir o anexo."); } };
  return <Modal titulo="Comprovantes e anexos" fechar={fechar}><div className="anexos-lancamento"><p className="descricao-anexo"><Paperclip size={17}/><span>Arquivos de <strong>{descricao}</strong></span></p>{erro && <p className="mensagem erro">{erro}</p>}<label className="drop-anexo"><UploadCloud size={23}/><span><strong>{enviando ? "Enviando arquivo..." : "Adicionar comprovante"}</strong><small>PDF, PNG, JPG ou WEBP — máximo 5 MB</small></span><input type="file" accept="application/pdf,image/png,image/jpeg,image/webp" onChange={enviar} disabled={enviando}/></label>{itens.length === 0 ? <div className="estado-vazio estado-vazio-anexo"><FileText size={25}/><p>Nenhum comprovante anexado.</p></div> : <div className="lista-anexos">{itens.map(item => <article key={item.id}><FileText size={21}/><div><strong>{item.nomeArquivo}</strong><small>{(item.tamanho / 1024).toFixed(0)} KB · {new Date(item.criadoEm).toLocaleDateString("pt-BR")}</small></div><button onClick={() => void baixar(item)} aria-label={`Baixar ${item.nomeArquivo}`}><Download size={18}/></button><button className="perigoso" onClick={() => void excluir(item.id)} aria-label={`Excluir ${item.nomeArquivo}`}><Trash2 size={18}/></button></article>)}</div>}</div></Modal>;
}
