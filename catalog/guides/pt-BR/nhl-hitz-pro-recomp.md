<!-- source: https://github.com/dalves408/nhlhitzpro-recomp/blob/HEAD/README.md | fingerprint: 46b1e4da0d45ff86 -->

**Requisitos**

- Windows 10 ou 11, 64 bits

- Uma placa de vídeo com Direct3D 11 (qualquer uma da última década)

- Um processador de vários núcleos (o trabalho de vértices e o áudio do jogo rodam no processador)

- Num notebook com duas placas de vídeo (NVIDIA Optimus, AMD intercambiável), o jogo pede ao Windows a mais rápida

- Cerca de 2 GB livres (mais cerca de 1,3 GB para o pacote opcional de texturas em alta resolução)

- O seu próprio disco do **NHL Hitz Pro (USA)**, como imagem `.iso` / `.xiso` ou como pasta com os arquivos do jogo (a que tem o `default.xbe`)

**Instalar**

1. Rode o `NHLHitzPro-Setup-1.0.8.exe` e escolha onde instalar.

2. Quando pedir, escolha a imagem ou a pasta do seu disco. O instalador confere se é a versão aceita e copia os arquivos do jogo que precisa (cerca de 1,5 GB).

3. Na página **Hi-res texture pack**, marque a caixa para acrescentar o pacote de texturas 4x. É opcional.

4. Abra o **NHL Hitz Pro** pelo menu Iniciar.

Se o jogo já estiver instalado, o instalador avisa e mostra a versão instalada e a nova. Uma versão diferente só atualiza os arquivos do programa depois de você confirmar (sem precisar do disco; arquivos do jogo e saves ficam); a mesma versão oferece Reparar ou Sair.

Saves e o cache de shaders ficam em `%LOCALAPPDATA%\NHLHitzPro`. Desinstalar remove os arquivos do jogo, mas deixa os saves.

**Controles**

- Ação · Controle de Xbox · Teclado

- Start · Start · Enter

- A / B / X / Y · A / B / X / Y · Z / X / C / V

- Mover, direcional · Analógico esquerdo, direcional · W A S D, setas

- Tela cheia / janela · F11 ou Alt+Enter (abre em tela cheia; o último modo fica guardado)

- Pausar e sair · Esc pausa o jogo e pergunta se quer sair: Sim (Y ou Enter) ou Não (N ou Esc, continua)

Esc pausa o jogo (relógios, cronômetros e som param) e pergunta "Do you want to quit?"; escolher Não continua exatamente de onde parou. O jogo abre em tela cheia sem bordas e lembra o último modo usado.

Controles XInput (Xbox 360, Xbox One, a maioria dos controles de PC) funcionam sem configurar, até quatro para o jogo de quatro jogadores: dá para conectar ou tirar com o jogo aberto, e o controle N é o jogador N. O teclado é uma entrada extra do jogador 1. A janela mantém o formato 4:3 do jogo; em tela cheia, fica centralizada com faixas pretas.

**Menu de configurações (F1, ou segure Back e aperte Start no controle 1):** pausa o jogo e deixa mudar a **Render Scale** (um multiplicador para todo o desenho do jogo: 1x é o 640x480 nativo do Xbox, até 5x, 3200x2400), tela cheia, **Stretch image** (desligado mantém o 4:3; ligado ocupa a janela, esticado), VSync, **Frame Rate** (30, 60, 120, 144, 240 ou 360 fps; os vídeos de introdução sempre rodam a 60), anti-aliasing (MSAA 2x, 4x ou 8x) e filtragem anisotrópica. Use o teclado (Cima/Baixo, Esquerda/Direita, Enter, Esc) ou o controle 1 (direcional ou analógico esquerdo, A para confirmar, B ou Start para fechar). As mudanças são salvas sozinhas em `%LOCALAPPDATA%\NHLHitzPro\settings.ini`. O Esc continua abrindo a pergunta de sair.

**Programa de configurações:** rode o **NHL Hitz Pro Settings** (`config.exe`, na pasta de instalação e no menu Iniciar) para ajustar antes de abrir o jogo. A aba **Video**, a primeira, tem as mesmas opções do menu F1. A aba **Controls** remapeia o teclado.

**Trocar as teclas** (aba Controls): escolha uma ação, clique em Change key e aperte a tecla que quiser; cada ação também tem uma tecla alternativa. Save aplica as teclas na próxima abertura do jogo, e Reset to defaults volta ao layout original. Esc, F11 e Alt+Enter são usados pelo próprio jogo e não podem ser atribuídos. As escolhas ficam em `%LOCALAPPDATA%\NHLHitzPro\keymap.ini`.

**Pacote de texturas em alta resolução (opcional):** 1.910 texturas redesenhadas em resolução 4x: o gelo, as placas e o vidro, os logos dos times, camisas e números, rostos e mais. É um complemento separado de 1,27 GB. A página **Hi-res texture pack** do instalador o instala: se o `NHLHitzPro-HiRes-Textures-1.exe` estiver na mesma pasta do instalador, ele usa esse arquivo; senão, oferece baixar (o download é conferido antes de usar). O complemento também roda sozinho depois de o jogo estar instalado. Quando o instalador roda de novo, ele confere o pacote instalado arquivo por arquivo com a versão atual e oferece atualizar ou reparar. O pacote fica em `<install folder>\texture_pack`; imagens colocadas em `%LOCALAPPDATA%\NHLHitzPro\texture_pack` substituem a mesma textura. A aba **Textures** do programa de configurações tem uma chave geral e uma caixa por tipo, e o menu F1 tem uma linha **Hi-res textures** que liga ou desliga o pacote inteiro durante o jogo. As imagens 4x só são usadas com **Render Scale 2x ou mais**; em 1x (nativo), a chave fica apagada e as texturas originais aparecem.

**Problemas conhecidos**

- Na primeira passagem por uma parte nova do jogo, pode dar uma travadinha enquanto os shaders são montados. Ela não volta depois que eles ficam guardados.

- Alguns detalhes visuais diferem do console original: a névoa do túnel de entrada, a aparência do gelo e das camisas e as marcas dos patins no gelo.

- Raramente a tela fica preta depois da introdução; feche o jogo e abra de novo.

- Um replay do time vencedor foi visto uma vez sem desenhar direito (sem travar); está sendo investigado.

- Só a versão americana é aceita. A conferência do disco recusa qualquer outra região ou revisão.
