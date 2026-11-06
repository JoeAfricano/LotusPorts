<!-- source: https://github.com/8bitninja64/TenchuZ-PC/blob/HEAD/README.md | fingerprint: a0c85298a281e0c5 -->

1. Baixe o `TenchuZ-windows-x64.zip` na página de [versões (Releases)](https://github.com/8bitninja64/TenchuZ-PC/releases) e extraia num lugar onde dê para gravar (não em Arquivos de Programas).

2. Instale o [Python 3.11 ou mais novo](https://www.python.org/downloads/) (marque "Add python.exe to PATH") e o [Visual Studio 2022 Build Tools](https://visualstudio.microsoft.com/visual-cpp-build-tools/) com "Desktop development with C++" e "C++ Clang tools for Windows". Eles são usados uma vez: na primeira abertura, o jogo se compila a partir do seu disco.

3. Abra o `TenchuZ.exe` e escolha a imagem do seu disco `Tenchu Z (USA, Europe)` (`.iso`) quando pedir. Confirme a compilação: uma janela de console mostra o progresso (cerca de 2 minutos num PC de 8 núcleos, mais com menos núcleos) e depois o jogo abre. Nas próximas vezes, abre na hora.

A imagem do disco é lida no lugar (nada é extraído) e lembrada no `TenchuZ.ini`. O código do jogo compilado (`tenchu_game.dll`), os saves e as configurações de perfil ficam em `%APPDATA%\TenchuZRecomp`. O jogo procura versão nova ao abrir (uma consulta ao GitHub; desligue em F1 > Update): **F1 > Update** baixa, confere o SHA-256 com o do GitHub, instala no lugar e reabre. Configurações, saves e o código compilado ficam; o código do jogo é recompilado sozinho quando a versão nova precisa. Extrair uma versão nova por cima da antiga também funciona. A versão não traz código nem dados do jogo; nunca compartilhe o `tenchu_game.dll` montado a partir do seu disco.

Windows 10/11 64 bits com placa de vídeo Direct3D 11. O runtime do Visual C++ vem junto.

### Jogar

As configurações ficam no `TenchuZ.ini` ao lado do executável, gravado com comentários na primeira abertura: a imagem do disco, tela cheia, tamanho da janela, vsync, `resolution_scale` (1 = 720p, 1.5 = 1080p, 2 = 1440p, 3 = 4K), `frame_rate_limit`, sensibilidade do mouse e cada tecla.

O `frame_rate_limit` começa em 60; 30 volta ao ritmo do console, e valores maiores (120, 144, 240) ou 0 (sem limite) vão além. A velocidade do jogo não muda com os quadros: o jogo mede o tempo real. Com vsync ligado e uma taxa de atualização múltipla do limite (60 numa tela de 60/120/240 Hz), os quadros acompanham a tela. As missões hoje chegam a uns 75-80 fps sem limite num notebook com RTX 3070; menus e o esconderijo rodam bem acima.

- **Esc** (ou **Back + Start** no controle): menu de configurações ("menu F1"), que pausa o jogo e o som: imagem (limite de quadros, resolução, contador de FPS, HDR), gráficos (oclusão de ambiente, nitidez, anti-aliasing), volume, controles (remapear controle e teclado, mouse), liberar todas as missões no seu save, cheats e atualizações. O controle também funciona: LB/RB trocam de seção, A escolhe, B volta.

- **Cheats** (F1 > Cheats; todos desligados a cada abertura): vida infinita, mortes com um golpe, invisível para inimigos, ninja silencioso, itens infinitos, curar veneno, defesa inquebrável, super pulo, corrida rápida, gravidade baixa, gancho rápido e longo, aterrissagem macia, compras grátis, posto Grand Master, registro de furtividade perfeito; e ações: dinheiro máximo, liberar todas as habilidades, encher o estoque de itens, liberar todas as roupas.

- **Alt+Enter** ou **F11**: alterna a tela cheia sem bordas.

- Controle: qualquer um que o SDL3 reconheça, com os botões pela posição de um controle de Xbox; o primeiro conectado é o jogador 1. Os layouts vêm do `gamecontrollerdb.txt` ao lado do executável; acrescente as suas linhas no `gamecontrollerdb_user.txt`. Teclado e mouse funcionam como controle 1: WASD anda (Alt caminha), clique na janela para o mouse controlar a câmera, Esc solta (e abre o menu). Botões padrão: Espaço = A, C = B, botão esquerdo = X, botão direito = Y, Q/E = LB/RB, F/R = LT/RT, Enter ou P = Start, Tab = Back, setas ou 1-4 = direcional.

- Saves e configurações de perfil ficam em `%APPDATA%\TenchuZRecomp` (Linux: `~/.local/share/TenchuZRecomp`).
