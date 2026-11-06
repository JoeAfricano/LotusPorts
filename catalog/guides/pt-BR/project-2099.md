<!-- source: https://github.com/GenryTheFox0/Project-2099/blob/HEAD/README.md | fingerprint: e1d8799c83e7af62 -->

1. Baixe o instalador anexado à versão correspondente no GitHub.

2. Deixe-o baixar e conferir o pacote de suporte da PC Edition correspondente.

3. Escolha uma fonte de Xbox 360 local aceita (a sua cópia).

4. Escolha uma pasta de destino vazia e termine a instalação.

5. Abra o `Launcher.exe` instalado.

Para instalar online, baixe o instalador da mesma versão do pacote correspondente. Para instalar offline, coloque o ZIP do pacote correspondente ao lado do instalador ou escolha-o no instalador. Não misture instalador e pacote de versões beta diferentes.

Guias de instalação: [English](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_EN.md) · [Русский](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_RU.md) · [Deutsch](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_DE.md) · [Français](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_FR.md) · [Italiano](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_IT.md) · [Español](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/INSTALL_ES.md)

A entrada pode ser uma ISO XDVDFS, um ZIP com a raiz do jogo, um ZIP com uma imagem GOD (o instalador a desempacota sozinho num disco com espaço e confere o tamanho de cada fragmento), uma pasta extraída com `Default.xex` e `Data`, uma pasta externa com uma raiz de jogo compatível ou um contêiner GOD/SVOD. Uma imagem GOD pode ser escolhida em qualquer nível que um jogador costuma clicar: a pasta externa, a pasta do title id, `00007000`, o próprio arquivo do contêiner ou a pasta `.data` dele. Se uma pasta escolhida tiver uma ISO ou um ZIP, o instalador abre sozinho.

Depois de uma instalação bem-sucedida, a ISO, a GOD ou a pasta extraída de origem não são mais necessárias. A PC Edition roda inteira a partir do destino escolhido e não depende da letra do disco de origem. Saves e caches do instalador usam o perfil do usuário atual do Windows.

**Só a extensão ou o cabeçalho de um contêiner não bastam.** O leitor atual lida com SVOD/GOD; ele não aceita qualquer pacote STFS que comece com LIVE, PIRS ou CON.

**Quais cópias funcionam**

O instalador reconhece fontes compatíveis de Xbox 360 pelo conteúdo dos arquivos, inclusive os pacotes de fases com diálogos embutidos. As conferências de idioma cobrem os arquivos de texto compartilhados e as legendas das cenas. Só o nome do contêiner, a região ou um `Default.xex` igual não provam que os textos podem ser reconstruídos corretamente.

Revisões registradas, mostradas na tela quando batem:

- `usa-europe-retail` e `usa-europe-retail-r2`: duas revisões comerciais EUA/Europa.

- `eu-retail`: a base de idioma original da PC Edition.

- `ru-god-alt`: uma GOD russa LIVE/XSF alternativa com tradução automática.

- `sazanoff-rus-god`: GOD RUS Region Free (SazanOFF v1.0b).

As duas últimas podem ter texto russo e codificação de fontes alterados. A Beta 4 reconstrói os idiomas conferidos inglês, alemão, francês, italiano e espanhol e mantém o russo aceito do Project 2099. Se um pacote de origem não puder ser identificado e reconstruído, o instalador informa esse pacote em vez de colocar o texto dele no idioma errado sem avisar.

**Conferência e recuperação**

O instalador confere os hashes dos arquivos de origem, monta a versão numa pasta temporária, confere o resultado final, continua downloads interrompidos, reaproveita pacotes válidos já baixados e bloqueia caminhos perigosos em ZIPs. Só uma instalação totalmente conferida vai para o destino escolhido.

A Beta 4 completou uma instalação limpa a partir da fonte SazanOFF fornecida; as pastas Original e Russo resultantes tinham **293 arquivos conferidos** cada. As cadeias de patches das cinco famílias de origem registradas foram conferidas à parte. Isso não garante compatibilidade com toda cópia modificada nem imagem correta em todos os capítulos.

**O que a versão traz**

O repositório público não tem ISO, pacote GOD nem os dados completos do jogo, e o instalador nunca procura o jogo original na internet.

O pacote de suporte traz componentes de PC e patches binários. Alguns patches incluem bytes substitutos derivados de recursos do jogo; mudar a extensão não muda o que esses bytes são. Veja as [Licenças de terceiros](https://github.com/GenryTheFox0/Project-2099/blob/HEAD/THIRD_PARTY_LICENSES.md) para o alcance exato dos avisos incluídos.
