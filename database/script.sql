-- Script de criação do banco de dados (SQLite)
-- Projeto: Cadastro de Produtos - Checkpoint 5
--
-- Como executar:
--   1) Instale o utilitário sqlite3 (https://sqlite.org/download.html) ou use uma
--      ferramenta gráfica como o "DB Browser for SQLite".
--   2) No terminal, dentro da pasta do projeto, execute:
--        sqlite3 database/produtos.db < database/script.sql
--      (no Windows PowerShell: Get-Content database/script.sql | sqlite3 database/produtos.db)
--   3) O arquivo database/produtos.db será criado com a tabela Produtos pronta para uso.
--
-- Observação: a própria aplicação também cria esta tabela automaticamente na primeira
-- execução (caso ainda não exista), utilizando exatamente o mesmo comando abaixo.
-- Este script é fornecido para permitir a criação manual/independente do banco.

CREATE TABLE IF NOT EXISTS Produtos (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT    NOT NULL,
    Preco     REAL    NOT NULL CHECK (Preco >= 0),
    Estoque   INTEGER NOT NULL CHECK (Estoque >= 0),
    Categoria TEXT    NOT NULL
);
