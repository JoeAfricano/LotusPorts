<!-- source: https://github.com/kirby-letsgo/oracles-decomp/blob/HEAD/README.md | fingerprint: dbd6d308f7d5cb94 -->

A aba de um jogo começa com **Resume** (o estado de quando você saiu) e os quatro espaços de estados salvos, cada um com a hora em que foi salvo e o mais novo marcado como **Latest**; escolher um começa dele. Abaixo, **Save files** abre os três arquivos do próprio jogo (nome, corações, essências), e escolher um abre direto nele; **Title screen** começa o jogo do início. Todos os menus funcionam com teclado, controle, mouse ou tocando nas linhas.

### Controles

- Ação · Teclado · Controle (Xbox / PlayStation)

- Andar · Setas · Direcional ou analógico esquerdo

- A · `X` · A / X

- B · `Z` · X / Quadrado

- Start · Enter · Start / Options

- Select · Backspace ou Shift direito · Back / Share

- Pausa · Esc · Guide / botão PS

- Acelerar · Segurar Tab · Segurar RT / R2

- Troca rápida · `C` · LB / L1

- Item X / Item Y · `A` / `S` · Y / Triângulo, B / Círculo

- Mudo · `M`

- Tela cheia · F11 ou Cmd+F

- Salvar / carregar estado no espaço 1 · Cmd+S / Cmd+R, ou Ctrl+S / Ctrl+R · L3 / R3

Todos os botões, inclusive Pausa, Acelerar, Troca, os botões de item e salvar / carregar estado, podem ser remapeados em Settings > CONTROLS: escolha uma linha e aperte a tecla ou o botão do controle (gatilhos também) que quiser. A troca rápida e os botões de item só fazem algo com as opções de jogo deles ligadas. A janela muda de tamanho em passos de pixel inteiro.

A barra de status, as caixas de item e as dicas dos menus mostram a tecla ou o botão de cada ação e trocam entre teclado e controle conforme você usa. Settings > Pad button names escolhe como os botões do controle são chamados: Auto segue o controle conectado (Xbox para o que não reconhecer), ou escolha Nintendo (o B e o A do próprio Game Boy), Xbox ou PlayStation.

### Configurações

Pelo launcher ou pelo menu de pausa: volume, filtro de tela (nítido, linhas de varredura, grade de LCD, CRT e os dois ampliadores de pixel art XBRZ e HQX, que arredondam diagonais serrilhadas em vez de transformar cada pixel num quadrado; são dois algoritmos diferentes, então vale testar os dois), escala (**pixel** para passos de pixel inteiro, **fill** para o maior tamanho que couber), cores do Game Boy Color, tela cheia, velocidade de aceleração (de 2x a 8x, 3x por padrão, igual em qualquer tela), controles e modo portátil.

Há também opções de conforto, **todas desligadas por padrão**:

- **Texto rápido** e **menus mais rápidos**

- **Troca rápida**: `C` troca os itens de A e B

- **4 espaços**: `A`/`S` viram mais dois botões de item. Destaque um item no inventário e aperte um deles para atribuir; depois, segure-o no jogo para usar o item. Em tela larga, eles ganham as próprias caixas X e Y à esquerda da barra de status, então os quatro itens ficam na tela ao mesmo tempo; sem tela larga, as caixas vão nas faixas pretas dos lados da imagem quando a janela é larga o bastante. Bombas, Bombchus e ferramentas de sementes mostram quanto resta.

### Estados salvos

Esc pausa o jogo: Resume, Save state, Load state (4 espaços, cada um com miniatura e a hora em que foi salvo, ao segundo) e Quit to menu, que salva um estado Resume e volta ao menu principal. Cmd+S e Cmd+R (Ctrl+S e Ctrl+R no Windows e no Linux), ou L3 e R3 no controle, são atalhos para o espaço 1. Mensagens como "Saved to slot 2" ou "Sound off" aparecem embaixo da imagem; depois de salvar, a mensagem oferece **Undo**, que devolve o que o espaço tinha antes.

Os estados salvos funcionam em todos os sistemas: todas as versões de 64 bits usam o mesmo formato, então um estado feito no computador carrega no celular.
