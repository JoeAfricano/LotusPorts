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

- **Critério de aceite ainda não cumprido:** instalar → jogar → atualizar → voltar versão → desinstalar num jogo real. Muda arquivos no PC (dentro de `%LOCALAPPDATA%\LotusPorts\library`), então exige aprovação do José. O ciclo completo está coberto por testes com downloads falsos, inclusive saves preservados.
- **Saves:** só os que ficam dentro da pasta do jogo (`persistentPaths` com `location: install`) seguem para a nova versão; a versão anterior fica guardada com os saves dela, e é isso que "Voltar para a versão anterior" restaura. Saves em pastas do Windows (`location: user`) nunca são tocados; backup deles fica com o Lotus.
- **Não instalados pelo app:** ports com instalador próprio (87) ou 7z (3) mostram "Abrir página oficial". Arquivo ambíguo (mais de um para Windows) também, sem adivinhar.
- **Ainda não feito:** progresso nos cartões da Biblioteca e do Início (hoje só na Página do jogo); tela Downloads com fila; leitura do `recomplauncher.json` (setup fácil, junto da Fase 3); pré-releases e "Fixar versão" no modo avançado (o modelo já guarda `PinnedTag`); retentativas de rede (Polly); "Instalados" nas prateleiras usando o `active.json`.
- **Integrações que dependem de outras fases:** verificar dependências e `Session.Start` (Fase 5), "Resolver" completo com causa e relatório (Fase 4). Hoje "Resolver" oferece voltar a versão quando o jogo fecha com erro em menos de 15 s logo depois de atualizar.
- **Operações interrompidas:** na abertura, sobras de instalação (`*.staging`, `*.part`) são apagadas e entradas abertas do diário viram "Cancelado"; não há tela para isso.