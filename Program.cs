using System.Data.Common;
using System.Globalization;
using Checkpoint5.Models;
using Checkpoint5.Repositories;
using Checkpoint5.Services;
using Microsoft.Extensions.Configuration;

var culturaBr = CultureInfo.GetCultureInfo("pt-BR");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada em appsettings.json.");

var repositorio = new ProdutoRepository(connectionString);

try
{
    repositorio.GarantirBancoCriado();
}
catch (DbException)
{
    Console.WriteLine("Não foi possível inicializar o banco de dados. Verifique o log em logs/app.log.");
    return;
}

ExibirMenuPrincipal();

void ExibirMenuPrincipal()
{
    var continuar = true;

    while (continuar)
    {
        Console.WriteLine();
        Console.WriteLine("====================================");
        Console.WriteLine("       CADASTRO DE PRODUTOS");
        Console.WriteLine("====================================");
        Console.WriteLine("1. Inserir produto");
        Console.WriteLine("2. Listar produtos");
        Console.WriteLine("3. Buscar produto por ID");
        Console.WriteLine("4. Atualizar produto");
        Console.WriteLine("5. Excluir produto");
        Console.WriteLine("6. Sair");
        Console.WriteLine();
        Console.Write("Escolha uma opção: ");

        var opcao = Console.ReadLine();

        try
        {
            switch (opcao)
            {
                case "1":
                    InserirProduto();
                    break;
                case "2":
                    ListarProdutos();
                    break;
                case "3":
                    BuscarProdutoPorId();
                    break;
                case "4":
                    AtualizarProduto();
                    break;
                case "5":
                    ExcluirProduto();
                    break;
                case "6":
                    continuar = false;
                    Console.WriteLine("Encerrando a aplicação...");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro de banco de dados: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Ocorreu um erro ao acessar o banco de dados. Detalhes foram registrados em logs/app.log.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Erro inesperado: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Ocorreu um erro inesperado. Detalhes foram registrados em logs/app.log.");
        }
    }
}

void InserirProduto()
{
    Console.WriteLine();
    Console.WriteLine("--- Inserir produto ---");

    var nome = LerTexto("Nome: ");
    var preco = LerDecimal("Preço: ");
    var estoque = LerInteiro("Estoque: ");
    var categoria = LerTexto("Categoria: ");

    var produto = new Produto { Nome = nome, Preco = preco, Estoque = estoque, Categoria = categoria };

    var erros = ProdutoValidator.Validar(produto);
    if (erros.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Não foi possível cadastrar o produto:");
        foreach (var erro in erros)
            Console.WriteLine($" - {erro}");
        return;
    }

    var id = repositorio.Inserir(produto);
    Logger.Info($"Produto inserido: {produto.Nome} (Id {id})");

    Console.WriteLine();
    Console.WriteLine($"Produto cadastrado com sucesso! Id: {id}");
}

void ListarProdutos()
{
    Console.WriteLine();
    Console.WriteLine("--- Lista de produtos ---");

    var produtos = repositorio.Listar();
    Logger.Info($"Produtos listados (total: {produtos.Count})");

    if (produtos.Count == 0)
    {
        Console.WriteLine("Nenhum produto cadastrado.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"{"Id",-5} {"Nome",-25} {"Preço",-12} {"Estoque",-10} {"Categoria",-15}");
    Console.WriteLine(new string('-', 70));
    foreach (var produto in produtos)
    {
        var precoFormatado = produto.Preco.ToString("C2", culturaBr);
        Console.WriteLine($"{produto.Id,-5} {produto.Nome,-25} {precoFormatado,-12} {produto.Estoque,-10} {produto.Categoria,-15}");
    }
}

void BuscarProdutoPorId()
{
    Console.WriteLine();
    Console.WriteLine("--- Buscar produto por ID ---");

    var id = LerInteiro("Id: ");
    var produto = repositorio.BuscarPorId(id);
    Logger.Info($"Busca realizada para o Id {id}");

    if (produto is null)
    {
        Console.WriteLine("Produto não encontrado.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Id: {produto.Id}");
    Console.WriteLine($"Nome: {produto.Nome}");
    Console.WriteLine($"Preço: {produto.Preco.ToString("C2", culturaBr)}");
    Console.WriteLine($"Estoque: {produto.Estoque}");
    Console.WriteLine($"Categoria: {produto.Categoria}");
}

void AtualizarProduto()
{
    Console.WriteLine();
    Console.WriteLine("--- Atualizar produto ---");

    var id = LerInteiro("Id do produto a atualizar: ");
    var existente = repositorio.BuscarPorId(id);

    if (existente is null)
    {
        Console.WriteLine("Produto não encontrado.");
        return;
    }

    Console.WriteLine($"Produto atual: {existente.Nome}, {existente.Preco.ToString("C2", culturaBr)}, Estoque {existente.Estoque}, {existente.Categoria}");
    Console.WriteLine("Informe os novos dados:");

    var nome = LerTexto("Nome: ");
    var preco = LerDecimal("Preço: ");
    var estoque = LerInteiro("Estoque: ");
    var categoria = LerTexto("Categoria: ");

    var produtoAtualizado = new Produto { Id = id, Nome = nome, Preco = preco, Estoque = estoque, Categoria = categoria };

    var erros = ProdutoValidator.Validar(produtoAtualizado);
    if (erros.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Não foi possível atualizar o produto:");
        foreach (var erro in erros)
            Console.WriteLine($" - {erro}");
        return;
    }

    var sucesso = repositorio.Atualizar(produtoAtualizado);
    if (sucesso)
    {
        Logger.Info($"Produto atualizado: Id {id}");
        Console.WriteLine("Produto atualizado com sucesso!");
    }
    else
    {
        Console.WriteLine("Não foi possível atualizar o produto.");
    }
}

void ExcluirProduto()
{
    Console.WriteLine();
    Console.WriteLine("--- Excluir produto ---");

    var id = LerInteiro("Id do produto a excluir: ");
    var existente = repositorio.BuscarPorId(id);

    if (existente is null)
    {
        Console.WriteLine("Produto não encontrado.");
        return;
    }

    Console.Write($"Confirma a exclusão de '{existente.Nome}'? (S/N): ");
    var confirmacao = Console.ReadLine();

    if (!string.Equals(confirmacao, "S", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Operação cancelada.");
        return;
    }

    var sucesso = repositorio.Excluir(id);
    if (sucesso)
    {
        Logger.Info($"Produto excluído: Id {id}");
        Console.WriteLine("Produto excluído com sucesso!");
    }
    else
    {
        Console.WriteLine("Não foi possível excluir o produto.");
    }
}

string LerTexto(string mensagem)
{
    while (true)
    {
        Console.Write(mensagem);
        var valor = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(valor))
            return valor.Trim();

        Console.WriteLine("Este campo é obrigatório. Tente novamente.");
    }
}

decimal LerDecimal(string mensagem)
{
    while (true)
    {
        Console.Write(mensagem);
        var valor = Console.ReadLine();
        if (decimal.TryParse(valor, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var resultado) && resultado >= 0)
            return resultado;

        Console.WriteLine("Valor inválido. Informe um número válido e não negativo (use ponto como separador decimal).");
    }
}

int LerInteiro(string mensagem)
{
    while (true)
    {
        Console.Write(mensagem);
        var valor = Console.ReadLine();
        if (int.TryParse(valor, out var resultado) && resultado >= 0)
            return resultado;

        Console.WriteLine("Valor inválido. Informe um número inteiro não negativo.");
    }
}
