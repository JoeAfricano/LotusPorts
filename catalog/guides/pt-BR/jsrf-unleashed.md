<!-- source: https://github.com/JuMatt/JetSetRadioFuture-Unleashed/blob/HEAD/README.md | fingerprint: 47b05beeb7fb97d9 -->

1. Extraia o arquivo onde quiser e mantenha a pasta `engine` ao lado do **JSRF Unleashed.exe**.

2. Abra o **JSRF Unleashed.exe**. Ele não tem assinatura digital (custa dinheiro todo ano), então na primeira vez o Windows pode dizer que protegeu o computador: clique em **Mais informações** e depois em **Executar assim mesmo**.

3. Na primeira vez, ele pergunta onde guardar configurações, cache, registros e imagens de disco extraídas: na própria pasta (**portátil**: a pasta inteira pode ir para outro computador, um pen drive ou SSD) ou em `%LOCALAPPDATA%\JSRF Unleashed`. Depois, onde está o seu jogo: escolha a pasta que tem o `default.xbe`, ou a pasta com a imagem do seu disco (`.iso`), que ele oferece desempacotar uma vez. Ele lembra a escolha.

Precisa de drivers de vídeo com OpenGL 3.3, que qualquer placa dos últimos dez anos tem; num notebook com duas placas, ele pede a dedicada (NVIDIA ou AMD). O menu **Video** no topo da janela define a resolução, de 1× a 6× o 640×480 do Xbox (2× por padrão; 6×, 3840×2880, é para uma tela 4K; o jogo reabre para aplicar), o anti-aliasing (SMAA, MSAA de 2× a 8×, ou os dois), a imagem (4:3, 16:9 ou 21:9 para tela ultrawide) e a tela cheia, tudo guardado: o jogo abre como ficou, em tela cheia ou em janela (`--fullscreen` e `--windowed` escolhem numa abertura). Os mesmos ficam no **F9**, **F8**, **F10** e **F11** (ou Alt+Enter), que também funcionam em tela cheia, onde o menu fica escondido. **Controls › Camera on the Right Stick** e **Camera on the Right Stick (Tracking)**, ou **F7** (ligado, acompanhando, desligado), escolhem como o analógico direito funciona. **Maps** leva direto a um mapa; **F6** mostra a mesma lista na tela. Os controles funcionam pelo SDL: Xbox, PlayStation, Switch Pro e muitos outros. Os saves ficam na pasta do jogo, em `UDATA`. O `README.txt` incluído lista as opções e onde ficam as configurações e os registros (`last-run.log`). Uma instalação portátil é um arquivo chamado `portable.txt` ao lado do **JSRF Unleashed.exe**: `--portable` cria depois (as configurações vêm junto), e apagar volta ao normal.

### Jogar

**Controle:** qualquer controle que o sistema reconheça (Xbox, DualSense, DualShock 4 e outros, por Bluetooth ou USB).

**Teclado.** As teclas valem pela posição, então `W A S D` num teclado QWERTY é `Z Q S D` num AZERTY:

- Tecla · Faz · Tecla · Faz

- W A S D · andar (analógico esquerdo), menus · Setas · câmera (analógico direito)

- Espaço ou J · A (pular) · K · B

- H · X · L · Y

- E · gatilho R (falar, pichar) · Q · gatilho L

- R · Black · F · White

- Enter · Start · Esc · Back

- C / V · clique do analógico esquerdo / direito

**Controls › Keyboard…** na barra de menu mostra a mesma lista.

**A câmera no analógico direito** (ligada por padrão): o analógico direito (ou as setas) gira a câmera em volta do patinador e a inclina, e ela volta suave para trás dele quando você solta; o gatilho L ainda a coloca atrás na hora, como no jogo original. O patinador vai para onde a câmera olha. O modo **Tracking** só olha em volta: o patinador continua na direção que estava, e para cima no analógico esquerdo continua sendo para a frente dele. Ou desligue para a câmera original. No Windows, **F7** (ou o menu Controls) alterna entre ligado, acompanhando e desligado.

**Modo foto:** segure **Back** (Esc no teclado) por cerca de um segundo enquanto patina, ou escolha **Controls › Photo Mode**. O analógico esquerdo move a câmera, o direito a gira, os gatilhos a abaixam e levantam; **Y** esconde o HUD, **A** o painel de ajuda, e **B** ou **Back** volta ao jogo. O mundo inteiro fica parado enquanto isso (patinadores, pessoas, carros, latas de spray) e continua de onde estava quando você volta.

**Vibração:** o controle vibra como no Xbox. Desligue com **Controls › Vibration**.

**Escolha de mapa:** vá direto a qualquer mapa e patine livre, como depois da história, sem nada para disputar. Escolha no menu **Maps** ou aperte **F6** para uma lista na tela (cima e baixo escolhem, **A** ou Enter vai, **B** ou Esc fecha). Escolha um mapa enquanto patina, ou na tela de título (ele vai quando você aperta Start). Essas são as últimas missões do jogo, então, se o jogo oferecer salvar enquanto você explora, diga não (ou guarde uma cópia do save) para o progresso da história ficar onde estava.

**Menu Video:**

- **Resolution**: a resolução interna, de 1× a 6× (2× por padrão; 6×, 3840×2880, é para uma tela 4K). O jogo reabre para aplicar.

- **Anti-aliasing**: suaviza as bordas serrilhadas, de dois jeitos que podem ser usados juntos. **SMAA** é uma passada rápida sobre cada quadro pronto, liga e desliga na hora. **MSAA** 2×, 4× ou 8× desenha cada borda com essa quantidade de amostras por pixel: mais limpo e mais trabalho para a placa de vídeo. Os dois vêm desligados.

- **Full Screen**: tela cheia.

- **4:3**, **Widescreen (16:9)**, **Ultrawide (21:9)**: o jogo aparece nesse formato com um campo de visão mais largo, sem esticar. Numa tela ultrawide, o 21:9 é o formato da própria tela, então a imagem a ocupa; em outra tela, aparece entre faixas. O HUD mantém o formato e fica nas bordas da tela; menus e diálogos ficam no meio. A tela de título continua 4:3.
