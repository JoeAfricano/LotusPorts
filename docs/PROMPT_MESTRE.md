# LOTUS PORTS — Prompt mestre para o Claude Code

> Cole este documento inteiro no Claude Code (ou salve como `docs/PROMPT_MESTRE.md` no repositório e peça: "Leia docs/PROMPT_MESTRE.md por completo e siga-o").
> Idioma de trabalho: **português brasileiro** (código, nomes de classes e commits em inglês; interface, mensagens ao usuário e documentação em PT-BR).

---

## 0. Como você (Claude Code) deve trabalhar neste projeto

1. **Leia este documento inteiro antes de escrever qualquer código.**
2. Comece em **modo de planejamento**: apresente um plano por fases, a estrutura da solução e as dúvidas reais. Só comece a implementar depois que eu (José) aprovar.
3. Crie um arquivo `CLAUDE.md` na raiz do repositório com um resumo das **Regras Invioláveis** (seção 2), da arquitetura e dos comandos de build/test. Mantenha-o atualizado.
4. Trabalhe **por fases** (seção 15). Ao final de cada fase:
   - rode `dotnet build` e `dotnet test` e mostre o resultado real;
   - faça um commit com mensagem clara (Conventional Commits);
   - liste o que ficou **PRONTO**, **EXPERIMENTAL** e **EM DESENVOLVIMENTO**.
5. **Nunca declare algo como funcionando sem ter compilado e, quando possível, executado.** Se não deu para testar, diga explicitamente "não testado" e por quê.
6. Quando houver decisão que mude arquitetura, dependências externas, formato de dados ou comportamento visível ao usuário, **pergunte antes**.
7. Prefira soluções simples, legíveis e bem nomeadas. Sem código morto, sem TODO escondido: pendências vão para `docs/PENDENCIAS.md`.
8. O desenvolvimento acontece localmente no Windows do José, com compilação, execução e testes em tempo real.

---

## 1. Visão do produto

**Lotus Ports** é um aplicativo nativo para Windows, irmão do **Lotus** (otimizador inteligente Windows Gaming). Ele reúne os **ports nativos para PC** de jogos clássicos (projetos de decompilação e recompilação estática publicados no GitHub) e transforma tudo em uma experiência simples: **escolher um jogo e jogar**.

### 1.1 Ideia central: o usuário escolhe **jogos**, não ports
Launchers do gênero mostram listas de projetos (Shipwright, 2Ship, Zelda64Recomp…), e quem não acompanha a cena fica perdido. O Lotus Ports mostra **o jogo** ("Majora's Mask") com **um port recomendado** e o motivo da recomendação em uma frase. As alternativas ficam recolhidas em "Outras formas de jogar".

### 1.2 Diferenciais
1. **Jogo em primeiro lugar** — catálogo organizado por jogo, com recomendação curada.
2. **Um botão só** — o botão principal muda conforme o estado (*Instalar* → *Jogar* → *Atualizar e jogar* → *Resolver*). Dependências, download, vínculo de arquivos e abertura acontecem por trás, com progresso no próprio cartão.
3. **Dois estilos em vez de dezenas de opções** — *Original* (fiel ao console) ou *Remasterizado* (widescreen, alta resolução, taxa de quadros maior), aplicados na configuração do port. Ajuste fino só no modo avançado.
4. **Arquivos encontrados sozinhos** — o usuário informa uma vez onde ficam os arquivos; o app identifica em segundo plano e cada cartão mostra "Pronto para jogar" ou "Falta o arquivo do jogo".
5. **Problema resolvido onde aparece** — sem aba de diagnóstico: se o jogo não abrir, o cartão mostra "Resolver", com causa em português e conserto em um clique.
6. **Saves protegidos sem esforço** — backup automático antes de cada atualização e restauração oferecida quando faz sentido.
7. **Linguagem simples** — selos como "Jogável do começo ao fim", "Jogável, com alguns bugs", "Ainda em construção".
8. **Integração profunda com o Lotus** — kit de dependências, diagnóstico combinado, perfil de desempenho por jogo, relatório da sessão, biblioteca e histórico unificados (seção 12).
9. **Identidade Lotus** — dark premium, rosé/magenta, referências japonesas, modo TV com controle.

Público: jogadores brasileiros, do leigo ao entusiasta. Interface em PT-BR por padrão, inglês como segundo idioma.

---

## 2. Regras Invioláveis

### 2.1 Regras herdadas do Lotus
- **NÃO REMOVA FUNCIONALIDADES.** Nenhuma funcionalidade aprovada pode ser removida ou simplificada sem autorização explícita. Simplificar a **interface** (esconder, agrupar, mostrar no contexto) é desejado; remover a **função** não é.
- **NÃO ACHATE O PROGRAMA.** Não reduza fluxos ou módulos a versões genéricas para "terminar mais rápido".
- **NÃO MOSTRAR FALSO SUCESSO.** Nunca exiba ✓/verde/"concluído" se a operação não foi verificada. Estados de qualquer verificação: `OK`, `AVISO`, `FALHA`, `NÃO VERIFICADO`.
- **Não inventar dados.** Nada de números, hashes, versões, chaves de configuração ou compatibilidades inventados. O que não foi obtido aparece como não obtido.
- Recursos incompletos aparecem rotulados como **EXPERIMENTAL** ou **EM DESENVOLVIMENTO**, nunca como operacionais.
- **Toda alteração real exige confirmação** do usuário (instalar, atualizar, remover, copiar arquivo, aplicar estilo, instalar dependência). Na primeira vez, a confirmação explica o que acontece; o usuário pode marcar "não perguntar de novo para este tipo de ação" (exceto remoções e instalação de dependências, que sempre confirmam).
- **Registrar estado ANTES/DEPOIS** de toda operação que altera disco ou sistema (seção 11).
- **Reversibilidade**: toda instalação, atualização e aplicação de estilo pode ser desfeita.

### 2.2 Regras legais e de segurança
- **PROIBIDO implementar** qualquer forma de busca, download, indexação, scraping ou link para ROMs, ISOs, arquivos de jogos, BIOS, firmware ou chaves de console. Isso inclui sugestões, fontes alternativas, campos de URL de arquivo de jogo no catálogo e integrações com sites que hospedem esses arquivos.
- **PROIBIDO** distribuir, embutir ou baixar assets dos jogos originais (modelos, texturas, áudio, dados).
- **PROIBIDO** implementar descriptografia, extração de chaves, quebra de DRM ou contorno de proteções. Se um port exige algo do tipo, o Lotus Ports apenas aponta para a documentação oficial do projeto.
- **Downloads apenas das releases oficiais** do repositório declarado no catálogo. Nunca de mirrors, repacks ou links de terceiros.
- **Nenhum dado do usuário sai do PC.** Sem telemetria. Rede usada apenas para a API pública do GitHub, o catálogo e redistribuíveis oficiais da Microsoft.
- **Capas e artes**: não embutir box art oficial (seção 9.8).
- Tela "Sobre/Legal" com aviso de não afiliação com fabricantes/publishers e as licenças de cada port.
- O app **roda sem privilégios de administrador**. UAC só para ações que exigem (ex.: redistribuível da Microsoft), sempre após confirmação.

