<!-- source: https://github.com/RetroPulse03/WinBackRecomp/blob/HEAD/README.md | fingerprint: 6f7252fce54ae521 -->

As versões são empacotadas para o [RecompLauncher](https://recomplauncher.com/) e o [Quiver Launcher](https://quiverlauncher.com/), que instalam e atualizam o jogo a partir das versões deste repositório. Para instalar à mão:

1. Baixe o `WinBackRecompiled-vX.Y.Z-Windows-x64.zip` na página de [versões (Releases)](https://github.com/RetroPulse03/WinBackRecomp/releases) e extraia.

2. Abra o `WinBackRecompiled.exe`. Na primeira abertura, ele procura o seu arquivo do jogo ao lado do executável e, se não achar, pede para você escolher. Também dá para passar `--rom <path>`. O arquivo é conferido e uma cópia fica na pasta do jogo, então isso só é preciso uma vez.

Requisitos: Windows 10 ou 11 de 64 bits e uma placa de vídeo com Vulkan.

### Jogar

- Opção · Efeito

- `--fullscreen` · Abre em tela cheia sem bordas. F11 ou Alt+Enter alterna entre janela e tela cheia a qualquer momento.

- `--console` · Mostra o registro.

- `--developer` · Mostra a interface de depuração do RT64.

**Teclado (jogador 1)**

- Tecla · Botão

- W A S D · Analógico

- F / R · A / B

- Espaço · Z

- Q / E · L / R

- Y / T / G / H · C-Cima / C-Esquerda / C-Baixo / C-Direita

- Setas · Direcional

- Enter · Start

- Esc · Menu do recomp

**Controle** (layout Classic, como na versão de PlayStation 2):

- Controle · Ação · N64

- Analógico esquerdo · Andar / mirar · Analógico

- X · Ação, se proteger · A

- A · Recarregar · B

- B · Trocar de arma · C-Cima

- Y · Travar mira · C-Baixo

- LB · Agachar · Z

- RB · Mirar · R

- LT / RT, analógico direito · Girar a câmera · C-Esquerda / C-Direita

- Clique do analógico direito · Câmera atrás do jogador · C-Esquerda/C-Direita duas vezes

- Direcional para baixo · Colocar / detonar C4 · D-Baixo

- Start · Pausa · Start

- Select (Back/View) · Menu do recomp · -

Nos menus e na tela de pausa, A confirma e B volta.

### Menu do recomp

Aperte Select (ou Esc) a qualquer momento para abrir o menu do recomp sobre o jogo; aperte de novo para fechar. O jogo não recebe comandos com o menu aberto. Mova com o direcional, o analógico esquerdo ou as setas, escolha com A (Enter) e volte com B (Backspace).

**Level Select** lista as fases de 00 a 30 com a área. Escolha uma fase e depois o **Stage start** ou um dos checkpoints dela, e o jogo pula para lá de onde você estiver, inclusive da tela de título.

- Um checkpoint vira **RECORDED** na primeira vez que você chega nele jogando normalmente. Os dados do checkpoint do próprio jogo são salvos em `saves/checkpoints/`, e o menu carrega exatamente esse estado dali em diante.

- Até lá, ele é **BASIC**: a fase começa do zero com você no checkpoint. Itens, portas abertas e outros avanços de antes na fase faltam, então algumas áreas podem se comportar diferente do jogo normal. Checkpoints alcançados depois de um carregamento BASIC não são registrados.

- **Stage start** começa a fase como o jogo faz, com a cena de abertura, vida cheia e o equipamento inicial.

**Display** reúne as configurações de imagem. Mude uma opção com esquerda e direita; ela vale na hora e é salva no `settings.json` ao lado do executável.

- **Window mode:** Windowed, Borderless (uma janela cobrindo a tela) ou Fullscreen (tela cheia exclusiva na resolução da Área de Trabalho).

- **Resolution:** a nitidez da cena 3D. Original é a do N64, 240 linhas (216 com Crop overscan), Double é o dobro e Match window (o padrão) desenha no tamanho da janela.

- **Crop overscan:** o WinBack deixa uma borda preta em volta da imagem, que as TVs da época escondiam além da tela. Ligado, corta a borda para a imagem ocupar mais da janela.

- **Integer scale:** ligado, amplia a imagem só em passos inteiros, para cada pixel do jogo ter o mesmo tamanho. O resto da janela fica preto.
