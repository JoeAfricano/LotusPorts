<!-- source: https://github.com/OverkillLabs3/SoS-PS5/blob/HEAD/README.md | fingerprint: ab2065efcdc3ee1c -->

1. Baixe o `SonsOfSparta-PS5-Native-0.4.1-win64.zip` e extraia onde quiser.

2. Copie os arquivos descriptografados do **seu próprio** jogo para a mesma pasta do `SonsOfSparta-PS5.exe`: `eboot.bin` (ou um `eboot.elf` descriptografado), `sce_sys`, `sce_module` e `Media`.

3. Clique duas vezes no `SonsOfSparta-PS5.exe`, confira as configurações e clique em **Play**.

Não precisa instalar nada. A primeira abertura prepara o jogo a partir dos seus arquivos: uma janelinha explica e leva cerca de dois minutos. Depois, abre rápido. Os seus arquivos nunca são alterados.

**Show launcher at startup** (no grupo Launcher do launcher) controla a janela de configurações. Desligue para abrir o jogo direto com as configurações salvas. As janelas de preparação da primeira abertura e dos ícones do controle ainda aparecem quando necessário, e os erros continuam sendo mostrados. Para ver a janela de configurações de novo, edite o `launcher.ini` e coloque `show=1` na seção `[launcher]`.

Fechar a janela do jogo encerra em poucos segundos. Os registros ficam na pasta `logs`.