### 2.3 Regras de simplicidade (novas)
- A interface parte de **duas telas principais** — **Estante** e **Página do jogo** — mais a **Engrenagem** (configurações). Prefira encaixar funções novas nessas áreas ou **no contexto** em que são necessárias. Novas abas ou telas podem ser criadas **quando forem realmente necessárias e fizerem sentido para o projeto** — explique o motivo ao propor.
- **Uma ação principal por tela/cartão.** Ações secundárias ficam em menu "⋯" ou recolhidas.
- **Modo avançado** (desligado por padrão) revela: versões e pré-releases, ajuste fino, logs, histórico de operações, lista completa de backups, token do GitHub, informações do catálogo.
- **Texto para leigos**: nada de jargão na interface padrão ("dump", "hash", "asset", "release" só aparecem no modo avançado). Ex.: "arquivo do jogo", "versão", "atualização".
- Antes de adicionar qualquer elemento visual, pergunte-se: "isso ajuda a pessoa a jogar agora?". Se não, vai para o modo avançado ou para o menu "⋯".

---

## 3. Stack técnica

| Item | Escolha | Observação |
|---|---|---|
| Linguagem | C# 12+ | `Nullable` habilitado, `TreatWarningsAsErrors` no Core |
| Runtime | .NET 8 (LTS) ou superior | Confirmar comigo antes de subir de versão |
| UI | **WinUI 3 (Windows App SDK)**, app **unpackaged** self-contained | Se surgir bloqueio sério, **pergunte** antes de trocar por WPF |
| Padrão | MVVM com `CommunityToolkit.Mvvm` | `ObservableObject`, `RelayCommand`, `[ObservableProperty]` |
| DI / Host | `Microsoft.Extensions.Hosting` + `DependencyInjection` | Generic Host no `App.xaml.cs` |
| Banco local | SQLite via `Microsoft.Data.Sqlite` (+ Dapper) | Migrações versionadas próprias |
| Logs | Serilog (arquivo rotativo) | Logs técnicos em inglês; mensagens ao usuário em PT-BR |
| HTTP | `HttpClient` via `IHttpClientFactory` + Polly | User-Agent: `LotusPorts/{versão}` |
| Compressão | `System.IO.Compression` + `SharpCompress` | Proteção contra zip-slip obrigatória |
| Hash | `System.Security.Cryptography` + `System.IO.Hashing` (CRC32) | Streaming, cancelável |
| Config dos ports | `System.Text.Json`, parser INI simples próprio, `Tomlyn` (TOML) se necessário | Perguntar antes de adicionar Tomlyn |
| Controle | `Windows.Gaming.Input` | Navegação por XYFocus |
| GPU/API checks | `Vortice.Windows` (DXGI/D3D12) — opcional | Só se o Lotus não estiver presente; perguntar antes |
| Testes | xUnit + FluentAssertions + NSubstitute | Cobertura forte no Core |

Toda dependência NuGet nova precisa ter licença compatível (MIT/Apache/BSD) e ser listada em `docs/DEPENDENCIAS.md`.

---

## 4. Estrutura da solução

```
LotusPorts/
├─ CLAUDE.md
├─ README.md
├─ docs/
│  ├─ PROMPT_MESTRE.md
│  ├─ ARQUITETURA.md
│  ├─ CATALOGO.md
│  ├─ INTEGRACAO_LOTUS.md       (contrato completo — seção 12)
│  ├─ UX.md                     (princípios e estados de interface — seção 9)
│  ├─ DEPENDENCIAS.md
│  └─ PENDENCIAS.md
├─ catalog/
│  ├─ catalog.schema.json
│  ├─ catalog.json              (índice)
│  ├─ games/{gameId}.json       (um arquivo por jogo)
│  └─ ports/{portId}.json       (um arquivo por port)
├─ src/
│  ├─ LotusPorts.App/           (WinUI 3: Views, ViewModels, Navigation)
│  ├─ LotusPorts.Core/          (domínio, interfaces, máquina de estados do botão, regras)
│  ├─ LotusPorts.Infrastructure/(GitHub, SQLite, FileSystem, Hashing, Archives, Processes, EventLog, PortConfig)
│  ├─ LotusPorts.Catalog/       (modelos, validação de schema, carregamento)
│  ├─ LotusPorts.Integration/   (cliente e servidor IPC com o Lotus, eventos, deep links)
│  ├─ Lotus.Integration.Contracts/ (contrato compartilhado com o Lotus — sem dependências)
│  └─ Lotus.Design/             (tokens, estilos e controles visuais compartilhados com o Lotus)
└─ tests/
   ├─ LotusPorts.Core.Tests/
   ├─ LotusPorts.Infrastructure.Tests/
   ├─ LotusPorts.Catalog.Tests/
   └─ LotusPorts.Integration.Tests/ (inclui servidor Lotus simulado)
```

Princípios:
- `Core` não conhece WinUI, SQLite nem HTTP — só interfaces (`IPortInstaller`, `IGameFileScanner`, `IReleaseSource`, `IDependencyChecker`, `IPortConfigWriter`, `ILotusBridge` etc.).
- `Lotus.Integration.Contracts` e `Lotus.Design` são projetos **pequenos e estáveis**, pensados para serem usados também pela solução do Lotus. **Pergunte** se devem virar repositório próprio, submódulo git ou pacote NuGet local.
- Cada serviço que altera disco passa pelo `IOperationJournal` (seção 11).

---

## 5. Catálogo

### 5.1 Conceito
Catálogo em JSON **curado manualmente**, versionado em um repositório GitHub do José (ex.: `lotus-ports-catalog`). Duas entidades:
- **Jogo** (`games/`): o que o usuário vê na Estante.
- **Port** (`ports/`): a forma técnica de jogar; um jogo pode ter vários ports.

O app embute uma cópia do catálogo no build (funciona offline), verifica atualizações por ETag, **valida contra o schema** e só então substitui. Catálogo inválido é rejeitado, o anterior é mantido e o erro vai para o log.

O catálogo **contém apenas** metadados e links para repositórios e documentação oficiais. Nunca links para arquivos de jogo.

### 5.2 Schema de um jogo (`catalog/games/{gameId}.json`)

```json
{
  "schemaVersion": 1,
  "id": "majoras-mask",
  "title": "The Legend of Zelda: Majora's Mask",
  "originalPlatform": "N64",
  "genre": ["aventura", "ação"],
  "descriptionPtBr": "Texto curto, 2 a 3 frases, para leigos.",
  "recommendedPortId": "zelda64recomp",
  "recommendationReasonPtBr": "Frase única explicando por que este é o recomendado.",
  "alternatives": [
    { "portId": "2ship2harkinian", "differencePtBr": "Ex.: mais opções de personalização" }
  ],
  "lotusGameKey": "port:majoras-mask"
}
```

