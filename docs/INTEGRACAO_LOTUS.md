# Integração Lotus ↔ Lotus Ports

Referência do protocolo para os dois apps. Os tipos ficam em `src/Lotus.Integration.Contracts` (sem dependências, para o repositório do Lotus usar igual). O Lotus Ports funciona inteiro sem o Lotus; a integração só acrescenta.

## Transporte

- Named pipe local por app: `\\.\pipe\lotus.integration.v1` (servidor no Lotus) e `\\.\pipe\lotusports.integration.v1` (servidor no Lotus Ports, Fase 6). ACL só do usuário atual.
- Cada mensagem: 4 bytes (int32 little-endian) com o tamanho, depois o JSON UTF-8. Máximo 1 MB.
- JSON-RPC 2.0: requisição tem `id` e `method`; notificação (evento) não tem `id`; resposta tem `result` ou `error`. Enums vão como texto (`"Ok"`, `"Warning"`).
- Tempo de resposta: 2 s. Sem resposta, o outro lado é tratado como indisponível; nada trava a interface.

## Descoberta e segurança

- O Lotus grava `HKCU\Software\Lotus\Integration`: `PipeName` (texto), `ProtocolVersion` (número, hoje `1`), `AppVersion` (texto), `ExePath` (caminho completo do executável).
- O Lotus Ports só conversa depois de conferir que o processo servidor do pipe (`GetNamedPipeServerProcessId`) é exatamente o `ExePath` registrado. Caso contrário: estado **Conexão recusada por segurança** e nada é enviado.
- Métodos desconhecidos são recusados com `-32601`; nenhum comando arbitrário é executado. `Ports.Launch` (Fase 6) aceitará só `gameId` do catálogo.

## Handshake

Primeira mensagem, sempre do cliente:

```json
{ "jsonrpc": "2.0", "id": "1", "method": "Handshake",
  "params": { "appName": "Lotus Ports", "appVersion": "0.1.0", "protocolVersion": 1,
              "capabilities": ["Game.Installed", "Game.Updated", "Game.RolledBack", "Game.Removed", "Preset.Applied", "Session.Started", "Session.Ended"] } }
```

Resposta do Lotus, com o que **ele** oferece (nomes de métodos e de eventos que aceita):

```json
{ "jsonrpc": "2.0", "id": "1",
  "result": { "appName": "Lotus", "appVersion": "1.2.0", "protocolVersion": 1,
              "capabilities": ["Diagnostics.RunSystemChecks", "Journal.QueryRecent", "Session.Start", "Session.End", "Game.Installed"] } }
```

Cada lado só chama o que o outro anunciou. O que não foi anunciado some da interface (aparece "Este Lotus ainda não oferece").

## Métodos que o Lotus Ports usa (Fase 5)

| Método | params | result |
|---|---|---|
| `Diagnostics.RunSystemChecks` | `{ graphicsApis: ["Vulkan"], exePath }` | `{ items: [{ id, state, titlePtBr, detailsPtBr }] }` |
| `Journal.QueryRecent` | `{ days: 7 }` | `{ changes: [{ at, kind, descriptionPtBr }] }` |
| `Session.Start` | `{ lotusGameKey, gameTitle, portId, portVersion, exePath, processId, preset }` | `{ sessionId }` |
| `Session.End` | `{ sessionId, exitCode, durationSeconds }` | qualquer (ignorado) |
| `Dependencies.Check` | `{ ids: ["vcredist-x64-2015-2022"] }` | `{ items: [{ id, state, installedVersion, details }] }` |
| `Dependencies.RequestInstall` | `{ ids, reason }` | — o Lotus confirma com o usuário e responde depois pelo evento `Dependencies.Changed` |

`state`: `Ok`, `Warning`, `Failure` ou `NotVerified` (= OK, AVISO, FALHA, NÃO VERIFICADO). Textos `…PtBr` são mostrados como vierem.

Ids de dependências: `vcredist-x64-2015-2022`, `directx-legacy-runtime`, `vulkan-runtime`, `dotnet-desktop-runtime` (`DependencyIds`).

## Eventos

- Lotus Ports → Lotus (se o Lotus anunciar o nome e o jogador permitir "Compartilhar o histórico"): `Game.Installed`, `Game.Updated`, `Game.RolledBack`, `Game.Removed`, `Preset.Applied` com `{ gameId, portId, version }`.
- Lotus → Lotus Ports: `Dependencies.Changed`, `Driver.Changed` `{ device, version, at }`, `Profile.Changed`, `Lotus.ShuttingDown` (o Lotus Ports fecha a conexão e tenta de novo depois).
- Evento perdido não quebra nada: ao reconectar, cada lado consulta de novo o que precisa.

## Erros

| Código | Quando |
|---|---|
| `-32700` / `-32600` | JSON inválido / requisição inválida |
| `-32601` | método não oferecido |
| `-32602` | parâmetros inválidos |
| `-32603` | erro interno |
| `-32001` | versão do protocolo incompatível |
| `-32002` | permissão desligada pelo usuário |
| `-32003` | jogo desconhecido |

## Estados mostrados no Lotus Ports

Lotus não encontrado · Lotus instalado, ainda sem a integração (sem a chave no registro) · Lotus fechado (registrado, sem resposta; tenta de novo a cada 30 s) · Versão incompatível — atualize o Lotus · Conexão recusada por segurança · Conectado ao Lotus (vX.Y).

## Permissões (Lotus Ports → Integração com o Lotus)

Todas ligadas por padrão e salvas em `lotus.allow.*`: kit de dependências, diagnóstico no "Resolver", perfil de desempenho ao jogar, relatórios e sugestões, deixar o Lotus listar e abrir jogos (Fase 6), compartilhar o histórico.

## Testes

`tests/LotusPorts.Integration.Tests`: um Lotus simulado com pipe de verdade cobre ausente, sem integração, fechado, completo, sem algumas capacidades, versão incompatível, impostor (inclusive a conferência real pelo caminho do processo), silêncio (timeout), resposta malformada, eventos e reconexão. `ContractTests` faz ida e volta das mensagens e pode ser copiado para o repositório do Lotus.

## Para o Lotus implementar

1. Servidor no pipe `lotus.integration.v1` com ACL do usuário atual; gravar a chave do registro ao iniciar (e apagar `PipeName` ao sair, ou mandar `Lotus.ShuttingDown`).
2. Conferir o cliente: o processo do outro lado deve ser o `ExePath` gravado pelo Lotus Ports em `HKCU\Software\LotusPorts\Integration`.
3. Responder o `Handshake` anunciando só o que já funciona.
4. Toda ação no sistema pedida pelo Lotus Ports (instalar dependência, aplicar perfil) é confirmada pelo usuário na interface do Lotus.
