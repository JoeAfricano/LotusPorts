<!-- source: https://github.com/hk21x/007eon-recomp/blob/HEAD/README.md | fingerprint: 0b2e9bc52840292c -->

Há dois jeitos de fazer o jogo rodar. Os dois montam o código do jogo no seu PC a partir do seu próprio disco: ele nunca é baixado, só o programa em volta dele.

### O launcher (recomendado)

O `EoNLauncher.exe` faz tudo por você: sem Visual Studio, sem Git, sem linha de comando, nada instalado no Windows e sem precisar de administrador.

**Você precisa de:**

- Windows 10 ou 11, 64 bits, e uma placa de vídeo com Direct3D 12 ou Direct3D 11.

- A sua própria cópia do jogo (Europa ou EUA): uma imagem `.iso` do seu disco, ou uma pasta com os arquivos do disco.

- Cerca de 1 GB livre, mais espaço para a sua cópia do jogo, e internet para o pacote do jogo (menos de 100 MB).

**Passos:**

1. Baixe o `EoNLauncher.exe` na [versão mais recente](https://github.com/hk21x/007eon-recomp/releases/latest).

2. Coloque numa pasta só dele com um caminho curto (por exemplo `C:\Games\EoN`) e abra. Tudo o que ele cria vai para uma pasta `EoN` ao lado. O Windows pode avisar sobre um app não reconhecido: escolha **Mais informações** e depois **Executar assim mesmo**.

3. **Seu jogo:** escolha a imagem do seu disco ou a pasta com os arquivos do disco. O launcher confere se é uma versão aceita.

4. **Preparar o jogo:** o launcher baixa o pacote do seu disco (o programa e um compilador, sem código do jogo; um para os discos europeus, outro para o americano) e depois o seu PC monta o código do jogo a partir do seu disco, em cerca de um minuto num PC recente. Dá para parar quando quiser; ele continua de onde parou.

5. **Jogar:** clique em **Play**. No jogo, **F1** abre as configurações de PC.

Quando sai uma versão nova, o launcher oferece no topo da janela: **Update the launcher** baixa, confere e reabre, e depois **Update** coloca a versão nova no lugar, mantendo saves e configurações. A versão nova é montada ao lado do jogo e só o substitui quando está pronta, então uma atualização que falha ou é interrompida deixa o jogo como estava.

**Vindo da 1.0.3 ou anterior:** essas versões montavam tudo a partir do código com cerca de 1,4 GB de ferramentas. O launcher novo mostra **A faster way to play**: clique em **Update**. O jogo continua funcionando até o novo ficar pronto, e saves e configurações são mantidos. Depois, **Manage > Remove the old build tools** libera o espaço que elas ocupavam.

As páginas do launcher: **Game** (o seu disco, os passos, Play), **Manage** (a instalação, as pastas, remover arquivos de montagem, montar o jogo de novo se ele não abrir) e **Log**.

Arquivos, todos na pasta `EoN` ao lado do launcher:

- `play\`: o jogo (`EoN.exe` e `EoNDriving.exe`, `EoN.dll` e `EoNDriving.dll` com o código do jogo montado no seu PC, `settings.toml`, `hdd\` com os saves, `logs\` com os registros do jogo e `compiler\`, que monta o código)

- `launcher.log`: o registro do launcher (envie junto com qualquer problema)

Para remover tudo, apague o launcher e a pasta `EoN` dele (guarde a `play\hdd` se quiser os seus saves).