`lotusGameKey` é a chave estável que o Lotus usa para associar perfil de desempenho e relatórios a este jogo (seção 12).

### 5.3 Schema de um port (`catalog/ports/{portId}.json`)

```json
{
  "schemaVersion": 1,
  "id": "ship-of-harkinian",
  "gameId": "ocarina-of-time",
  "title": "Ship of Harkinian",
  "portType": "decomp-port",
  "status": "JOGAVEL",
  "knownIssuesPtBr": [],
  "verifiedBy": "José",
  "verifiedAt": "2026-11-05",
  "repo": { "owner": "HarbourMasters", "name": "Shipwright" },
  "license": "VERIFICAR NO REPOSITÓRIO",
  "windowsAsset": {
    "pattern": "(?i)win.*\\.zip$",
    "excludePattern": "(?i)(debug|pdb|symbols)",
    "preferPrerelease": false
  },
  "executable": "soh.exe",
  "launchArgs": [],
  "setup": {
    "kind": "beside-exe",
    "targetFileName": null,
    "notesPtBr": "Na primeira execução o jogo prepara os arquivos a partir do arquivo do jogo."
  },
  "requiredGameFiles": [
    {
      "label": "Cartucho N64",
      "kind": "single-file",
      "format": "n64-rom",
      "canonicalFormat": "z64",
      "acceptedVersions": [
        {
          "region": "PREENCHER",
          "version": "PREENCHER",
          "sha1": "PREENCHER A PARTIR DA DOCUMENTAÇÃO OFICIAL DO PROJETO",
          "source": "URL da documentação oficial de onde o hash foi copiado"
        }
      ],
      "officialDocsUrl": "PREENCHER"
    }
  ],
  "persistentPaths": [
    { "path": "Save/", "kind": "saves" },
    { "path": "shipofharkinian.json", "kind": "config" },
    { "path": "mods/", "kind": "mods" }
  ],
  "experiencePresets": {
    "configFile": "shipofharkinian.json",
    "configFormat": "json",
    "original":     { "values": { "PREENCHER.chave": "valor" }, "verified": false },
    "remasterizado":{ "values": { "PREENCHER.chave": "valor" }, "verified": false }
  },
  "dependencies": ["vcredist-x64-2015-2022"],
  "graphicsApis": ["D3D11", "OpenGL"],
  "controllerSupport": "full",
  "highlightsPtBr": ["Widescreen", "Alta resolução"],
  "links": {
    "repo": "https://github.com/HarbourMasters/Shipwright",
    "docs": "PREENCHER",
    "community": "PREENCHER"
  },
  "art": { "icon": null, "banner": null, "artLicense": null },
  "minWindowsBuild": 19041
}
```

### 5.4 Tipos de setup (`setup.kind`)
| kind | Comportamento |
|---|---|
| `beside-exe` | Copia (com confirmação) o arquivo do jogo para a pasta do executável, com o nome esperado se `targetFileName` existir. |
| `select-on-first-launch` | O port pede o arquivo na primeira execução. O app mostra uma instrução curta, copia o caminho para a área de transferência e abre o jogo. |
| `extract-tool` | Executa a ferramenta de extração do port com `setup.extractCommand` e verifica `setup.expectedOutput`. |
| `game-folder` | O port exige uma **pasta** de arquivos do jogo; o app valida `setup.requiredEntries`. |
| `manual` | Passo a passo curto em PT-BR + link para a documentação oficial. Vínculo fica `NÃO VERIFICADO` até o usuário confirmar. |

### 5.5 Status e textos para o usuário
| Enum interno | Texto na interface |
|---|---|
| `JOGAVEL` | Jogável do começo ao fim |
| `EXPERIMENTAL` | Jogável, com alguns bugs |
| `EM_DESENVOLVIMENTO` | Ainda em construção |
| `DESCONTINUADO` | Não recebe mais atualizações |

### 5.6 Estilos de experiência (Original / Remasterizado)
- Cada port declara em `experiencePresets` as chaves de configuração de cada estilo.
- **As chaves devem ser confirmadas na documentação ou no arquivo de configuração real do port. Não invente.** Enquanto `verified` for `false`, o seletor de estilo **não aparece** para aquele port.
- Aplicar um estilo: backup do arquivo de configuração → escrita apenas das chaves declaradas (preservando todo o resto) → verificação lendo o arquivo de volta → registro no diário. Reverter é um clique.
- Se o port sobrescrever a configuração ao abrir, detectar na leitura seguinte e mostrar `AVISO` em vez de fingir que aplicou.

### 5.7 Catálogo inicial (somente após verificação)
Para cada candidato, **verifique no GitHub** que o repositório existe, leia o README, confirme o asset Windows, o tipo de setup, os caminhos persistentes, as versões aceitas do jogo e as chaves de configuração **na documentação oficial**. O que não puder confirmar fica `"PREENCHER"` e o jogo não aparece publicamente até eu revisar.

Candidatos:
- Ship of Harkinian — `HarbourMasters/Shipwright`
- 2 Ship 2 Harkinian — `HarbourMasters/2ship2harkinian`
- Starship — `HarbourMasters/Starship`
- SpaghettiKart — `HarbourMasters/SpaghettiKart`
- Zelda 64: Recompiled — `Zelda64Recomp/Zelda64Recomp`
- Unleashed Recompiled — `hedge-dev/UnleashedRecomp`

A recomendação de port por jogo (`recommendedPortId` e motivo) é **decisão minha**: proponha e aguarde aprovação.

### 5.8 Fontes de catálogo importáveis
Além do catálogo oficial do José, o usuário pode **importar outras fontes de catálogo de ports** (Engrenagem → Catálogo → "Adicionar fonte").

**Link base aceito:**
- Repositório GitHub de catálogo: `https://github.com/{owner}/{repo}` — o app lê `catalog.json` da branch padrão via `https://raw.githubusercontent.com/{owner}/{repo}/{branch}/catalog.json`;
- ou URL direta de um `catalog.json` hospedado em `raw.githubusercontent.com`.

