<!-- source: https://github.com/Snaity-CORE/pe2-pc-port/blob/HEAD/README.md | fingerprint: 70a970e419195c95 -->

Você precisa do Windows 10 ou 11 e de uma placa de vídeo com Vulkan ou OpenGL 3.3. No Steam Deck e em outros Linux, a mesma versão roda pelo Proton: veja [docs/STEAM_DECK.md](https://github.com/Snaity-CORE/pe2-pc-port/blob/HEAD/docs/STEAM_DECK.md).

Pegue a [versão mais recente](https://github.com/Snaity-CORE/pe2-pc-port/releases/latest), ou a [última compilação do main](https://github.com/Snaity-CORE/pe2-pc-port/releases/download/nightly/pe2-pc-port-nightly-windows-x86.zip) (a versão de teste "nightly", sem testes além do CI), ou compile como explica o [docs/BUILDING.md](https://github.com/Snaity-CORE/pe2-pc-port/blob/HEAD/docs/BUILDING.md); depois, abra o `pe2.exe`. O Setup do launcher pede as imagens dos seus discos, os cartões de memória e um nível de qualidade; depois disso, Play abre o jogo. Settings e Controls mudam tudo depois.

Teclas padrão (Controls remapeia):

- PlayStation · Teclado · Controle

- Direcional · Setas · Direcional (ou analógico esquerdo)

- X / Círculo / Quadrado / Triângulo · Z / X / A / S · A / B / X / Y

- L1 / R1 / L2 / R2 · Q / W / E / R · LB / RB / LT / RT

- Start / Select · Enter / Backspace · Start / Back

- Tecla · Faz

- Alt+Enter · Liga e desliga a tela cheia sem bordas

- P · Pausa e continua

- F9 · Captura de tela (em `screenshots/` da pasta de dados)

- F10 · Troca de disco à mão (normalmente o jogo pede e troca sozinho)

- Esc (ou Start + Select no controle) · Menu de pausa, nas janelas do próprio jogo: continuar, opções (vídeo, áudio, controles e cheats, salvos no pe2.toml), voltar à tela de título, sair do jogo (pergunta antes, a não ser que desligue em Settings)

### Onde ficam os seus arquivos

Configurações (`pe2.toml`), os cartões de memória padrão, as capturas e o registro do jogo ficam na pasta de dados: `%APPDATA%\PE2PC` no Windows (Settings tem um botão que abre). Para uma instalação portátil, coloque um `portable.txt` vazio ao lado do `pe2.exe` e tudo fica na pasta dele. O `pe2.example.toml` documenta cada configuração.
