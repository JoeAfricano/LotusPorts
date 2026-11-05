# Lotus Ports — continuidade no Claude Code

Responda em português brasileiro. Código, nomes de classes e commits em inglês; interface e documentação em PT-BR.
Especificação completa: `docs/PROMPT_MESTRE.md`. As decisões abaixo **prevalecem** sobre o prompt mestre onde divergem.

## Regras Invioláveis (resumo)

- Não remover nem achatar funcionalidades aprovadas. Simplificar a interface, sim; remover a função, não.
- Sem falso sucesso: estados `OK`, `AVISO`, `FALHA`, `NÃO VERIFICADO` (`CheckState`). Nada de dados, hashes, versões ou chaves inventados.
- Incompleto aparece como **EXPERIMENTAL** ou **EM DESENVOLVIMENTO**.
- Toda alteração real pede confirmação, grava ANTES/DEPOIS no diário (`JournaledOperation` + `IOperationJournal`) e pode ser desfeita.
- Proibido: buscar, baixar, indexar ou linkar ROMs, ISOs, BIOS, firmware ou chaves; assets originais; quebra de DRM. Downloads só das releases oficiais do repositório declarado. Sem telemetria. App roda como `asInvoker`.
- Texto para leigos; jargão só no modo avançado. Uma ação principal por tela/cartão.

## Decisões do José (05/11/2026)

- .NET 10 (LTS), WinUI 3 unpackaged, **self-contained** completo. Asserções com AwesomeAssertions (Apache-2.0).
- `Lotus.Design` e `Lotus.Integration.Contracts` ficam isolados aqui; virarão o repositório `Lotus.Shared` (submódulo git do Lotus e do Lotus Ports) quando o José pedir.
- **GitHub: não usar** até o José informar. Só git local, branch `main`.
- **Saves e backups ficam com o Lotus**, que já faz isso. O Lotus Ports não tem seção de saves, backups antes de atualizar, tabela `Backups` nem operações Backup/Restore. Atualizar ainda migra `persistentPaths`, e o rollback de versão continua.
- **A arte conceitual prevalece** sobre a seção 9: navegação lateral (Biblioteca, Catálogo, Downloads, Integração com o Lotus, Configurações, Ajuda), hero com cena japonesa, cartões e botões rosé. O item "Saves e Backups" da arte foi retirado pela decisão acima. Tokens ajustados à paleta da arte (`docs/UX.md`).

## Arquitetura

`src/`: `LotusPorts.App` (WinUI, Views, ViewModels) · `LotusPorts.Core` (domínio, sem WinUI/SQLite/HTTP) · `LotusPorts.Infrastructure` (SQLite + Dapper, pastas) · `LotusPorts.Catalog` e `LotusPorts.Integration` (vazios até as fases 1 e 5) · `Lotus.Integration.Contracts` (JSON-RPC, framing, nomes do protocolo, sem dependências) · `Lotus.Design` (tokens, estilos, LotusPetal, StateBadge, EmptyState, SakuraHero). Detalhes em `docs/ARQUITETURA.md`.

Dados em `%LOCALAPPDATA%\LotusPorts\` (`data\lotusports.db`, `logs\`). Migrações em `Infrastructure/Database/Migrations` (versão em `PRAGMA user_version`).
Textos em `src/LotusPorts.App/Strings/{pt-BR,en-US}/Resources.resw`; no XAML use `{local:Str Key=...}`. Um teste exige as mesmas chaves nos dois idiomas, e outro proíbe cor solta (`#RRGGBB`) nas telas.

## Comandos

```
dotnet build LotusPorts.slnx -p:Platform=x64
dotnet test LotusPorts.slnx -p:Platform=x64
src\LotusPorts.App\bin\Debug\net10.0-windows10.0.26100.0\win-x64\LotusPorts.exe [--page=library|catalog|downloads|lotus|settings|help|game]
```
`--page` só funciona em Debug, para abrir telas que ainda não têm caminho de navegação.

## Estado

- **Fase 0 (Fundação): concluída em 05/11/2026.** Build Debug sem avisos nem erros, 40 testes aprovados. Telas conferidas por captura de tela. Detalhes e pendências em `docs/PENDENCIAS.md`.
- Próxima: Fase 1 (Catálogo por jogo). Aguardar o comando do José.
