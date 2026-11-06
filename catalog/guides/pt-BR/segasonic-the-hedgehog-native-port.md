<!-- source: https://github.com/cheviman/segasonic-port/blob/HEAD/README.md | fingerprint: ff76eefe8f0046e6 -->

### Download

A [versão mais recente](https://github.com/cheviman/segasonic-port/releases/latest) tem um pacote para **Windows** (x64), **Linux** (x64) e **macOS** (Apple Silicon e Intel, um app só), e um para os **portáteis H700 da Anbernic**. Extraia onde quiser:

- Pacote · O que tem

- Windows `segasonic-windows-x64.zip` · `SegaSonic.exe` (a versão para console), `SegaSonic Arcade.exe` (a de fliperama) e as imagens que a janela usa para letras e controles; `SegaSonic NSP.exe` e `SegaSonic APK.exe`, que fazem os jogos de Switch e Android, com as versões do jogo para esses sistemas ao lado (`switch/`, `android/`)

- Anbernic `segasonic-anbernic.zip` · `PORTS/`, a pasta para `Roms/` do portátil: `SegaSonic.sh` e o jogo em `segasonic/`, onde vai o seu `segasonic.assets` (abaixo)

Cada um tem um `README.txt` que diz qual jogo é qual, e as licenças do que os executáveis contêm. O SDL2 e o runtime de C vêm dentro dos executáveis: nada mais para instalar. Os apps de macOS não têm assinatura de desenvolvedor: na primeira vez, abra com botão direito → Abrir (no macOS 15 em diante, tente abrir e depois Ajustes do Sistema → Privacidade e Segurança → Abrir Mesmo Assim).

### A primeira abertura

O jogo precisa do conjunto de ROMs do fliperama: o `sonic` do MAME (SegaSonic The Hedgehog, Japão, rev. C), o `sonic.zip` de 18 arquivos, tirado da sua própria placa. Coloque ao lado do jogo (do `.app` no macOS) antes de abrir, ou abra o jogo e arraste para a janela que pede. O jogo confere cada arquivo pelo CRC32, tira o que usa para o `segasonic.assets` (na pasta onde guarda os arquivos, abaixo) e começa; dali em diante, ele carrega esse pacote e nunca mais abre o conjunto de ROMs. O pacote é feito da sua cópia: é seu, não é para compartilhar.

### A versão para console: SegaSonic

O `SegaSonic` é o jogo de fliperama feito para jogar em casa, numa camada própria sobre o mesmo programa; o jogo de fliperama por baixo roda como na placa.

- **Sem fichas.** Ao ligar, aparece o logo da SEGA (com uma vinheta própria) e o título: PRESS START abre um menu, START GAME, TIME ATTACK, OPTIONS, EXIT. Uma partida tem CONTINUES (3 no começo) em vez de créditos, ou é livre.

- **Até três jogadores**, como o gabinete de 3 jogadores do fliperama: cada um com as próprias teclas e controle. Um jogador entra na escolha de jogador, ou quando o aviso de start dele aparece.

- **WIDE** (OPTIONS → VIDEO): imagem 16:9. As fases, o título, o ranking e a escolha de jogador ficam mais largos; além de onde a câmera da placa olhava, o mapa da fase continua. As cenas, desenhadas para 4:3, aparecem entre faixas.

- **OPTIONS**: DIFFICULTY e VITALITY (as configurações do operador), CONTINUES, VIDEO (tamanho da janela, tela cheia, PIXELS: nítido, suave, macio ou com linhas e máscara de CRT, WIDE), CONTROLS, TRACKBALL, VOLUME, SOUND TEST (a música escolhida vira a música do título) e STAGE SELECT.

- **TIME ATTACK**: uma fase, ou o jogo inteiro a partir do lugar do Eggman no mapa. O relógio corre por fases, cenas e continues, com parciais no fim de cada fase; BEST TIMES e BEST RUN guardam os recordes.

- **STAGE SELECT**: o mapa da ilha com todas as fases abertas.

- **Em inglês**: o "como jogar" e as falas do Eggman aparecem nas caixas de texto em inglês do próprio jogo, que o programa nunca mostrava (a placa japonesa mostra as japonesas).

- **Pausa**: start (ou voltar) numa fase pausa o jogo e as cenas.

- **CONTROLS**: as teclas e botões de cada ação, para cada jogador, na tela.

- **TRACKBALL** (Windows, macOS e Linux): o quanto o analógico, as teclas, o direcional e o mouse fazem o Sonic rolar. As configurações ficam no `segasonic.ini`.

### Controles

As ações da versão para console, por padrão (OPTIONS → CONTROLS muda):

- Ação · Jogador 1 · Jogador 2 · Jogador 3 · Controle

- Mover (o trackball) · setas ou WASD, ou o mouse (depois de um clique) · I J K L · 8 4 5 6 do teclado numérico · direcional ou analógico esquerdo

- Pular, girar · Z ou Espaço, ou o botão esquerdo do mouse · U · 0 do teclado numérico · A

- Start, pausa · Enter ou P · 2 · 3 · Start

- Aceitar (menus) · Enter, Z ou Espaço · U · 0 do teclado numérico · A

- Voltar (menus), pausa · Esc ou Backspace · O · . do teclado numérico · B

O mouse é um ponteiro até um clique na janela torná-lo o trackball do jogador 1 (o ponteiro some); uma pausa (Esc) devolve o ponteiro, e um clique o pega de novo. Alt+F4 ou Cmd+Q fecham o jogo a qualquer momento; o menu do título tem EXIT.

F2 abre o modo de teste do fliperama (o menu do operador). Nele, cima e baixo movem o cursor e Enter (ou A) escolhe, muda e sai; 9 e F2 fazem o mesmo, como os botões de serviço e teste do gabinete.

No Windows, cada mouse (inclusive um trackball) é um trackball próprio: o primeiro a se mexer é o do jogador 1, e os próximos a apertar um botão são os jogadores 2 e 3.

A janela, nas duas versões: F11 ou Alt+Enter tela cheia, F3 pixels suaves ou nítidos, F4 se a bola do mouse continua rolando quando ele para, F5 e F6 o mouse mais lento ou mais rápido.

Controles, nas duas versões: os que o SDL conhece (Xbox, PlayStation, Switch e muitos outros) funcionam direto. Outro, como um controle USB genérico, precisa do mapeamento no `gamecontrollerdb.txt` da pasta onde o jogo guarda os arquivos: as linhas do [SDL_GameControllerDB](https://github.com/mdqinc/SDL_GameControllerDB). O Steam Input também funciona.

### A versão de fliperama: SegaSonic Arcade

O `SegaSonic Arcade` é o jogo de fliperama como a placa o roda, igual ao MAME quadro a quadro: coloque uma ficha (5) e depois start (1). Controles do jogador 1:

- Controle · Teclas

- Trackball · o mouse, ou as setas

- Botão · botão esquerdo do mouse, Z ou espaço

- Fichas 1-3, starts 1-3 · 5, 6, 7 e 1, 2, 3

- Serviço, teste · 9 e F2

- Sair · Esc

O mouse é sempre o trackball aqui, e gira como o do fliperama: a bola gira com o mouse e, quando ele para, continua rolando e vai parando; o botão direito a segura. Cada controle é um jogador: o analógico esquerdo é o trackball, A o botão, Start o start e Back uma ficha.

### Onde o jogo guarda os arquivos

Ao lado do executável no Windows; `~/.local/share/SegaSonic` no Linux; `~/Library/Application Support/SegaSonic` no macOS.
