<!-- source: https://github.com/obaidr1/hercules-recompiled/blob/HEAD/README.md | fingerprint: 185ad450a0af5394 -->

As cinco **versões PAL** (SCES-00891 a -00895) funcionam: a raiz de `port/` monta a alemã, e `port/versions/<id>/` cada uma das outras (`cmake -S versions/<id> -B build-<id>`). As versões americana e japonesa não são aceitas. Testado no Linux (Fedora) e no Windows (Wine); o `port/scripts/build_windows.sh` monta a versão para Windows com MinGW-w64.

Precisa de GCC/G++, CMake ≥ 3.20, Ninja, pkg-config, git, Python 3 com numpy e Pillow e, para o launcher, PyGObject com GTK 4, libadwaita e WebKitGTK 6.0.

```
git clone --recursive https://github.com/obaidr1/hercules-recompiled
cd hercules-recompiled/port
cd psxrecomp && for p in ../patches/psxrecomp/*.patch; do git apply "$p"; done && cd ..
mkdir -p disc   # copy your dump here, Redump names:
                # "Disneys Hercules Action-Spiel (Germany).cue" + ".bin"
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate --config game.toml --project-root . \
    --disc "disc/Disneys Hercules Action-Spiel (Germany).cue"
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
python3 launcher/build_assets.py --sd   # launcher art from your disc
./hercules-launcher
```

Copie a cópia do seu disco para a pasta `disc` com os nomes do Redump antes de gerar o código.

**Os gráficos HD** são feitos a partir do seu disco no seu PC (a primeira abertura do launcher pergunta; cerca de 4 horas num processador de notebook). Eles precisam de um Python com torch e spandrel (por exemplo o que vem com o [chaiNNer](https://chainner.app/)) e de três modelos de ampliação que você mesmo baixa (as licenças deles proíbem redistribuir), colocados em `~/psx-decomp/models/` (ou `$HERC_MODELS`):

- `IllustrationJaNai/4x_IllustrationJaNai_V1_DAT2_190k.pth`

- `Models/HDcube/4xHDCube4Compact.pth`

- `Models/HDcube/4xHDcube-Alpha.pth`

O `decomp/tools/hd_pack_build.py` faz a mesma montagem pela linha de comando.
