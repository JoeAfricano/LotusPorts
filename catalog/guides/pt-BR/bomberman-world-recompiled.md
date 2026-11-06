<!-- source: https://github.com/TechnicallyComputers/Bomberman-World-Recomp/blob/HEAD/README.md | fingerprint: 45e04ea1cd2ed62e -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `bombermanworld`.
