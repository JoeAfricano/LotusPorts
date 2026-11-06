<!-- source: https://github.com/TekRantGaming/point-blank-recompiled/blob/HEAD/README.md | fingerprint: 9ec38a9797f84369 -->

- **Browse For Disc**: escolha o seu `Point Blank (USA).cue`

- O launcher confere se é o jogo certo (número de série, região e imagem) e mostra **Disc verified**

- **Install To Game Folder** copia o disco para o lado do jogo com uma barra de progresso, para continuar funcionando se o original for movido

- Dois cartões de memória, dispositivos dos jogadores e **Skip launcher on boot**

### Primeiros passos

**Você precisa de:** Windows 10 ou 11 (64 bits), uma placa de vídeo com OpenGL 3.3 e uma imagem do seu próprio disco do **Point Blank (USA)** em `.cue` com o `.bin` (Redump `Point Blank (USA)`, número de série SLUS-00481).

1. Baixe o `PointBlank-PC-Port-*-windows-x64.zip` na [versão mais recente](https://github.com/TekRantGaming/point-blank-recompiled/releases/latest) e extraia onde quiser.

2. Abra o **PointBlank_Recompiled.exe**.

3. No launcher, clique em **Browse For Disc** e escolha o seu `Point Blank (USA).cue`. Quando aparecer **Disc verified**, clique em **Install To Game Folder** (opcional, mas recomendado).

4. Escolha as configurações e clique em **Play**. Na primeira vez, o jogo pede para calibrar a arma: atire no centro do alvo e aperte **A** (botão direito ou Start).

**O download não traz dados do jogo.** As imagens, os sons e as fases vêm todos do seu próprio disco. O SmartScreen do Windows pode avisar sobre um app não reconhecido porque o programa não tem assinatura digital; escolha Mais informações e depois Executar assim mesmo.

As configurações ficam ao lado do jogo em `settings.toml`, e os saves em `saves/`. Rode `PointBlank_Recompiled.exe --no-launcher` para ir direto ao jogo.

Compilar a partir do código (para desenvolvedores):

```
git clone --recursive https://github.com/TekRantGaming/point-blank-recompiled.git
cd point-blank-recompiled
.\Build-PointBlank.ps1 -Cue "C:\path\to\Point Blank (USA).cue"
```

O script carrega o ambiente do Visual Studio e depois:

1. compila o recompilador: `cmake -S psxrecomp/recompiler -B build-recompiler -G Ninja`, alvos `psxrecomp-game psxrecomp-bios`

2. traduz o jogo: `python psxrecomp/psxrecomp_cli.py generate --config game.toml --project-root . --disc <cue>` (grava `generated/`, que nunca vai para o repositório)

3. compila o runtime com `clang-cl` em `build/` (o `cl` do MSVC puro não compila os atômicos C11 do runtime); depois disso, o `build.bat` recompila

As configurações do jogo ficam no [game.toml](https://github.com/TekRantGaming/point-blank-recompiled/blob/HEAD/game.toml); `[controller] guncon_ports = [1, 2]` é o que conecta as armas.
