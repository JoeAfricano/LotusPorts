<!-- source: https://github.com/Rinnegatamante/Crash-Team-Racing-High-Octane/blob/HEAD/README.md | fingerprint: 0fa58afd76721736 -->

- Faça a cópia da sua versão americana (US) do `Crash Team Racing` de PS1 e coloque o arquivo bin na pasta `assets` com o nome `ctr-u.bin`.

### Como usar as vozes da versão PAL

- Instale o Python 3.11 ou mais novo ([https://www.python.org/downloads/](https://www.python.org/downloads/)).

- Baixe [este script](https://github.com/Rinnegatamante/Crash-Team-Racing-High-Octane/raw/refs/heads/vita/tools/extract_pal_voices.py) clicando com o botão direito no link e escolhendo "Salvar link como..." ou, se o script abrir no navegador, "Salvar página como...".

- Coloque a cópia `.bin` do seu Crash Team Racing PAL na mesma pasta do script.

- Abra um prompt de comando nessa pasta digitando `cmd` na barra de endereço do Explorador de Arquivos e apertando Enter.

- Rode `python extract_pal_voices.py YOUR_DUMP_NAME.bin` (troque pelo nome do seu arquivo).

- Quando a extração terminar, coloque a pasta `pal-voices` gerada:

- no PS Vita: em `ux0:data/ctr/mods/`.

- no PC: na pasta `mods`, ao lado do executável do CTR: High Octane, para o caminho final ficar `mods/pal-voices`.
