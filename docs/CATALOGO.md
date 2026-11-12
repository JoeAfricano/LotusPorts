# Catálogo

O catálogo lista **jogos** (o que o jogador vê) e **ports** (as formas de jogar cada jogo). Ele contém só metadados e links para repositórios e documentação oficiais. Nunca contém links para arquivos de jogo.

## Onde fica

| Caminho | O que é |
|---|---|
| `catalog/catalog.schema.json` | JSON Schema (draft 2020-12). Vale para o catálogo oficial e para fontes importadas. |
| `catalog/games/{id}.json` | Um arquivo por jogo. Fonte para edição. |
| `catalog/ports/{id}.json` | Um arquivo por port. Fonte para edição. |
| `catalog/catalog.json` | Pacote gerado a partir de `games/` e `ports/`. Vai embutido no app e funciona offline. |
| `catalog/research/candidates.txt` | Repositórios avaliados e de onde vieram. |
| `catalog/research/scan.json` | Evidências coletadas no GitHub: release, arquivos, executáveis e linhas do README. |
| `catalog/research/seeds.json` | Curadoria: ligação repositório → jogo e fatos confirmados no README. |

## Regras (validadas pelo app e pelos testes)

1. O schema é verificado. Campo desconhecido, valor fora da lista ou campo obrigatório ausente fazem o catálogo ser **recusado**.
2. **Arquivos de jogo, nunca.** Qualquer campo com nome de ROM, ISO, BIOS, firmware ou chaves, ou qualquer URL terminada em extensão de arquivo de jogo (`.z64`, `.iso`, `.bin`, `.xex`…), recusa o catálogo inteiro. Uma fonte importada assim fica na lista com **AVISO** e nunca é ativada.
3. Links só para `github.com`, `raw.githubusercontent.com` ou os domínios de documentação e comunidade que o próprio port declara.
4. `links.repo` precisa ser o repositório declarado em `repo`. Os padrões de arquivo (`windowsAsset.pattern`) precisam ser expressões regulares válidas.
5. Referências cruzadas: o port recomendado e as alternativas existem e pertencem ao jogo, e todo port aponta para um jogo existente.
6. **Revisão:** um jogo oficial só aparece para o jogador com `review.state = "approved"`. Os pendentes só aparecem com "Mostrar jogos em revisão" (modo avançado), com o selo **Em revisão**.
7. O seletor de estilo (Original/Remasterizado) só aparece quando os dois estilos têm `verified: true` e chaves reais de configuração.
8. Valor que não foi confirmado fica `null` (o app mostra "não verificado") ou `"PREENCHER"` (versão/região de um hash). Nunca se inventa.

Se o catálogo embutido falhar na validação, o app não mostra jogos e registra o erro no log. Os testes impedem que isso chegue a um build.

## Como atualizar

```
# 1. Coletar evidências (opcional: GITHUB_TOKEN aumenta o limite da API)
$env:GITHUB_TOKEN = (gh auth token)
dotnet run --project tools/CatalogScout -- scan catalog/research/candidates.txt catalog/research/scan.json

# 2. Gerar arquivos de jogos/ports novos a partir de seeds.json (não sobrescreve edições manuais; --force sobrescreve)
dotnet run --project tools/CatalogScout -- generate catalog/research/seeds.json catalog/research/scan.json catalog

# 3. Depois de editar games/ ou ports/ à mão, refazer o pacote
dotnet run --project tools/CatalogScout -- bundle catalog

# 4. Aprovar jogos revisados (ficam públicos)
dotnet run --project tools/CatalogScout -- approve catalog José zelda-ocarina-of-time super-mario-64
dotnet run --project tools/CatalogScout -- approve catalog José --all

# 4b. Aplicar as decisões da página de revisão (um JSON por jogo, lidos do banco da página)
dotnet run --project tools/CatalogScout -- apply-review <pasta-das-revisoes> catalog José

# 5. Validar
dotnet test LotusPorts.slnx -p:Platform=x64

# 6. Publicar para quem já tem o app: commit de catalog/ e push para a branch main
```

### Catálogo pela internet (desde 12/11/2026)

Jogos e ports novos chegam ao jogador sem uma versão nova do app:
- O app lê `https://raw.githubusercontent.com/JoeAfricano/LotusPorts/main/catalog/catalog.json` 10 s depois de abrir e depois a cada 24 h, enquanto está aberto. Com ETag: quando nada mudou, é só uma consulta pequena, e o arquivo bruto não conta no limite da API do GitHub.
- O catálogo baixado passa pela mesma validação do embutido. Inválido, ou com qualquer referência a arquivo de jogo, é recusado e o app continua com a cópia que já usava.
- Só é usado se o `version` for **mais novo** que o do catálogo em uso. O `bundle` grava a data do dia e acrescenta `.2`, `.3`… quando já houve um pacote no mesmo dia.
- A cópia fica em `%LOCALAPPDATA%\LotusPorts\data\catalog-sources\official.json` (com `official.etag`). Um app mais novo com catálogo embutido mais recente ignora uma cópia baixada mais velha.
- Os jogos que chegam assim aparecem como "Chegou!". Em Catálogo → Fontes, o catálogo oficial mostra "atualizado pela internet".
- **Publicar = push na `main`.** Tudo o que estiver aprovado em `catalog/catalog.json` na `main` chega a todos os jogadores em até um dia. Antes do push, rodar os testes.

