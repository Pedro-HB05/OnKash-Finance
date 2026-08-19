namespace OnKashFinance.API.DTOs;

public class DashboardPessoalResposta
{
    public decimal Saldo { get; set; }

    public decimal Entradas { get; set; }

    public decimal Saidas { get; set; }

    public decimal ResultadoMes { get; set; }
}

public class DashboardEmpresarialResposta
{
    public decimal Saldo { get; set; }

    public decimal Entradas { get; set; }

    public decimal Saidas { get; set; }

    public decimal Resultado { get; set; }

    public decimal ContasAPagar { get; set; }

    public decimal ContasAReceber { get; set; }

    public decimal ValoresVencidos { get; set; }

    public decimal PagarVencido { get; set; }

    public decimal ReceberVencido { get; set; }
}

public class GastoCategoriaResposta
{
    public Guid? CategoriaId { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public decimal Total { get; set; }
}

public class ResumoMensalResposta
{
    public DateOnly Mes { get; set; }

    public decimal Entradas { get; set; }

    public decimal Saidas { get; set; }

    public decimal Resultado { get; set; }
}