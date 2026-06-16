namespace ScheduleClin.Models;

// Nomes dos papéis (roles) do Identity — evita "magic strings" espalhadas
public static class Perfis
{
    public const string Gestor     = "Gestor";
    public const string Secretaria = "Secretaria";
    public const string Psicologo  = "Psicologo";
    public const string Paciente   = "Paciente";

    public static readonly string[] Todos = { Gestor, Secretaria, Psicologo, Paciente };
}
