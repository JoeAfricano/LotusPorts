<!-- source: https://github.com/goodshepherd23/AstroBoy-OmegaFactor-Recomp/blob/HEAD/README.md | fingerprint: 12b13ee982285ba1 -->

Instale pelo launcher Quiver, ou extraia a versão onde quiser e abra o **AstroBoy.exe**.

A primeira abertura mostra uma janela de configuração que pede o arquivo do jogo e depois a BIOS; em seguida, ela:

1. baixa ferramentas em versões fixas e conferidas: llvm-mingw 20260616 (clang 22), CMake 3.31.12, Ninja 1.13.2 e SDL 2.32.10;

2. baixa o código do gbarecomp no commit `c1a5fda` (tag v0.4.1), com arm-recomp-core `63dd32d` e toml++ `3017243`;

3. traduz a sua BIOS e o seu cartucho para C++ usando o `kit/game/game.toml` deste kit;

4. compila o jogo (de 15 a 40 minutos, conforme o PC; em PCs com pouca memória livre ele faz menos tarefas ao mesmo tempo).

Depois disso, o AstroBoy.exe abre o jogo direto.

- Comando · O que faz

- `AstroBoy.exe` · Jogar (faz a configuração antes, se precisar)

- `AstroBoy.exe --setup` · Escolher de novo o arquivo do jogo e a BIOS

- `AstroBoy.exe --rebuild` · Montar o jogo do zero
