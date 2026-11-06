<!-- source: https://github.com/GetLostIn/CTR-NitroFueled-PC/blob/HEAD/README.md | fingerprint: 2beea108f5f64d88 -->

**O mais fácil: a versão portátil.** Baixe o `CTR-NitroFueled-PC-portable-windows.zip` na página de [versões (Releases)](https://github.com/GetLostIn/CTR-NitroFueled-PC/releases), extraia onde quiser e abra o **`CTR-NitroFueled-PC.exe`**, o launcher: escolha as suas pastas `CUSA13795` (jogo base) e `CUSA13795-patch` (atualização 1.21), clique em "Prepare the game" uma vez (ele junta as duas com links, sem espaço extra) e depois em Play. O Python vem junto; você só precisa de uma placa de vídeo com Vulkan 1.3.

**Imagem** (launcher: Settings > Display; no jogo: Insert):

- Resolução interna: o tamanho em que o jogo desenha a cena 3D. 480p e 720p para PCs modestos; de 1440p a 2160p geram uma imagem 4K, que também suaviza as bordas numa tela 1080p. Precisa reabrir (o menu do jogo faz isso). Limitada a 1440p com o ultrawide ligado.

- Proporção (v0.4, por tcosi; em Settings, não em Patches): uma escolha de proporção/saída: 16:9 com 1280x720, 1920x1080, 2560x1440 ou 3840x2160; 21:9 UltraWide com 2560x1080 ou 3440x1440; 32:9 Super UltraWide com 5120x1440, com o HUD centralizado. O upscaling fica desligado com o ultrawide. A primeira abertura prepara cerca de 9 GB de arquivos com patch (os originais ficam); abra uma vez normalmente até o menu antes, para o cache de shaders.

- Upscaling, um de cada vez, trocado ao vivo (desligado com o ultrawide):

- **AMD FSR 1** (padrão): uma ampliação espacial mais nítida que a do próprio jogo.

- **AMD FSR 3** (qualquer placa), **AMD FSR 4** (experimental, o modelo INT8 v07; precisa de suporte a "INT8 dot product", conferido ao abrir; o FSR 3 roda enquanto o modelo carrega) e **NVIDIA DLSS** (GeForce RTX; os arquivos vêm no pacote): ampliação temporal que reconstrói os detalhes ao longo de vários quadros a partir da profundidade e do movimento do jogo, e funciona como anti-aliasing na mesma resolução. Desligue se aparecerem rastros.

- Geração de quadros (v0.4, experimental, ao vivo): a interpolação de quadros do AMD FSR 3.1.6 mostra um quadro gerado entre cada par de quadros do jogo, cerca do dobro de imagens na tela; o jogo, o limite de FPS e o tempo dele não mudam. Melhor numa tela de 120 Hz ou mais com o jogo na metade da taxa (por exemplo limite de 72 FPS a 144 Hz, com V-Sync/Fifo ou Mailbox). Funciona com qualquer upscaler. Elementos rápidos do HUD podem tremeluzir. Detalhes: [docs/sacredfire/16-framegen.md](https://github.com/GetLostIn/CTR-NitroFueled-PC/blob/HEAD/docs/sacredfire/16-framegen.md).

- Com DLSS: qualidade do DLSS (Auto segue a resolução interna; DLAA, Quality, Balanced, Performance e Ultra Performance usam a proporção da NVIDIA da saída do DLSS, 1080p ou 4K, depois de reabrir) e modelo do DLSS (Transformer, recomendado, ou o CNN, mais leve).

**Idioma do jogo** (launcher: Settings > Console): os idiomas desta versão (CUSA13795, as Américas): inglês, espanhol latino-americano, francês e **português do Brasil**. O espanhol da Espanha não está nela. Com a edição europeia (CUSA14876), a lista mostra os idiomas da versão PAL (inglês, espanhol da Espanha, francês, alemão, italiano, árabe).

O launcher também tem os patches do jogo, limite de FPS, tela, monitor e controles, cópias e importação de saves (inclusive saves de PS4 descriptografados) e DLC opcional: acrescente os seus próprios pacotes de DLC (`.pkg`) e o port os informa como instalados (as DLCs do CTR Nitro-Fueled só trazem a licença; o conteúdo já está no jogo). Em espanhol e inglês.
