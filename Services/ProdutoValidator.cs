using Checkpoint5.Models;

namespace Checkpoint5.Services;

/// <summary>
/// Regras de validação de Produto, isoladas do banco de dados para permitir testes unitários.
/// </summary>
public static class ProdutoValidator
{
    /// <summary>
    /// Valida um produto e retorna a lista de erros encontrados (vazia se o produto for válido).
    /// </summary>
    public static List<string> Validar(Produto produto)
    {
        var erros = new List<string>();

        if (produto is null)
        {
            erros.Add("Produto não informado.");
            return erros;
        }

        if (string.IsNullOrWhiteSpace(produto.Nome))
            erros.Add("Nome é obrigatório.");

        if (produto.Preco < 0)
            erros.Add("Preço não pode ser negativo.");

        if (produto.Estoque < 0)
            erros.Add("Estoque não pode ser negativo.");

        if (string.IsNullOrWhiteSpace(produto.Categoria))
            erros.Add("Categoria é obrigatória.");

        return erros;
    }

    public static bool EhValido(Produto produto) => Validar(produto).Count == 0;
}
