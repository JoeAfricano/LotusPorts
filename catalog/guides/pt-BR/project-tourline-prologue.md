<!-- source: https://github.com/mathewdouglas/Tourline-Prologue/blob/HEAD/README.md | fingerprint: 81a3605110beef2e -->

1. Baixe o ZIP para Windows da [v0.2.4](https://github.com/mathewdouglas/Tourline-Prologue/releases/tag/v0.2.4) na [página de versões](https://github.com/mathewdouglas/Tourline-Prologue/releases).

2. Extraia o ZIP inteiro numa pasta comum. Não abra de dentro do ZIP.

3. Abra o **tourline-prologue.exe**. Escolha onde guardar saves, configurações e shaders (a pasta padrão, ou uma pasta vazia em outro disco). Depois, escolha a pasta do seu disco extraído.

4. Se o `EMAIN.ELF` ainda não estiver disponível, escolha o seu RPCS3 instalado quando pedir, ou um `EMAIN.ELF` descriptografado feito a partir do seu próprio `EMAIN.SELF`.

5. Use o **tourline-prologue.exe** para todas as próximas aberturas. A configuração fica guardada. Aperte **F1** para as configurações. A primeira predefinição é 720p com MSAA 2x.

O arquivo `runtime/gt5p.exe` é o motor. Não é o ícone para clicar duas vezes.

Preparação opcional de shaders, depois da configuração e com o jogo fechado: rode o **Prepare shaders.cmd**. Ele lê a pasta do jogo que você já escolheu. Não precisa de cópia de arquivos, Python nem compilador. Pode levar alguns minutos e usa mais espaço. Cancelar mantém o que já terminou; rode de novo para continuar. As próximas aberturas só usam a preparação depois de conferi-la. `tourline-prologue.exe --no-prepared` volta ao cache comum e não apaga nenhum dos dois caches.

Para mover saves e shaders, feche o jogo e rode o **Change storage.cmd**. Para usar outra cópia, rode `tourline-prologue.exe --setup`. Para atualizar, feche o jogo e extraia o ZIP novo numa pasta própria. O armazenamento escolhido continua onde está.

Mais detalhes: [launcher/START-HERE.txt](https://github.com/mathewdouglas/Tourline-Prologue/blob/HEAD/launcher/START-HERE.txt), [docs/player-setup.md](https://github.com/mathewdouglas/Tourline-Prologue/blob/HEAD/docs/player-setup.md), [docs/release-v0.2.4.md](https://github.com/mathewdouglas/Tourline-Prologue/blob/HEAD/docs/release-v0.2.4.md). Mudanças por versão: [CHANGELOG.md](https://github.com/mathewdouglas/Tourline-Prologue/blob/HEAD/CHANGELOG.md).
