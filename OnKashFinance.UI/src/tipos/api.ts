export type TipoConta = "PESSOAL" | "EMPRESARIAL"; export type Perfil = "ADMINISTRADOR" | "FUNCIONARIO";
export interface Sessao { token:string; usuarioId:string; nome:string; email:string; tipoConta:TipoConta; empresaId?:string; perfil?:Perfil; }
export interface Conta { id:string; nome:string; tipo:string; saldoInicial:number; saldoAtual:number; ativo:boolean; }
export interface Categoria { id:string; nome:string; tipo:"ENTRADA"|"SAIDA"; padrao:boolean; ativo:boolean; }
export interface PessoaCadastro { id:string; nomeRazaoSocial:string; cpfCnpj?:string; telefone?:string; email?:string; observacao?:string; ativo:boolean; }
export interface ContaReceber { id:string; clienteId?:string; cliente?:string; descricao:string; valor:number; vencimento:string; dataRecebimento?:string; categoriaId:string; contaId?:string; status:"PENDENTE"|"RECEBIDO"|"ATRASADO"|"CANCELADO"; observacao?:string; }
export interface ContaPagar { id:string; fornecedorId?:string; fornecedor?:string; descricao:string; valor:number; vencimento:string; dataPagamento?:string; categoriaId:string; contaId?:string; status:"PENDENTE"|"PAGO"|"ATRASADO"|"CANCELADO"; observacao?:string; }
export interface LancamentoPessoal { id:string; contaId:string; conta:string; categoriaId?:string; categoria?:string; tipo:"ENTRADA"|"SAIDA"; descricao:string; valor:number; data:string; observacao?:string; cancelado:boolean; }
export interface LancamentoEmpresarial { id:string; tipo:"RECEITA"|"DESPESA"|"TRANSFERENCIA"; contaId:string; conta:string; contaDestinoId?:string; contaDestino?:string; categoriaId?:string; categoria?:string; clienteId?:string; cliente?:string; fornecedorId?:string; fornecedor?:string; contaPagarId?:string; contaReceberId?:string; descricao:string; valor:number; data:string; observacao?:string; cancelado:boolean; criadoEm:string; }
export interface Cartao { id:string; nome:string; instituicao:string; limite:number; diaFechamento:number; diaVencimento:number; ativo:boolean; }
export interface Fatura { id:string; cartaoId:string; cartao:string; competencia:string; dataFechamento:string; dataVencimento:string; status:"ABERTA"|"FECHADA"|"PAGA"|"ATRASADA"; valorTotal:number; }
export interface DashboardPessoal { saldo:number; entradas:number; saidas:number; resultadoMes:number; }
export interface DashboardEmpresarial { saldo:number; entradas:number; saidas:number; resultado:number; contasAPagar:number; contasAReceber:number; valoresVencidos:number; pagarVencido:number; receberVencido:number; }
export interface UsuarioEmpresa { empresaUsuarioId:string; usuarioId:string; nome:string; email:string; perfil:Perfil; ativo:boolean; permissoes?:Record<string,boolean>; }
