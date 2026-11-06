<!-- source: https://github.com/sp00nznet/redalert2-recomp/blob/HEAD/README.md | fingerprint: 9b154ba286f435e6 -->

Você precisa da **sua própria cópia**: a versão da Steam ou do EA App (Origin) do Command & Conquer: Red Alert 2 e Yuri's Revenge, a pasta com o `game.exe` e o `gamemd.exe`. Nada do jogo está neste repositório nem é baixado para você; tudo é montado no seu PC a partir da sua cópia.

### O instalador (o mais fácil)

1. Baixe o do seu sistema na **[versão mais recente](https://github.com/sp00nznet/redalert2-recomp/releases)**: `RA2-Recomp-Installer-windows-x64.exe`, `-linux-x64.tar.gz` ou `-macos-arm64.zip`.

2. Abra. Ele encontra o seu jogo na Steam ou no EA App, ou você indica a pasta.

3. Clique em **Build my recomp** e, quando terminar, em **Play**.

A primeira montagem leva de 30 a 60 minutos e cerca de 6 GB; ela instala as ferramentas que precisa (no Windows, o Build Tools do Visual Studio, pedindo permissão uma vez). Os instaladores ainda não têm assinatura: no Windows, o SmartScreen pergunta antes (**Mais informações** e depois **Executar assim mesmo**); no Mac, clique com o botão direito e escolha **Abrir**.

### Os scripts de instalação

1. Baixe este repositório (o botão verde **Code** e depois **Download ZIP**) e extraia num lugar com 6 GB livres.

2. Windows: clique duas vezes em **`Setup.cmd`**. Linux e macOS: `./setup.sh`.

3. Jogue com `Yuri's Revenge (recomp)` ou `Red Alert 2 (recomp)` nessa pasta.

O script pergunta antes de instalar qualquer coisa, encontra o jogo e cataloga, converte e monta os dois jogos; rodar de novo pula as etapas terminadas. Montar à mão, os detalhes de cada sistema, o Wine e a linha de comando estão no docs/building.md do repositório.

No jogo: F10 configurações, F11 tela cheia, F12 escala, F9 mods no menu principal (Linux e macOS). No Mac, segure fn para as teclas F e use Option+Return para tela cheia.
