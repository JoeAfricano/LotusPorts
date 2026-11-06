<!-- source: https://github.com/cheviman/outrunners-port/blob/HEAD/README.md | fingerprint: 95fa8d9079eb1623 -->

As [versões](https://github.com/cheviman/outrunners-port/releases) trazem um pacote para Windows, Linux e macOS (um .app para Apple Silicon e Intel), com os dois jogos e as ferramentas que fazem os jogos dos outros aparelhos. Deixe o `orunners.zip` (a sua própria cópia) ao lado do jogo, ou arraste para a janela quando ele pedir: na primeira abertura, ele tira os recursos dele (alguns segundos) e os guarda. O `README.txt` do pacote diz o que cada ferramenta precisa.

- **Nintendo Switch**: o OutRunners NSP faz o `OutRunners.nsp` com a sua cópia e as chaves do seu console, para um console com Atmosphere (`ports/switch/README.md`).

- **Portáteis H700 da Anbernic** (RG35XX, RG40XX e parecidos): o OutRunners Anbernic faz a pasta para PORTS (`ports/anbernic/README.md`).

O que qualquer ferramenta faz é só seu, com os dados do jogo dentro: nunca compartilhe.

### Começando (compilar)

Requisitos: Python 3.10 ou mais novo, CMake 3.20 ou mais novo e um compilador C11 / C++17 (Visual Studio Build Tools no Windows, GCC ou Clang no Linux, as ferramentas de linha de comando do Xcode no macOS). O port em janela precisa do SDL2 2.30 ou mais novo: no Windows, o pacote de desenvolvimento para Visual C++ (`SDL2-devel-2.32.10-VC.zip`) extraído em `build/downloads/sdl2/`; nos outros, o do sistema (`libsdl2-dev` no Debian e no Ubuntu); ou, com `-DORUNNERS_STATIC_SDL2=ON`, o SDL2 2.32.10 compilado a partir do código dentro do executável (o CMake busca no GitHub). No Linux e no macOS, os comandos abaixo rodam com `python3`.

O decompilador, as ferramentas de análise e os formatos de recursos são um projeto à parte, o [arcade-decomp](https://github.com/cheviman/arcade-decomp), que este repositório guarda em `tools/core/` como submódulo do git: clone com o submódulo (`git submodule update --init` num clone feito sem ele). A compilação precisa dele para tirar os recursos da sua cópia:

```
git clone --recursive https://github.com/cheviman/outrunners-decomp
```

1. Coloque o seu `orunners.zip` do MAME, ou os arquivos de dentro dele, em `rom/` e confira:

```
   python tools/verify_rom.py
```

2. Compile as ferramentas em `build/bin/` (a compilação tira os recursos da cópia para `assets/`, que o jogo carrega da pasta do repositório ou de ao lado do executável; nenhum executável traz a ROM):

```
   cmake -S . -B build/cmake
```

```
   cmake --build build/cmake --config Release
```

3. Rode o programa original na referência, aqui o modo de demonstração por um minuto, com um quadro das duas telas a cada dez segundos em BMP:

```
   build/bin/s32ref --script tests/controls/attract.txt --video-dump 600 --video-dump 1200 --video-dir build/frames
```

A pasta `tests/controls/` traz os roteiros de controle. Com `--sound-wav FILE`, a placa de som também roda e o que ela toca é gravado num WAV.

4. Rode todos os cenários na referência, traduza o programa para C a partir do que eles executam, compile essa tradução (`orunners_headless`) e compare com a referência, quadro a quadro:

```
   python tools/check_scenarios.py --reference-only
```

```
   python tools/recompile.py --trace build/scenarios/*/executed.txt --calls build/scenarios/*/called.txt
```

```
   cmake -S . -B build/cmake -DORUNNERS_SCAFFOLD=ON
```

```
   cmake --build build/cmake --config Release
```

```
   python tools/check_scenarios.py --translated --native-only
```

A tradução vai para `build/generated/` e fica fora do repositório: é o código do jogo.

5. O programa decompilado fica em `src/`, e a compilação faz o `orunners_game_headless` a partir dele. Compare com a referência:

```
   python tools/check_scenarios.py --native-only
```

Com imagem e som:

```
   python tools/check_scenarios.py --video --sound
```

6. Jogue numa janela:

```
   build/bin/orunners
```

As duas telas lado a lado, os dois assentos do fliperama. Setas: o volante, acelerador e freio; Q e A: subir e descer marcha; espaço: o DJ, `[` e `]`: as músicas; 5 e 6: fichas; 1 e 2: start; 9: serviço; F2: teste; F11: tela cheia; Esc: sair. Um controle por jogador: o analógico esquerdo é o volante, os gatilhos os pedais, os botões de ombro as marchas.

O jogo guarda a EEPROM (as configurações, `orunners.eeprom`) e a parte da RAM da placa de som mantida pela bateria (a contabilidade e os dez recordes das pistas, `orunners.bookkeeping`) ao lado do executável no Windows e, no Linux e no macOS, na pasta do usuário para ele (`~/.local/share/OutRunners`, `~/Library/Application Support/OutRunners`). Ele carrega os recursos da pasta `assets` ao lado do executável (ao lado do .app no macOS), dessa pasta do usuário ou da pasta atual (`--assets FOLDER` escolhe outra). Sem nenhuma, a primeira abertura tira os recursos da sua cópia (o `orunners.zip` ou uma pasta `rom` ao lado do jogo ou na pasta do usuário, ou a cópia arrastada para a janela) para a pasta do usuário. Os pacotes feitos pelo CI trazem só o port em janela.
