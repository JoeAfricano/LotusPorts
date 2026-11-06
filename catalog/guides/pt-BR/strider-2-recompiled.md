<!-- source: https://github.com/strider973/STRIDER-2-Recompiled/blob/HEAD/README.md | fingerprint: 3f2073d57cbf319b -->

Baixe o zip "setup-host" na página de [versões (Releases)](https://github.com/strider973/STRIDER-2-Recompiled/releases) (Windows, Linux ou macOS). Ele traz só o launcher e a base, sem código do jogo, disco ou BIOS, e monta a versão jogável no seu PC a partir do seu próprio disco na primeira abertura.

1. Coloque a cópia legal do seu Strider 2 (`BIN` + `CUE`, SLUS-01163) em algum lugar do computador.

2. Abra o executável.

3. Use **Open .CUE** no launcher e escolha o arquivo `CUE`.

4. Clique em **Play**. A primeira abertura roda o Generate sozinha (disco → C → compilação) com o OpenBIOS que vem junto; nas próximas, vai direto ao jogo.

### Começo rápido (desenvolvimento)

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `strider`.
