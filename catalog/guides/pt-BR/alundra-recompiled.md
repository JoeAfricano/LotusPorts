<!-- source: https://github.com/alexbeavs-ps1-ports/alundra-recomp/blob/HEAD/README.md | fingerprint: da496c38c8e05a9b -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `Alundra-Recomp`.

### Reabertura corrigida após a configuração

A versão pública `v0.3.0` pode chamar o executável errado depois da primeira compilação no PC. O código da `v0.3.3` usa `Alundra_Recompiled` no CMake, na reabertura da configuração e no empacotador. A verificação do código passa; a reabertura automática a partir do ZIP exato ainda é um requisito para a próxima versão.
