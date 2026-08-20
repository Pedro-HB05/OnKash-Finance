import type { Sessao } from "@/tipos/api";
const baseUrl = (process.env.NEXT_PUBLIC_API_URL ?? "").replace(/\/$/, "");
export class ErroApi extends Error {
  constructor(
    public status: number,
    mensagem: string,
  ) {
    super(mensagem);
  }
}
const mensagem = (s: number) =>
  ({
    401: "Sua sessão expirou. Entre novamente.",
    403: "Você não tem permissão para acessar esta área.",
    404: "O recurso solicitado não foi encontrado.",
    500: "Não foi possível concluir a operação. Tente novamente.",
  })[s] ?? "Não foi possível concluir a operação. Verifique os dados informados.";
export async function requisicao<T>(
  caminho: string,
  opcoes: RequestInit = {},
  token?: string,
): Promise<T> {
  const resposta = await fetch(`${baseUrl}${caminho}`, {
    ...opcoes,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...opcoes.headers,
    },
  });
  if (!resposta.ok) {
    const corpo = (await resposta.json().catch(() => null)) as {
      mensagem?: string;
      title?: string;
    } | null;
    throw new ErroApi(
      resposta.status,
      corpo?.mensagem ?? corpo?.title ?? mensagem(resposta.status),
    );
  }
  if (resposta.status === 204) return undefined as T;
  return resposta.json() as Promise<T>;
}
export const entrar = (email: string, senha: string) =>
  requisicao<Sessao>("/api/login", { method: "POST", body: JSON.stringify({ email, senha }) });
export const cadastrar = (dados: {
  nome: string;
  email: string;
  senha: string;
  tipoConta: "PESSOAL" | "EMPRESARIAL";
  nomeEmpresa?: string;
}) => requisicao("/api/cadastro", { method: "POST", body: JSON.stringify(dados) });
