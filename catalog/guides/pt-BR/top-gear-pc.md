<!-- source: https://github.com/thebradshaw/topgear-pc/blob/HEAD/README.md | fingerprint: 106a96cb49cc7580 -->

Abra o `topgear-launcher.exe`, escolha o seu arquivo do jogo, ajuste as opções e clique em Start. O launcher funciona com mouse, teclado ou controle (direcional para escolher, A para mudar, Start para jogar), lembra do arquivo e guarda as opções no mesmo `settings.txt` do jogo, então o menu de opções (Tab) no jogo continua mudando tudo.

Também dá para pular o launcher: coloque o `Top Gear (USA).sfc` ao lado do `topgear.exe`, ou arraste o arquivo para o `topgear.exe`.

- Ação · Teclado · Controle

- Direcional · Setas · Direcional ou analógico esquerdo

- B / A / Y / X · Z / X / A / S · Sul / Leste / Oeste / Norte

- L / R · Q / W · Botões de ombro ou gatilhos

- Start / Select · Enter / Shift direito · Start / Back

O primeiro controle é o jogador 1 e o segundo é o jogador 2 (o menu de opções pode trocar). O jogador 2 também pode usar o teclado quando você der teclas a ele no menu de opções. Os controles ficam no `controls.txt` ao lado do `topgear.exe` (gravado com estes padrões na primeira abertura).

- Tecla · Função

- Tab (ou o botão guia do controle) · Menu de opções: todas as opções abaixo, controles dos dois jogadores no teclado e nos controles (aperte a tecla ou o botão novo), trocar qual controle é o jogador 1, cores como numa TV em vez de exatas (também `--tv-colour`), VSync (espera a atualização da tela para a imagem nunca rasgar; ligado por padrão, `--no-vsync` desliga; o jogo roda na mesma velocidade dos dois jeitos), reiniciar e sair. O jogo espera enquanto o menu está aberto

- F11 ou Alt+Enter · Tela cheia

- Esc · Sai da tela cheia

- F2 · Proporção: TV 4:3 ou pixels quadrados 8:7

- F3 · Escala inteira

- F4 · Filtro de tela: pixels nítidos, sharp-bilinear (bordas suaves sem pixels desiguais) ou linhas de varredura (também `--filter sharp-bilinear` ou `--filter scanlines`)

- F5 · Reiniciar

- F6 · Movimento suave: a corrida atualiza a cada quadro em vez de a cada três (desligado por padrão; também `--smooth`)

- Shift+F6 · Taxa de quadros alta: numa tela de 120 Hz ou mais, imagens extras são desenhadas entre os quadros do jogo, para a pista, os carros e os objetos da beira da estrada se moverem suaves na taxa da tela (o jogo continua a 60 quadros por segundo). Liga o movimento suave também (desligado por padrão; também `--high-fps`)

- F8 · Vibração do controle quando o carro bate em algo e quando o nitro dispara (desligado por padrão; também `--rumble`)

- F9 · Visão de tela cheia: em corridas de 1 jogador, a sua visão ocupa a tela inteira em vez de dividir com a do computador. O horizonte fica baixo e a pista plana, como no original. Aperte F9 de novo para uma pista mais alta que ocupa mais a tela, e de novo para desligar (desligado por padrão; também `--full-view`, `--tall-road`). No menu de opções (Tab), dá para colocar o mapa em cima ou embaixo à esquerda, e o velocímetro e o cronômetro em cima ou embaixo à direita (`--map-top`, `--speed-bottom`)

- F10 · Tela larga: as corridas são desenhadas na largura da janela (até 21:9), com pista, cenário e objetos continuando além das bordas originais e o HUD levado para os lados; os menus ganham faixas pretas. Em janela, também troca a janela entre 16:9 e 4:3 (desligado por padrão; também `--wide`)

- F12 · Lado a lado: em corridas de 2 jogadores com tela larga, as duas visões ficam uma ao lado da outra, cada uma com a altura da tela inteira, em vez de uma em cima da outra (desligado por padrão; também `--side-by-side`)

- Shift+F1 · Salvamento automático: os países que você alcançou em cada nível de dificuldade ficam guardados, então não precisa de senhas depois de reabrir (desligado por padrão; também `--auto-save`). Guardado em `progress.txt` na pasta de configurações

- F1 · Carro fantasma: em corridas de 1 jogador, a sua melhor corrida terminada em cada pista é reproduzida como um carro transparente para você disputar. As melhores corridas ficam salvas em `ghosts.bin` na pasta de configurações (`%APPDATA%\topgear-pc\Top Gear`) assim que você faz uma (desligado por padrão; também `--ghost`)

As suas escolhas nessas teclas ficam guardadas entre sessões no `settings.txt` da pasta de configurações (`%APPDATA%\topgear-pc\Top Gear`).
