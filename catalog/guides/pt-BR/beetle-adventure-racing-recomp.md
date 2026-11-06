<!-- source: https://github.com/danielgomesvieira2000/beetle-adventure-racing-recomp/blob/HEAD/README.md | fingerprint: 23a3f7d13fbd0b6e -->

> Detalhes completos e pré-requisitos: **[BUILDING.md](BUILDING.md)**.

```
# 1. Fetch dependencies and build the recompiler (N64Recomp + RSPRecomp).
scripts/setup.sh                  # or scripts/setup.ps1 on Windows PowerShell

# 2. Build the decomp ELF (in WSL), then copy it in.
#    (WSL) cd ~/projects/bar-decomp && source .venv/bin/activate && make recomp
scripts/fetch-elf.sh              # or scripts/fetch-elf.ps1

# 3. Recompile the game to C.
./N64Recomp beetle-adventure-racing-recomp.toml     # emits RecompiledFuncs/*.c
scripts/fix-recompiled.sh         # REQUIRED codegen fixup — re-run after every regeneration

# 4. Configure + build the port (clang-cl on Windows, clang on Linux/macOS).
cmake -S . -B build-cmake -G Ninja -DCMAKE_C_COMPILER=clang-cl -DCMAKE_CXX_COMPILER=clang-cl -DCMAKE_BUILD_TYPE=Release
cmake --build build-cmake -j
```

Em resumo: prepare as dependências e o recompilador, gere o ELF da decompilação (no WSL), recompile o jogo para C (rodando a correção obrigatória depois de cada geração) e compile o port.
