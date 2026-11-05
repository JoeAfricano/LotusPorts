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

- Indicador de downloads com fila: Fase 2. A tela Downloads hoje mostra o estado vazio real.
- Tratar operações interrompidas na abertura (limpar ou desfazer). `ListOpenAsync` já existe; a interface vem na Fase 2, quando houver operações.
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