O gerador **falha em vez de adivinhar** nestes casos:
- o padrão de download pega zero ou mais de um arquivo da release;
- o zip tem vários executáveis e nenhum foi escolhido;
- o executável escolhido não existe dentro do zip.

O scanner lê só o índice no fim de cada zip oficial, com uma leitura parcial, sem baixar o jogo.

## Estado atual (05/11/2026)

- **65 jogos e 70 ports**, todos com release oficial para Windows e executável conferido dentro do pacote.
- Plataformas: Nintendo 64 (28), Xbox 360 (12), PlayStation (9), GameCube (4), Game Boy Advance (3), PC (3), celular (3), Wii U, Super Nintendo e PlayStation 2 (1 cada).
- Fontes da pesquisa:
  - [awesome-unofficial-pc-ports](https://github.com/Sebastrion/awesome-unofficial-pc-ports) (lista curada, só ports fáceis de instalar);
  - [awesome-rea-games](https://github.com/HaozheH3/awesome-rea-games/wiki);
  - allrecomps.com;
  - RecompLauncher.
- **Todos os jogos estão `pending`**, aguardando a revisão do José.
- **Todos os ports estão `NAO_AVALIADO`** ("Ainda não avaliado"): o status Jogável/Com bugs/Em construção é decisão de curadoria.

### Página de revisão

A revisão é feita na página [Revisão do catálogo](https://claude.ai/artifact/ChbDVyUECKt5sDCrv7zV4o), que é privada e só grava com a conta do dono. Para cada jogo:
- aprovar, recusar ou deixar pendente;
- escolher o port recomendado e escrever o motivo;
- definir o status de cada port;
- deixar observações.

As decisões ficam no banco da página (coleção `reviews`). O Claude lê essa coleção e aplica com `apply-review`. Se algum jogo ou port não existir, nada é gravado.

### O que revisar

1. **Recomendação nos jogos com mais de um port** (propostas):

   | Jogo | Recomendado (proposta) | Alternativa |
   |---|---|---|
   | Majora's Mask | Zelda 64: Recompiled | 2 Ship 2 Harkinian |
   | Super Mario 64 | Ghostship | sm64coopdx (online) |
   | Mario Kart 64 | SpaghettiKart (1.0 estável) | Mario Kart 64: Recompiled (0.9) |
   | Banjo-Kazooie | Banjo: Recompiled | Lighthouse (multijogador, randomizador) |
   | Crash Team Racing | High Octane (v1.5 estável) | ctr-native (beta) |

2. **Status de cada port** (Jogável do começo ao fim / Jogável, com alguns bugs / Ainda em construção / Não recebe mais atualizações).
3. **Setup não verificado** em 23 ports (o README não deixa claro como o arquivo do jogo é usado):
   - 3on3-nhl-recomp, battleship, battletanx-ga-pc-port, case-west-recomp, case-zero-recomp;
   - ctr-native, devilutionx, dk64-recompiled, dusklight, earthworm-jim-hd-recompiled;
   - lamborghini-recomp, mega-man-64-recompiled, no-mercy-recompiled, redriver2;
   - rsdkv3-sonic-cd, rsdkv4-sonic-1, rsdkv4-sonic-2, sm64coopdx, sonic-mania-decomp;
   - strikers-pc, symphony-recomp, wipeout-phantom-edition, yfm-recompiled.
4. **Versões com hash e região "PREENCHER"**: o hash foi copiado do README oficial, mas o README não diz qual revisão ele é.
5. **11 ports só têm versões de teste** (`preferPrerelease: true`):
   - bomberman-hero, ctr-native, darkness-recomp, golden-sun (x2), kings-field-enhanced;
   - os 5 de luta livre do jessetbh.
6. **25 ports sem licença declarada** no GitHub: o app mostra "Não informada no repositório".

### Fora do catálogo, e por quê

- **Dinosaur Planet Recompiled:** é um protótipo vazado, sem cópia legal possível.
- **pinyon-shift:** é uma prévia técnica sem executável de jogo.
- **auto-tmc:** é uma ferramenta auxiliar do tmc, que já está no catálogo.
- **jak-project:** o catálogo usa o launcher oficial OpenGOAL.
- **Sem release para Windows (só código-fonte), 17 repositórios:**
  - Crash-Team-Racing-Turbocharged, sm64rt, cannonball, sm_rewrite, sonic3air, foxhollow;
  - DownpourRecomp, doaxvb-re, BloodyRoar2Recomp, wipeout-rewrite, Super-Metroid-Launcher, Zelda-3-Launcher;
  - ChameleonTwist1-JP-Recomp, Quest64-Recomp, sm64ex, snesrev/sm, smw, zelda3.
- **fgsfdsfgs/doom64ex-plus:** o repositório não existe mais.

## Fontes importadas

Na tela **Catálogo → Fontes de catálogo**, o jogador cola `https://github.com/{dono}/{repo}` (o app lê `catalog.json` da branch padrão) ou um link `raw.githubusercontent.com/.../catalog.json`.
- A fonte passa pela mesma validação e é atualizada por ETag.
- Uma atualização inválida mantém a cópia anterior.
- Os jogos aparecem com o selo **Fonte externa** e ficam "não verificados" até a fonte ser marcada como confiável.
- Em conflito de id, o oficial prevalece e o port externo vira "Outras formas de jogar".
- Remover a fonte não desinstala jogos.

**Adicionar port por link do GitHub** (modo avançado) lê as releases, sugere o arquivo para Windows e o executável e cria uma entrada local com status Jogável, com alguns bugs (EXPERIMENTAL) e o selo **Adicionado por você**. "Exportar meus ports" gera um JSON no formato do catálogo para incluir no oficial.

## Capas

`dotnet run --project tools\CatalogScout -- covers catalog` grava em cada jogo só o endereço da capa (`cover.url`); o app baixa ao exibir, confere a assinatura PNG/JPEG e guarda uma cópia reduzida em `%LOCALAPPDATA%\LotusPorts\cache\covers`.

1. **SteamGridDB** (preferida): capas verticais 600x900, mesmo formato em todos os cartões. Só aceita nome exato (sem palpite). Resultados ficam em `catalog/research/steamgriddb.json`, então uma nova execução só consulta jogos novos.
2. **libretro-thumbnails** (reserva): box art oficial por console.

A chave da API do SteamGridDB é do curador e nunca entra no repositório nem no app. Crie em steamgriddb.com → Preferences → API e salve num destes lugares:

- arquivo `%USERPROFILE%\.lotusports\steamgriddb.key` (só a chave, uma linha); ou
- variável de ambiente `SGDB_API_KEY`.

Sem chave, ou com o GitHub indisponível, o comando mantém as capas que cada jogo já tem.

## Consoles recentes (06/11/2026)

- Busca no GitHub por recompilações de Switch 2, Switch, PS4, PS5 e outras para Windows (`catalog/research/candidates-modern.txt`, conferidas em `scan-modern.json`, fatos em `map-modern.json`).
- Entraram com versão para Windows: Pokémon Sword and Shield, EA Sports UFC 4, NHL 24, Sengoku Basara 4 Sumeragi, The Binding of Isaac; e como outras formas de jogar: Tears of the Kingdom (TotK Port), The Order: 1886 (TOport e Order1886-PC), Conker: Live & Reloaded.
- Em "Em breve" (ainda sem versão para baixar, botão Acompanhar): Donkey Kong Bananza (Switch 2), Super Mario Odyssey, Echoes of Wisdom, Super Smash Bros. Ultimate, LittleBigPlanet 3, Shadow of the Colossus (PS4), White Knight Chronicles II; e alternativas em construção para Sonic 2006, God of War, inFAMOUS Second Son e Drakengard 3.
- Fora: ports que vão *para* Switch/PS4/PS5/Android/Mac (o Lotus Ports roda no Windows), os que só embutem um emulador, ferramentas genéricas, protótipos vazados, repositórios arquivados.
- Plataforma nova: `SWITCH_2`. No gerador, `"soon": true` lista um projeto sem versão em "Em breve" e `"exe"` indica o programa quando o pacote tem mais de um.
- Repetir a busca de tempos em tempos: os usuários procuram muito por esses consoles.

## Revisão de 06/11/2026

- **632 de 640 jogos aprovados.** Os 76 que estavam pendentes tiveram o índice do zip oficial lido (sem baixar o jogo) e o programa de entrada escolhido pela regra: o launcher ou instalador do próprio port quando existe, senão o executável do jogo; ferramentas de compilação nunca. Def Jam Vendetta e HCTP usam o "Settings.exe", que o README de cada um indica como entrada.
- **8 continuam em revisão**, com o motivo em `review.notesPtBr`: FIFA 2010 (só pasta de compilação), 50 Cent (compilação de teste), Final Fantasy NES (zip de código-fonte), Namco System 22 (13 jogos num pacote só), Kirby Air Ride (só o editor de mapas), Tekken Tag 2 (só o servidor), Shadow Fight 2 (só ferramentas) e Cut the Rope (7z não conferido).
- O status dos ports (Jogável, Com bugs…) continua "Ainda não avaliado": isso só se afirma jogando.
