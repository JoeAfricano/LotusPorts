<!-- source: https://github.com/m1ndcrap/Sega-Superstars-Tennis-ReMatch/blob/HEAD/README.md | fingerprint: 5f513bef32d6fdae -->

1. Baixe o zip da versão mais recente na aba Releases para o seu sistema e extraia onde quiser.

2. Siga as instruções do README.

Requisitos no Windows:

- Windows 10/11 x64.

- Uma placa de vídeo com Direct3D 12.

- O [Microsoft Visual C++ 2015–2022 Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe), se o Windows avisar que falta o `msvcp140.dll` ou o `vcruntime140.dll`.

Requisitos no Linux:

- Linux 64 bits com glibc 2.38 ou mais novo (Ubuntu 24.04+, Debian 13, Fedora 39+)

- Um driver com Vulkan 1.3

Os shaders do jogo já vêm compilados, então não há compilação de shaders na primeira abertura. Os poucos shaders que o jogo gera durante a partida são compilados na primeira vez que aparecem (os da tela primeiro) e guardados em `cache/shaders` para as próximas vezes.
