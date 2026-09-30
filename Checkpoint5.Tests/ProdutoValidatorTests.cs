using Checkpoint5.Models;
using Checkpoint5.Services;
using Xunit;

namespace Checkpoint5.Tests;

public class ProdutoValidatorTests
{
    [Fact]
    public void Validar_ProdutoValido_NaoRetornaErros()
    {
        var produto = new Produto { Nome = "Notebook", Preco = 3500m, Estoque = 10, Categoria = "Informática" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Empty(erros);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validar_NomeVazioOuNulo_RetornaErro(string? nome)
    {
        var produto = new Produto { Nome = nome!, Preco = 10m, Estoque = 1, Categoria = "Categoria" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Contains(erros, e => e.Contains("Nome"));
    }

    [Fact]
    public void Validar_PrecoNegativo_RetornaErro()
    {
        var produto = new Produto { Nome = "Mouse", Preco = -1m, Estoque = 5, Categoria = "Periféricos" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Contains(erros, e => e.Contains("Preço"));
    }

    [Fact]
    public void Validar_EstoqueNegativo_RetornaErro()
    {
        var produto = new Produto { Nome = "Teclado", Preco = 100m, Estoque = -5, Categoria = "Periféricos" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Contains(erros, e => e.Contains("Estoque"));
    }

    [Fact]
    public void Validar_CategoriaVazia_RetornaErro()
    {
        var produto = new Produto { Nome = "Monitor", Preco = 800m, Estoque = 3, Categoria = "" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Contains(erros, e => e.Contains("Categoria"));
    }

    [Fact]
    public void Validar_MultiplosCamposInvalidos_RetornaTodosOsErros()
    {
        var produto = new Produto { Nome = "", Preco = -10m, Estoque = -1, Categoria = "" };

        var erros = ProdutoValidator.Validar(produto);

        Assert.Equal(4, erros.Count);
    }

    [Fact]
    public void EhValido_ProdutoValido_RetornaTrue()
    {
        var produto = new Produto { Nome = "Cadeira", Preco = 450m, Estoque = 2, Categoria = "Móveis" };

        Assert.True(ProdutoValidator.EhValido(produto));
    }

    [Fact]
    public void EhValido_ProdutoInvalido_RetornaFalse()
    {
        var produto = new Produto { Nome = "", Preco = 450m, Estoque = 2, Categoria = "Móveis" };

        Assert.False(ProdutoValidator.EhValido(produto));
    }
}
