<!-- source: https://github.com/james2doyle/ffta-recomp/blob/HEAD/README.md | fingerprint: d88b76191214df39 -->

As versões prontas não trazem o jogo: baixe uma, use a sua própria cópia legal do Final Fantasy Tactics Advance (USA) e escolha na tela de abertura. Uma BIOS feita do zero vem junto e conferida, então não precisa de cópia da BIOS da Nintendo.

Pegue a versão mais recente na [página de versões (releases)](https://github.com/james2doyle/ffta-recomp/releases).

- Versão · Arquivo · Requisitos

- Windows x64 · `FFTARecomp-*-windows-x64.zip` · Windows 10+ x86-64; executável independente, nada extra

Além do OpenGL/X11 do sistema (e do runtime padrão de C++), as versões são independentes; não precisa instalar outras bibliotecas.

### Abrir

- **Windows:** extraia o zip num lugar onde dê para gravar e abra o `FFTARecomp.exe`.

### Escolher o arquivo do jogo

O caminho é o mesmo em todas as versões:

1. Abra o jogo. No computador, aparece a tela de abertura (a primeira vez já abre o seletor de arquivo); no Android, aparece a tela de configuração.

2. Escolha **Select ROM** e indique a sua cópia americana legal. No computador, também dá para arrastar um arquivo `.gba` para a janela.

3. Clique em **Play**. O arquivo é conferido pelo hash, a escolha fica guardada e **Select ROM** troca depois. A BIOS que vem junto não precisa ser escolhida.

### Carregar um save antigo (opcional)

Os saves de bateria são imagens flash brutas de 64 KiB (65.536 bytes).

- **Computador:** copie o seu `.sav` antigo para o arquivo de save do jogo, chamado `ffta_us.sav`, antes de abrir; os caminhos exatos estão abaixo.

O jogo carrega o save importado na próxima abertura; os saves do jogo atualizam o mesmo arquivo.

Onde ficam os arquivos:

- `saves/ffta_us.sav`: save de bateria do cartucho (crie a pasta `saves/` se precisar)

- `rom.cfg`: caminho guardado do arquivo do jogo

- `config.ini`, `keybinds.ini`: configurações do menu e teclas

Opções de imagem e tela larga (`--view-width 448`, `--resize-view`, `--scale`, tela cheia) estão no [docs/README.md](https://github.com/james2doyle/ffta-recomp/blob/HEAD/docs/README.md).
