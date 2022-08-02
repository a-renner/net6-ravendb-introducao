using CadastroClienteRavenDb.Models;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace CadastroClienteRavenDb.Controllers;

[ApiController, Route("[controller]")]
public class ClientesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Cliente>> Inserir(Cliente cliente)
    {
        using (var session = DocumentStoreHolder.Store.OpenAsyncSession())
        {
            await session.StoreAsync(cliente);
            await session.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, cliente);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> Obter(string id)
    {
        using var session = DocumentStoreHolder.Store.OpenAsyncSession();
        var cliente = await session.LoadAsync<Cliente>(id);
        return cliente is null ? NotFound("Cliente não encontrado") : Ok(cliente);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> Listar()
    {
        using var session = DocumentStoreHolder.Store.OpenAsyncSession();
        var clientes = await session.Query<Cliente>().ToListAsync();
        return Ok(clientes);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Cliente>> Alterar([FromBody] Cliente clienteAlterado, [FromRoute] string id)
    {
        using (var session = DocumentStoreHolder.Store.OpenAsyncSession())
        {
            var cliente = await session.LoadAsync<Cliente>(id);

            cliente.Nome = clienteAlterado.Nome;
            cliente.Email = clienteAlterado.Email;
            cliente.Endereco = clienteAlterado.Endereco;
            
            await session.SaveChangesAsync();
        }

        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Deletar(string id)
    {
        using (var session = DocumentStoreHolder.Store.OpenAsyncSession())
        {
            var cliente = await session.LoadAsync<Cliente>(id);
            session.Delete(cliente);
            await session.SaveChangesAsync();
        }

        return Ok();
    }
}