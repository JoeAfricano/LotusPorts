<!-- source: https://github.com/ZeldaWWHDRecomp/ZeldaWWHDRecomp/blob/HEAD/README.md | fingerprint: 181a942765dbf90f -->

As versões são portáteis: extraia, abra o **Wind Waker HD** e escolha a sua cópia. As versões trazem só o runtime, as ferramentas e a instalação do projeto: **nenhum arquivo do jogo, nenhum código do jogo e nenhuma chave**. O código do jogo só passa a existir quando é montado a partir da sua própria cópia, então a primeira abertura prepara o jogo uma vez, no seu computador (cerca de dois minutos); as próximas abrem o jogo direto.

1. Baixe o zip do seu sistema na página de [versões (Releases)](https://github.com/ZeldaWWHDRecomp/ZeldaWWHDRecomp/releases) e extraia onde quiser (uma pasta de jogos, um disco externo):

- **Windows**: x86-64 (`windows-x86_64`, Windows 10 versão 1903 ou mais novo, ou Windows 11) ou arm64 (`windows-arm64`, Windows 11 em ARM, por exemplo Snapdragon X), com placa de vídeo com drivers Vulkan 1.3 (ou Vulkan 1.1 / 1.2 com `VK_KHR_dynamic_rendering`). O zip arm64 roda nativo, e só em ARM64.

2. Abra o **Wind Waker HD** (`Wind Waker HD.exe` no Windows). A primeira abertura pede:

- a **imagem do seu disco** (`.wux` ou `.wud`), um **arquivo do Cemu** (`.wua`) ou uma **pasta do jogo já extraída** (com `code`, `content` e `meta`, por exemplo do dumpling ou do Cemu). Uma pasta extraída é usada onde está, nada é copiado; uma imagem de disco ou arquivo do Cemu é extraído na pasta da versão (cerca de 1,7 GB);

- para uma imagem de disco, a **chave do disco** (um arquivo `.key` com o nome da imagem ao lado é usado sozinho) e a **chave comum do Wii U**, tiradas do seu próprio console (um arquivo de chave, ou cole os 32 dígitos hexadecimais no campo escondido; um `common.key` ao lado da imagem ou na pasta da versão é usado sozinho). As chaves são conferidas antes de extrair, nunca guardadas e não aparecem em nenhum registro. Um arquivo do Cemu ou uma pasta extraída não precisam de chaves.

- Um arquivo do Cemu muitas vezes traz o jogo, a atualização e DLCs juntos. A instalação usa o próprio jogo, título 00050000-10143500 (EUA) ou 00050000-10143600 (Europa), versão 0; uma atualização no arquivo não é usada, porque o port foi feito para o código da versão 0.

Depois, ele prepara o jogo (extrai, traduz o código para C, compila com um compilador de versão fixa) e oferece trazer um save: uma pasta `cking.sav` do Wind Waker HD (Cemu, Wii U), um `.gci` de GameCube (convertido para o HD), ou os saves e configurações de uma instalação anterior (copiados, nunca movidos). As versões americana e europeia são aceitas, na versão 0 (o lançamento em disco ou eShop, sem a atualização): antes de traduzir, a instalação confere o código do jogo (`code/cking.rpx`) com o SHA-256 de cada uma e explica o que usar quando não bate com nenhuma. O jogo europeu roda em inglês, francês, alemão, italiano ou espanhol, escolhido na aba Language das configurações.

3. Pronto: abra o Wind Waker HD para jogar. Para reparar, atualizar ou trocar o jogo, segure **Shift** ao abrir (Windows) ou abra com `--setup` (Linux).

Na primeira abertura no **Windows**: a versão não tem assinatura digital, então o SmartScreen pode dizer "O Windows protegeu o computador": **Mais informações › Executar assim mesmo**. O Python vem junto (`tools/python`). A primeira abertura baixa o compilador (llvm-mingw, 190 MB) para a pasta da versão, conferido pelo SHA-256, sem administrador; no fim, você pode remover o compilador (ele só é preciso para reparar e é baixado de novo então).

**Tudo fica na pasta da versão** (em `data/`): o jogo montado, os arquivos extraídos, os saves (`data/save`), configurações, controles, estados salvos e caches de shaders (`data/user`), registros de falhas (`data/captures`), o registro da instalação e o compilador baixado. Nada é gravado nas pastas do usuário, a não ser que você marque "add a shortcut" no fim. Para remover tudo, apague a pasta. Para usar uma versão mais nova: extraia ao lado da antiga, abra, escolha o seu jogo (a `data/game` da pasta antiga pode ser usada no lugar) e copie os saves e configurações da pasta antiga.

A instalação também roda num terminal (alternativa): `tools/Setup in a console window.bat` (Windows), `tools/setup-in-terminal.sh` (Linux). Como funciona: [tools/installer/README.md](https://github.com/ZeldaWWHDRecomp/ZeldaWWHDRecomp/blob/HEAD/tools/installer/README.md).

### Jogar

Duas janelas abrem: a TV e a tela do GamePad (mapa, itens, menus). Clique e arraste na janela do GamePad para usar a tela de toque.

**Apresentação no Vulkan** (painel de configurações › Graphics › Presentation): Vsync (suave, o padrão), Low latency (o quadro mais novo a cada atualização, sem rasgar) ou Off (pode rasgar). Só os modos que o driver oferece podem ser escolhidos; a mudança vale na hora e fica salva.

**Painel de configurações:** aperte **F1** na janela do jogo, ou segure Select / Menos por meio segundo, ou aperte Home no controle, para um menu sobre a imagem: estados salvos, gráficos, tela (tela cheia, escala da imagem, a tela do GamePad), áudio (volume geral e caixas de som), mods de jogabilidade e cheats, controles (o mesmo desenho do controle: escolha um botão e aperte a tecla ou o botão a usar) e o idioma do console (só os idiomas que o seu jogo tem; o americano tem inglês, francês e espanhol, o europeu inglês, francês, alemão, italiano e espanhol). Mouse, teclado (setas, Enter, Esc) e controle (direcional / analógico, A, B; L / R trocam de aba) funcionam. O jogo continua rodando, mas não recebe comandos com o painel aberto; Esc, F1 ou B fecha. Shift+F1 salva o estado no espaço 1; **F6 carrega o espaço 1**.

**Digitar texto (a tela do nome):** quando o jogo pede um texto (o seu nome ao começar um arquivo novo), aparece uma janela de texto embaixo do campo de nome do jogo, com um contador (até 8 caracteres) e um teclado na tela. Só aparecem os caracteres que a fonte do jogo consegue desenhar. Digite no teclado (qualquer layout, acentos), **Enter** = OK, **Esc** = Cancelar. Com controle: direcional ou analógico escolhem a tecla, **A** digita, **B** apaga, **X** coloca espaço, **Y** é o Shift, **L / R** trocam entre letras, letras acentuadas e símbolos, **ZL / ZR** movem o cursor, **Start** confirma.

### Controles

Layout padrão do teclado:

- Teclado · Wii U GamePad

- W A S D · analógico esquerdo (andar)

- setas · analógico direito (câmera)

- K ou Espaço · A

- J · B

- L · X

- I · Y

- Q / E · L / R

- Shift esquerdo · ZL (mirar)

- C · ZR

- Enter / Tab · + / −

- H · Home

- 1 2 3 4 · direcional cima / baixo / esquerda / direita

- X / V · clique do analógico esquerdo / direito

**Input › Controls…** remapeia tudo num desenho do controle: clique num botão, direção ou clique de analógico e aperte uma tecla ou botão do controle (cada comando tem uma tecla, uma alternativa e um botão do controle); Esc cancela, botão direito limpa. Os botões apertados acendem e os analógicos mostram a inclinação, para testar; uma tecla usada duas vezes ganha um aviso. As mudanças valem na hora. A zona morta dos analógicos e a opção de inverter a câmera ficam embaixo, com **Reset to Defaults…**. O mapeamento fica salvo no `controls.json`; apagá-lo volta ao padrão.

**Settings → Shortcuts** remapeia os atalhos: configurações, tela cheia, captura de tela (**F10**), salvar e carregar os espaços 1–5 e os atalhos gráficos. Cada ação tem uma tecla com Shift/Ctrl/Alt, uma alternativa e uma combinação no controle (segure um botão e aperte outro, por exemplo **Select + Direcional Cima**). **Apertar Home sempre abre as configurações**, e Esc sempre as fecha.

**Gráficos** (alternáveis durante o jogo; o título da janela da TV mostra o que está ativo e os quadros por segundo): 60 fps por interpolação de quadros (**6**; 120 e 240 fps no menu), resolução interna 1x / 1,5x / 2x / 3x (**R** alterna; o jogo renderiza em 1280x720, e 2x em 2560x1440), suavização de bordas (FXAA, **8**), modo de oclusão de ambiente (**O** alterna), profundidade de oclusão em tamanho cheio (**M**), filtragem anisotrópica 16x (**N**), proporção da tela, renderizador (Vulkan), **captura de tela** (**F10**: a imagem da TV em PNG na resolução interna) e uma captura de quadro para depuração (**P**, gravada em `captures/`; ela contém imagens do jogo, então guarde só para você). As escolhas ficam guardadas entre aberturas.

A aba **Graphics** também tem o **3D estereoscópico** experimental (só Vulkan): Half SBS, Full SBS e cima-e-baixo (TAB), com controles de profundidade, convergência e troca de olhos. Ele começa desligado. **30 FPS é o recomendado para o 3D por enquanto**: o ritmo dos quadros é irregular em valores mais altos.

**Gameplay FOV**, na mesma aba, ajusta o campo de visão da câmera de 75% a 250% (100% é a visão original). Cenas, diálogos, menus e transições mantêm o campo de visão original. A visão em primeira pessoa, a mira de itens e a luneta ficam mais abertas pelo mesmo fator, então o zoom delas fica mais fraco acima de 100%.

**Saída HDR** (Vulkan, Windows e Linux) usa scRGB quando o HDR do Windows está ligado e a tela suporta. O brilho do branco, o pico de brilho, a exposição, a expansão dos realces e o brilho do menu de configurações são ajustáveis. Isso expande a imagem SDR do jogo para HDR; não recupera iluminação já cortada. O HDR começa desligado e precisa ser ligado. Calibre para a sua tela. As capturas PNG continuam SDR.

O menu **Gameplay** tem mudanças opcionais na jogabilidade, todas desligadas por padrão: escalar qualquer parede (com uma roda de energia; B ou A solta), câmera direta no analógico direito (sem suavização, velocidade ajustável), câmera pelo mouse (clique na imagem para capturar o ponteiro, Esc solta), primeira pessoa na roda do mouse, portas rápidas e trocas de cena rápidas. Também tem cheats: todos os itens, a Master Sword e o Mirror Shield com poder total, 20 corações / magia dupla / 5000 rupias, e vida, magia ou munição infinitas. Cheats de história (todas as músicas, fragmentos da Triforce, mapa/bússola/chave do chefe, uma chave pequena) podem mudar ou quebrar eventos da história, então use um arquivo de save reserva. Os cheats editam o save em uso; salve no jogo para mantê-los.

O menu **Save States** (e a aba Saves do painel) salva num de 5 espaços e carrega de volta (**Shift+F1–F5** salvam, **F2–F5** carregam; F6 carrega o espaço 1); cada espaço mostra a hora e a área. Por padrão, um espaço guarda um **estado portátil** (`slotN.wwstate`, poucos KB): o seu progresso e o lugar do Link, sem dados do jogo, seguro para anexar em relatos de erro. Ao carregar, o progresso vai para o jogo e o Link entra na fase salva no ponto salvo (no barco, se estava nele). Não é uma cópia exata: inimigos, cenas e outros personagens começam do zero. Um estado portátil só pode ser salvo enquanto você controla o Link (não em cenas, diálogos ou trocas de fase). Os **estados completos** (desligados por padrão, para depuração) salvam o jogo inteiro em execução (`slotN.bin`, cerca de 270 MB) e o restauram exatamente; eles contêm código e dados do jogo, **nunca compartilhe**.