**Regras de uma fonte importada:**
- Precisa seguir **o mesmo schema** (seção 5.2/5.3) e passar pela **mesma validação** (seção 5.9). Fonte inválida é rejeitada com explicação.
- Cada port de fonte importada precisa apontar para um **repositório GitHub** e é baixado **somente das releases oficiais desse repositório**. Qualquer entrada com URL de download fora disso é descartada.
- Entradas com campos de arquivo de jogo, ROM, ISO, BIOS, firmware ou chaves são **rejeitadas** (regra 2.2), e a fonte inteira fica marcada `AVISO` com o motivo.
- Jogos de fontes importadas aparecem com selo **"Fonte externa"** e estado `NÃO VERIFICADO` até o usuário marcar como confiável.
- Em conflito de `id` com o catálogo oficial, o oficial prevalece; o item externo aparece em "Outras formas de jogar".
- Cada fonte pode ser atualizada, desativada ou removida; remover a fonte não desinstala jogos já instalados (avisar).
- Tabela `CatalogSources` (id, url, nome, ativa, confiável, etag, última atualização, estado da validação).

**Adicionar port por link do GitHub** (modo avançado):
- O usuário cola `https://github.com/{owner}/{repo}` de um port que ainda não está em nenhum catálogo.
- O app lê as releases, sugere o arquivo Windows e o executável, e abre um formulário curto para completar o que faltar (tipo de setup, caminhos de saves).
- O item vira uma entrada local com status **EXPERIMENTAL** e selo "Adicionado por você".
- Opção "Exportar como JSON" para o José incluir depois no catálogo oficial.

### 5.9 Validação
- `catalog.schema.json` (JSON Schema draft 2020-12) + validação no carregamento.
- Teste automatizado que valida todos os arquivos de `games/` e `ports/`, incluindo: todo `recommendedPortId` e `alternatives[].portId` existe; todo port aponta para um `gameId` existente.
- Nenhum campo de URL pode apontar para fora de `github.com`, do domínio de documentação oficial declarado ou de comunidades oficiais do projeto.
- `docs/CATALOGO.md` explica, passo a passo, como adicionar e validar jogos e ports.

---

## 6. Fonte de atualizações (GitHub)

- `GET https://api.github.com/repos/{owner}/{repo}/releases` (e `/releases/latest`).
- Cache por repositório com **ETag/If-None-Match**; respeitar `X-RateLimit-Remaining`/`X-RateLimit-Reset`. Ao esgotar o limite, informar quando volta, sem travar o app.
- Token pessoal do GitHub **opcional** (modo avançado), salvo com Windows Credential Locker / DPAPI, nunca em texto puro nem em log.
- Seleção do arquivo de download por `windowsAsset.pattern`/`excludePattern`; com 0 ou mais de 1 candidato, **não adivinhe**: marque `NÃO VERIFICADO` e, no modo avançado, deixe o usuário escolher.
- Integridade: se a API informar digest SHA-256 do arquivo, verificar (`OK`/`FALHA`); sem digest, `NÃO VERIFICADO` (visível no modo avançado e no relatório). Conferir tamanho.
- Novidades da atualização: resumo curto na Página do jogo; changelog completo recolhido.
- Pré-releases só no modo avançado, por jogo.

---

## 7. Instalação, atualização, remoção e rollback

### 7.1 Pastas
```
%LOCALAPPDATA%\LotusPorts\
├─ data\lotusports.db
├─ logs\
├─ cache\downloads\
├─ backups\{portId}\{timestamp}\
└─ library\{portId}\
   ├─ versions\{tag}\
   └─ active.json
```
- Pasta da biblioteca configurável (ex.: outro SSD). Bloquear `Program Files`, raiz de disco e pastas sincronizadas pelo OneDrive, explicando o porquê.

### 7.2 Orquestrador "Jogar" (`PlayPipeline`)
O botão principal dispara um **orquestrador único** que executa somente as etapas necessárias, nesta ordem, mostrando progresso no cartão:
1. Verificar dependências (via Lotus, se conectado — seção 12).
2. Baixar e verificar a versão (se não instalada ou se o usuário aceitou atualizar).
3. Extrair com segurança e validar o executável.
4. Vincular o arquivo do jogo conforme `setup.kind`.
5. Aplicar o estilo padrão escolhido (se verificado e ainda não aplicado).
6. Avisar o Lotus (`Session.Start`) e abrir o jogo.

Cada etapa registra ANTES/DEPOIS. Falha em qualquer etapa → o botão vira **Resolver** com a causa (seção 10). Cancelar é sempre possível até a etapa 5.

### 7.3 Máquina de estados do botão principal (em `Core`, com testes)
| Estado | Botão | Secundário (menu ⋯ ou link) |
|---|---|---|
| Port ainda em construção | Acompanhar | — |
| Falta arquivo do jogo | Adicionar arquivo do jogo | Ver quais versões servem |
| Não instalado | Instalar | Outras formas de jogar |
| Instalando/atualizando | barra de progresso + Cancelar | — |
| Pronto | Jogar | Estilo, saves, desinstalar |
| Atualização disponível (auto-update ligado) | Atualizar e jogar | Jogar sem atualizar |
| Atualização disponível (auto-update desligado) | Jogar | Atualizar |
| Problema detectado | Resolver | Detalhes |
| Em execução | Jogando… (tempo) | Fechar jogo |

### 7.4 Atualização
- Backup automático de `persistentPaths` antes de atualizar.
- Nova versão em pasta separada; migração de `persistentPaths`; troca da versão ativa; manter as 2 últimas versões.
- Se a primeira execução após atualizar falhar, o cartão oferece **"Voltar para a versão anterior"** (rollback + restauração do backup).
- "Fixar versão" disponível no modo avançado.

### 7.5 Remoção
- Opções no menu ⋯: desinstalar mantendo saves / remover tudo.
- Saves nunca apagados sem confirmação e sem backup prévio.
- **Nunca** apagar os arquivos de jogo do usuário.

### 7.6 Segurança de extração
Proteção contra zip-slip, ignorar links simbólicos, limite de tamanho descompactado com aviso.

---

## 8. Arquivos de jogo

### 8.1 Sem aba própria
- No **primeiro uso** (seção 9.3), o app pergunta onde ficam os arquivos de jogo (pode pular).
- O scanner roda em segundo plano (prioridade baixa, cancelável) e mantém cache por `caminho + tamanho + data de modificação`.
- O resultado aparece **nos cartões**: "Pronto para jogar" ou "Falta o arquivo do jogo".
- Ao clicar em "Adicionar arquivo do jogo": seletor de arquivo/pasta → identificação imediata → mensagem humana: "Arquivo reconhecido: versão USA 1.0. Pronto!" ou "Este arquivo é de outra versão. Este jogo aceita: …" (com link para a documentação oficial do port).
- Gestão das pastas observadas e lista de arquivos reconhecidos ficam na **Engrenagem → Arquivos de jogo**.
- Arrastar e soltar um arquivo em qualquer lugar da janela também identifica.

### 8.2 Regras
- **Nunca mover, renomear ou alterar o arquivo original.**
- Cópias só com confirmação, registradas no diário.
- Nenhum hash ou caminho é enviado para fora do PC.

