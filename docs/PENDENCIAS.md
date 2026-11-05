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
| Biblioteca e Catálogo (sem catálogo ainda) | 1 |
| Fontes de catálogo | 1 |
| Arquivos de jogo | 3 |
| Página do jogo (todo id mostra "não encontrado") | 1 |
| Integração com o Lotus (não procura nem conecta) | 5 e 6 |
| Modo TV | 7 |
| Iniciar com o Windows | 9 |

## Previstos

- Indicador de downloads com fila: Fase 2. A tela Downloads hoje mostra o estado vazio real.
- Tratar operações interrompidas na abertura (limpar ou desfazer). `ListOpenAsync` já existe; a interface vem na Fase 2, quando houver operações.
- Ícone do app e instalador: Fase 9.
