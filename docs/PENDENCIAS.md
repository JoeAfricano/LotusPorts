# Pendências

## Decisões em aberto

- **Recomendação de port por jogo:** decisão do José (seção 5.7).
- **Criar o repositório `Lotus.Shared`** com Design e Contracts, como submódulo: quando o José pedir.

## Fase 0: o que não foi validado

- Validação interativa:
  - as telas foram conferidas por captura (`--page=`), mas cliques não foram exercitados por automação;
  - não testados: busca com `Ctrl+K`, troca de idioma com reinício, interruptores, seletor de pasta e modo avançado com diário.
- A camada por trás (preferências, banco, diário, regras de pasta e busca) tem testes automatizados.
- O diário nunca mostrou uma linha real, porque nenhuma operação existe ainda (chega na Fase 2).

## Rotulados como EM DESENVOLVIMENTO na interface

| Item | Fase |
|---|---|
| Arquivos de jogo | 3 |
| Integração com o Lotus (não procura nem conecta) | 5 e 6 |
| Modo TV | 7 |
| Iniciar com o Windows | 9 |

## Previstos

- Indicador de downloads com fila: ainda não feito (ver Fase 2: pendências).
- Operações interrompidas: limpeza automática na abertura (ver Fase 2: pendências).
- Ícone do app e instalador: Fase 9.

## Fase 1: pendências

- **Revisão do catálogo pelo José:** 65 jogos pendentes, recomendações, status e setups. Lista completa em `docs/CATALOGO.md`.
- **Catálogo oficial remoto (atualização por ETag):** o mecanismo existe e é usado pelas fontes importadas, mas o catálogo oficial ainda só vem embutido no app, porque o repositório `lotus-ports-catalog` depende do GitHub, que fica para quando o José avisar.
- **Polly e retentativas de rede:** entram na Fase 2, junto com os downloads. Na Fase 1 as falhas de rede só viram mensagem e a cópia anterior é mantida.
- **Não testado por automação:**
  - busca com `Ctrl+K` digitando um jogo;
  - adicionar uma fonte real, porque ainda não existe um catálogo público de terceiros nesse formato (o fluxo está coberto por testes com servidor falso);
  - "Adicionar port por link", que lê o GitHub de verdade, e exportar JSON;
  - favoritar pela Página do jogo.
- **Fonte com campo de arquivo de jogo:** fica listada com AVISO e não pode ser ativada; a ação "Ativar" só funciona se houver uma cópia válida guardada.
- **Ports "Adicionado por você":** o padrão do arquivo fica fixo no nome de hoje, e quem incluir no catálogo oficial deve generalizar.

## Fase 2: pendências

- **Critério de aceite cumprido em 06/11/2026 (aprovado pelo José), com Zelda 64: Recompiled:**
  - instalar v1.2.1 (3,9 s, SHA-256 do GitHub conferido: OK) → abrir (menu do jogo, fechou com código 0) → atualizar para v1.2.2 (3,5 s, OK; v1.2.1 guardada ao lado) → abrir → voltar para v1.2.1 → abrir → desinstalar mantendo saves;
  - diário com 4 entradas ANTES/DEPOIS, todas "Sucesso";
  - a pasta de saves do jogo (`%LOCALAPPDATA%\Zelda64Recompiled`, 4 arquivos criados pelo próprio jogo) ficou idêntica em todos os passos e continua no PC;
  - o roteiro usou o mesmo motor do app sobre a pasta real; os cliques na interface não foram automatizados, mas a Página do jogo foi conferida por captura após instalar ("Jogar" + "Nova versão v1.2.2 disponível") e após atualizar ("Voltar para a versão anterior (v1.2.1)");
  - achado e corrigido no teste: sobravam pastas vazias depois de desinstalar.
- **Saves:** só os que ficam dentro da pasta do jogo (`persistentPaths` com `location: install`) seguem para a nova versão; a versão anterior fica guardada com os saves dela, e é isso que "Voltar para a versão anterior" restaura. Saves em pastas do Windows (`location: user`) nunca são tocados; backup deles fica com o Lotus.
- **Não instalados pelo app:** ports com instalador próprio (87) ou 7z (3) mostram "Abrir página oficial". Arquivo ambíguo (mais de um para Windows) também, sem adivinhar.
- **Ainda não feito:** progresso nos cartões da Biblioteca e do Início (hoje só na Página do jogo); tela Downloads com fila; leitura do `recomplauncher.json` (setup fácil, junto da Fase 3); pré-releases e "Fixar versão" no modo avançado (o modelo já guarda `PinnedTag`); retentativas de rede (Polly); "Instalados" nas prateleiras usando o `active.json`.
- **Integrações que dependem de outras fases:** verificar dependências e `Session.Start` (Fase 5), "Resolver" completo com causa e relatório (Fase 4). Hoje "Resolver" oferece voltar a versão quando o jogo fecha com erro em menos de 15 s logo depois de atualizar.
- **Operações interrompidas:** na abertura, sobras de instalação (`*.staging`, `*.part`) são apagadas e entradas abertas do diário viram "Cancelado"; não há tela para isso.
## Como jogar: traduções