### 8.3 N64 — ordem de bytes
Detectar pelos 4 primeiros bytes:
- `80 37 12 40` → `.z64` (big-endian, canônico);
- `37 80 40 12` → `.v64` (byte-swapped de 16 bits);
- `40 12 37 80` → `.n64` (little-endian de 32 bits).

Hash calculado sobre a versão canônica (z64) em stream. Se o port exigir `.z64`, oferecer gerar uma **cópia** convertida. Testes unitários para as três ordens.

### 8.4 Pastas de jogo (`game-folder`)
Validar a estrutura conforme `setup.requiredEntries` e explicar exatamente o que falta.

---

## 9. Interface e experiência

### 9.1 Princípios
- Base de duas telas + Engrenagem (regra 2.3); novas abas só quando necessárias.
- Uma ação principal por cartão.
- Tudo que é técnico fica no modo avançado.
- Progresso e problemas aparecem **onde o usuário já está olhando** (no cartão), não em outra tela.
- Cada tela tem estados de carregando (skeleton), vazio (com orientação útil), erro (mensagem humana + detalhes recolhíveis + ação) e offline.
- Documentar tudo em `docs/UX.md`.

### 9.2 Barra superior (única)
Logo Lotus Ports · **busca única** (também com `Ctrl+K`): busca jogos e ações ("atualizar tudo", "abrir Lotus", "modo TV") · indicador discreto de downloads ativos (popover com a fila, sem tela própria) · indicador de conexão com o Lotus (pétala acesa/apagada) · Engrenagem.

### 9.3 Primeiro uso (3 passos, puláveis)
1. Boas-vindas + pasta da biblioteca (padrão já preenchido).
2. "Onde ficam seus arquivos de jogo?" (escolher pastas ou pular).
3. Estilo padrão (*Original* ou *Remasterizado*) + status da integração com o Lotus (se o Lotus estiver instalado: "Conectado"; se não: convite discreto).

### 9.4 Tela 1 — Estante
Prateleiras horizontais, nesta ordem, cada uma só aparece se tiver conteúdo:
1. **Chegou!** — novos jogos no catálogo ou atualizações grandes (até 3 cartões; dispensáveis).
2. **Continuar jogando** — por última sessão.
3. **Prontos para jogar** — instalados ou com arquivo reconhecido.
4. **Para descobrir** — demais jogos jogáveis do catálogo.
5. **Em breve** — jogos com port ainda em construção (botão "Acompanhar").

Filtros mínimos em chips (plataforma original, gênero) acima das prateleiras. Nada além disso.

**Cartão de jogo**: arte, título, plataforma original, selo de status em linguagem simples, linha de estado ("Pronto para jogar", "Falta o arquivo do jogo", "Atualização disponível") e o **botão principal** da seção 7.3. Progresso de instalação aparece no próprio cartão.

### 9.5 Tela 2 — Página do jogo
De cima para baixo:
1. **Topo**: arte, título, selo, botão principal grande.
2. **Estilo**: seletor *Original* / *Remasterizado* (só se verificado — seção 5.6), com uma frase explicando cada um.
3. **Resolver** (só aparece com problema): causa em português, botão de conserto, detalhes técnicos recolhidos (seção 10).
4. **Sua experiência** (com Lotus conectado): resumo da última sessão medida pelo Lotus e sugestões (seção 12.6). Sem Lotus: cartão convidando a conectar, sem números inventados.
5. **Saves**: data do último backup, "Fazer backup agora", "Restaurar".
6. **Sobre este port**: por que é o recomendado, destaques, novidades da última atualização, links oficiais.
7. **Outras formas de jogar** (recolhido): alternativas com a frase de diferença.
8. **Menu ⋯**: abrir pasta do jogo, desinstalar, (modo avançado) versões, fixar versão, pré-releases, logs do jogo.

### 9.6 Engrenagem
Seções: **Geral** (idioma, pasta da biblioteca, estilo padrão, atualizações automáticas, iniciar com o Windows, modo TV) · **Arquivos de jogo** (pastas observadas, arquivos reconhecidos) · **Catálogo** (fontes de catálogo importadas, adicionar fonte, adicionar port por link do GitHub — seção 5.8) · **Lotus** (status, permissões da integração — seção 12.10) · **Modo avançado** (interruptor + tudo o que ele libera) · **Sobre/Legal**.

### 9.7 Identidade visual
Dark gamer premium, minimalista, futurista, com referências japonesas. Se o José fornecer imagem de referência, ela prevalece.

Tokens em `Lotus.Design` (compartilhados com o Lotus; nunca cores soltas nas views):

| Token | Valor proposto | Uso |
|---|---|---|
| `Bg.Base` | `#0B0B0F` | fundo |
| `Bg.Surface` | `#15151C` | cartões |
| `Bg.Elevated` | `#1E1E28` | hover, diálogos |
| `Stroke.Subtle` | `#2A2A36` | bordas |
| `Text.Primary` | `#F4F1F5` | texto principal |
| `Text.Secondary` | `#A9A3B0` | texto de apoio |
| `Accent.Rose` | `#E8A3BF` | acento principal |
| `Accent.Pink` | `#FF5FA2` | ações, foco |
| `Accent.Magenta` | `#D6249F` | destaques, progresso |
| `State.Ok` | `#5FD3A3` | somente quando verificado |
| `State.Warn` | `#F2C46D` | aviso |
| `State.Fail` | `#FF6B6B` | falha |
| `State.Unknown` | `#8C8799` | não verificado |

- Segoe UI Variable; detalhes decorativos em katakana com moderação.
- Segoe Fluent Icons; motivo de pétala de lótus (logo, carregamento, indicador do Lotus, estados vazios).
- Cantos 8–12 px, sombras suaves, contraste AA. Animações curtas; respeitar "reduzir animações" do Windows.
- Selos sempre com texto, não só cor.

### 9.8 Artes dos jogos
- Não embutir capas oficiais.
- Usar ícone/banner do **projeto de port** quando `art.artLicense` permitir.
- Senão, **cartão tipográfico gerado**: título, plataforma original, gradiente Lotus e cor de assinatura por plataforma.

### 9.9 Modo TV / controle
- As **mesmas duas telas**, ampliadas: prateleiras grandes, foco com brilho rosé.
- `A` principal, `B` voltar, `X` menu ⋯, `Y` Página do jogo, `LB/RB` trocar prateleira, `Menu` Engrenagem.
- Entrar automaticamente ao detectar controle (opção) ou por atalho. Toda função essencial alcançável sem mouse.

### 9.10 Acessibilidade e idioma
Navegação completa por teclado, `AutomationProperties.Name` em todos os controles, textos em `.resw` (PT-BR padrão, en-US secundário).

---

## 10. Execução e "Resolver"

### 10.1 Jogar
- Executável com `WorkingDirectory` = pasta da versão ativa e `launchArgs` do catálogo.
- Registrar sessão (início, fim, duração, código de saída).
- Código ≠ 0 em menos de 15 s → cartão vira **Resolver** automaticamente.

