<!-- source: https://github.com/crazyriddler/FinestHourRemastered/blob/HEAD/README.md | fingerprint: f4d03e784ce4610e -->

1. Baixe a [versão 1.0](https://github.com/crazyriddler/FinestHourRemastered/releases/tag/v1.0): o `FinestHourRemasteredSetup.exe`, o `FinestHourRemastered.index` e todas as partes `FinestHourRemastered.data.NNN`, todos na mesma pasta. O instalador sempre traz a atualização mais recente.

2. Abra o `FinestHourRemasteredSetup.exe`.

3. Escolha a sua cópia do jogo: a **pasta** (a que tem `default.xbe`, `COD.PAK`, `AUDIO`, `SHELL`, ...) ou uma **imagem de disco** (`.iso`, uma imagem "redump" completa ou um XISO). O instalador confere se é a versão aceita.

4. Escolha onde instalar (cerca de 10 GB) e os atalhos e clique em **Install**.

5. Jogue. A primeira abertura prepara cada shader que o jogo usa para a sua placa de vídeo (alguns segundos), para nenhum ser compilado durante o jogo.

O instalador também roda sem perguntas, para scripts: `FinestHourRemasteredSetup.exe --source "D:\Games\COD.iso" --target "C:\Games\Finest Hour Remastered" --shortcuts --log install.log` (código de saída 0 instalado, 1 falhou, 2 não é a versão aceita).

### Atualizações

As correções chegam como atualizações. A versão [latest update](https://github.com/crazyriddler/FinestHourRemastered/releases/tag/update) traz o `FinestHourRemasteredUpdate.exe`, publicado de novo a cada atualização (a data dele é a da atualização). Ela é cumulativa: abra, escolha a pasta da instalação e clique em **Update**, qualquer que seja a versão instalada; configurações e saves são mantidos. A versão instalada está no `version.txt` ao lado do jogo. Sem perguntas: `FinestHourRemasteredUpdate.exe --update "C:\Games\Finest Hour Remastered" --log update.log` (código de saída 0 atualizado, 1 falhou).

O próprio jogo procura uma atualização nova ao abrir, em segundo plano (uma conexão lenta ou ausente nunca atrasa a abertura): quando há uma, o menu principal mostra o que ela traz e oferece **Update**, **Not Now** ou **Don't Ask Again**. Update baixa o atualizador para a pasta Temp (tamanho e SHA-256 conferidos), o jogo fecha, o atualizador instala e abre o jogo de novo, e o jogo apaga o atualizador baixado na abertura seguinte. Don't Ask Again desliga a conferência; **Check for Updates** (PC Settings, General) liga de novo. Nada é baixado sem você escolher Update.

As configurações ficam no `codfh.toml` ao lado do exe, os saves em `hdd` e os shaders preparados em `shadercache`. Escolha **PC Settings** no menu principal (ou aperte **F10** a qualquer momento) para todas as opções.