- A Página do jogo mostra os passos de uso do README do port. Em português: a seção que os autores escreveram em português, se houver; senão a tradução do catálogo (`catalog/guides/pt-BR/{portId}.md`); senão o original marcado "em inglês".
- **Todas feitas (366), em 06/11/2026.** Quando um README muda, o comando abaixo lista a tradução como desatualizada.
- Para continuar: `dotnet run --project tools/CatalogScout -- guides catalog` exporta o inglês para `catalog/research/guides-en/` (fora do git) e lista as traduções que faltam ou ficaram desatualizadas. Cada tradução começa com a linha `<!-- source: … | fingerprint: … -->` copiada do arquivo exportado; o app avisa quando o original muda depois da tradução.
- Links para arquivos de dados de jogo (ex.: `spawn.mpq`) não entram nas traduções.

## Fase 4: pendências

- Nenhum port tem estilos `verified` no catálogo: o seletor Original/Remasterizado não aparece em nenhum jogo até a curadoria confirmar as chaves na configuração real de cada port. TOML ainda não é escrito.
- A janela "Resolver" não foi vista na tela com um jogo real quebrado (exigiria quebrar uma instalação no PC); o comportamento está coberto por testes com um programa real que fecha com erro.
- Placa de vídeo e dependências: sem o Lotus, o Resolver só confere se as bibliotecas existem no Windows (o suporte da placa fica "Não verificado"). Nenhum port declara `dependencies` no catálogo ainda.

## Fase 5: pendências

- **Lado do Lotus:** o Lotus (outro repositório) ainda não tem o servidor do pipe nem grava `HKCU\Software\Lotus\Integration`. O roteiro está em `docs/INTEGRACAO_LOTUS.md` ("Para o Lotus implementar"). Não mexi no projeto do Lotus.
- **Não feito:** pedir instalação de dependência ao Lotus (`Dependencies.RequestInstall`) e o modo autônomo que instala redistribuíveis oficiais da Microsoft (Authenticode, UAC, códigos 0/3010/1638); `Dependencies.Check` só faz sentido quando os ports declararem `dependencies` (nenhum declara ainda); aviso no cartão após `Driver.Changed`; deep links `lotus://` ("Ver relatório no Lotus"), que dependem do Lotus registrar o protocolo.
- O Lotus Ports ainda não grava `HKCU\Software\LotusPorts\Integration` nem serve o próprio pipe: Fase 6.

## Fase 6: pendências

- Fluxo completo testado só com Lotus simulado; o Lotus real precisa implementar os dois lados (cliente do pipe do Lotus Ports e servidor do próprio).
- Janela única (06/11/2026): uma segunda abertura, inclusive por link `lotusports://`, repassa o link para a janela já aberta e fecha. Testado: a janela aberta foi para a página do jogo.
- `System.GetHardwareProfile` está no contrato, mas o primeiro uso (que sugeriria o estilo padrão) ainda não existe.
- Instalação cruzada (convite discreto para instalar o Lotus, baixando a release oficial com confirmação) não feita: hoje há só o botão "Baixe o Lotus" que abre a página de releases.

## Temas: pendências

- Estúdio de temas (criar e editar tema dentro do Lotus Ports) e fundos animados do Lotus não foram trazidos; temas novos se criam no Lotus e chegam por "Igual ao Lotus" ou pelo código.
- O motor de temas está copiado do Lotus: qualquer mudança lá precisa ser repetida aqui até o `Lotus.Shared` existir.

## Fase 7: pendências

- A navegação por controle não foi testada com um controle de verdade (nenhum foi usado nos testes): falta você conferir no PC com um controle conectado.
- Sem vibração nem ícones de botões do controle na tela; menus suspensos e diálogos usam o foco padrão do WinUI.

## Fase 8: pendências

- A seção Mods não foi vista na tela: aparece só com um port que declara mods instalado, e nenhum está instalado no PC agora. O motor está coberto por testes.
- Só 3 ports declaram mods (os que têm instruções oficiais no README). Outros entram quando a curadoria confirmar no repositório de cada port.
- O Lotus Ports não baixa mods: o jogador baixa na página oficial e escolhe o arquivo. Mods em pastas (não arquivo único) ainda não são aceitos.

## Biblioteca viva e primeiro uso: pendências

- Estados "Baixando", "Jogando", "Atualização disponível" e "Precisa de atenção" nos cartões e o aviso "Atualizar tudo" só foram vistos em código e testes: nenhum port está instalado no PC agora para conferir na tela.
- Atualizações automáticas (ligadas por padrão): a verificação diária instala sozinha as versões novas dos ports instalados, uma por vez, nunca com o jogo aberto; a anterior fica para "Voltar". Só roda com o app aberto (20 s após abrir e depois a cada 24 h); não há tarefa em segundo plano com o app fechado.
- A boas-vindas aparece uma vez (`general.welcomeDone`); em Ajuda há "Ver de novo".

## Fase 9: pendências

- Atualização do próprio Lotus Ports: pronta, mas desligada até a publicação. Quando o repositório existir, preencher `AppUpdateService.Repository` (dono/nome). As releases precisam de um `*Setup*.exe`; o SHA-256 vem do próprio GitHub. Versões de teste só chegam para quem já está numa versão de teste.
- Instalador: `tools\Build-Installer.ps1` + `tools\installer\LotusPorts.iss` (Inno Setup 6, por usuário, sem administrador). Ainda não foi gerado: só quando você pedir. Arte própria do assistente (como a do Lotus) não foi feita.
- "Iniciar com o Windows" não foi feito (o Lotus Ports não precisa ficar aberto; o Lotus é quem acompanha o PC).
