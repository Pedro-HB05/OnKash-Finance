export const moeda = (valor?: number) =>
  new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(valor ?? 0);
export const data = (valor?: string) => {
  if (!valor) return "—";
  const correspondencia = valor.match(/^(\d{4})-(\d{2})-(\d{2})/);
  return correspondencia
    ? `${correspondencia[3]}/${correspondencia[2]}/${correspondencia[1]}`
    : "—";
};
export const hojeIso = () => new Date().toISOString().slice(0, 10);
export const textoEnum = (valor?: string) =>
  ({
    ENTRADA: "Entrada",
    SAIDA: "Saída",
    RECEITA: "Receita",
    DESPESA: "Despesa",
    TRANSFERENCIA: "Transferência",
    PENDENTE: "Pendente",
    RECEBIDO: "Recebido",
    PAGO: "Pago",
    ATRASADO: "Atrasado",
    CANCELADO: "Cancelado",
    ABERTA: "Aberta",
    FECHADA: "Fechada",
    PAGA: "Paga",
    ADMINISTRADOR: "Administrador",
    FUNCIONARIO: "Funcionário",
  })[valor ?? ""] ??
  valor ??
  "—";
