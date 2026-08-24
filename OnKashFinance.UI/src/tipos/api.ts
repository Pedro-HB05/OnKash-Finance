export type TipoConta = "PESSOAL" | "EMPRESARIAL";
export type Perfil = "ADMINISTRADOR" | "FUNCIONARIO";
export interface Sessao {
  token: string;
  usuarioId: string;
  nome: string;
  email: string;
  tipoConta: TipoConta;
  empresaId?: string;
  perfil?: Perfil;
}
export interface Conta {
  id: string;
  nome: string;
  tipo: string;
  saldoInicial: number;
  saldoAtual: number;
  ativo: boolean;
}
export interface Categoria {
  id: string;
  nome: string;
  tipo: "ENTRADA" | "SAIDA";
  padrao: boolean;
  ativo: boolean;
}
export interface PessoaCadastro {
  id: string;
  nomeRazaoSocial: string;
  cpfCnpj?: string;
  telefone?: string;
  email?: string;
  observacao?: string;
  ativo: boolean;
}
export interface ContaReceber {
  id: string;
  clienteId?: string;
  cliente?: string;
  descricao: string;
  valor: number;
  vencimento: string;
  dataRecebimento?: string;
  categoriaId: string;
  contaId?: string;
  status: "PENDENTE" | "RECEBIDO" | "ATRASADO" | "CANCELADO";
  observacao?: string;
}
export interface ContaPagar {
  id: string;
  fornecedorId?: string;
  fornecedor?: string;
  descricao: string;
  valor: number;
  vencimento: string;
  dataPagamento?: string;
  categoriaId: string;
  contaId?: string;
  status: "PENDENTE" | "PAGO" | "ATRASADO" | "CANCELADO";
  observacao?: string;
}
export interface LancamentoPessoal {
  id: string;
  contaId: string;
  conta: string;
  categoriaId?: string;
  categoria?: string;
  tipo: "ENTRADA" | "SAIDA";
  descricao: string;
  valor: number;
  data: string;
  observacao?: string;
  cancelado: boolean;
}
export interface LancamentoEmpresarial {
  id: string;
  tipo: "RECEITA" | "DESPESA" | "TRANSFERENCIA";
  contaId: string;
  conta: string;
  contaDestinoId?: string;
  contaDestino?: string;
  categoriaId?: string;
  categoria?: string;
  clienteId?: string;
  cliente?: string;
  fornecedorId?: string;
  fornecedor?: string;
  contaPagarId?: string;
  contaReceberId?: string;
  descricao: string;
  valor: number;
  data: string;
  observacao?: string;
  cancelado: boolean;
  criadoEm: string;
}
export interface Cartao {
  id: string;
  nome: string;
  instituicao: string;
  limite: number;
  diaFechamento: number;
  diaVencimento: number;
  ativo: boolean;
}
export interface Fatura {
  id: string;
  cartaoId: string;
  cartao: string;
  competencia: string;
  dataFechamento: string;
  dataVencimento: string;
  status: "ABERTA" | "FECHADA" | "PAGA" | "ATRASADA";
  valorTotal: number;
}
export interface DashboardPessoal {
  saldo: number;
  entradas: number;
  saidas: number;
  resultadoMes: number;
  entradasAnteriores: number;
  saidasAnteriores: number;
  resultadoAnterior: number;
}
export interface DashboardEmpresarial {
  saldo: number;
  entradas: number;
  saidas: number;
  resultado: number;
  contasAPagar: number;
  contasAReceber: number;
  valoresVencidos: number;
  pagarVencido: number;
  receberVencido: number;
  entradasAnteriores: number;
  saidasAnteriores: number;
  resultadoAnterior: number;
}
export interface UsuarioEmpresa {
  empresaUsuarioId: string;
  usuarioId: string;
  nome: string;
  email: string;
  perfil: Perfil;
  ativo: boolean;
  permissoes?: Record<string, boolean>;
}
export interface OrcamentoPessoal {
  id: string; categoriaId: string; categoria: string; mes: string;
  limite: number; utilizado: number; percentual: number;
}
export interface RecorrenciaPessoal {
  id: string; contaId: string; conta: string; categoriaId: string; categoria: string;
  tipo: "ENTRADA" | "SAIDA"; descricao: string; valor: number;
  frequencia: "SEMANAL" | "MENSAL" | "ANUAL"; proximaExecucao: string; ativo: boolean;
}
export interface AlertaFinanceiro {
  tipo: string; titulo: string; descricao: string; severidade: "INFO" | "ATENCAO" | "CRITICO"; link?: string;
}
export interface MovimentoImportacao { data: string; descricao: string; valor: number; }
export interface ResultadoImportacao { importados: number; conciliados: number; duplicados: number; }
export interface PontoProjecao { data: string; entradas: number; saidas: number; saldoProjetado: number; }
export interface ProjecaoCaixa { saldoAtual: number; saldoProjetado: number; pontos: PontoProjecao[]; }
export interface LinhaDre { categoria: string; valor: number; }
export interface DreSimplificada { inicio: string; fim: string; receitaBruta: number; despesas: number; resultado: number; margem: number; receitasPorCategoria: LinhaDre[]; despesasPorCategoria: LinhaDre[]; }
export interface AnexoFinanceiro { id: string; nomeArquivo: string; tipoConteudo: string; tamanho: number; criadoEm: string; }
export interface LimiteUso { chave: string; nome: string; utilizado: number; limite?: number; unidade: string; }
export interface PlanoOferta { codigo: string; nome: string; descricao: string; atual: boolean; disponivel: boolean; destaque: boolean; recursos: string[]; }
export interface AssinaturaResumo {
  plano: string; nomePlano: string; status: string; periodoAtualFim?: string;
  uso: LimiteUso[]; planos: PlanoOferta[]; possuiSolicitacaoPendente: boolean;
}
export interface SolicitacaoPrivacidade { protocolo: string; tipo: string; status: string; detalhes?: string; criadoEm: string; concluidoEm?: string; }
export interface PrivacidadeResumo {
  controlador: string; marca: string; localizacao: string; canal: string; versaoAtual: string;
  aceiteAtual: boolean; aceitoEm?: string; solicitacoes: SolicitacaoPrivacidade[];
}
