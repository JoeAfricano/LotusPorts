<!-- source: https://github.com/almost-drowned-rat/THPS2X-Recompiled/blob/HEAD/README.md | fingerprint: a06508ce73064cfd -->

### Requisitos

- Windows 10/11, 64 bits, ou Linux com Proton / Wine (sem garantia)

- **É obrigatória uma placa de vídeo com pelo menos Vulkan 1.3** (o driver precisa oferecer Vulkan 1.3 com "dynamic rendering" e `VK_KHR_push_descriptor`: NVIDIA GTX 900 e mais novas, AMD RX 400 e mais novas, Intel HD 500 / UHD 600 e mais novas, Intel Arc, Steam Deck). Aparelhos mais antigos, só com Vulkan 1.1 / 1.2, não são aceitos. Num PC sem um aparelho assim, o `THPS 2X Recomp.exe` e o `THPS2X.exe` mostram uma mensagem dizendo isso e os dispositivos de vídeo encontrados; atualizar o driver muitas vezes acrescenta o Vulkan 1.3.

- A sua própria cópia do Tony Hawk's Pro Skater 2X (USA)

- Cerca de **2 GB** livres (não precisa de internet: o compilador vem dentro do zip da versão)

### Versão aceita

- Versão · Title ID · SHA-1 do `default.xbe`

- USA (NTSC-U) · 41560001 · `469818E015AED2730641C0BDCBD2557D8A6B8BB4`

Outras versões são recusadas com: "This THPS 2X version is not supported. Supported version(s): USA. Please provide a supported version."

### Primeira abertura

1. Baixe o `THPS2X-Recompiled-v<version>-win64.zip` (cerca de 210 MB, com o compilador) na página da versão e extraia (mantenha a pasta `tools\` ao lado do exe).

2. Abra o `THPS 2X Recomp.exe`. A janela de configuração pede duas coisas: a imagem do seu disco (`.iso`, XISO ou imagem redump completa) e a pasta de instalação (uma pasta nova ao lado do exe é sugerida; espaços no caminho funcionam). Clique em **Go**.

3. A configuração copia a si mesma e o compilador para essa pasta e prepara tudo lá, com o progresso na mesma janela:

- a imagem é conferida, extraída e verificada (a imagem em si é só lida),

- o compilador ([llvm-mingw](https://github.com/mstorsjo/llvm-mingw), uma versão fixa, em `tools\download\` do zip) é conferido pelo SHA-256 e extraído; só se esse arquivo faltar ele é baixado (cerca de 190 MB); se o download falhar, aparece o motivo real com **Retry** e o link para baixar à mão,

- o código do jogo é traduzido e compilado no seu PC (alguns minutos, uma vez só).

4. Clique em **Play**. Depois, abra o `THPS 2X Recomp.exe` na pasta de instalação: ele vai direto ao jogo.

5. Opcional: acrescente as suas músicas, um pacote de shaders e o vídeo de abertura.

Colocar a imagem do disco ou os arquivos extraídos em `game\` à mão e abrir o `THPS 2X Recomp.exe` ali continua funcionando (uma imagem em `game\` é apagada depois de uma instalação conferida).

### Atualizar de uma versão anterior

Extraia o zip novo por cima da pasta de instalação antiga (a que tem `THPS 2X Recomp.exe`, `game\` e `saves\`). O jogo é montado mais uma vez na próxima abertura (alguns minutos); as pastas `saves\` e `media\` ficam.

> **Aviso do Windows:** o exe não tem assinatura. O **SmartScreen** mostra "O Windows protegeu o computador": clique em **Mais informações → Executar assim mesmo**. O **Controle Inteligente de Aplicativos** (Windows 11) bloqueia apps sem assinatura por completo; o port só roda com ele desligado.
