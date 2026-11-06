<!-- source: https://github.com/ThiagoLira/trouble-makers-pc-recomp/blob/HEAD/README.md | fingerprint: 8de2544a3f3abbe4 -->

```
git clone --recurse-submodules https://github.com/ThiagoLira/trouble-makers-pc-recomp
cd trouble-makers-pc-recomp

# Supply your own big-endian US 1.1 ROM. It is ignored by git and never copied
# into the resulting executable.
mkdir -p input
cp /path/to/your/mischief-makers-us-1.1.z64 input/troublemakers.us1.z64
echo "e00ab74c3dee79efaafe8e10f2a6b67784d7327ab588d8ef90ad8945427da627  input/troublemakers.us1.z64" | sha256sum -c -

# Runtime fixes not yet upstreamed (message delivery, overlay registration,
# EEPROM semantics):
git -C lib/N64ModernRuntime am "$(pwd)"/patches/N64ModernRuntime/*.patch

# RT64 (renderer), pinned to the fork/commit Zelda64Recomp uses, plus the
# local rendering fixes:
git clone https://github.com/rt64/rt64 lib/rt64
git -C lib/rt64 checkout 23cab603
git -C lib/rt64 submodule update --init --recursive
git -C lib/rt64 apply "$(pwd)"/patches/rt64/*.patch

# Translate the game and audio microcode directly from your ROM, using the
# symbol/section map checked into this repository:
cmake -B tools/N64Recomp/build tools/N64Recomp
cmake --build tools/N64Recomp/build --target N64RecompCLI RSPRecomp -j
tools/N64Recomp/build/N64Recomp troublemakers.us1.toml
tools/N64Recomp/build/RSPRecomp aspMain.us1.rsp.toml
```

Em resumo: clone o projeto, coloque a sua própria cópia americana 1.1 (big-endian) em `input/` (ela nunca é copiada para o executável) e confira o SHA-256, aplique as correções do runtime e do RT64 e traduza o jogo e o microcódigo de áudio a partir do seu arquivo.

A configuração e as compilações incrementais conferem se os patches do RT64 estão aplicados. A pasta `lib/rt64` fica de fora do repositório: atualizar este projeto não atualiza os patches aplicados nela. Se a conferência acusar um patch faltando ou alterado, ajuste esse patch no código local do RT64 antes de recompilar.
