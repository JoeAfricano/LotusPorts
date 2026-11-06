<!-- source: https://github.com/rac08241988/DemonsCrestSNESRecomp/blob/HEAD/README.md | fingerprint: 21ece937d82079e3 -->

Isto foi confirmado jogando de verdade (e com roteiros automáticos), não só com conferências quadro a quadro:

- **Abertura → jogo**: logo, introdução, tela de título, demonstração e jogo novo funcionam.

- **Fases e chefes**: a caverna de gelo, as montanhas e o castelo rodam com chefes, HUD, ícones dos brasões e formas intactos.

- **Tela larga**: o campo ampliado mostra de verdade as fases, as margens, a posição do HUD e o mapa em Mode 7, como descrito no repositório.

- **Formas**: trocar as formas de gárgula funciona e os sprites de cada forma aparecem.

- **Entrada por roteiro**: o servidor de depuração controla o jogo desde o reinício (`set_controller`, `run_to_frame`), que é como os testes de chefes e portas do port irmão são feitos.

- **Gravação** (novo): dá para gravar a entrada e reproduzir quadro a quadro, e gravar quadros ou um buffer contínuo em disco. Veja [docs/RECORDING.md](https://github.com/rac08241988/DemonsCrestSNESRecomp/blob/HEAD/docs/RECORDING.md).
