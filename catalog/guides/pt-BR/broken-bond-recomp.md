<!-- source: https://github.com/jorjbarbosa/broken-bond-recomp/blob/HEAD/README.md | fingerprint: 4c7c1843a3466b33 -->

A partir de uma cópia do repositório, no PowerShell 7 e com as ferramentas C++ do Visual Studio instaladas:

```
git -c core.symlinks=true submodule update --init --recursive
./scripts/extract-game.ps1          # extracts your ISO into game/
./scripts/build-sdk.ps1             # builds and installs the patched SDK
./scripts/build-game.ps1            # generates the C++ code
# Prepare pinned native dependencies as described in the build guide, then:
./scripts/build-game-native.ps1 -PlumeSource out/plume `
  -XenosRecompThirdparty tools/xenosrecomp/thirdparty
./build/native/broken_bond.exe
```

O [guia de compilação](https://github.com/jorjbarbosa/broken-bond-recomp/blob/HEAD/docs/build-and-run.md) explica cada passo, a compilação do renderizador nativo e as formas de abrir o jogo.
