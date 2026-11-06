<!-- source: https://github.com/victor-takai/dragon-valor-recomp/blob/HEAD/README.md | fingerprint: 56799d22343dc89a -->

1. Extraia uma versão e abra o **`DragonValorRecompiled`** (no Windows, o `.exe`; ele não abre janela de console). A partir de uma pasta de compilação, o `DragonValor.bat` faz o mesmo para `build-release`.

2. O launcher abre. Na primeira vez, indique o seu próprio `Dragon Valor (USA) (Disc 1).cue` (veja o [DISC.md](https://github.com/victor-takai/dragon-valor-recomp/blob/HEAD/DISC.md) para as cópias que funcionam); ele confere o número de série, a região e o cabeçalho ISO e lembra o local. A mesma tela tem a escolha de Disco 1 / Disco 2, o controle e o modo dele (Analógico / Direcional), cartões de memória, configurações de vídeo e áudio e mods.

3. **Jogue.** Aperte Start durante o vídeo de introdução para pular.

4. **Esc** (ou **Select+Start** no controle) abre o menu do jogo: pausa, cheats (HP/MP infinitos, uma gema ou pergaminho a cada N inimigos derrotados, Val, atributos, níveis de magia, itens), filtragem de texturas, linhas de varredura, tela cheia, volume, salvar/carregar estado, troca de disco, voltar ao launcher. Setas / direcional movem, Enter / X escolhe, Esc / Círculo volta; o mouse também funciona (passar por cima, clicar, roda, botão direito = voltar). Cada linha fica guardada entre aberturas (as de vídeo em `settings.toml`, os mods em `mods/state.toml`, o volume e o visual do menu em `dragonvalor_settings.toml`, todos ao lado do exe).

Controles de Xbox / PlayStation / Switch funcionam direto; o jogo lê o analógico. Os cartões de memória ficam em `saves/`.

A página **Mods** do launcher lista os recursos incluídos, desligados por padrão menos o PGXP (ligado) e o grupo de Cheats (ligado, com todas as opções desligadas): as melhorias da base (pacotes de texturas HD, PGXP, velocidade do CD, 8 MB de RAM, moldura) e os pacotes deste projeto: **Dragon Valor Cheats** (HP infinito, MP infinito, uma gema ou pergaminho a cada N inimigos; também dá para ligar no jogo, onde Val, atributos, magias e itens também podem ser editados), **Dragon Valor Skip Intro** (aperta Start para você assim que o vídeo de introdução começa) e **Dragon Valor Widescreen** (experimental: as fases aparecem em 16:9 ou mais largas, com cenário de verdade nas laterais; título, menus e vídeos continuam 4:3). Marque um recurso, ajuste as opções e clique em Play. Veja o [docs/DEVELOPMENT.md](https://github.com/victor-takai/dragon-valor-recomp/blob/HEAD/docs/DEVELOPMENT.md#modding-what-the-framework-offers).

- PlayStation · Teclado · PlayStation · Teclado

- Direcional · Setas · L1 / R1 · `Q` / `W`

- X · `X` · L2 / R2 · `E` / `R`

- Círculo · `S` · L3 / R3 · `T` / `Y`

- Quadrado · `Z` · Start · `Enter`

- Triângulo · `A` · Select · `Shift direito`

- Tecla · Ação

- `F` · Mostra o FPS

- `F7` · Escolha do espaço de estado salvo: Esquerda / Direita escolhem, `S` salva, `L` carrega, `F7` / `Esc` fecham

- `F8` · Voltar no tempo (segurar; só com o Rewind ligado no launcher)

- `F6` · Linhas de varredura

- `+` / `-` do teclado numérico · Volume

- `Tab` (segurar) / `F9` · Turbo

- `Alt`+`Enter`, `Ctrl`+`F` · Tela cheia

### Começo rápido (desenvolvimento)

A receita completa (ferramentas no `PATH`, `generate`, `rebuild` das duas pastas de compilação, `--disc`) está no [docs/DEVELOPMENT.md](https://github.com/victor-takai/dragon-valor-recomp/blob/HEAD/docs/DEVELOPMENT.md#build). Em resumo, pelo Git Bash com `C:\retcomm-toolchain\bin` primeiro no `PATH` e `PSXRECOMP_TOOLCHAIN_DIR` definido:

```
git submodule update --init --recursive
bash psxrecomp/tools/ci/build_emitters.sh
python psxrecomp/psxrecomp_cli.py rebuild --config game.toml --project-root . --build-dir build-release --no-pgo
```

A pasta `generated/` está no repositório, então um clone novo não precisa de disco para compilar.

Versões: tag `vX.Y.Z` ou rode o workflow "Release builds". O CI compila o C de `generated/` no Linux, no Windows e no macOS e anexa `DragonValorRecompiled-<version>-<platform>.zip`, o jogo compilado. Versões 0.x saem como versões de teste no GitHub. Localmente: `scripts/package_release.sh build-release windows-x64`.
