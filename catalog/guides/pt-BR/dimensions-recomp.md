<!-- source: https://github.com/NeverCookFirst/DimensionsRecomp/blob/HEAD/README.md | fingerprint: 71dfaf65c5bde777 -->

**Nenhum dado do jogo vem aqui.** Você traz o seu, e o instalador confere se é o certo antes de fazer qualquer coisa.

Tenha em mãos:

- **O disco de Xbox 360, copiado e extraído.** Copie o seu disco para uma ISO e extraia essa ISO numa pasta com uma ferramenta como o [extract-xiso](https://github.com/XboxDev/extract-xiso) ou o Xbox Image Browser. O instalador quer **a pasta, não a ISO**. Ela está certa quando tem o `Default.xex` junto com o `GAME.DAT` e o `GAME.HDR`. Só o lançamento original funciona, e o instalador avisa se o seu for outra versão.

- **Title Update 23. Nem 24, nem 22.** O recomp foi feito a partir do executável da TU23 e não roda com outra, então esta é conferida pelo hash e uma atualização errada é recusada. Indique o arquivo de pacote bruto, chamado `tu00000003_00000000` e sem extensão, ou uma pasta com `Default.xexp`, `PATCH.DAT` e `PATCH.HDR`.

- **DLC, opcional.** Indique ao instalador uma pasta com os seus pacotes de DLC e ele pega todos de uma vez. Comprou um pacote depois? Rode o instalador de novo e escolha **"Add DLC to an install I already have"** na primeira tela: ele pede só os pacotes e a pasta da instalação, não grava mais nada e não mexe nos seus saves, configurações e mods.

Depois, é só rodar o [DimensionsRecompiled-Setup.exe](https://github.com/NeverCookFirst/DimensionsRecomp/releases/latest) e seguir o assistente. Ele grava um `README.txt` ao lado do jogo com os atalhos, as configurações e onde ficam os saves.

As atualizações são feitas pelo jogo. Desligue em `F4` → Online → **Check for updates at start**.
