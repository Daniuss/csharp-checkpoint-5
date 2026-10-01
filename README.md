# Cadastro de Produtos — Checkpoint 5 (ADO.NET)

## 1. Sobre o projeto

Aplicação de console em C# para cadastro e gerenciamento de produtos, desenvolvida para
demonstrar o uso de **ADO.NET puro** (sem ORM) na persistência de dados em banco relacional.
A aplicação implementa um CRUD completo (inserir, listar, buscar, atualizar e excluir
produtos), utilizando SQL parametrizado, tratamento de exceções de banco de dados e registro
de logs em arquivo.

## 2. Tecnologias utilizadas

- **C# 14 / .NET 10**
- **ADO.NET** (via `Microsoft.Data.Sqlite`, um provider ADO.NET — não é ORM)
- **SQLite** como banco de dados
- **Microsoft.Extensions.Configuration** para leitura do `appsettings.json`
- **xUnit** para testes unitários (bônus)

## 3. Estrutura do projeto

```
Checkpoint5.csproj          Projeto principal (console)
Program.cs                  Interface de menu (console)
appsettings.json            Connection string (NUNCA hardcoded no código)
Models/
  Produto.cs                Classe de domínio Produto
Repositories/
  ProdutoRepository.cs      Único ponto de acesso ao banco (ADO.NET + SQL parametrizado)
Services/
  Logger.cs                 Gravação de logs em logs/app.log
  ProdutoValidator.cs       Regras de validação (testáveis sem banco de dados)
database/
  script.sql                Script de criação da tabela Produtos
logs/
  .gitkeep                  Mantém a pasta no git (o app.log é gerado em runtime)
Checkpoint5.Tests/
  ProdutoValidatorTests.cs  Testes unitários (xUnit)
setup.bat                   Script de configuração/execução rápida (Windows)
```

Fluxo da aplicação:

```
Program.cs (menu)
      ↓
ProdutoRepository (SQL parametrizado)
      ↓
ADO.NET (Microsoft.Data.Sqlite)
      ↓
Banco de Dados (SQLite)
```

Nenhum SQL é escrito no `Program.cs` — toda a lógica de acesso a dados está isolada em
`ProdutoRepository`.

## 4. Pré-requisitos

