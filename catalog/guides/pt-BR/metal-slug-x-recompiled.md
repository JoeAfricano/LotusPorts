<!-- source: https://github.com/TechnicallyComputers/Metal-Slug-X-Recomp/blob/HEAD/README.md | fingerprint: 53d5476039f034a0 -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `msx`.
