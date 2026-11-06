<!-- source: https://github.com/hachi23/unicorn_overlord_pc/blob/HEAD/README.md | fingerprint: c30480171339cbf6 -->

**Primeira abertura:** escolha a pasta da sua cópia. O programa de abertura confere (título e versão), instala em `data/game` e abre o jogo. No mesmo disco, a instalação leva um instante e não ocupa espaço extra (links); de outro disco, ele copia cerca de 7 GB com uma janela de progresso, e uma instalação cancelada continua na próxima abertura. **Nas próximas aberturas**, ele vai direto ao jogo; a preparação da imagem do jogo é pulada enquanto nada mudar.

As versões empacotadas trazem o programa de abertura e o executável do jogo. A partir do código:

```
python launcher/unicorn_launcher.py
```

A tela **System → Options** do próprio jogo (pelo título ou pelo menu do jogo) tem uma seção **PC** no fim: modo de tela, VSync, resolução de renderização (vale na próxima abertura), ampliação FSR 1, nitidez e intensidade, ícones dos botões, zona morta do analógico e a sobreposição de desempenho. As configurações ficam no `vw.ini`; **Restore Defaults** também as zera.

> **Toda mudança de resolução de renderização exige reabrir.** Aperte **Insert** para abrir o menu do port, mude a resolução em **Performance** e escolha **Apply and restart**. Se mudar na seção **System → Options → PC** do jogo, feche e abra de novo.

No jogo, **Insert** (R3+L2 no controle) abre o menu do port com todas as configurações. Ele tem quatro abas:

- **Image:** modo de tela, VSync, **Anime4K**, FSR 1 e nitidez RCAS. O Anime4K (Fast ou Quality) redesenha os traços do jogo mais nítidos; quando a janela é mais de 1,2 vez maior que a imagem do jogo nos dois sentidos, ele também dobra a imagem antes do FSR 1. Ele custa tempo da placa de vídeo por quadro, medido numa placa da classe RX 560X em 1080p: Fast 3,3 ms, Quality 7,5 ms (um quadro a 60 FPS tem 16,7 ms), então o Quality é para uma placa mais forte.

- **Performance:** resolução de renderização, quadros na fila e as opções de leitura da placa de vídeo.

- **Overlay:** a sobreposição de desempenho.

- **Controls:** teclas do teclado e botões do controle, salvos no `vw.ini`, e os **ícones dos botões** que o jogo mostra: Auto (segue o controle em uso), PlayStation, Xbox ou Nintendo. Os ícones de Xbox e Nintendo são do próprio jogo, das versões de Xbox e Switch. A **zona morta do analógico** (15% por padrão) impede que analógicos que não voltam ao centro façam o cursor andar sozinho: aumente se o cursor ou a câmera ainda se moverem sozinhos.

As configurações ficam no `vw.ini`, na pasta de dados. A aba Overlay também tem um botão **Change game folder** para reabrir a configuração.

**Atualizações:** ao abrir, o programa procura uma versão mais nova no GitHub e pergunta antes de baixar; a atualização mantém configurações, saves e o jogo instalado. Desligue com **Check for updates at start** no menu Insert.

Algo errado? Veja [docs/KNOWN_ISSUES.md](https://github.com/hachi23/unicorn_overlord_pc/blob/HEAD/docs/KNOWN_ISSUES.md) e [docs/TROUBLESHOOTING.md](https://github.com/hachi23/unicorn_overlord_pc/blob/HEAD/docs/TROUBLESHOOTING.md). Cada abertura grava um registro em `logs/`, na pasta de dados. Quando o jogo para com erro, o programa de abertura oferece salvar um ZIP de diagnóstico (o registro com as suas pastas ocultadas, mostrado antes para você revisar) para anexar à issue. Nenhum arquivo do jogo nem save vai junto.

### Resolução de renderização

O jogo renderiza em 1920×1080 por padrão. Escolha outro tamanho no menu (Performance, vale ao reabrir) ou com `VW_RENDER_SCALE`: `720p` e `900p` para placas fracas, `1800p` e `4K` para supersampling: o jogo é desenhado maior e reduzido para a janela, o que dá bordas suaves e arte mais nítida. Todo tamanho usa o caminho de PS4 Pro do próprio jogo (`VW_NEO_MODE=1`), que organiza as camadas 2D a partir do tamanho final, então o enquadramento e o HUD ficam iguais em qualquer resolução.

### Ultrawide (experimental)

**Aspect ratio** no menu (Performance, vale ao reabrir), `aspect_ratio=` no `vw.ini` ou `VW_ASPECT_RATIO`: `21:9` (2560×1080 na resolução nativa) ou `32:9` mostram mais do mapa de batalha e das cenas nas laterais em vez de esticar; menus e textos ficam em 16:9 no meio. O `VW_ASPECT_RATIO` também aceita qualquer `W:H`, como `43:18` para 3440×1440. Ele combina com a resolução: 4K em 21:9 renderiza 5120×2160. Numa tela 16:9, uma imagem mais larga ganha faixas pretas em cima e embaixo.

É experimental: foi feito num notebook 16:9 e não foi testado num monitor ultrawide. Relatos são bem-vindos, dando certo ou errado. Abra uma issue com o modelo **Ultrawide report** e anexe:

- o ZIP de diagnóstico: abra o programa com `--export-diagnostics` (ou responda **Sim** na janela de erro depois de uma falha);

- capturas do mapa de batalha, de uma cena com texto, de um menu e de qualquer coisa esticada, cortada ou fora do lugar (Win+Shift+S no Windows);

- a resolução do seu monitor e o modo de tela (janela, sem bordas ou tela cheia).

### Outros idiomas (experimental)

Traduções de fãs feitas para a versão de Switch (`Unicorn_US.CPK`, como a de **português do Brasil**) e traduções que você mesmo escrever em JSON funcionam como mods em `data/mods/`. Elas substituem o texto em chinês, então coloque **Text Language** (menu Insert) em chinês tradicional ou simplificado. Só o menu de título e o diálogo de abertura foram conferidos até agora. Passo a passo: [docs/LANGUAGE_MODS.md](https://github.com/hachi23/unicorn_overlord_pc/blob/HEAD/docs/LANGUAGE_MODS.md). Relatos são bem-vindos com o modelo de issue **Translation report**.

### DLC

Coloque as DLCs que você tem numa pasta `dlc` na pasta de dados (ao lado do `vw.ini`): o pacote de PS4 como está (por exemplo `HP0177-CUSA36381_00-PRIVILEGEDEMBLEM-A0000-V0100.pkg`) ou a pasta extraída dele. Um `.zip` precisa ser extraído antes. Na próxima abertura, o registro lista cada uma (`DLC: PRIVILEGEDEMBLEM (...)`) e o jogo a vê como sua. Só o nome do pacote é lido: as DLCs do Unicorn Overlord liberam conteúdo que já está no jogo. Os menus de DLC do jogo mostram o que foi liberado.

### Teclado

- Tecla · Botão

- WASD · analógico esquerdo

- Setas · analógico direito

- Espaço · X

- Shift esquerdo · Círculo

- E · Quadrado

- Q · Triângulo

- 1 e 3 · L1 e R1

- R e F · L2 e R2

- Z e C · L3 e R3

- I, K, J, L · direcional

- Enter · Options

- Tab · touchpad

- Insert · o menu do port
