using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;

namespace CadastroClienteRavenDb;

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