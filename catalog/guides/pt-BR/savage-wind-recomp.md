<!-- source: https://github.com/Revivedx/SavageWindRecomp/blob/HEAD/README.md | fingerprint: 5aa7fc1b20b41aa9 -->

**Você precisa de:** um processador de 64 bits com SSE4.2 (com AVX2 roda mais rápido), uma placa de vídeo com driver Vulkan 1.3 e 16 GB de RAM. Com vídeo integrado, que usa a RAM do PC, 8 GB não bastam: os 3 GB de memória do jogo precisam ficar disponíveis para a placa de vídeo ([#117](https://github.com/Revivedx/SavageWindRecomp/issues/117)).

1. Baixe na página de [versões (Releases)](https://github.com/Revivedx/SavageWindRecomp/releases):

- **Windows:** `SavageWind-0.4.1-alpha.4-windows.zip`. Extraia numa pasta em que você possa gravar (não em Arquivos de Programas): os arquivos do jogo, os saves e as configurações ficam nessa pasta.

2. Abra o `savagewind.exe`. Na primeira vez ele pede os arquivos do jogo: escolha a pasta com o seu jogo extraído (`exefs/` e `romfs/`, versão 1.9.0) e clique em **Set up game files**. Ele confere a versão, copia os arquivos e recompila o código do jogo no seu PC, uma vez só (cerca de 6 minutos para o código com um processador de 12 threads, mais a cópia e a pré-compilação opcional de shaders). Os seus arquivos não são alterados. Ao terminar, o jogo começa. Para o DLC, veja a seção The DLC no GitHub.

3. Daí em diante, o Savage Wind vai direto para o jogo. As configurações, incluindo os idiomas de texto e voz, ficam nas Opções do jogo (menu de pausa > System > Options, ou Options na tela de título).

4. O **Savage Wind Configuration** (`Savage Wind Configuration.exe`) abre a tela de configuração: Jogar, Configurações, seus arquivos do jogo (preparar de novo, recompilar o código, apagar) e Sair. No jogo, "Quit to Start Screen" também volta para ela.

Vindo da Alpha 3: extraia a Alpha 4 por cima da pasta da Alpha 3. Arquivos do jogo, saves e configurações continuam; a tela inicial pede para recompilar o código do jogo (Set up game files) uma vez. As novidades estão no [CHANGELOG.md](https://github.com/Revivedx/SavageWindRecomp/blob/HEAD/CHANGELOG.md).

Vindo da Alpha 4 (0.4.0): a 0.4.1 é uma correção rápida. Extraia por cima da pasta da Alpha 4: não precisa preparar nada de novo, tudo continua.

Para guardar arquivos do jogo, saves e caches em outro disco, veja [Movendo a pasta de dados](https://github.com/Revivedx/SavageWindRecomp/blob/HEAD/docs/data-folder.md).

As telas de preparação e configuração mostram a arte do próprio Savage Wind. Para ver uma imagem sua no lugar, coloque-a ao lado do `savagewind.exe` com o nome `splash.jpg` ou `splash.png`; ela só é lida da sua pasta e nunca faz parte do Savage Wind.

Precisa do Windows 10 ou mais novo, processador com SSE4.2 (de 2008 em diante), placa de vídeo com Vulkan 1.3 e cerca de 20 GB livres no disco. AVX2 não é obrigatório: num processador sem AVX2 / FMA, a preparação compila o código do jogo num modo compatível, que roda mais devagar.
