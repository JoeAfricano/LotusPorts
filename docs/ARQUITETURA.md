# Arquitetura

## Projetos

| Projeto | Papel | Depende de |
|---|---|---|
| `LotusPorts.Core` | Domínio e regras: `CheckState`, diário de operações (`IOperationJournal`, `JournaledOperation`), preferências, regras da pasta da biblioteca, busca, anonimização de caminhos. Não conhece WinUI, SQLite nem HTTP. | — |
| `LotusPorts.Infrastructure` | `AppPaths`, `SqliteDatabase`, `MigrationRunner`, `SqliteSettingsStore`, `SqliteOperationJournal`. | Core, Microsoft.Data.Sqlite, Dapper |
| `LotusPorts.Catalog` | Modelos, schema e carregamento do catálogo (Fase 1). | Core |
| `LotusPorts.Integration` | Cliente e servidor do pipe com o Lotus (Fases 5 e 6). | Core, Contracts |
| `Lotus.Integration.Contracts` | Contrato compartilhado com o Lotus: nomes de pipe, registro, métodos e eventos, `DependencyIds`, mensagem JSON-RPC 2.0 e framing com prefixo de 4 bytes. Sem dependências. | — |
| `Lotus.Design` | Tokens, estilos e controles visuais compartilhados com o Lotus. Sem lógica de app. | Windows App SDK |
| `LotusPorts.App` | WinUI 3: janela, telas, ViewModels (CommunityToolkit.Mvvm), Generic Host com DI e Serilog. | todos |

## Inicialização

`App.OnLaunched`:
1. cria as pastas `data` e `logs`;
2. liga o Serilog (arquivo diário, 14 dias);
3. monta o Host;
4. aplica as migrações (banco mais novo que o app → erro amigável, banco intacto);
5. carrega as preferências e o idioma;
6. abre a janela.

Uma falha na inicialização abre a `ErrorWindow`, com detalhes anonimizados e o botão de copiar.
Exceções não tratadas da interface são registradas e mostradas na mesma janela, e o app continua.

## Diário de operações

Toda operação que altera disco ou sistema passa por `JournaledOperation.RunAsync`:
- grava o estado ANTES (JSON validado);
- executa a operação;
- grava o estado DEPOIS e o resultado.

Em falha ou cancelamento, o resultado também é gravado, e o estado DEPOIS fica nulo (não obtido) em vez de adivinhado.
`ListOpenAsync` lista as operações sem resultado. Chamado na abertura do app, aponta as que foram interrompidas.

## Banco

SQLite com WAL. A migração v1 cria `Settings` e `OperationJournal`. As demais tabelas da seção 13 entram com as fases que as usam. Não haverá tabela `Backups`, porque saves e backups ficam com o Lotus.
