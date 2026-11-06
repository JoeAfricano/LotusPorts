<!-- source: https://github.com/TechnicallyComputers/Klonoa-Door-to-Phantomile/blob/HEAD/README.md | fingerprint: fa6c89c2b0a35004 -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `kdp`.
