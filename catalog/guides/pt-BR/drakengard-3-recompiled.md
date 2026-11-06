<!-- source: https://github.com/soyjxck/dod3-recomp/blob/HEAD/README.md | fingerprint: 793f7d414d895afe -->

Do que você precisa:

- Um PC com Windows 10 ou 11 (64 bits), placa de vídeo com Direct3D 12 e processador com AVX2 (Intel Haswell / AMD Zen ou mais novo), ou um Mac com Apple Silicon no macOS 13 ou mais novo. Cerca de 16 GB livres, mais 10 GB para o pacote de vozes japonesas e cerca de 3 GB para a tradução em espanhol.

- A sua cópia do jogo, versão americana, como:

- o disco (BLUS31197), copiado para uma pasta ou um `.iso` **descriptografado** (o [PS3 Disc Dumper](https://github.com/13xforever/ps3-disc-dumper) grava um; uma imagem feita por uma ferramenta de disco comum continua criptografada e é recusada), ou

- a versão PSN (NPUB31251): o pacote do jogo como o PlayStation 3 baixa (`UP0082-NPUB31251_00-DOD3MAINGAME0000`, 15,6 GB). Não precisa de arquivos de licença.

Outras regiões, inclusive a versão PSN europeia, são recusadas.

- A atualização 1.01, como o pacote que o PlayStation 3 baixa: `UP0082-BLUS31197_00-DOD3PATCH0000000-A0101-V0101-PE.pkg`. É a atualização da versão em disco; a versão PSN também a aceita.

- Opcional: os pacotes de DLC que você tem (`UP0082-NPUB31251_00-...`). Não precisa de arquivos de licença.

- Opcional: mods, como a tradução em espanhol feita por fãs e o DLC Undub (vozes japonesas nos capítulos de DLC), que o launcher instala na página Mods; veja [docs/PLAYING.md](https://github.com/soyjxck/dod3-recomp/blob/HEAD/docs/PLAYING.md#mods).

Windows:

1. Baixe o `dod3-recomp-<version>-win64.zip` na página de [versões (releases)](https://github.com/soyjxck/dod3-recomp/releases) e extraia onde quiser (não dentro de Arquivos de Programas).

2. Abra o `dod3.exe`. O SmartScreen pode avisar na primeira vez, porque o programa não tem assinatura: "Mais informações" e depois "Executar assim mesmo".

3. Na configuração que abre, acrescente os seus arquivos (a imagem ou pasta do disco, ou o pacote PSN do jogo; o pacote de atualização; os pacotes de DLC) com Add Files / Add Folder, em qualquer ordem, e clique em Install. Os arquivos do jogo são copiados para a pasta `game`, ao lado do `dod3.exe` (alguns minutos).

4. Jogue. Dali em diante, o `dod3.exe` abre o launcher: Play, e os mods, DLCs e configurações do jogo.

macOS:

1. Baixe o `dod3-recomp-<version>-macos-arm64.zip` na página de [versões](https://github.com/soyjxck/dod3-recomp/releases), extraia e mova o "Drakengard 3 Recompiled.app" para Aplicativos.

2. O app não é notarizado, então o macOS recusa na primeira vez. No macOS 15 em diante: abra uma vez e permita em Ajustes do Sistema > Privacidade e Segurança ("Abrir Mesmo Assim"). No macOS 13 e 14: clique com Control no app, escolha Abrir e depois Abrir de novo.

3. A configuração abre como no Windows. Os arquivos do jogo vão para `~/Library/Application Support/Drakengard 3 Recompiled`.

Linux:

1. Baixe o `dod3-recomp-<version>-linux-x86_64.tar.gz` na página de [versões](https://github.com/soyjxck/dod3-recomp/releases) e extraia num lugar onde dê para gravar. Ele precisa de um driver Vulkan 1.3 (RADV, ANV do Mesa ou o da NVIDIA), do SDL2 (`sdl2` ou `sdl2-compat`; o SteamOS tem) e do glibc 2.38 ou mais novo.

2. Rode o `dod3` na pasta. A configuração abre como no Windows, e os arquivos do jogo são copiados para a pasta `game` ao lado. O Add Files usa o seletor da área de trabalho (kdialog ou zenity); sem nenhum dos dois, `./dod3 --install <files...>` num terminal faz o mesmo.

3. No Steam Deck: instale pelo Modo Desktop e adicione o `dod3` à Steam como jogo de fora da Steam para jogar no Modo Jogo. O `dod3.log` ao lado diz o que aconteceu se não abrir.

As configurações (resolução, modo de tela, limite de quadros, anti-aliasing, campo de visão, distância de desenho, pular a introdução, teclas do teclado) ficam nas Settings do launcher, no menu de configurações do próprio jogo ou no `dod3.ini`. O [docs/PLAYING.md](https://github.com/soyjxck/dod3-recomp/blob/HEAD/docs/PLAYING.md) traz os controles, os saves, as vozes japonesas e o que fazer se algo der errado.
