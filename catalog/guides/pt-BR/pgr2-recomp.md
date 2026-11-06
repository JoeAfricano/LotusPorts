<!-- source: https://github.com/hk21x/pgr2-recomp/blob/HEAD/README.md | fingerprint: 343f0584b6868b5b -->

Há dois jeitos de fazer o jogo rodar. Os dois montam o código do jogo no seu PC a partir do seu próprio disco: ele nunca é baixado, só o programa em volta dele.

### O launcher (recomendado)

O `PGR2Launcher.exe` faz tudo por você: sem Visual Studio, sem Git, sem linha de comando, nada instalado no Windows e sem precisar de administrador.

**Você precisa de:**

- Windows 10 ou 11, 64 bits, e uma placa de vídeo com Direct3D 12 ou 11.

- A sua própria cópia do jogo (PAL ou NTSC): uma imagem `.iso` do seu disco, ou uma pasta com os arquivos do disco.

- Cerca de 1 GB livre, mais espaço para a sua cópia do jogo, e internet para o pacote do jogo (cerca de 70 MB).

**Passos:**

1. Baixe o `PGR2Launcher.exe` na [versão mais recente](https://github.com/hk21x/pgr2-recomp/releases/latest).

2. Coloque numa pasta só dele com um caminho curto (por exemplo `C:\Games\PGR2`) e abra. Tudo o que ele cria vai para uma pasta `PGR2` ao lado. O Windows pode avisar sobre um app não reconhecido: escolha **Mais informações** e depois **Executar assim mesmo**.

3. **Seu jogo:** escolha a imagem do seu disco ou a pasta com os arquivos do disco. O launcher confere se é uma versão aceita.

4. **Preparar o jogo:** o launcher baixa o pacote do jogo (o programa e um compilador, sem código do jogo) e depois o seu PC monta o código do jogo a partir do seu disco. Levou cerca de 20 segundos num PC rápido de 16 threads; depende do seu PC e da conexão. Dá para parar quando quiser; ele continua de onde parou.

5. **Jogar:** clique em **Play**. A primeira abertura prepara o disco rígido emulado e copia o cache do jogo enquanto passam os vídeos de abertura (cerca de dois minutos); as próximas são rápidas.

Quando sai uma versão nova, o launcher oferece no topo da janela: **Update the launcher** baixa, confere e reabre, e depois **Update** coloca a versão nova no lugar, mantendo saves, configurações e músicas. A versão nova é montada ao lado do jogo e só o substitui quando está pronta, então uma atualização que falha ou é interrompida deixa o jogo como estava.

**Vindo da 1.5.1 ou anterior:** essas versões montavam tudo a partir do código com cerca de 1,4 GB de ferramentas. O launcher novo mostra **A faster way to play**: clique em **Update**. O jogo continua funcionando até o novo ficar pronto, e saves, configurações e músicas são mantidos. Depois, **Manage > Remove the old build tools** libera cerca de 2 GB (o FFmpeg, para as suas músicas, fica).

As páginas do launcher: **Game** (o seu disco, os passos, Play), **Your music**, **Manage** (a instalação, as pastas, remover arquivos de montagem, montar o jogo de novo se ele não abrir) e **Log**.

Arquivos, todos na pasta `PGR2` ao lado do launcher:

- `play\`: o jogo (`PGR2.exe`, `PGR2.dll` com o código do jogo montado no seu PC, `pgr2.toml` com as configurações, `hdd\` com perfis, saves, caches e as suas músicas, `logs\pgr2.log` com o registro do jogo, `achievements.toml` e `compiler\`, que monta o código)

- `launcher.log`: o registro do launcher (envie junto com qualquer problema)

- `tools\ffmpeg`: só se você importar as suas músicas

Para remover tudo, apague o launcher e a pasta `PGR2` dele (guarde a `play\hdd` se quiser os seus saves).