### 10.2 Verificações do "Resolver"
Cada item: `OK`, `AVISO`, `FALHA` ou `NÃO VERIFICADO`, com explicação em PT-BR e ação. Só os itens com problema aparecem por padrão; a lista completa fica em "Detalhes".
1. Executável presente (se sumiu, mencionar possível quarentena do antivírus sem afirmar sem evidência).
2. Arquivo do jogo vinculado e válido.
3. Dependências (via Lotus quando conectado — seção 12.4).
4. Sistema (via Lotus quando conectado — seção 12.5): driver de vídeo, suporte às APIs gráficas exigidas, mudanças recentes no sistema. Sem Lotus: checagem local best-effort (`vulkan-1.dll`, adaptador DXGI/D3D12) ou `NÃO VERIFICADO`.
5. Caminho: caracteres especiais, comprimento, OneDrive, permissão de escrita.
6. Espaço em disco.
7. Último crash: Log de Eventos do Windows (Application Error / WER) filtrado pelo executável — módulo com falha e código.
8. Saves/configs graváveis.
9. Atualização recente: se o problema começou após atualizar, oferecer rollback em destaque.

**Relatório copiável** (texto/Markdown) para pedir ajuda na comunidade do port, anonimizando o nome de usuário nos caminhos.

---

## 11. Diário de operações (ANTES/DEPOIS)

Tabela `OperationJournal`: id, timestamp, tipo (`Install`, `Update`, `Rollback`, `Remove`, `CopyGameFile`, `ConvertGameFile`, `ApplyPreset`, `RevertPreset`, `Backup`, `Restore`, `DependencyInstall`), portId, estadoAntes (JSON), estadoDepois (JSON), resultado (`Sucesso`, `Falha`, `Cancelado`, `Parcial`), detalhes, `correlationId` (para cruzar com o histórico do Lotus).

- Toda operação grava ANTES no início e DEPOIS + resultado no fim, inclusive em falha.
- Visível no modo avançado (Engrenagem) e consultável pelo Lotus (seção 12.8).
- Operações interrompidas são detectadas na próxima abertura, com opção de limpar/desfazer.

---

## 12. Integração completa com o Lotus

### 12.1 Princípios
- O Lotus Ports **funciona 100% sem o Lotus**; a integração complementa e aprofunda.
- Os dois apps se comportam como **uma família**: mesma identidade visual (`Lotus.Design`), mesma linguagem, um abre o outro no ponto certo.
- O Lotus é o **especialista em sistema e desempenho**; o Lotus Ports é o **especialista em ports e jogos**. Nenhum duplica a função principal do outro quando o outro está presente.
- Status sempre visível e honesto: `Conectado ao Lotus (vX.Y)`, `Lotus não encontrado`, `Versão incompatível — atualize o Lotus`.
- Toda ação que o Lotus executa no sistema é **confirmada pelo usuário na interface do Lotus**, nunca pelo Lotus Ports em nome dele.

### 12.2 Transporte, descoberta e deep links
- **Named pipes**, um por app:
  - `\\.\pipe\lotus.integration.v1` (servidor no Lotus);
  - `\\.\pipe\lotusports.integration.v1` (servidor no Lotus Ports).
- ACL restrita ao usuário atual; somente conexões locais.
- Mensagens **JSON-RPC 2.0** com prefixo de tamanho de 4 bytes; requisições (`id`, `method`, `params`) e **notificações** (sem `id`) para eventos.
- **Descoberta** via registro:
  - Lotus grava `HKCU\Software\Lotus\Integration` → `PipeName`, `ProtocolVersion`, `AppVersion`, `ExePath`;
  - Lotus Ports grava `HKCU\Software\LotusPorts\Integration` com os mesmos campos.
- **Deep links** (registrados por usuário em `HKCU\Software\Classes`):
  - `lotusports://game/{gameId}` — abre a Página do jogo; `lotusports://play/{gameId}` — inicia o orquestrador "Jogar";
  - `lotus://dependencies?ids=a,b` — abre o kit de dependências com itens pré-selecionados; `lotus://session/{sessionId}` — abre o relatório da sessão; `lotus://profile/{lotusGameKey}` — abre o perfil de desempenho do jogo.
- Timeouts curtos (2 s), reconexão silenciosa, nada bloqueia a UI.
- **Negociação de capacidades** no `Handshake`: cada lado anuncia `capabilities[]` e só usa métodos que o outro anunciou. Recursos não suportados somem da interface (não aparecem quebrados).

### 12.3 Segurança da integração
- Validar o processo do outro lado: obter o PID do cliente/servidor do pipe, conferir que o executável é o caminho registrado pelo app irmão (e, quando os apps forem assinados, a assinatura Authenticode).
- Limite de tamanho de mensagem, validação de schema de cada `params`, rejeição de métodos desconhecidos.
- `Ports.Launch` aceita **apenas `gameId`** do catálogo — nunca caminhos de executável vindos do outro lado.
- Nada de execução de comandos arbitrários pela integração.

### 12.4 Kit de dependências
| Método | Direção | Função |
|---|---|---|
| `Dependencies.Check` | Ports → Lotus | `{ ids[] }` → `{ id, state: OK/FALTANDO/DESATUALIZADO/NAO_VERIFICADO, installedVersion, details }[]` |
| `Dependencies.RequestInstall` | Ports → Lotus | Abre o kit do Lotus com os itens pré-selecionados; o **Lotus confirma com o usuário** e devolve o resultado final por evento `Dependencies.Changed`. |

IDs de dependências em `Lotus.Integration.Contracts/DependencyIds.cs` (fonte única de verdade para os dois apps).

**Modo autônomo (sem Lotus)**: detecção local somente leitura (VC++ 2015–2022 x64 em `HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64`; DLLs do DirectX legado exigidas pelo catálogo; `vulkan-1.dll`; .NET Desktop Runtime quando exigido). Instalação apenas de redistribuíveis **oficiais da Microsoft**, com verificação Authenticode (editor Microsoft), confirmação, UAC e tratamento de códigos de saída (`0`, `3010` reinício necessário, `1638` versão mais nova já instalada). Registrar no diário.

### 12.5 Diagnóstico combinado
| Método | Direção | Função |
|---|---|---|
| `Diagnostics.RunSystemChecks` | Ports → Lotus | `{ graphicsApis[], exePath }` → itens de sistema (driver, suporte a APIs gráficas, memória de vídeo, problemas conhecidos detectados pelo Lotus) no mesmo formato `OK/AVISO/FALHA/NÃO VERIFICADO`. |
| `Journal.QueryRecent` | Ports → Lotus | Mudanças recentes no sistema registradas pelo Lotus (ex.: driver atualizado ontem). O "Resolver" mostra como pista: "O problema começou depois de: …". |

O "Resolver" do Lotus Ports **mescla** esses itens com os seus, sem duplicar.

