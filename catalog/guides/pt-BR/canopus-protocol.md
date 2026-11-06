<!-- source: https://github.com/riomoo/canopus-protocol/blob/HEAD/README.md | fingerprint: 6ac022850ea6d7e9 -->

O jogo procura os dados numa pasta `data` na pasta atual, ou onde o `--data` indicar:

```
build/linux/ds2rb --data data          # Linux (or: make run)
build\windows\ds2rb.exe --data data    # Windows, from this folder
```

No Windows, o `run.bat` desta pasta abre a versão que você montou. O `run.sh` faz o mesmo no Linux. Para jogar de outro lugar, copie o `.exe` e passe o caminho completo da `data` com `--data`.

### Controles

- 3DS · Teclado

- Direcional · Setas

- A / B · X / Z

- X / Y · S / A

- L / R · Q / W

- START / SELECT · Enter / Backspace

- Circle Pad · I J K L

- Tela de toque · Botão esquerdo do mouse na tela de baixo

- Menu de cheats · F1

### Opções de imagem

A imagem é desenhada com vsync (no ritmo da atualização do monitor), então não rasga. No Windows, ela passa pelo OpenGL, ou pelo GDI se não houver driver OpenGL; no Linux, pelo renderizador acelerado do SDL (normalmente OpenGL). Acrescente estas opções depois de `--data data`, ou na linha do `run.bat` / `run.sh`:

- Opção · Efeito

- `--no-vsync` · não espera o monitor (pode rasgar; um pouco menos de atraso)

- `--smooth` · ampliação suave (bilinear) em vez de pixels nítidos

- `--output gdi` · Windows: desenha com GDI em vez de OpenGL

- `--output software` · Linux: o renderizador por software do SDL

O `ds2rb.log` diz qual saída está em uso (a linha `window: output:`). O 3D do jogo ainda é desenhado pelo processador; só a imagem final passa pela placa de vídeo.

Os saves ficam em `data/save/`. A janela pode ser redimensionada. A barra de título mostra os quadros por segundo e o quanto o renderizador está ocupado.

### Menu de cheats

Aperte **F1** no jogo para abrir o menu de cheats sobre a tela de cima (o jogo para de ler os controles enquanto ele está aberto). As setas movem, Enter escolhe, Esc volta e F1 fecha; nas listas, dá para digitar para buscar e usar Page Up / Page Down para pular.

- Item · O que faz

- Max Macca · deixa o seu dinheiro em 999.999

- Add a demon · qualquer demônio, em qualquer nível do nível base até 99, num espaço livre do estoque (também registrado no compêndio)

- Party and stock · por membro ou demônio: nível 99, todos os atributos em 40, ou os dois; Fate no máximo

- Learn skills · libera qualquer habilidade de comando, automática ou racial para herança, uma a uma ou todas de uma vez

- Max Fate with everyone · Fate nível 5 com todos os personagens

- Fill the compendium · registra todos os demônios

As mudanças são feitas na memória; **salve no jogo** para mantê-las. O menu é para usar fora de batalhas e cenas (no mapa ou num menu).

### Se algo der errado

O jogo grava o `ds2rb.log` na pasta de onde roda. Ele registra cortes de som, travadas (linhas `hitch` e `no new picture`), uma linha de quadros por segundo a cada 10 segundos e, numa falha, os registradores e a pilha. Inclua esse arquivo ao relatar um problema.

Se o jogo parar ao abrir com "code.bin ... is not the code this build was recompiled from", os dados do jogo mudaram depois da última recompilação (por exemplo, a atualização foi instalada com uma cópia antiga deste Makefile). Rode `make recomp` e monte de novo.

Opções de teste: `-v` registra mais, `--headless` roda sem janela. Variáveis de ambiente para depuração:

- Variável · Efeito

- `DS2RB_INPUT` · entrada por roteiro, por exemplo `2000:START;2600:UP;2700-40000/90:A`

- `DS2RB_DUMP=n` · grava as duas telas em `frames/` a cada n quadros

- `DS2RB_WAV=file` · também grava o som num arquivo WAV

- `DS2RB_SPEED=x` · roda o relógio do jogo x vezes mais rápido

- `DS2RB_TRACE=s` · registra cada chamada de sistema depois de s segundos

- `DS2RB_GPU_SYNC` · deixa o renderizador síncrono
