namespace OnKashFinance.API.Modelos;

public enum TipoContaUsuario
{
    PESSOAL,
    EMPRESARIAL
}

public enum TipoCategoria
{
    ENTRADA,
    SAIDA
}

public enum TipoLancamentoPessoal
{
    ENTRADA,
    SAIDA,
    TRANSFERENCIA
}

public enum StatusFatura
{
    ABERTA,
    FECHADA,
    PAGA,
    ATRASADA
}

public enum TipoLancamentoEmpresarial
{
    RECEITA,
    DESPESA,
    TRANSFERENCIA
}

public enum StatusContaPagar
{
    PENDENTE,
    PAGO,
    ATRASADO,
    CANCELADO
}

public enum StatusContaReceber
{
    PENDENTE,
    RECEBIDO,
    ATRASADO,
    CANCELADO
}

public enum PerfilEmpresa
{
    ADMINISTRADOR,
    FUNCIONARIO
}
