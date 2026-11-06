<!-- source: https://github.com/GTTeancum/Timesplitters/blob/HEAD/README.md | fingerprint: 7f35ab4d2bdbb8b5 -->

### Windows

Extraia o `TimeSplitters-Recomp-<version>-windows.zip` numa pasta onde dê para gravar, coloque a cópia do seu disco ao lado do `timesplitters.exe` e clique duas vezes nele. A primeira abertura desempacota o disco numa pasta `game-data` (cerca de 600 MB, um ou dois minutos, com uma janela de progresso) e o jogo abre; nas próximas, vai direto à tela de título. Detalhes, a organização das pastas e notas sobre mods: [README-PC-SETUP.md](https://github.com/GTTeancum/Timesplitters/blob/HEAD/README-PC-SETUP.md).

Requisitos: Windows 10 ou 11 de 64 bits com driver de vídeo com OpenGL 3.3. O programa não precisa de DLLs ao lado.

### Xbox

A versão de Xbox é uma pasta no disco rígido do Xbox. A versão traz só o programa (`default.xbe` com os ícones do painel); os arquivos do jogo são feitos a partir da cópia do seu disco num PC com Windows pelo `Prepare Xbox Files.exe`, que vem na versão para Windows (a pasta `xbox` dela traz o mesmo programa):

1. Extraia a versão para Windows e coloque a cópia do seu disco na pasta.

2. Clique duas vezes no `Prepare Xbox Files.exe` e escolha onde criar a pasta. Em bem menos de um minuto, ele cria a `TSRecomp` (cerca de 1,1 GB): o programa, o disco como `.iso` com as imagens dos botões do Xbox, os arquivos do jogo e um pacote de texturas preparado para a placa do Xbox.

3. Copie a pasta `TSRecomp` inteira para o disco rígido do Xbox (por FTP, por exemplo como `E:\TSRecomp`) e abra o `default.xbe` dela pelo seu painel.

Os saves ficam em `E:\UDATA\54530001` (aparece como "TimeSplitters" no painel), então trocar a `TSRecomp` por uma versão mais nova os mantém. Passos completos e o conteúdo da pasta: [README-XBOX-SETUP.md](https://github.com/GTTeancum/Timesplitters/blob/HEAD/README-XBOX-SETUP.md).
