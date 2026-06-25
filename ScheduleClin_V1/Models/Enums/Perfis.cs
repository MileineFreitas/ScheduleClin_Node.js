namespace ScheduleClin.Models;

// Nomes dos papéis (roles) do Identity — evita "magic strings" espalhadas
public static class Perfis
{
    public const string Gestor    = "Gestor";
    public const string Psicologo = "Psicologo";
    public const string Paciente  = "Paciente";

    public static readonly string[] Todos = { Gestor, Psicologo, Paciente };

    // Perfis permitidos no cadastro de novos usuários (funcionário ou paciente)
    public static readonly string[] Cadastro = { Psicologo, Paciente };
}
