<!-- source: https://github.com/LAB0ULANGE/stuntman-pc/blob/HEAD/README.md | fingerprint: 2dccb652a60b5204 -->

1. Baixe o `Stuntman-PC-<version>-windows-x64.zip` na página de [versões (Releases)](https://github.com/LAB0ULANGE/stuntman-pc/releases) e extraia num lugar onde dê para gravar (não em Arquivos de Programas).

2. Abra o `stuntman.exe`. Na primeira vez, ele pede a imagem do seu disco e confere se é o disco certo: a **versão europeia, SLES-50288**.

3. Jogue. Alt+Enter alterna entre janela e tela cheia. As configurações ficam no `pc_settings.ini`, ao lado do executável (veja o README.txt dele).

### Requisitos

O jogo roda a 25 quadros por segundo, como no console. Para manter isso:

- Mínimo (`resolution=original`) · Recomendado (`resolution=3x`)

- Sistema · Windows 10/11, 64 bits · Windows 10/11, 64 bits

- Processador · 4 núcleos, rápido por núcleo: Intel Core i5-12400, AMD Ryzen 5 5600 ou melhor · Intel Core i7-10700K, AMD Ryzen 5 5600X ou melhor

- Placa de vídeo · Vulkan 1.3: NVIDIA RTX 3060 / AMD RX 6600 (estimativa) · NVIDIA RTX 3070 / AMD RX 6700 XT

- Memória · 8 GB · 8 GB

Testado num Intel Core i7-10700K com uma NVIDIA RTX 3070: 25 quadros por segundo em todas as resoluções a partir de 4 núcleos (com 2 núcleos, cerca de 15). A parte mais pesada roda num núcleo só, então a velocidade por núcleo importa mais que a quantidade. A placa de vídeo mínima é uma estimativa; relatos são bem-vindos.

### Informar um problema

Abra uma issue contando o que você fez e o que aconteceu, com o `stuntman.log` (ao lado do executável, gravado a cada abertura).