### 12.6 Desempenho por jogo (perfil, sessão e sugestões)
| Método/Evento | Direção | Função |
|---|---|---|
| `Session.Start` | Ports → Lotus | `{ lotusGameKey, gameTitle, portId, portVersion, exePath, processId, preset }` → `{ sessionId }`. O Lotus aplica o **perfil de desempenho** do jogo (se existir) e inicia o monitoramento. |
| `Session.End` | Ports → Lotus | `{ sessionId, exitCode, durationSeconds }`. O Lotus reverte o perfil e fecha o relatório. |
| `Session.GetReport` | Ports → Lotus | `{ sessionId }` → resumo **apenas com valores medidos** (duração, média de FPS, 1% low, picos de travamento, temperaturas máximas) + `measured: true/false`. |
| `System.GetHardwareProfile` | Ports → Lotus | Perfil resumido do hardware para sugerir o estilo padrão no primeiro uso. |
| `Recommendations.ForGame` | Ports → Lotus | `{ lotusGameKey, lastSessionIds[] }` → sugestões baseadas em medição (ex.: "trocar para Original deve estabilizar os quadros neste PC"), com a evidência usada. |

Na Página do jogo, seção **"Sua experiência"**:
- "Última sessão: 2h10 · 1% low 58 · 3 travamentos" (só o que o Lotus mediu; se `measured: false`, não mostrar números);
- interruptor **"Usar perfil do Lotus ao jogar"** (ligado por padrão quando conectado);
- sugestão do Lotus, se houver, com botão "Aplicar" (aplica o estilo pelo fluxo da seção 5.6) e, depois da próxima sessão, a **comparação antes/depois** feita pelo Lotus;
- link "Ver relatório completo no Lotus" (`lotus://session/{sessionId}`) e "Editar perfil no Lotus" (`lotus://profile/{lotusGameKey}`).

### 12.7 Biblioteca unificada
| Método | Direção | Função |
|---|---|---|
| `Ports.ListGames` | Lotus → Ports | Jogos do Lotus Ports com estado (instalado, pronto, atualização disponível), `lotusGameKey`, arte e tempo jogado. |
| `Ports.Launch` | Lotus → Ports | `{ gameId }` — o Lotus pede para jogar; o Lotus Ports executa o orquestrador "Jogar" (garantindo dependências, vínculo e sessão). |
| `Ports.Open` | Lotus → Ports | `{ gameId? }` — traz o Lotus Ports para frente na Página do jogo ou na Estante. |

Assim, o Lotus pode mostrar os jogos do Lotus Ports na própria biblioteca/perfis, e o "Continuar jogando" funciona nos dois apps.

### 12.8 Histórico unificado
| Método | Direção | Função |
|---|---|---|
| `Journal.Query` | Lotus → Ports | Operações do Lotus Ports (instalações, atualizações, rollbacks, estilos aplicados) com `correlationId`, para o Lotus exibir numa linha do tempo única do PC. |

### 12.9 Eventos (notificações)
- **Ports → Lotus**: `Game.Installed`, `Game.Updated`, `Game.RolledBack`, `Game.Removed`, `Preset.Applied`, `Session.Started`, `Session.Ended`.
- **Lotus → Ports**: `Dependencies.Changed`, `Driver.Changed` (o Lotus Ports avisa no cartão: "o primeiro jogo depois de atualizar o driver pode engasgar enquanto se prepara"), `Profile.Changed`, `Lotus.ShuttingDown`.
- Eventos perdidos não quebram nada: ao reconectar, cada lado reconsulta o estado necessário.

### 12.10 Permissões da integração (Engrenagem → Lotus)
Interruptores, todos ligados por padrão quando conectado:
- Usar o kit de dependências do Lotus;
- Usar o diagnóstico do Lotus no "Resolver";
- Aplicar perfil de desempenho ao jogar;
- Mostrar relatórios e sugestões do Lotus;
- Permitir que o Lotus liste e abra meus jogos;
- Compartilhar o histórico de operações com o Lotus.

### 12.11 Instalação cruzada
- Sem Lotus: convite discreto (nunca bloqueante) "Instale o Lotus para diagnóstico e desempenho completos", com download **apenas** da release oficial do repositório do Lotus, confirmação e registro no diário.
- O Lotus pode oferecer o Lotus Ports da mesma forma (requisito no Apêndice A).

### 12.12 Testes da integração
- Servidor Lotus **simulado** em `LotusPorts.Integration.Tests` cobrindo: handshake, capacidades ausentes, versão incompatível, timeouts, reconexão, eventos, validação de peer, mensagens malformadas.
- Testes de contrato (serialização ida e volta) em `Lotus.Integration.Contracts`, reutilizáveis pelo repositório do Lotus.
- `docs/INTEGRACAO_LOTUS.md` com todos os métodos, exemplos de JSON, códigos de erro e tabela de capacidades.

---

## 13. Persistência (SQLite)

Tabelas mínimas (migrações versionadas em `Infrastructure/Database/Migrations`):
- `Games` (gameId, catalogVersion, followed, dismissedFromNew)
- `InstalledPorts` (portId, gameId, activeVersion, libraryPath, pinnedVersion, autoUpdate, presetApplied, installedAt)
- `InstalledVersions` (portId, tag, path, assetName, assetSize, sha256, integrityState, installedAt)
- `GameFiles` (id, path, size, modifiedAt, sha1, sha256, crc32, format, canonicalSha1, identifiedGame, region, version, lastScanAt)
- `GameFileLinks` (portId, gameFileId, linkKind, state, linkedAt)
- `PlaySessions` (id, portId, startedAt, endedAt, exitCode, durationSeconds, lotusSessionId)
- `LotusReportsCache` (lotusSessionId, json, fetchedAt)
- `Backups` (id, portId, path, reason, createdAt, sizeBytes)
- `ReleaseCache` (repo, etag, json, fetchedAt)
- `CatalogSources` (seção 5.8)
- `OperationJournal` (seção 11)
- `Settings` (key, value)

WAL habilitado; acesso só via repositórios em `Infrastructure`.

---

## 14. Qualidade, testes e logs

