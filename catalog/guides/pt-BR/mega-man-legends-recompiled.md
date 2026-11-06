<!-- source: https://github.com/alexbeavs-ps1-ports/mega-man-legends-recomp/blob/HEAD/README.md | fingerprint: 55d257db3443d8bc -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `Mega-Man-Legends-Recomp`.

### Reabertura corrigida após a configuração

A versão pública `v0.3.0` pode chamar o executável errado depois da primeira compilação no PC. O código da `v0.3.4` usa `Mega_Man_Legends_Recompiled` no CMake, na reabertura da configuração e no empacotador.

A versão `v0.3.4` aceita o formato Redump canônico de dois arquivos. Ela confere a primeira faixa de dados em vez do antigo BIN com o disco inteiro juntado. A identificação antiga, com o disco juntado, continua aceita por compatibilidade. A verificação do código passa; a reabertura automática a partir do ZIP exato ainda é um requisito para a próxima versão.
