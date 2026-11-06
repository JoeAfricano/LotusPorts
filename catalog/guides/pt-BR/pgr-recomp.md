<!-- source: https://github.com/hk21x/pgr1-recomp/blob/HEAD/README.md | fingerprint: 011520f00cd6b459 -->

Há dois jeitos de fazer o jogo rodar. Os dois montam o código do jogo no seu PC a partir do seu próprio disco: o código do jogo nunca é baixado.

### O launcher (recomendado)

O `PGR1Launcher.exe` faz tudo por você: sem Visual Studio, sem Git, sem linha de comando, nada instalado no Windows e sem precisar de administrador.

**Você precisa de:**

- Windows 10 ou 11, 64 bits, e uma placa de vídeo com Direct3D 12 ou 11.

- A sua própria cópia do jogo (uma versão aceita): uma imagem `.iso` do seu disco, ou uma pasta com os arquivos do disco.

- Espaço para o pacote do jogo (cerca de 72 MB) e a montagem, mais a sua cópia do jogo, e internet para o pacote.

**Passos:**

1. Baixe o `PGR1Launcher.exe` na [versão mais recente](https://github.com/hk21x/pgr1-recomp/releases/latest).

2. Coloque numa pasta só dele com um caminho curto (por exemplo `C:\Games\PGR1`) e abra. Tudo o que ele cria vai para uma pasta `PGR1` ao lado. O Windows pode avisar sobre um app não reconhecido: escolha **Mais informações** e depois **Executar assim mesmo**.

3. **Seu jogo:** escolha a imagem do seu disco ou a pasta com os arquivos do disco. O launcher confere se é uma versão aceita.

4. **Preparar o jogo:** o launcher baixa o pacote do seu disco (cada disco aceito tem um: o programa e um compilador, sem código do jogo) e depois o seu PC monta o código do jogo a partir do seu disco (cerca de 12 segundos num PC rápido de 16 threads). Dá para parar quando quiser; ele continua de onde parou.

5. **Jogar:** clique em **Play**. A primeira abertura formata o disco rígido emulado e copia o cache do jogo (cerca de 90 segundos de tela preta e um vídeo curto); as próximas são rápidas.

Quando sai uma versão nova, o launcher oferece no topo da janela: **Update the launcher** baixa, confere e reabre, e depois **Update** coloca a versão nova no lugar, mantendo saves e configurações. A versão nova é montada ao lado do jogo e só o substitui quando está pronta, então uma atualização que falha ou é interrompida deixa o jogo como estava.

**Vindo da 1.3 ou anterior:** essas versões montavam tudo a partir do código com cerca de 1,4 GB de ferramentas. O launcher novo mostra **A faster way to play**: clique em **Update**. O jogo continua funcionando até o novo ficar pronto, e saves e configurações são mantidos. Depois, **Manage > Remove the old build tools** libera as ferramentas e o código antigos.

Arquivos, todos na pasta `PGR1` ao lado do launcher:

- `play\`: o jogo (`PGR1.exe`, `PGR1.dll` com o código do jogo montado no seu PC, `pgr1.toml` com as configurações, `hdd\` com perfis, saves e caches, `logs\pgr1.log` com o registro do jogo, `achievements.toml` e `compiler\`, que monta o código)

- `launcher.log`: o registro do launcher (envie junto com qualquer problema)

Para remover tudo, apague o launcher e a pasta `PGR1` dele (guarde a `play\hdd` se quiser os seus saves).
