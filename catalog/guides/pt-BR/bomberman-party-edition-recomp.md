<!-- source: https://github.com/TechnicallyComputers/BombermanPartyEditionRecomp/blob/HEAD/README.md | fingerprint: 66bfc9c8e0a84af6 -->

Configure sem a pasta `generated/` do jogo/BIOS (ou com `-DPSXRECOMP_FORCE_SETUP_HOST=ON`) para compilar um **setup host** com a tela de Generate & rebuild e sem BIOS embutida. Na primeira abertura, a geração cria no seu PC o OpenBIOS (e, se quiser, o SCPH1001) e o C do jogo, e a recompilação junta tudo.

Os zips das versões usam o `scripts/package_setup_release.sh` e embutem o `psxrecomp-game` e o `psxrecomp-bios` (mais a linha de comando) dentro de `psxrecomp/`, mas não uma pasta `toolchain/` portátil. O Retro / o assistente baixam o `cmake-clang-v1` do `retcomm-toolchains`, ou aceitam um zip offline / `RETCOMM_TOOLCHAIN_DIR`. O CI não depende de cópia de BIOS nem de arquivos particulares.

Quando faltam os códigos (ou com `BPE_FORCE_SETUP=1`), o launcher oferece **Generate & rebuild** pelo `psxrecomp/psxrecomp_cli.py` (veja `psxrecomp/docs/LOCAL_CODEGEN_SDK.md`). O BPE vem com `[pgo] enabled = false`: a versão Release comum basta, e a configuração não faz treino de PGO.

```
# Headless generate + rebuild:
python3 psxrecomp/psxrecomp_cli.py generate --config game.toml \
  --disc "/path/to/Bomberman - Party Edition (USA).bin"
python3 psxrecomp/psxrecomp_cli.py rebuild --config game.toml \
  --build-dir build-release --exe-basename Bomberman_Party_Edition_Recompiled
```

PGO local opcional (não usado na configuração do BPE): defina `[pgo] enabled = true` ou rode `scripts/pgo_bpe_intro.sh`.
