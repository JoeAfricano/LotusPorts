<!-- source: https://github.com/josiahg2502-glitch/TomodachiLife-recomp/blob/HEAD/README.md | fingerprint: 48ef81d1b0945c01 -->

1. Baixe o **`Tomodachi-Life-PC-Setup.exe`** na [versão mais nova](https://github.com/josiahg2502-glitch/TomodachiLife-recomp/releases) e abra. O Windows pode dizer que protegeu o computador (o instalador não tem assinatura): clique em **Mais informações** e depois em **Executar assim mesmo**.

2. Escolha onde instalar (`C:\Games\TomodachiLife` por padrão; cerca de 15 GB). O launcher abre.

3. Ele encontra sozinho os arquivos do seu jogo e as suas chaves (Downloads, Área de Trabalho, Documentos e pastas de emuladores). Escolha o que ele não achar e clique em **INSTALL**. Uma cópia já extraída (uma pasta com `exefs\` e `romfs\`) também funciona: **OR A DUMP FOLDER**; aí não precisa de chaves.

4. Espere. A primeira instalação leva cerca de 40 minutos num PC recente (uma hora ou mais nos mais antigos): ela traduz e compila o código do jogo (cerca de 25 minutos) e depois compila cada shader para a sua placa de vídeo (cerca de 13 minutos). Dá para parar, ou fechar o launcher, e continuar depois: as etapas terminadas não se repetem.

5. Clique em **PLAY**. Da próxima vez, abra pelo atalho **Tomodachi Life** na Área de Trabalho.

Na pasta: o **`Tomodachi Life.exe`** abre o jogo direto, e o **`Tomodachi Life Launcher.exe`** abre o launcher (jogar, configurações, dados de save, instalação).

Os mesmos arquivos em zip, para quem preferir: **`Tomodachi-Life-PC.zip`** (extraia numa pasta própria e abra o `Tomodachi Life Launcher.exe`). Um zip chamado `tomodachi-runtime-...` não é o jogo: é uma parte que o instalador baixa sozinho.

O instalador baixa só o que precisa, cada arquivo conferido com um SHA-256 fixo: Python 3.13 (python.org), LLVM/clang 9 (a versão oficial, reempacotada em zip), o runtime pronto desta versão (só o código do projeto), libopus e FFmpeg.

Os shaders ficam prontos quando o instalador termina, então o jogo abre em segundos. Depois de atualizar o driver de vídeo, ou ao mudar a tela larga ou a escala de resolução, o jogo os prepara mais uma vez ao abrir: a tela de carregamento mostra "preparing shaders (once)" com a contagem, o tempo gasto e o que falta. Na primeira vez que você chega a um lugar ou evento novo, pode dar uma travadinha enquanto as imagens são preparadas; elas ficam guardadas, então depois fica liso.
