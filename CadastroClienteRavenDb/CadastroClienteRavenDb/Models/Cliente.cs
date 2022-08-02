namespace CadastroClienteRavenDb.Models;

public class Cliente
{
    public string? Id { get; }

    public string? Nome { get; set; }

    public string? Email { get; set; }

    public Endereco? Endereco { get; set; }
}