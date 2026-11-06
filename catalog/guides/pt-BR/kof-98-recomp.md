<!-- source: https://github.com/danilopaulinodasilva/kof98recomp/blob/HEAD/README.md | fingerprint: 39a617ae51ecec59 -->

### Começo rápido (Windows)

> A instalação para Windows veio do jogo de referência do kit e ainda não foi testada com o KOF98. Se ela travar, os comandos passo a passo abaixo funcionam, e uma issue dizendo onde parou ajuda.

1. Baixe este repositório: **Code → Download ZIP** e extraia. Um `git clone` também serve.

2. Coloque os seus **`kof98h.zip`** e **`neogeo.zip`** (conjuntos do MAME, tirados das suas próprias placas) na pasta extraída, ou em `Downloads`.

3. Clique duas vezes no **`Setup.cmd`**.

Ele confere Git, CMake, as ferramentas de compilação C++ do Visual Studio e o SDL2, e pergunta antes de instalar o que faltar (dizendo o quê e o tamanho). Ele encontra e confere os seus arquivos, recompila e monta o jogo (alguns minutos) e deixa um launcher **KOF98** na pasta. Se algo falhar, ele para com uma frase sobre o que fazer e guarda os detalhes no `setup.log`. Rodar de novo continua de onde parou.

4. Clique duas vezes no **KOF98**.

Teclas: jogador 1 **setas** e **Z X C V** (botões A B C D), **1** start; jogador 2 **R F D G** e **A S Q W**, **2** start. Controles também funcionam: o primeiro é o jogador 1, o segundo o jogador 2.

**Esc** pausa e abre o menu: volume, tela cheia, linhas de varredura, 4:3 ou 16:9, as teclas de cada jogador, reiniciar e sair. **Debug** nesse menu vence o round, escolhe a próxima fase (0-8; a 8 é a do Rugal), pula para uma luta da escada (Match 1-7; 0 sai da escada, 7 é o chefe) ou coloca o Rugal como próximo adversário, e salva ou carrega um estado. Um estado só carrega na largura em que foi salvo. As configurações ficam guardadas para a próxima vez. Ele roda como AES, então não há fichas: aperte start na tela de título. Start pausa durante um round. Para tela larga, rode `./kof98.sh --widescreen`.

### Passo a passo

Estes são os comandos que o `Setup.cmd` roda.

**Pré-requisitos:** Git; CMake 3.21+; um compilador C e C++ (Visual Studio 2022 com "Desktop development with C++" no Windows, gcc e g++ ou clang nos outros); SDL2 para a janela (opcional, já que as versões sem janela não precisam de nada). No Linux: `sudo apt install git cmake gcc g++ libsdl2-dev`. No macOS: `xcode-select --install` e depois `brew install cmake sdl2`.

1. **Clone com o submódulo do kit:**

```
   git clone --recurse-submodules https://github.com/danilopaulinodasilva/kof98recomp
   cd kof98recomp
```

2. **Extraia os seus arquivos em `roms/`.** Do `kof98h.zip`: `242-pn1.bin 242-p2.bin 242-s1.bin 242-mg1.bin 242-c1.bin`..`242-c8.bin 242-v1.bin`..`242-v4.bin`. Do `neogeo.zip` (ou de um `kof98h.zip` não mesclado): `neo-po.bin sfix.sfix sm1.sm1 000-lo.lo`. O [docs/game-notes.md](https://github.com/danilopaulinodasilva/kof98recomp/blob/HEAD/docs/game-notes.md) lista os CRC32.

3. **Configure.** No Windows, indique o SDL2 ao CMake: o arquivo de toolchain do vcpkg, ou `-DSDL2_DIR=<SDL2-devel-VC>/cmake`.

```
   cmake -S . -B build -DKOF98_ROM_DIR=roms
```

4. **Compile.** O gerador recompila os seus arquivos em `build/generated` antes:

```
   cmake --build build --config Release
```

Esperado na saída:

```
   [m68krecomp] pointer scan: 38567 seeds
   [m68krecomp] kof98: 1555625 instructions emitted across routines
   [m68krecomp] 39917 routines, 57050 dispatch entries -> .../build/generated
```

5. **Perfil (opcional).** Joga duas sessões automáticas sem janela para achar código que só aparece rodando e recompila com ele:

```
   cmake --build build --config Release --target profile
   cmake --build build --config Release
```

6. **Jogar:**

```
   build/Release/kof98 --rom-path roms          (Linux/macOS: build/kof98)
```

Problemas comuns:

- *`error: cannot open roms/242-pn1.bin`*: os arquivos não estão em `roms/`, ou você está rodando de outra pasta. Passe `--rom-path`.

- Uma tela "FOR USE WITH ORIGINAL SNK BOARDS ONLY": você rodou com a ROM de sistema do MVS (`--bios sp-s2.sp1`). Não use `--bios`.

- "Submodules missing": você clonou sem `--recurse-submodules`. Rode `git submodule update --init --recursive`.

- Sem janela, e "built without SDL2; run with --headless": o CMake não achou o SDL2 (veja o passo 3).

### Uso

```
kof98 --rom-path roms                                      # play
kof98 --rom-path roms --widescreen                         # play in 16:9
kof98 --headless --frames 4400 --input tests/start_fight.txt --record run.mp4
kof98 --headless --verify --frames 19000 --input tests/play_long.txt
kof98 --headless --interp --frames 4400 --screenshot 4350:interp.png
```

O [docs/running.md](https://github.com/danilopaulinodasilva/neogeorecomp/blob/master/docs/running.md) do kit cobre cada opção e o formato dos roteiros de entrada.
