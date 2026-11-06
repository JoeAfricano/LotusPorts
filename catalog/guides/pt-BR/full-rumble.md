<!-- source: https://github.com/GameBreaker20XX/FullRumble/blob/HEAD/README.md | fingerprint: 326c3bad894fa429 -->

A versão para jogadores foi feita para instalar como um jogo comum de Windows:

- não precisa do .NET SDK no computador do jogador

- não precisa de ferramentas de desenvolvimento

- não tem passo de `dotnet install` ou `dotnet run`

- é uma versão independente para Windows x64 que já traz o runtime do .NET

O pacote oficial é um ZIP com o programa pronto; extraia e abra o `FullRumble.exe` direto. Na primeira abertura, escolha a cópia do seu próprio disco. O Full Rumble confere, prepara e recompila a biblioteca do jogo no seu PC com o compilador que vem junto. Essa primeira configuração pode levar alguns minutos. Não precisa instalar .NET SDK, Git nem Python.

### Instalar e jogar

1. Baixe o ZIP para Windows na [página oficial de versões (Releases)](https://github.com/GameBreaker20XX/FullRumble/releases).

2. Extraia o ZIP numa pasta onde dê para gravar e abra o `FullRumble.exe`.

3. Indique a cópia que você fez do seu próprio disco do NASCAR Rumble.

4. Espere enquanto o Full Rumble recompila o jogo no seu PC; depois, ele abre. Nas próximas vezes, ele aproveita essa biblioteca.

A cópia original do disco é copiada, nunca movida nem apagada. O Full Rumble guarda a configuração e a biblioteca do jogo gerada em `%APPDATA%\FullRumble`. O app é independente e não precisa de .NET SDK, runtime do .NET, Git ou Python instalados à parte.

### Atualizar o Full Rumble

Abra **Settings > Updates** no Full Rumble para conferir se há uma versão estável oficial mais nova. O app pergunta antes de baixar, confere a soma de verificação do pacote e prepara a atualização em segundo plano. Feche o Full Rumble quando quiser instalar: um pequeno ajudante aplica a atualização, mantém a pasta `disc/` local e as configurações e o cache de montagem em `%APPDATA%\FullRumble` e abre o jogo de novo. As atualizações nunca se instalam sozinhas, e versões de teste não são oferecidas. Quem está na Race 1.0 precisa baixar e extrair a Race 1.1 uma vez para ter o atualizador dentro do app.
