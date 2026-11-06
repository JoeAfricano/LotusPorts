<!-- source: https://github.com/Supermedo/bloodborne_pc/blob/HEAD/README.md | fingerprint: 400f1c4e95662329 -->

1. Baixe o zip na página de [versões (Releases)](https://github.com/Supermedo/bloodborne_pc/releases/latest) e extraia onde quiser.

2. Abra o `Bloodborne.exe`. Na primeira vez, um guia rápido confere a pasta do jogo (a que tem o `eboot.bin`) e o seu PC, e ajusta a imagem para o seu hardware.

3. Clique em **PLAY**.

**DLC.** A edição Game of the Year (CUSA03173) já inclui The Old Hunters. Com a cópia do jogo base, copie também os complementos: cada um é uma pasta com nome como `SPEXPANSIONDLC03`, só com `sce_sys` dentro, porque os dados vêm na atualização 1.09. Coloque todos numa pasta e escolha-a como **DLC folder** em **Game files** (visão avançada), ou copie para `user\addcont\<title id>\` (a mesma organização do shadPS4 e do bbport).

No jogo, **Insert** (ou **L3+R3** no controle) abre o menu de configurações. O menu **System** do próprio jogo também tem **PC Graphics**, **PC Effects** e **PC Options**: as principais configurações na tela de opções do jogo (`pc_menu=0` no bbport.ini as remove). Teclado: WASD anda, setas giram a câmera, Espaço = X, Shift esquerdo = Círculo, E = Quadrado, V = Triângulo, 1/3 = L1/R1, R/F = L2/R2, Z = L3, Q/C = R3 (travar mira), I/K/J/L = direcional, Enter = Options, Tab / Backspace = metade esquerda / direita do touchpad. Controles de Xbox: Back + direcional esquerda / direita tocam a metade esquerda / direita do touchpad (gestos / itens-chave); Back sozinho, a metade esquerda. O teclado agora funciona junto com um controle conectado. Tudo isso pode ser mudado no `input.ini` (mesma sintaxe do shadPS4: um arquivo de Bloodborne feito para o shadPS4 pode ser copiado por cima), inclusive a câmera pelo mouse: veja [docs/INPUT.md](https://github.com/Supermedo/bloodborne_pc/blob/HEAD/docs/INPUT.md).

### Jogar online (beta)

1. Crie uma conta shadNet em [shadnet.shadps4.net](https://shadnet.shadps4.net/).

2. No launcher, abra **Online play**, marque **Play online** e digite o nome da conta (não o e-mail) e a senha.

3. Clique em **Test connection**: as três linhas devem mostrar "reachable". Depois clique em **PLAY** e escolha online na tela de título. A mensagem de boas-vindas do The Hunter's Dream indica que você está online.

Todos com quem você quiser jogar precisam da versão 1.09 do jogo e do mesmo servidor cooperativo. Um servidor shadNet privado (do projeto [shadp2p](https://github.com/Wozzardman/shadp2p)) também funciona: coloque o endereço dele em **Server address**.

Jogar com quem usa bbhost no grupo cooperativo do The Hunter's Dream fica desligado neste port até funcionar direito com o servidor deles. Não peça ajuda sobre o bbport ao The Hunter's Dream: use o nosso [servidor no Discord](https://discord.gg/WtYT4fXhEN).
