<!-- source: https://github.com/danielgomesvieira2000/body-harvest-recomp/blob/HEAD/README.md | fingerprint: d3f3b597b313e0ac -->

Baixe o pacote para Windows ou Linux na página de [versões (Releases)](https://github.com/danielgomesvieira2000/body-harvest-recomp/releases/latest). Windows: extraia e abra o `body-harvest-recomp.exe`. Linux: extraia e rode `./body-harvest-recomp.sh`. Escolha a sua cópia do jogo no launcher.

Para compilar por conta própria:

```
bash tools/setup_linux.sh --install
bash tools/build_linux.sh "/path/to/Body Harvest (USA).z64"
./build-linux/body-harvest-recomp
```

No Windows, é preciso o Visual Studio Build Tools com clang-cl, CMake, Ninja, Python e WSL (a compilação da descompilação e o recompilador rodam lá); os passos estão no [docs/BUILDING.md](https://github.com/danielgomesvieira2000/body-harvest-recomp/blob/HEAD/docs/BUILDING.md), que também lista os controles padrão.
