<!-- source: https://github.com/kitaaaaaa/Oratan-recomp/blob/HEAD/README.md | fingerprint: feb1467810026b7c -->

Você precisa da sua própria cópia do jogo de Xbox Live Arcade. Nenhum arquivo do jogo vem aqui.

1. **Baixe** o `Oratan-recomp-…-win64.zip` mais recente na página de [versões (Releases)](https://github.com/kitaaaaaa/Oratan-recomp/releases) e extraia onde quiser.

2. **Acrescente o seu jogo** à pasta `assets` dentro dele (veja abaixo).

3. **Clique duas vezes no `oratan.exe`.**

Precisa do Windows 10 ou 11 (64 bits) com uma placa de vídeo DirectX 12.

### Acrescentando o seu jogo

Copie o arquivo de pacote Xbox Live Arcade do jogo para `assets`. É um arquivo só, chamado `2A944528D84678B7C9F0270A564B8E653520EF72`, que fica no Xbox 360 em `Content\0000000000000000\58410985\000D0000\` (ou na pasta `content` do Xenia). Ele é desempacotado sozinho na primeira abertura.

Arquivos já extraídos (`default.xex` e a pasta `media`) também funcionam.

### Controles

- Controles de Xbox funcionam como no console.

- **F1** abre as opções (resolução, anti-aliasing, filtragem anisotrópica, VSync, tela cheia, versus local).

- **Alt+Enter** alterna entre tela cheia e janela. Para levar o jogo a outro monitor: Alt+Enter, arraste a janela, Alt+Enter de novo. A escolha fica guardada.

Um registro é gravado na pasta `logs`, ao lado do `oratan.exe`; anexe-o em relatos de erro.

### Gráficos

**F1 → Resolution** oferece o 1280×720 original e **1920×1080**, **2560×1440** e **3840×2160** nativos. O jogo original desenha a cena 3D numa imagem de 1024×1024 e a estica para a tela 720p. Os modos nativos desenham a cena e o HUD exatamente no tamanho escolhido, então a imagem fica nítida em vez de ampliada. As sombras continuam na resolução original. Mudar a resolução precisa reabrir; a janela de opções oferece um botão **Restart now**.

Com uma resolução nativa:

- **Anti-aliasing:** desligado, MSAA 2× ou 4× (a opção do próprio jogo). Vale para a cena 3D e precisa reabrir.

- **Filtragem anisotrópica:** de desligada a 16×, vale na hora. Mantém nítidos o chão e outras superfícies vistas em ângulo.

Todos os modos nativos rodam a 60 fps numa placa DirectX 12 intermediária (testado numa RTX 2080, inclusive em 4K).

### Versus local para dois jogadores (experimental)

A versão Xbox Live Arcade só tinha versus online. Com dois controles conectados:

1. Aperte **F1** e marque **Player 2 controls the opponent** e **Split screen during battles**. **Split display** escolhe o layout: escalonado (jogador 1 em cima à esquerda, jogador 2 embaixo à direita), lado a lado ou um monitor por jogador (a janela ocupa dois monitores lado a lado, como os gabinetes gêmeos do fliperama). **Swap sides** coloca o jogador 1 à direita.

2. Comece qualquer partida (Training ou Arcade) com o controle 1. O controle 2 assume o robô adversário, e cada jogador ganha a própria visão.

O controle 2 também assume os chefes do Arcade quando aparecem, como Ajim, Bradtos e Tangram. Desmarcar **Player 2 controls the opponent** no meio da partida devolve o adversário ao computador.

### Versus online (experimental)

Dois jogadores em PCs diferentes podem jogar versus pela internet ou numa rede local. O jogo precisa começar em sincronia nos dois PCs, então começar ou sair de uma sessão reabre o jogo.

1. **O jogador 1 cria:** aperte **F1**, abra **Netplay**, escolha **Host** e clique em **Host (restarts the game)**. O jogo reabre e espera o jogador 2. O painel lista os endereços deste PC para passar ao jogador 2.

2. **O jogador 2 entra:** aperte **F1**, abra **Netplay**, escolha **Join**, digite o endereço e a porta do anfitrião (por exemplo `192.168.1.20:7000`) e clique em **Join (restarts the game)**.

3. Conectados, os dois jogos começam juntos a partir da tela de título. O jogador 2 controla o adversário, e cada um vê o próprio robô.

Os dois PCs precisam dos mesmos arquivos do jogo, saves e configurações. Pela internet, o anfitrião redireciona a porta UDP (7000 por padrão) para o PC dele, ou os dois usam uma VPN como ZeroTier ou Tailscale. **Input delay** (definido pelo anfitrião) esconde o atraso da rede: 3 serve para rede local, 4-5 para uma conexão de uns 40 ms. **Leave netplay** reabre o jogo offline. Se o outro jogador desconectar, o jogo segue offline depois de 10 segundos.

### System Link: versus na rede local (experimental)

O modo System Link do próprio jogo, entre PCs na mesma rede:

1. Em cada PC, libere o jogo no firewall do Windows uma vez: clique duas vezes em **allow-firewall.bat** (ele pede administrador e só abre as portas UDP 1000-1002 e 3076 para a rede local).

2. Aperte **F1**, marque **System Link on the local network**, digite um nome de jogador e clique em **Restart now**.

3. Em cada PC: **Multiplayer > System Link**, escolha um robô, **OK**. O primeiro jogador espera um adversário; o próximo entra. Todos os PCs precisam da mesma versão do port.

Se os PCs não se encontrarem (Wi-Fi que bloqueia transmissões, VPNs), indique os outros PCs no `Virtual-On OT.toml`, por exemplo `lan_peers = "192.168.1.20"`.

**Modos Xbox LIVE na rede local** (também experimental): marque em F1 (e reabra), e **Multiplayer > Player Match** funciona entre os PCs da rede: **Custom Match** abre um lobby, **Quick Match** em outro PC o encontra. Não há listas de amigos nem convites.

### Fases escondidas e Bradtos (Training)

O jogo tem fases e um chefe que os menus nunca oferecem. No Training:

- **Escolha de fase:** aperte **LB** ou **RB** (ou Page Up / Page Down) para ir à segunda página. As duas vagas depois de RANDOM viram **DISTORTED SHRINE** (a fase do chefe intermediário) e **TANGRAM** (a fase final). Aperte de novo para voltar.

- **Escolha de personagem:** com o cursor em **AJIM**, aperte **LB** ou **RB** para trocar essa vaga por **BRADTOS** (e de volta). Como adversário do computador, o BRADTOS faz as cenas de abertura e derrota do Arcade.

Outras fases e robôs não listados travam ou funcionam mal e ficaram de fora.
