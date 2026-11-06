<!-- source: https://github.com/kirby1237/PaRappaTheRapperRecomp/blob/HEAD/README.md | fingerprint: 10950cbe421d749c -->

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
git add generated && git commit -m "Regenerate game C"
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Versões: tag `vX.Y.Z` (ou rode o workflow "Release builds"). O CI compila o C de `generated/` no Linux e no Windows e anexa `prtr-<version>-<platform>.zip`, o jogo compilado. No seu PC: `scripts/package_release.sh build-release linux-x64`.
