<!-- source: https://github.com/Onaga00/ProjectStonehenge/blob/HEAD/README.md | fingerprint: 219ca0344032165e -->

1. Baixe a versão mais recente na [página de versões (Releases)](https://github.com/Onaga00/ProjectStonehenge/releases).

2. Extraia o arquivo numa pasta à sua escolha. O port é um executável só: sem instalador, sem outras DLLs e sem precisar do Visual C++ Redistributable.

3. Faça a cópia do seu próprio jogo para uma ISO (por exemplo com o ImgBurn no PC, ou seguindo o [guia de cópia](https://pcsx2.net/docs/setup/dumping/) do PCSX2).

4. Abra o `ProjectStonehenge.exe`. Na primeira abertura, o port pede a imagem do disco: clique em **Select ISO...** e escolha a sua ISO. O caminho fica guardado em `settings.json`. Também dá para abrir com `ProjectStonehenge.exe --iso "C:\path\to\game.iso"`.

5. Jogue. Tudo o que o port grava vai para `%LOCALAPPDATA%\ProjectStonehenge\`:

- `settings.json`: configurações do port (gráficos, tipos de controle, opções de depuração)

- `controls.ini`: teclas de teclado, mouse e controle

- `saves\`: o cartão de memória (formato de pasta do PCSX2)

- `logs\`: o registro da última sessão (anexe em relatos de erro)

- `cache\`: pipelines gráficos (recompilados a cada mudança de driver de vídeo)

### Opções de linha de comando (versões prontas)

- Opção · Efeito

- `--iso <path>` · usa esta imagem de disco em vez da do `settings.json`

- `--user-data <dir>` · usa outra pasta em vez de `%LOCALAPPDATA%\ProjectStonehenge\`

- `--internal-scale <1-8>` · resolução interna nesta sessão

- `--antialiasing <off | fxaa | smaa>` · anti-aliasing nesta sessão

- `--fps-overlay` / `--no-fps-overlay` · mostra ou esconde o contador de quadros

- `--debug-overlay` / `--no-debug-overlay` · liga ou desliga o depurador nesta sessão

- `--verify-iso` · confere a imagem do disco de novo em vez de usar o resultado guardado

- `--version`, `--help` · mostra a versão ou o texto de uso

Os valores da linha de comando valem só para a sessão atual e nunca são gravados no `settings.json`. As versões de desenvolvimento aceitam muitas outras opções (começar numa missão, gravar e reproduzir a entrada, capturas, cópias da RAM, ...): veja [docs/port/command-line.md](https://github.com/Onaga00/ProjectStonehenge/blob/HEAD/docs/port/command-line.md).
