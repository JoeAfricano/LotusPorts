<!-- source: https://github.com/DrummingBird1/gow-psp-recomp/blob/HEAD/README.md | fingerprint: 1b6c49921274bb36 -->

Você só baixa **um arquivo**, o launcher. Sem Python, sem configuração manual.

1. Baixe o [GoW_PSP_Launcher-v1.4.0.zip](https://github.com/DrummingBird1/gow-psp-recomp/releases/latest/download/GoW_PSP_Launcher-v1.4.0.zip) na [versão mais recente](https://github.com/DrummingBird1/gow-psp-recomp/releases/latest) e extraia onde quiser: você recebe uma pasta `GoW_PSP` com o `GoWLauncher.exe` e um `README.txt` curto. Tudo o que o launcher cria (códigos, montagens, saves, configurações) vai para `GoW_PSP_data`, ao lado do exe.

2. Abra o `GoWLauncher.exe` e escolha a pasta com as **imagens dos seus próprios discos** (`.chd` ou `.iso`). Ele identifica cada jogo pelo ID do disco, não pelo nome do arquivo.

3. Clique em **Build**. Na primeira vez, o launcher pede para baixar as ferramentas de compilação portáteis (**cerca de 246 MB**, uma vez; cerca de 550 MB extraídas): [llvm-mingw](https://github.com/mstorsjo/llvm-mingw) (compilador Clang), CMake e Ninja, das versões oficiais, conferidos por hashes SHA-256 fixos. Eles ficam em `GoW_PSP_data`; **sem Visual Studio, sem administrador, nada instalado no Windows.**

4. Espere a montagem, feita uma vez: cerca de 4 minutos no primeiro jogo (com o download) e menos de 3 no segundo, num processador de 20 threads.

5. Escolha uma predefinição gráfica e clique em **Play**.

> **Aviso do Windows?** O `GoWLauncher.exe` não tem assinatura e é feito com PyInstaller, então o SmartScreen pode mostrar "O Windows protegeu o computador" (clique em **Mais informações** e depois em **Executar assim mesmo**), e alguns antivírus marcam programas PyInstaller como falso positivo. Confira se o seu download é o verdadeiro (SHA-256 da v1.4.0):
> - `GoW_PSP_Launcher-v1.4.0.zip`: `a212651f306f214646ab72df9592a05daad70cb2c019074523a0fc24bf51a03e`
> - `GoW_PSP\GoWLauncher.exe`: `0a9c4afd8e04d31fa59309a0cd3fb4bda94a8d7fb9ac335de899e28038795474`
> (PowerShell: `Get-FileHash <file>`). Cada versão lista os hashes nas notas.

O launcher não abre janela de console. O jogo abre em tela cheia (F11 ou Alt+Enter troca para janela; a opção Fullscreen do launcher define o padrão). O que ele escreve vai para `GoW_PSP_data\build\launcher_coo.log` (ou `launcher_gos.log`), os botões apertados na sessão para `launcher_coo_input.txt` (ou `launcher_gos_input.txt`) e o tempo de cada quadro para `launcher_coo_perf.csv` (ou `launcher_gos_perf.csv`). O próximo Play sobrescreve esses arquivos, então copie logo depois de o problema acontecer.

### Informar um erro

Abra uma [issue](https://github.com/DrummingBird1/gow-psp-recomp/issues/new/choose), diga em qual fase e o que acontece e anexe os dois arquivos de `GoW_PSP_data\build\`. Eles só têm texto: o registro do jogo e as teclas apertadas. Diga se começou um jogo novo ou de um save. Nunca anexe arquivos do jogo, saves ou capturas do jogo.

Problemas? Veja [Solução de problemas e FAQ](https://github.com/DrummingBird1/gow-psp-recomp/blob/HEAD/docs/troubleshooting.md): SmartScreen, onde ficam os arquivos, atualizar, falhas de montagem, travadinhas.

Por que montar no seu PC? O código do jogo é gerado a partir do seu próprio disco, então não pode ser distribuído; o launcher o compila no seu PC, uma vez.

### Controles

- Teclado · PSP · Controle

- Setas · Direcional · Direcional

- Z / X / A / S · X / Círculo / Quadrado / Triângulo · A / B / X / Y (layout do PSP)

- Q / W · L / R · botões de ombro ou gatilhos

- Espaço ou Enter · Start · Start

- V · Select · Back

- I / J / K / L · analógico · analógico esquerdo

- O / U (segurar) · gira o analógico no sentido horário / anti-horário, para os comandos de "girar o analógico" · girar o analógico esquerdo

- C (segurar) · turbo: os botões de ação e L/R segurados são apertados e soltos 10 vezes por segundo, para os comandos de "apertar várias vezes" · clique do analógico direito

- esquiva (L + R + analógico) · analógico direito

- **Pular um vídeo:** Enter, Espaço, um clique do mouse, ou Start ou X do controle. O jogo segue para o menu ou a fase como se o vídeo tivesse acabado.

- **Sair:** Alt+F4 ou o botão de fechar saem na hora. Esc pergunta "Quit the game?" com o jogo pausado: Enter ou X do controle = sim, Esc ou Círculo do controle = não. No controle, segure Back + Start por 2 segundos. O jogo só salva nos próprios pontos de save, então sair perde o progresso desde o último save. O launcher volta quando o jogo fecha.

- F11 ou Alt+Enter: tela cheia / janela. F12: captura de tela. F3: quadros por segundo no canto. F2: os botões apertados, embaixo à esquerda.

- **Para mudar**, use o botão Controls… do launcher (Options > Advanced): teclas, botões do mouse e do controle. A esquiva no analógico direito pode ser desligada ali.

Enquanto o jogo roda, o launcher se minimiza e para a animação, para não tirar desempenho do jogo.

### Preparar o ambiente de desenvolvimento

1. Ferramentas: Ghidra 12.1.3 + ghidra-allegrex v21.4, JDK 21 e Python 3.15 para a análise. Para compilar, as ferramentas portáteis bastam (llvm-mingw, CMake, Ninja: `python tools/check_portable_toolchain.py` baixa, ou `GOW_TOOLCHAIN=portable`); o Visual Studio 2022+ com a carga C++ e o CMake também funciona.

2. EBOOT descriptografado: o launcher faz isso (em `build/eboot/`). À mão, compile o `gow_disc` (`cmake -S . -B build/cmake_tools -DGOW_TOOLS_ONLY=ON` e depois `cmake --build build/cmake_tools --config Release`) e rode `build/cmake_tools/Release/gow_disc.exe --disc <your disc>.chd --decrypt-eboot re/dumps/UCUS98653_EBOOT.BIN` (`UCUS98737_EBOOT.BIN` para o Ghost of Sparta).

3. Ambiente Python para os testes: `py -3 -m venv .venv` e depois `.venv/Scripts/python -m pip install unicorn`.

4. Os códigos de referência do PPSSPP usados pelo `build_nid_db.mjs` são opcionais; a lista completa de arquivos está no README do repositório.

5. Para capturas e gravações de progresso: `.venv/Scripts/python.exe -m pip install pillow numpy` e o ffmpeg.

Veja o [recomp/README.md](https://github.com/DrummingBird1/gow-psp-recomp/blob/HEAD/recomp/README.md) para os comandos de recompilar/montar/testar e a pasta [docs/](https://github.com/DrummingBird1/gow-psp-recomp/blob/HEAD/docs/) para os resultados.
