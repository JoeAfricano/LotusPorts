<!-- source: https://github.com/enrique363/voldorium/blob/HEAD/README.md | fingerprint: ca589848e2bc791b -->

Você precisa do Windows 10 ou 11, da cópia do seu próprio disco do SoulCalibur (USA, T-1401N, V1.000) como arquivo `.gdi` com os arquivos das faixas, de cerca de 1 GB livre e de internet na primeira abertura. Não precisa instalar mais nada.

### Windows (download, a partir da 1.1.0)

1. Na página de [versões (Releases)](https://github.com/enrique363/voldorium/releases/latest), em **Assets**, baixe o **`Voldorium--windows.zip`** (cerca de 2 MB) e extraia num lugar onde dê para gravar (não em `Program Files`).

2. Abra o **`Voldorium.exe`** e escolha o seu arquivo `.gdi` quando pedir.

3. A primeira abertura monta o jogo a partir do seu disco. Antes, ele diz o que vai baixar e pergunta: o compilador [llvm-mingw](https://github.com/mstorsjo/llvm-mingw) (cerca de 190 MB), dois arquivos pequenos do [Flycast](https://github.com/flyinghead/flycast) (a tabela de senos do SH-4 e a fonte substituta da BIOS) e, só se você não tiver Python 3.10 ou mais novo, um Python portátil (cerca de 11 MB). Cada um tem versão fixa e é conferido pelo SHA-256, e tudo vai para a pasta `builder`; nada é instalado no sistema. Uma janela mostra as três etapas (extrair os arquivos de abertura da sua cópia, traduzir o código do jogo para C, compilar no `sc_game.dll`), que levam de meio minuto a alguns minutos. O jogo abre quando termina; nas próximas vezes, vai direto ao jogo.

O download não traz código do jogo: o `Voldorium.exe` é o runtime (renderizador, som, jogo em rede, lobby), e o código do jogo é feito a partir do seu próprio disco no seu PC. Depois de atualizar para uma versão nova, a primeira abertura remonta o `sc_game.dll` do mesmo jeito (as ferramentas já estão lá). Todos na mesma versão montam um `sc_game.dll` idêntico byte a byte, então o Online Versus funciona entre quaisquer dois jogadores dessa versão.

Para desinstalar, apague a pasta. As suas configurações e o cartão de memória estão nela (`voldorium.ini`, `vmu_a1.bin`): guarde esses dois arquivos para levá-los a uma versão nova.

### Jogar

Abra o `Voldorium.exe` na pasta do jogador. Na primeira vez, ele pede a cópia do seu disco (o arquivo `.gdi`) e lembra dela no `voldorium.ini`. Ele abre em tela cheia na resolução da sua Área de Trabalho; o item **GRAPHICS** do menu principal muda resolução, tela cheia, anti-aliasing, VSync e velocidade de carregamento (original ou rápida). Controles e teclado funcionam pelo SDL2.

O **Online Versus** fica no menu principal. Ele lista jogadores no mundo todo (e na sua rede local) procurando partida com a mesma versão. Escolha um para desafiar; o outro jogador vê o seu ping e aceita ou recusa. Os jogadores aparecem como OPPONENT, a não ser que definam um apelido (`[net] name = ...` no `voldorium.ini`); nomes de computador nunca são enviados. Os dois precisam ter montado a partir do mesmo código: o lobby só junta versões idênticas.

Configurações (`voldorium.ini`): `[video] fullscreen, resolution, window, msaa, backgrounds` (`stretch`, `blur` ou `replace`), `[net] lobby, name`, `[game] disc, vmu, loading` (`original` ou `fast`). Mais detalhes no `port/README.md` e no README.txt da pasta do jogador.
