<!-- source: https://github.com/bertibertig/Amped2-Recomp/blob/HEAD/README.md | fingerprint: 89718abba98e3de8 -->

1. Baixe o pacote para o seu sistema na [página de versões (releases)](https://github.com/bertibertig/Amped2-Recomp/releases) e extraia.

2. Coloque os arquivos do **seu próprio** jogo na pasta `game_files/` (veja "Preparing the game files" no repositório).

3. Abra com `./run.sh` (Linux; `./run.sh --settings` para as configurações gráficas) ou, no Windows, clicando duas vezes em `run.bat` ou com `run.ps1` (botão direito → Executar com o PowerShell, ou `powershell -ExecutionPolicy Bypass -File .\run.ps1`); no Windows, a saída do console vai para `run.log`.

Precisa do glibc 2.39 ou mais novo (Ubuntu 24.04+, Fedora 40+, Arch, SteamOS 3.6+), SDL2 e um driver Vulkan (`sudo apt install libsdl2-2.0-0 libvulkan1 mesa-vulkan-drivers`).

**O Windows (10/11) é experimental e ainda não dá para jogar**: só renderização por software (cerca de 4 a 6 fps nos menus, 2 fps na pista), ainda sem configurações gráficas (`--settings` não funciona lá) e testado até agora só no Wine e no Proton, onde os vídeos ficam pretos e a música pode faltar.
