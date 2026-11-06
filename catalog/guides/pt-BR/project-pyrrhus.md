<!-- source: https://github.com/undecuple/project-pyrrhus/blob/HEAD/README.md | fingerprint: b689ff80be86f0c8 -->

**Você precisa de:**

- **O seu disco do Killzone 2**, copiado como pasta ou como `.iso` descriptografado: americano (BCUS98116, testado) ou europeu (BCES00081), asiático (BCAS20066) ou japonês (BCJS30032), que são aceitos mas não testados: o código é o mesmo, os dados mudam, e ninguém jogou até o fim ainda ([docs/GETTING-FILES.md](https://github.com/undecuple/project-pyrrhus/blob/HEAD/docs/GETTING-FILES.md#regions)).

- **A atualização 1.29 da mesma região** da sua cópia (no PS3 ou no RPCS3: a pasta `dev_hdd0/game/<title id>`, como `dev_hdd0/game/BCUS98116`).

- **O EBOOT 1.29 descriptografado** (`EBOOT.elf`), feito a partir do `EBOOT.BIN` dessa atualização.

- Windows 10 ou 11 (64 bits) com um processador com AVX e FMA3 (de 2013 em diante, mais ou menos), 16 GB de RAM recomendados para a montagem e uma placa de vídeo com Direct3D 12: [docs/requirements.md](https://github.com/undecuple/project-pyrrhus/blob/HEAD/docs/requirements.md).

Nenhum emulador roda enquanto você joga. Se você joga no emulador de PS3 RPCS3, a instalação encontra os três itens na sua instalação do RPCS3 ("Use my RPCS3 install"). O que cada um precisa ter: [docs/GETTING-FILES.md](https://github.com/undecuple/project-pyrrhus/blob/HEAD/docs/GETTING-FILES.md). Este projeto não fornece nenhum deles e não dá passos de descriptografia.

1. Baixe o instalador para Windows (recomendado) ou o zip portátil da [v0.1.11](https://github.com/undecuple/project-pyrrhus/releases/tag/v0.1.11) na [página de versões](https://github.com/undecuple/project-pyrrhus/releases) (por enquanto, toda versão é de teste).

2. Abra e indique a cópia do seu disco, a atualização 1.29 e o EBOOT descriptografado.

3. Ele traduz e monta o código do jogo no seu PC; nenhum código, arquivo ou chave do jogo é enviado ou baixado.

4. Abra o jogo pelo launcher.

Não instale numa pasta do OneDrive. Os arquivos de configuração (`keybinds.ini`, `gameplay.ini`; este é criado com os padrões na primeira abertura, e um existente nunca é mudado) ficam em `%APPDATA%\Project Pyrrhus`, ou ao lado do `kz2.exe` no zip portátil. Mais: [docs/INSTALL.md](https://github.com/undecuple/project-pyrrhus/blob/HEAD/docs/INSTALL.md), [CHANGELOG.md](https://github.com/undecuple/project-pyrrhus/blob/HEAD/CHANGELOG.md).
