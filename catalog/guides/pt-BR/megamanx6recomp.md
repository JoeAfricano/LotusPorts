<!-- source: https://github.com/mstan/MegaManX6Recomp/blob/HEAD/README.md | fingerprint: 04965ba69978781c -->

### Pacote da versão (recomendado)

1. Baixe o `MegaManX6Recomp-v*-windows-x64.zip` na página de versões (Releases) e extraia.

2. Abra o `MegaManX6Recomp.exe`. Uma **janela de launcher** aparece.

3. O OpenBIOS já vem escolhido. Se quiser, escolha na linha da BIOS o seu `SCPH1001.BIN`, obtido legalmente.

4. Indique o **disco** do jogo: escolha a imagem do seu Mega Man X6 (USA, v1.1, SLUS-01395), obtida legalmente. O launcher confere o cabeçalho ISO9660, a região e o número de série.

5. Se quiser, ajuste renderizador, supersampling, aparência da tela, tela larga e controles e clique em **Launch**. As escolhas ficam guardadas.

Formatos de disco aceitos: `.cue` + `.bin` (o preferido; escolha o `.cue`) e `.bin`. **Não converta o disco para um `.iso` "cozido" de 2048 bytes**: isso descarta os setores XA Mode-2 Form-2 de onde o MMX6 lê os vídeos e o áudio. Se o cabeçalho ou o ID do jogo não bater com `SLUS-01395`, o launcher avisa e tenta rodar assim mesmo.

Os caminhos escolhidos ficam ao lado do executável (`disc.cfg` e `settings.toml`). Limpe a linha da BIOS para voltar de uma BIOS de console opcional para o OpenBIOS.

### Ajude o jogo a ficar mais rápido, só jogando

**Por que o jogo ainda não roda em velocidade total em todo lugar?** A maior parte do código do MMX6 é convertida ("recompilada") antes num programa nativo rápido. Mas jogos de PlayStation não guardam todo o código na memória de uma vez: eles leem pedaços extras do disco conforme você chega a áreas novas (os overlays; o MMX6 lê a maior parte da lógica de `ROCK_X6.DAT`). Não dá para converter um pedaço que nunca foi visto, e o único jeito de ver é alguém visitar aquela área. Até lá, o código dessa área roda num modo de compatibilidade mais lento.

**As versões já vêm com um adiantamento.** A pasta `cache`, ao lado do executável, traz o código nativo já convertido das áreas cobertas até agora, e esse trabalho é aproveitado entre aberturas. Enquanto você joga, o programa registra as áreas novas em `overlay_captures.json` e o seu próprio cache cresce sozinho.

**Não publique o `overlay_captures.json`.** Ele tem cópias exatas do código do jogo lidas do seu disco, que é material protegido por direitos autorais. Guarde no seu PC, junto com a imagem do seu disco.
