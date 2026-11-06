<!-- source: https://github.com/goodshepherd23/MOHFrontline-Recomp/blob/HEAD/README.md | fingerprint: 36a1c86922e3f0c2 -->

Abra o `MOHFrontline.exe`. Na primeira vez, ele pergunta onde instalar os arquivos do jogo (cerca de 3 GB; o padrão é `%LOCALAPPDATA%\MOHFrontline-Recomp`) e depois abre uma janela de configuração que pede o seu disco e monta o jogo. **Isso leva de 20 a 60 minutos**, conforme o processador (cerca de 20 minutos num notebook ligado na tomada com um Intel i7-1355U), e a janela mostra o progresso e o tempo estimado. O seu disco é lido só uma vez. Atualizações deste kit só remontam o jogo quando a própria montagem mudou.

Depois disso, uma janela de configurações aparece antes de cada abertura:

- **Resolution:** nativa (a mais rápida) até 4x.

- **Graphics:** Direct3D 11 (recomendado), Vulkan ou OpenGL.

- **Fullscreen**, **Widescreen** e **Show speed in the title bar**.

- **Accurate audio:** o DSP LLE do Dolphin, ligado por padrão. O HLE, mais rápido, perde efeitos sonoros neste jogo. Desmarque para ganhar alguns por cento de velocidade.

- **Reset controls:** volta aos controles padrão abaixo.

- **Change disc**, **Rebuild** e **Open folder**.

Marque "Don't show this window" para ir direto ao jogo; segure **Shift** ao abrir para ver a janela de novo. `MOHFrontline.exe --setup` e `--rebuild` também continuam funcionando.

Configurações e saves ficam na pasta escolhida, então atualizar o app os mantém. Para guardar tudo na pasta do app, coloque um `portable.txt` vazio ao lado do `MOHFrontline.exe`.

### Controles

Um controle estilo Xbox (XInput) e teclado e mouse funcionam direto. O jogo usa o layout **MOH Sharpshooter** (escolha-o quando um jogo novo perguntar):

- Ação (GameCube) · Controle · Teclado e mouse

- Andar (analógico) · Analógico esquerdo · W A S D (ou setas)

- Olhar (C-stick) · Analógico direito · Mouse

- Atirar (R) / Mirar (L) · Gatilho direito / esquerdo · Botão esquerdo / direito do mouse

- Ação (A) · A · E (Enter nos menus)

- Corpo a corpo (B) · X · F (Backspace volta nos menus)

- Agachar (X) / Pular (Y) · B / Y · C ou Ctrl esquerdo / Espaço

- Recarregar (Z) · Botão de ombro direito · R

- Armas (direcional esquerda/direita) · Direcional · Roda do mouse, ou Z / X

- Direcional cima / baixo · Direcional · 1 / 2

- Pausa (Start) · Start · Esc

O mouse fica preso à janela do jogo enquanto ela está em foco. Se você mudou os controles na v0.1.0, clique em **Reset controls** na janela de configurações para ter este layout. O mapeamento é do Dolphin, em `user\Config\GCPadNew.ini` na pasta do jogo.

### Tela larga

O kit liga o hack de tela larga do Dolphin, então a visão 3D é um 16:9 de verdade. Menus e HUD ficam esticados para os lados, o que o hack não consegue evitar. Desmarque **Widescreen** na janela de configurações para a imagem 4:3 original.
