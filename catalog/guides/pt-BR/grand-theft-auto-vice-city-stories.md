<!-- source: https://github.com/gothamor/gta-vcs-recomp/blob/HEAD/README.md | fingerprint: 33fa57181a2e3a44 -->

1. Baixe o `VCS-Recomp-<version>-windows.zip` mais novo na página de [versões (Releases)](https://github.com/gothamor/gta-vcs-recomp/releases) e extraia num lugar curto e local, como `C:\Games\VCS` (não em Arquivos de Programas nem numa pasta sincronizada pelo OneDrive, como Área de Trabalho ou Documentos: a montagem grava milhares de arquivos).

2. Abra o **`VCSLauncher.exe`** nessa pasta. Ele guia você por quatro passos, um de cada vez, e depois mostra um grande botão **Play**:

- **Pasta**: o jogo é montado em `build\`, ao lado do launcher, ou numa pasta que você escolher (outro disco com mais espaço, por exemplo): "Choose another folder" neste passo.

- **Disco**: ele procura o seu `.iso` em Downloads, Área de Trabalho, Documentos e na própria pasta; senão, escolha você (ou arraste para a janela, ou copie o seu disco de um leitor de DVD). Ele confere se é a versão americana.

- **Ferramentas de compilação**: o jogo é compilado no seu PC com as ferramentas C++ gratuitas da Microsoft. Se faltarem, "Install build tools" roda o instalador da Microsoft com as opções certas (cerca de 6 GB; o Windows pede permissão).

- **Montagem**: recompila o jogo a partir do seu disco e compila, com uma barra de progresso: de 20 a 60 minutos na primeira vez, alguns minutos depois de uma atualização. Nada é baixado durante a montagem: a versão traz o Python e as bibliotecas.

- **Play**.

O launcher também faz cópia e restaura os seus saves, cria um atalho na Área de Trabalho, procura versão nova (só quando você pede, ou ao abrir se você ligar isso em Settings) e junta os registros num relatório para relatar erros. O Windows pode avisar que o launcher é de um editor desconhecido (não tem assinatura): Mais informações > Executar assim mesmo. Como funciona: [docs/LAUNCHER.md](https://github.com/gothamor/gta-vcs-recomp/blob/HEAD/docs/LAUNCHER.md). Tudo o que ele faz é um script em `tools/` que você também pode rodar. Se a fonte Pricedown (as letras do GTA, gratuita para uso pessoal, da Typodermic) estiver instalada no seu PC, o launcher a usa no título e no botão Play; a licença dela não permite incluí-la, então ela não vem junto.

### 1. Instale as ferramentas (uma vez)

**Windows**

- **Python 3**: [python.org](https://www.python.org/downloads/). Na instalação, marque "Add python.exe to PATH".

- **Git**: [git-scm.com](https://git-scm.com/download/win) (as opções padrão servem). A primeira montagem o usa para baixar as bibliotecas que o jogo precisa (SDL3, FFmpeg, ...), então também precisa de internet.

Só isso: não há compilador para instalar. A primeira montagem baixa o [llvm-mingw](https://github.com/mstorsjo/llvm-mingw) (clang, lld, libc++ e os cabeçalhos do Windows num pacote só; ~190 MB uma vez, em `build\build-tools`). CMake e Ninja são encontrados onde estiverem (os do Visual Studio, o instalador do CMake, winget, scoop, Chocolatey, pip, CLion...); quando nenhum é novo o bastante (CMake 3.21, Ninja 1.10), a montagem também os baixa (~55 MB).

Com o Visual Studio 2022 ou mais novo instalado ("Desktop development with C++" e "C++ Clang tools for Windows"), `set VCS_TOOLCHAIN=msvc` antes de montar usa o clang-cl dele. O jogo é o mesmo e roda igual.

**Linux**

- Python 3 e Git (a maioria das distribuições tem os dois); depois, deixe a montagem listar o resto: na pasta do código (passo 2),

```
  python3 tools/build.py --check-deps
```

mostra o que falta e o comando que instala (`apt`, `dnf` ou `pacman`): clang 19 ou mais novo com lld, glslang e os pacotes de desenvolvimento do FFmpeg e do SDL3 (CMake e Ninja também, se quiser; sem eles, ou com versões antigas demais, a montagem baixa os próprios). No Ubuntu 24.04 / Linux Mint 22:

```
  sudo apt install clang-20 lld-20 llvm-20 cmake ninja-build pkg-config git glslang-tools \
      libavcodec-dev libavformat-dev libavutil-dev libswscale-dev libswresample-dev \
      libx11-dev libxext-dev libxrandr-dev libxcursor-dev libxi-dev libxfixes-dev libxss-dev \
      libwayland-dev wayland-protocols libxkbcommon-dev libdecor-0-dev \
      libpipewire-0.3-dev libpulse-dev libasound2-dev libudev-dev libdbus-1-dev
```

- Um driver de vídeo com Vulkan 1.3: Mesa (AMD, Intel) ou o da própria NVIDIA. `vulkaninfo --summary` mostra.