- **Testes unitários obrigatórios**: máquina de estados do botão principal; orquestrador "Jogar" (cada etapa falhando); ordem de bytes N64 e hash canônico; seleção do arquivo de download; validação do catálogo (jogos + ports); zip-slip; migração de `persistentPaths`; rollback; aplicação e reversão de estilo preservando chaves não declaradas; códigos de saída de redistribuíveis; contrato IPC.
- **Testes de integração** com servidor HTTP falso para a API do GitHub (ETag, rate limit, release sem arquivo Windows, digest divergente) e com o Lotus simulado.
- Serilog em `logs\` (rotação diária, 14 dias). Nunca registrar token, nome de usuário ou caminhos pessoais sem anonimizar.
- Tratamento global de exceções com tela amigável e opção de copiar detalhes.
- UI sempre responsiva; disco/rede assíncronos e canceláveis.

---

## 15. Fases de entrega e critérios de aceite

**Fase 0 — Fundação**
Solução, projetos (incluindo `Lotus.Design` e `Lotus.Integration.Contracts`), DI/Host, Serilog, SQLite, tokens, shell com barra superior, Estante e Página do jogo vazias com estados honestos, Engrenagem com modo avançado. `CLAUDE.md`, `docs/UX.md`.
✅ Compila, abre, navega, tema aplicado, testes rodando.

**Fase 1 — Catálogo por jogo**
Schemas de jogo e port, carregamento + atualização por ETag, validação, Estante com prateleiras, cartões, busca `Ctrl+K`, filtros em chips, Página do jogo com "Sobre este port" e "Outras formas de jogar".
Fontes de catálogo importáveis e "adicionar port por link do GitHub" (seção 5.8).
✅ Catálogo inválido rejeitado; jogos "Ainda em construção" só em "Em breve"; fonte importada com entrada de arquivo de jogo é rejeitada (teste automatizado).

**Fase 2 — Orquestrador "Jogar" e ciclo de vida**
Cliente GitHub, downloads com progresso no cartão, integridade, extração segura, versões, backups automáticos, migração de `persistentPaths`, rollback no cartão, diário de operações, máquina de estados do botão.
✅ Instalar → jogar → atualizar → voltar versão → desinstalar em ao menos um jogo real, com ANTES/DEPOIS e saves preservados.

**Fase 3 — Arquivos de jogo**
Primeiro uso, scanner em segundo plano, identificação, mensagens humanas, N64 byte order, vínculo por tipo de setup, Engrenagem → Arquivos de jogo.
✅ Três formatos N64 identificados; original nunca alterado (teste de hash antes/depois).

**Fase 4 — Estilos e "Resolver"**
Original/Remasterizado (somente verificados), "Resolver" local completo, Log de Eventos, relatório copiável.
✅ Estilo aplicado e revertido sem perder outras configurações; crash simulado gera "Resolver" com causa; nenhum `OK` sem verificação.

**Fase 5 — Integração Lotus I (cliente)**
Handshake, capacidades, segurança de peer, `Dependencies.*`, `Diagnostics.RunSystemChecks`, `Journal.QueryRecent`, `Session.Start/End`, deep links `lotus://`, indicador de conexão, permissões, modo autônomo de dependências.
✅ Funciona com Lotus ausente, com Lotus simulado completo e com Lotus simulado sem algumas capacidades.

**Fase 6 — Integração Lotus II (servidor e experiência)**
Pipe próprio, `Ports.ListGames`, `Ports.Launch`, `Ports.Open`, `Journal.Query`, eventos nos dois sentidos, `Session.GetReport`, `Recommendations.ForGame`, `System.GetHardwareProfile`, seção "Sua experiência", deep links `lotusports://`, instalação cruzada.
✅ Fluxo completo com Lotus simulado: o Lotus lista e abre um jogo; sessão gera relatório; sugestão aplicada e comparada.

**Fase 7 — Modo TV e controle**
Mesmas duas telas ampliadas, navegação total por controle.

**Fase 8 — Mods (EXPERIMENTAL)**
Somente ports com fonte oficial de mods declarada; instalar/desativar/remover com backup; preferencialmente dentro da Página do jogo.

**Fase 9 — Acabamento**
Atualização do próprio Lotus Ports pelas releases do repositório do José, instalador, ícone, Sobre/Legal, revisão de textos para leigos e de acessibilidade.

---

## 16. Checklist antes de cada commit

- [ ] Nenhuma busca, link, campo ou integração relacionada a ROMs, ISOs, BIOS, firmware ou chaves.
- [ ] Nenhuma funcionalidade aprovada removida.
- [ ] Se criei aba ou tela nova, ela é necessária, faz sentido para o projeto e o motivo foi explicado.
- [ ] Nenhum "sucesso" sem verificação real.
- [ ] Nenhum hash, chave de configuração, versão ou número inventado.
- [ ] Toda alteração em disco pede confirmação e passa pelo diário.
- [ ] Integração com o Lotus degrada graciosamente quando ele está ausente ou sem a capacidade.
- [ ] Nenhum dado do usuário enviado para fora do PC.
- [ ] Textos novos em `.resw` (PT-BR e en-US), sem jargão no modo padrão.
- [ ] `dotnet build` e `dotnet test` passaram — com a saída mostrada.

---

## 17. Primeira resposta esperada de você

1. Resumo, em até 15 linhas, do produto, das Regras Invioláveis e das regras de simplicidade.
2. Plano detalhado da **Fase 0** (arquivos, pacotes NuGet com licença, comandos).
3. Proposta de como compartilhar `Lotus.Design` e `Lotus.Integration.Contracts` com o repositório do Lotus.
4. Até 5 dúvidas que realmente bloqueiam.
5. Aguardar minha aprovação antes de criar arquivos.

---

## Apêndice A — O que o Lotus precisa implementar (entregar à sessão de desenvolvimento do Lotus)

1. Referenciar `Lotus.Integration.Contracts` e `Lotus.Design` (mesma fonte usada pelo Lotus Ports).
2. Servidor `\\.\pipe\lotus.integration.v1` (JSON-RPC 2.0, prefixo de tamanho, ACL do usuário atual, validação do peer) e registro em `HKCU\Software\Lotus\Integration`.
3. Métodos: `Handshake`, `Dependencies.Check`, `Dependencies.RequestInstall`, `Diagnostics.RunSystemChecks`, `Journal.QueryRecent`, `Session.Start`, `Session.End`, `Session.GetReport`, `System.GetHardwareProfile`, `Recommendations.ForGame`.
4. Eventos emitidos: `Dependencies.Changed`, `Driver.Changed`, `Profile.Changed`, `Lotus.ShuttingDown`. Eventos consumidos: os da seção 12.9.
5. Cliente do pipe `\\.\pipe\lotusports.integration.v1` para `Ports.ListGames`, `Ports.Launch`, `Ports.Open`, `Journal.Query`.
6. Deep links `lotus://dependencies`, `lotus://session/{id}`, `lotus://profile/{lotusGameKey}`.
7. Interface do Lotus:
   - kit de dependências abrindo com itens pré-selecionados e confirmação do usuário;
   - perfis de desempenho associados a `lotusGameKey`;
   - jogos do Lotus Ports na biblioteca do Lotus com botão "Jogar" (via `Ports.Launch`);
   - linha do tempo unificada com as operações do Lotus Ports;
   - convite discreto para instalar o Lotus Ports quando ausente.
8. Mesmas regras: não mostrar falso sucesso, não inventar números (relatórios só com valores medidos), confirmação de alterações reais, ANTES/DEPOIS, reversibilidade.
