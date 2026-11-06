<!-- source: https://github.com/strider973/Street-Fighter-EX2-Plus-Recompiled/blob/HEAD/README.md | fingerprint: c4087ee24292b120 -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue \
  --bios /path/to/SCPH1001.BIN
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `sfex2p`.
