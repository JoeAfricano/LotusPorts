<!-- source: https://github.com/mstan/MegaManX5Recomp/blob/HEAD/README.md | fingerprint: 2a0069adf9ca9828 -->

**Por que o jogo ainda não roda em velocidade total em todo lugar?** A maior parte do código do MMX5 é convertida ("recompilada") antes num programa nativo rápido. Mas jogos de PlayStation não guardam todo o código na memória de uma vez: eles leem pedaços extras do disco conforme você chega a áreas novas (esses pedaços se chamam overlays; o MMX5 lê a maior parte da lógica de `ROCK_X5.DAT` / `ROCK_X5.BIN`). Não dá para converter um pedaço que nunca foi visto, e o único jeito de ver é alguém visitar aquela área. Até lá, o código dessa área roda num modo de compatibilidade mais lento.

**As versões já vêm com um adiantamento.** A pasta `cache`, ao lado do executável, traz o código nativo já convertido das áreas cobertas até agora, e esse trabalho é aproveitado entre aberturas. Enquanto você joga, o programa registra as áreas novas em `overlay_captures.json` e o seu próprio cache cresce sozinho.

**Não publique o `overlay_captures.json`.** Ele tem cópias exatas do código do jogo lidas do seu disco, que é material protegido por direitos autorais. Guarde no seu PC, junto com a imagem do seu disco.
