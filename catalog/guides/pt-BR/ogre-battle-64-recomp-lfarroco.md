<!-- source: https://github.com/lfarroco/ogre-battle-64-recomp/blob/HEAD/README.md | fingerprint: 4f0fb78cf03cd39a -->

Veja **Reproduce** no [PLAN.md](https://github.com/lfarroco/ogre-battle-64-recomp/blob/HEAD/PLAN.md). Resumo:

```
# tools (macOS)
brew install mips-linux-gnu-binutils cmake
# Xcode 26+: `metal` is a separate download; skip if `xcrun --find metal` works
xcodebuild -downloadComponent MetalToolchain
python3 -m venv tools/venv && tools/venv/bin/pip install 'splat64[mips]'
git clone --recurse-submodules https://github.com/N64Recomp/N64Recomp.git tools/N64Recomp
git -C tools/N64Recomp apply ../../patches/n64recomp-ob64.patch
cmake -S tools/N64Recomp -B tools/N64Recomp/build -DCMAKE_BUILD_TYPE=Release
cmake --build tools/N64Recomp/build --target N64RecompCLI -j4

# ROM: put your .z64 dump in assets/, then regenerate the recompiled code
# (see docs/guides/app-build.md -> "Regenerating the recompiled code")
make regenerate
```

Coloque a cópia `.z64` do seu jogo em `assets/` e gere o código recompilado com `make regenerate`.

O arquivo precisa ser a cópia americana Rev A (40 MB, `.n64` com bytes trocados de 16 bits ou já convertido para `.z64`). O `tools/convert_rom.py` converte `.n64` → `.z64`.

**Um clone novo não compila o app antes de rodar `make regenerate` uma vez**: o C recompilado (`RecompiledFuncs/`, `Bank*Funcs/`, `RspFuncs/`, `app/src/bank_funcs.inc`) é gerado a partir do seu próprio arquivo e fica de fora do repositório de propósito. O `make regenerate` roda o splat, a ligação MIPS, as 34 unidades de banco, a recompilação principal e o microcódigo RSP na única ordem que funciona.
