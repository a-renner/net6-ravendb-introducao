# RavenDB e .NET - Primeiros passos

Esse artigo tem como objetivo apresentar o RavenDB através da construção de um serviço de cadastro de clientes. Apenas para fim de exemplo, este serviço possui os endpoints necessários para um CRUD e não se preocupa em seguir padrões de desenvolvimento.

## RavenDB

[RavenDB](https://ravendb.net) é um banco NoSQL do tipo documento desenvolvido pela [Hibernating Rhinos](https://hibernatingrhinos.com) em .NET. Evidenciam-se como destaque sua alta performance e escalabilidade, além de ser um banco com as propriedades ACID (Atomicity, Consistency, Isolation, Durability).

Há mais de 10 anos no mercado, o RavenDB dispõe bibliotecas de *clients* para diversas linguagens e uma baixa complexidade de implementação para desenvolvedores. Dispensando um DBA mesmo em alguns casos onde a performance é uma necessidade.

O RavenDB possui uma linguagem de query chamada RQL (Raven Query Language) similar ao SQL e uma indexação dinâmica, onde todas as consultas realizadas utilizam índices. A cada nova versão uma série de features são [adicionadas](https://ravendb.net/why-ravendb/whats-new). Nesse [link](https://ravendb.net/features) é possível visualizar seus principais recursos.

## **Conceitos**

Para a criação da aplicação precisamos ter alguns conceitos claros em mente:

**Documentos**

Para o RavenDB um documento se refere à qualquer dado serializado, como por exemplo, formatos JSON e XML. Cada documento contém uma chave única para sua identificação. Quando não informamos essa chave um identificador único é gerado automaticamente.

**Coleções**

No RavenDB os documentos são salvos no mesmo espaço em disco, neste cenário as coleções são formas de agrupar documentos do mesmo tipo logicamente.

## **Instalação**

O RavenDB é um banco multiplataforma, é possível o instalar no Windows, Linux, Mac OS ou Raspberry Pi. Para o exemplo utilizarei uma imagem docker com os seguintes parâmetros:

```docker
docker run -p 8080:8080 --name RavenDb-PrimeirosPassos -e RAVEN_Setup_Mode=None -e RAVEN_License_Eula_Accepted=true -e RAVEN_Security_UnsecuredAccessAllowed=PrivateNetwork ravendb/ravendb
```

Após a execução do comando acima o terminal exibira uma mensagem como esta:

![1Untitled](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/40357477-cae3-4275-8c52-6b78fa074eea)


Neste primeiro artigo sobre RavenDB não entrarei em detalhes da instalação.

A forma de instalação não influencia no restante do artigo. Caso você queira realizar a instalação de outra maneira basta seguir esses [exemplos](https://ravendb.net/docs/article-page/5.3/csharp/start/installation/setup-wizard).

## **Management Studio**

O RavenDB possui uma interface de gerenciamento que pode ser acessado pelo browser sem a necessidade de realizar nenhum tipo de download. Podemos apenas acessar a porta parametrizada. Em nosso caso: http://localhost:8080/.

Dentro de diversos recursos do Management Studio podemos realizar queries, gerenciar servidores e manipular índices.

![Visualização da dashboard principal do Management Studio](![2Untitled](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/f658293e-8b9f-478e-829d-304ce4fd0e45)

Visualização da dashboard principal do Management Studio

## **Criação da Base**

Para criarmos uma base de dados utilizando o Management Studio podemos ir em Database → “New database” e informar o nome da base. Neste exemplo utilizarei o nome “MinhaOrganizacao”.

![Untitled](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/e79bf872-1919-4f00-99a9-06cd0c043454)


## Implementação no .NET 6

Para iniciar nosso exemplo podemos criar uma Web API e adicionarmos o pacote de *client* do RavenDB. Executando pela CLI do .NET:

```csharp
dotnet add package RavenDB.Client -v 5.4.1
```

Assim podemos criar a classe DocumentStoreHolder que será um Singleton da interface **IDocumentStore**:

```csharp
public static class DocumentStoreHolder
{
    private static readonly Lazy<IDocumentStore> LazyStore = new(() =>
    {
        return new DocumentStore
        {
            Urls = new[] { "http://localhost:8080/" },
            Database = "MinhaOrganizacao",
            Conventions = new DocumentConventions
            {
                IdentityPartsSeparator = '-'
            }
        }.Initialize();
    });

    public static IDocumentStore Store => LazyStore.Value;
}
```

O **DocumentStore** é responsável por gerenciar a comunicação entre a aplicação e o RavenDB. Essa comunicação é realizada por requests HTTP.

O DocumentStore nos permite configurar algumas propriedades; para o exemplo utilizaremos a seguinte configuração:

**Urls:** A lista das URLs do cluster do RavenDB.

**Database:** O nome da base de dados.

**Conventions:**  Opções customizadas de comportamentos do nosso *client*. No exemplo estou utilizando o ‘-’ como separador das partes que compõem o id dos documentos.

OBS: Vale ressaltar que o padrão Singleton é recomendado pela [documentação](https://ravendb.net/docs/article-page/5.4/csharp/client-api/creating-document-store) do RavenDb pelo aumento da utilização de recursos que mais de uma instância pode causar.

Para continuarmos precisamos criar a classe Cliente que representará o cliente que queremos cadastrar:

```csharp
public class Cliente
{
    public string? Id { get; }

    public string? Nome { get; set; }

    public string? Email { get; set; }

    public Endereco? Endereco { get; set; }
}
```

```csharp
public record Endereco(string Logradouro, string Cidade)
{
}
```

### Inclusão de Clientes

Adicionamos a Controller Clientes e criamos um método POST:

```csharp
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
```

Explicando o código: Uma **Session** representa uma única transação e garante a integridade dos dados. A partir da Session podemos utilizar o método **StoreAsync** para salvar nosso novo cliente no banco de dados e em seguida persisti-lo com o **SaveChangesAsync**. O método SaveChangesAsync persiste todas as operações realizadas na session. 

Executando a nossa API e realizando a requisição teremos o seguinte resultado:

![Dados enviados na requisição](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/df8e1557-6a75-48ff-aa35-75089f71fd7e)

Dados enviados na requisição

![Retorno da requisição](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/426b15e7-1012-4378-9268-43a16d760018)

Retorno da requisição

### **Observando os registros:**

Acessando o Mangement Studio podemos notar a criação da coleção “Clientes” e do documento referente ao cliente criado.

![6Untitled](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/6c64810a-603c-41b1-bc2a-f30d8a4580bc)

A coleção é criada automaticamente e pluralizada de acordo com o nome da classe que estamos inserindo.

Como não informamos um ID o RavenDB gerou o mesmo de acordo com a configuração que informamos no nosso DocumentStore, inclusive utilizando o ‘-’ como separador das partes do ID.

Repare que também são persistidos dados de metadatas.

### Obter Cliente por ID

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Cliente>> Obter(string id)
{
    using var session = DocumentStoreHolder.Store.OpenAsyncSession();
    var cliente = await session.LoadAsync<Cliente>(id);
    return cliente is null ? NotFound("Cliente não encontrado") : Ok(cliente);
}
```

Para obter o cliente podemos chamar o método **LoadAsync** também disponível na nossa session.

![7Untitled](https://github.com/a-renner/net6-ravendb-introducao/assets/110235420/90c202a2-9250-4de4-8c30-320c5c45471c)


### Listando todos os clientes

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Cliente>>> Listar()
{
    using var session = DocumentStoreHolder.Store.OpenAsyncSession();
    var clientes = await session.Query<Cliente>().ToListAsync();
    return Ok(clientes);
}
```

A Session também nos permite realizar queries na sintaxe LINQ. No exemplo acima estou apenas listando todos os documentos da coleção “Clientes” com a instrução **Query**.

### Alterando o cliente

```csharp
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
```

Para alterarmos um documento podemos carrega-lo como fizemos anteriormente com o método **LoadAsync**, ****modificarmos as propriedades desejadas e então persistir com o método **SaveChangesAsync**. A API do RavenDB nos proporciona o track da entidade.

### Deletando o cliente

```csharp
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
```

Para deletarmos o registro utilizamos o método **Delete** presente na Session.

### O projeto está disponível no [Github](https://github.com/arirennerf/net6-ravendb-introducao).

## Finalizando

Como podemos visualizar neste artigo, o RavenDB é um banco de dados NoSQL extremamente fácil de instalar, configurar e implantar. Claro que com um software mais robustos as soluções são diferentes, mas o RavenDB conta com diversos recursos para os mais variados cenários e necessidades.

Em um futuro próximo pretendo escrever um novo artigo focado na realização de queries no RavenDB.

## Referências:

[RavenDB - An ACID NoSQL Document Database](https://github.com/ravendb/ravendb)

[ravendb.net](https://ravendb.net/docs/article-page/5.4/csharp)

[Zero to RavenDB](https://ravendb.net/learn/inside-ravendb-book/reader/4.0/2-zero-to-ravendb)

Syn-Hershko, Itamar. **RavenDB in Action.** Manning Publications, 2015.
