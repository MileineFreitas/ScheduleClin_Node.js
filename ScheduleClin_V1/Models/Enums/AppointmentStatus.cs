namespace ScheduleClin.Models;

// Status possíveis de uma consulta (Calendar) — evita "magic strings" espalhadas
public static class AppointmentStatus
{
    public const string Pendente = "Pendente";
    public const string Confirmada = "Confirmada";
    public const string ReagendamentoSolicitado = "Reagendamento Solicitado";
    public const string Cancelada = "Cancelada";

    public static readonly string[] Todos = { Pendente, Confirmada, ReagendamentoSolicitado, Cancelada };
}
