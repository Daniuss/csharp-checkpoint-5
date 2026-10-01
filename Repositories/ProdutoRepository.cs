using System.Data.Common;
using Checkpoint5.Models;
using Checkpoint5.Services;
using Microsoft.Data.Sqlite;

namespace Checkpoint5.Repositories;

/// <summary>
/// Responsável exclusivamente pelo acesso ao banco de dados (ADO.NET puro, sem ORM).
/// Todos os comandos SQL utilizam parâmetros para evitar SQL Injection.
/// </summary>
public class ProdutoRepository
{
    private readonly string _connectionString;

    public ProdutoRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        // O SQLite cria o arquivo do banco automaticamente, mas não cria a pasta onde ele fica.
        var dataSource = new SqliteConnectionStringBuilder(_connectionString).DataSource;
        var diretorio = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(diretorio))
            Directory.CreateDirectory(diretorio);
    }

    /// <summary>
    /// Garante que a tabela Produtos exista. Equivalente ao conteúdo de database/script.sql.
    /// </summary>
    public void GarantirBancoCriado()
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS Produtos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Preco REAL NOT NULL CHECK (Preco >= 0),
                Estoque INTEGER NOT NULL CHECK (Estoque >= 0),
                Categoria TEXT NOT NULL
            );
            """;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao inicializar o banco de dados: {ex.Message}");
            throw;
        }
    }

    public int Inserir(Produto produto)
    {
        const string sql = """
            INSERT INTO Produtos (Nome, Preco, Estoque, Categoria)
            VALUES (@Nome, @Preco, @Estoque, @Categoria);
            """;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add("@Nome", SqliteType.Text).Value = produto.Nome;
            command.Parameters.Add("@Preco", SqliteType.Real).Value = produto.Preco;
            command.Parameters.Add("@Estoque", SqliteType.Integer).Value = produto.Estoque;
            command.Parameters.Add("@Categoria", SqliteType.Text).Value = produto.Categoria;

            command.ExecuteNonQuery();

            using var comandoId = connection.CreateCommand();
            comandoId.CommandText = "SELECT last_insert_rowid();";
            var novoId = Convert.ToInt32((long)comandoId.ExecuteScalar()!);

            produto.Id = novoId;
            return novoId;
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao inserir produto '{produto.Nome}': {ex.Message}");
            throw;
        }
    }

    public List<Produto> Listar()
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos ORDER BY Id;";
        var produtos = new List<Produto>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(MapearProduto(reader));
            }

            return produtos;
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao listar produtos: {ex.Message}");
            throw;
        }
    }

    public Produto? BuscarPorId(int id)
    {
        const string sql = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @Id;";

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add("@Id", SqliteType.Integer).Value = id;

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearProduto(reader) : null;
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao buscar produto por Id {id}: {ex.Message}");
            throw;
        }
    }

    public bool Atualizar(Produto produto)
    {
        const string sql = """
            UPDATE Produtos
            SET Nome = @Nome, Preco = @Preco, Estoque = @Estoque, Categoria = @Categoria
            WHERE Id = @Id;
            """;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add("@Nome", SqliteType.Text).Value = produto.Nome;
            command.Parameters.Add("@Preco", SqliteType.Real).Value = produto.Preco;
            command.Parameters.Add("@Estoque", SqliteType.Integer).Value = produto.Estoque;
            command.Parameters.Add("@Categoria", SqliteType.Text).Value = produto.Categoria;
            command.Parameters.Add("@Id", SqliteType.Integer).Value = produto.Id;

            var linhasAfetadas = command.ExecuteNonQuery();
            return linhasAfetadas > 0;
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao atualizar produto Id {produto.Id}: {ex.Message}");
            throw;
        }
    }

    public bool Excluir(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id;";

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add("@Id", SqliteType.Integer).Value = id;

            var linhasAfetadas = command.ExecuteNonQuery();
            return linhasAfetadas > 0;
        }
        catch (DbException ex)
        {
            Logger.Error($"Erro ao excluir produto Id {id}: {ex.Message}");
            throw;
        }
    }

    private static Produto MapearProduto(DbDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Nome = reader.GetString(reader.GetOrdinal("Nome")),
        Preco = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("Preco"))),
        Estoque = reader.GetInt32(reader.GetOrdinal("Estoque")),
        Categoria = reader.GetString(reader.GetOrdinal("Categoria"))
    };
}
