<!-- source: https://github.com/LeithSilver/mgsm/blob/HEAD/README.md | fingerprint: 16db9b5700b9af44 -->

- Sistema · Compilação

- Windows · `mgs_native\build.bat` (Visual Studio 2022, CMake, Ninja, Python com `pip install unicorn==2.1.4`)

- New 3DS · `mgs_native\3ds\build.bat` (devkitPro com `3ds-dev`). Copie o seu `.n-gage` para `sdmc:/3ds/mgs_mobile/`; a primeira abertura instala

- PS Vita · `mgs_native/vita/build.sh` (VitaSDK; rode `mgs_native/vita/build_vitagl.sh` uma vez antes). Precisa do kubridge (HENkaku / Ensō). Instale o VPK e copie o seu `.n-gage` para `ux0:data/mgs_mobile/`; a primeira abertura instala

Abra o `mgs_mobile` e indique na janela de configuração o seu `Metal.Gear.Solid.n-gage` (ou use `--install <file>`).

**Teclado**: setas andam, Z = ação, C = primeira pessoa, Enter = OK, A = menu de pausa, X = equipamento, F1 = configurações.
