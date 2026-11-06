<!-- source: https://github.com/alexbeavs-ps1-ports/alone-in-the-dark-the-new-nightmare-recomp/blob/HEAD/README.md | fingerprint: 0b318345b5497904 -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `Alone-in-the-Dark-The-New-Nightmare-Recompiled`.
