<!-- source: https://github.com/TeamGDB/Yakumo/blob/HEAD/README.md | fingerprint: 240e290edda947d0 -->

Em resumo:

1. Prepare o executável do jogo a partir da imagem do seu disco: compile o `Yakumo` uma vez sem o código recompilado e rode com `--install /path/to/image.iso`. Não precisa de nenhuma ferramenta externa de descriptografia. Depois, ligue a imagem e esse executável ao perfil com `profiles/mhp3rd/scripts/prepare_game.sh`.

2. Configure, gere o código recompilado com `profiles/mhp3rd/scripts/generate.sh` e compile o `Yakumo`.

3. Recompile as partes de código extras (overlays) com `profiles/mhp3rd/scripts/build_overlays.sh` (cerca de 40 minutos na primeira vez; dá para continuar de onde parou).

4. Abra o `out/mhp3rd/bin/Yakumo`.

As instruções completas, com todas as configurações, estão em [profiles/mhp3rd/README.md](https://github.com/TeamGDB/Yakumo/blob/HEAD/profiles/mhp3rd/README.md). Compilar em cada sistema, inclusive no Windows, quanto tempo leva cada etapa e como mexer no código sem recompilar tudo: [docs/BUILDING.md](https://github.com/TeamGDB/Yakumo/blob/HEAD/docs/BUILDING.md).