- [.NET SDK 10.0 ou superior](https://dotnet.microsoft.com/download) instalado.
- Não é necessário instalar SQL Server nem SQLite manualmente: a biblioteca
  `Microsoft.Data.Sqlite` já inclui o motor SQLite embarcado (via pacote NuGet), e o arquivo
  de banco (`database/produtos.db`) é criado automaticamente na primeira execução.

### Por que SQLite?

SQLite foi escolhido em vez de SQL Server/LocalDB porque não exige instalação de um serviço
de banco de dados separado nem configuração adicional — o banco é um único arquivo local
(`database/produtos.db`), o que torna o projeto simples de clonar, configurar e executar em
qualquer máquina com o .NET SDK instalado, sem depender de infraestrutura externa. O acesso
continua sendo feito via ADO.NET puro (`Microsoft.Data.Sqlite`), sem uso de ORM.

## 5. Configuração

A connection string fica em `appsettings.json`, na raiz do projeto:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=database/produtos.db"
  }
}
```

Não há segredos ou credenciais nesse arquivo (SQLite não usa usuário/senha). Caso queira
apontar para outro caminho de arquivo, basta alterar o valor de `Data Source`.

## 6. Banco de dados

O script de criação da tabela está em [`database/script.sql`](database/script.sql):

```sql
CREATE TABLE IF NOT EXISTS Produtos (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT    NOT NULL,
    Preco     REAL    NOT NULL CHECK (Preco >= 0),
    Estoque   INTEGER NOT NULL CHECK (Estoque >= 0),
    Categoria TEXT    NOT NULL
);
```

**Criação automática:** a própria aplicação executa esse mesmo comando (`GarantirBancoCriado`
em `ProdutoRepository`) na primeira inicialização, criando `database/produtos.db` caso ele
ainda não exista. Não é necessário nenhum passo manual para rodar o projeto.

**Criação manual (opcional):** se quiser recriar o banco manualmente (por exemplo, para
inspecionar a tabela antes de rodar a aplicação), use o utilitário `sqlite3` ou uma ferramenta
gráfica como o [DB Browser for SQLite](https://sqlitebrowser.org/):

```bash
sqlite3 database/produtos.db < database/script.sql
```

No PowerShell:

```powershell
Get-Content database/script.sql | sqlite3 database/produtos.db
```

## 7. Execução

Na raiz do projeto:

```bash
dotnet restore
dotnet build
dotnet run --project Checkpoint5.csproj
```

Ou, no Windows, basta executar `setup.bat` (restaura, compila e roda automaticamente).

## 8. Funcionalidades

Menu principal:

1. **Inserir produto** — cadastra um novo produto (`INSERT` via `ExecuteNonQuery`)
2. **Listar produtos** — lista todos os produtos (`SELECT` via `ExecuteReader`)
3. **Buscar produto por ID** — busca um produto específico (`SELECT ... WHERE Id = @Id`)
4. **Atualizar produto** — atualiza os dados de um produto existente (`UPDATE` via `ExecuteNonQuery`)
5. **Excluir produto** — remove um produto (com confirmação) (`DELETE` via `ExecuteNonQuery`)
6. **Sair** — encerra a aplicação

Após cada operação o usuário retorna automaticamente ao menu principal.

## 9. Segurança (SQL Injection)

Todos os comandos SQL usam **parâmetros tipados** (`SqliteParameter` via
`command.Parameters.Add("@Param", SqliteType.X)`), nunca concatenação de strings com dados
do usuário. Exemplo (`ProdutoRepository.BuscarPorId`):

```csharp
command.CommandText = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @Id;";
command.Parameters.Add("@Id", SqliteType.Integer).Value = id;
```

Isso garante que valores fornecidos pelo usuário (nome, categoria, etc.) sejam sempre
tratados como dados, e nunca interpretados como parte do comando SQL.

## 10. Logs

Todas as operações (inserção, listagem, busca, atualização, exclusão e erros) são
registradas em `logs/app.log`, com data/hora, no formato:

```
[2026-09-30 20:15:32] INFO - Produto inserido: Notebook Dell (Id 1)
[2026-09-30 20:16:02] INFO - Produtos listados (total: 2)
[2026-09-30 20:16:45] ERROR - Erro ao buscar produto por Id 99: ...
```

Nenhuma informação sensível (como connection strings completas ou dados de outros usuários)
é gravada no log — apenas nome/ID dos produtos e mensagens de erro do provider de banco.

## 11. Tratamento de erros

- **Erros de banco de dados:** todos os métodos de `ProdutoRepository` capturam
  `System.Data.Common.DbException` (classe base implementada por `SqliteException`),
  registram o erro no log e relançam a exceção. O `Program.cs` captura `DbException` no
  laço principal do menu, exibe uma mensagem amigável ao usuário e mantém a aplicação em
  execução (não fecha o programa).
- **Entradas inválidas do usuário:** IDs não numéricos, preços/estoques inválidos ou
  negativos e campos obrigatórios vazios são validados antes de qualquer chamada ao banco
  (`ProdutoValidator` e os métodos `LerTexto`/`LerDecimal`/`LerInteiro` em `Program.cs`),
  solicitando novamente a entrada até que um valor válido seja informado.

### Sobre transações

As operações de CRUD implementadas (inserir, atualizar, excluir) executam um único comando
SQL cada, que já é atômico no SQLite por padrão. Por isso, não há necessidade de transações
explícitas (`BeginTransaction`) neste projeto — seu uso adicionaria complexidade sem
benefício real, já que não há operações compostas por múltiplos comandos dependentes entre
si.

## 12. Testes unitários (bônus)

O projeto `Checkpoint5.Tests` contém testes unitários com **xUnit** para as regras de
validação de `ProdutoValidator`, que não dependem do banco de dados:

```bash
dotnet test
```

Resultado obtido ao rodar os testes:

```
Aprovado!  – Com falha: 0, Aprovado: 10, Ignorado: 0, Total: 10
```

## 13. Demonstração (roteiro rápido)

1. Execute `dotnet run --project Checkpoint5.csproj` (ou `setup.bat`).
2. Escolha `1` e cadastre um produto (ex.: Notebook, 3500.50, 10, Informática).
3. Escolha `2` para listar e confirmar que o produto aparece na tabela.
4. Escolha `3` e informe o ID retornado para buscar o produto específico.
5. Escolha `4` para atualizar o preço/estoque do produto e confirme com `3` novamente.
6. Escolha `5` para excluir o produto (confirme com `S`) e liste novamente com `2` para
   confirmar a remoção.
7. Escolha `6` para sair.
8. Abra `logs/app.log` para conferir o registro de todas as operações realizadas.

## Prints para entrega

Capture pelo menos estas três evidências em funcionamento:

1. **Inserção de produto** — tela do menu após inserir um produto com sucesso (opção 1).
2. **Listagem e busca** — tela da listagem (opção 2) e/ou busca por ID (opção 3) mostrando o
   produto cadastrado.
3. **Atualização ou exclusão** — tela confirmando a atualização (opção 4) ou exclusão
   (opção 5) de um produto, seguida da listagem confirmando a mudança.

Opcionalmente, inclua também um print do arquivo `logs/app.log` mostrando o histórico de
operações.
