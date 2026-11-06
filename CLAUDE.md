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
- Validação do catálogo com JsonSchema.Net (MIT). Meta do José: o catálogo com a maior quantidade possível de ports, sem inventar dados.
- **RecompLauncher é a base** (decisão de 05/11/2026): reimplementado em C#, sem copiar código (GPL-3.0), com a cara do Lotus e o pacote de arte premium como base visual. Plano em `docs/PLANO_BASE_RECOMPLAUNCHER.md`.
- **Revisão do catálogo delegada ao Claude**, por critérios escritos (`CatalogScout auto-review`): aprova jogos com forma de abrir conferida; o status "Jogável" exige teste real e não é atribuído sem ele.
- **Capas dos jogos**: buscadas na hora de exibir a partir de libretro-thumbnails (GitHub), reduzidas para JPEG de 400 px num cache local; nunca embutidas no app ou no catálogo (que guarda só o endereço). Sem capa, vale a capa tipográfica.
- Interface intuitiva é o diferencial: animações fluidas e leves (só durante interação, desligadas com "reduzir animações").
- **Navegação**: Início (vitrine), Biblioteca (os jogos do jogador), Catálogo (tudo, com filtros e fontes), Downloads, Integração com o Lotus, Configurações, Ajuda.
- **Biblioteca** = a estante do jogador (prateleiras e filtros Todos/Favoritos/Instalados/Não instalados). **Catálogo** = todos os jogos para descobrir, mais as fontes de catálogo importadas.

## Arquitetura

`src/`: `LotusPorts.App` (WinUI, Views, ViewModels) · `LotusPorts.Core` (domínio, sem WinUI/SQLite/HTTP) · `LotusPorts.Infrastructure` (SQLite + Dapper, pastas, GitHubClient, CatalogService) · `LotusPorts.Catalog` (modelos, validação com JSON Schema, junção de fontes, prateleiras) · `LotusPorts.Integration` (vazio até a fase 5) · `Lotus.Integration.Contracts` (JSON-RPC, framing, nomes do protocolo, sem dependências) · `Lotus.Design` (tokens, estilos, LotusPetal, StateBadge, EmptyState, SakuraHero). Detalhes em `docs/ARQUITETURA.md`.

Dados em `%LOCALAPPDATA%\LotusPorts\` (`data\lotusports.db`, `logs\`). Migrações em `Infrastructure/Database/Migrations` (versão em `PRAGMA user_version`).
Textos em `src/LotusPorts.App/Strings/{pt-BR,en-US}/Resources.resw`, gerados de `tools/strings.tsv` (chave, pt-BR, en-US) com `tools/Generate-Strings.ps1 -Tsv tools/strings.tsv -OutRoot src/LotusPorts.App/Strings`; no XAML use `{local:Str Key=...}`. Um teste exige as mesmas chaves nos dois idiomas, e outro proíbe cor solta (`#RRGGBB`) nas telas.

## Comandos

```
dotnet build LotusPorts.slnx -p:Platform=x64
dotnet test LotusPorts.slnx -p:Platform=x64
src\LotusPorts.App\bin\Debug\net10.0-windows10.0.26100.0\win-x64\LotusPorts.exe [--page=library|catalog|downloads|lotus|settings|help|game]
```
`--page` só funciona em Debug, para abrir telas que ainda não têm caminho de navegação (`--page=game:<id>` abre um jogo).

Catálogo: `catalog/` (schema, `games/`, `ports/`, pacote `catalog.json` embutido) e a ferramenta `tools/CatalogScout` (scan, generate, bundle, approve). Guia completo em `docs/CATALOGO.md`.

## Estado

- **Fase 0 (Fundação): concluída em 05/11/2026.** Build Debug sem avisos nem erros, 40 testes aprovados. Telas conferidas por captura de tela. Detalhes e pendências em `docs/PENDENCIAS.md`.
- **Fase 1 (Catálogo por jogo): concluída em 05/11/2026.** 65 jogos e 70 ports verificados no GitHub (release Windows e executável conferidos), todos `pending` aguardando revisão do José; 77 testes; zero avisos. Pendências em `docs/PENDENCIAS.md` e no "O que revisar" de `docs/CATALOGO.md`.
- **Expansão (05/11/2026):** 628 jogos e 804 ports (descoberta pela lista do recomplauncher.com e pelo tópico pc-port, tudo conferido no GitHub); 552 aprovados, 76 pendentes com motivo; 387 com capa. Início, Biblioteca nova, artes do pacote, animações. 77 testes, zero avisos.
- **06/11/2026:** capas do SteamGridDB (570 de 628; chave só em `%USERPROFILE%\.lotusports\steamgriddb.key`, nunca no repositório). Perfil do jogador lido do Lotus (`%LOCALAPPDATA%\Lotus\preferences.json`, só leitura) no canto da barra lateral; sem perfil, o botão abre o Lotus para criar.
- **Fase 2 (orquestrador Jogar): critério de aceite cumprido em 06/11/2026** com Zelda 64: Recompiled (instalar → jogar → atualizar → voltar → desinstalar, saves intactos). Restam itens listados em `docs/PENDENCIAS.md` (progresso nos cartões, fila de downloads, modo avançado). Motor em `Infrastructure/Install` (ReleasePicker, download com SHA-256, SafeExtractor, PortLibrary com `library\{portId}\versions\{versão}` + `active.json`, PortInstaller com diário ANTES/DEPOIS, GameLauncher sem elevação), máquina de estados do botão em `Core/Install`, `App/PlayService` e botão na Página do jogo. Com o GitHub limitado, instala a versão conferida pelo catálogo (SHA-256 registrado). 130 testes.
- **Como jogar** (06/11/2026): a Página do jogo mostra a parte de uso do README do port (Quick Start, How to install…), lida de raw.githubusercontent.com com cópia de 1 dia em `cache\guides`; só Windows, sem compilação nem outros sistemas, links para arquivos de jogo viram texto. `Core/Guides/ReadmeGuide` + `Infrastructure/Catalog/PortGuideService`. 24 dos 40 ports mais populares têm guia; sem seção de uso, o cartão não aparece.
