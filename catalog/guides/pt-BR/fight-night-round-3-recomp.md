<!-- source: https://github.com/djultra64/Fight-Night-Round-3-Recomp/blob/HEAD/README.md | fingerprint: 22cac5657d4fa61c -->

[Baixe a versão 1.0.5](https://github.com/djultra64/Fight-Night-Round-3-Recomp/releases/tag/v1.0.5): escolha o arquivo para Linux ou o ZIP para Windows, extraia numa pasta nova e rode o launcher que vem junto.

```
./launch.sh
```

Windows (PowerShell):

```
.\launch.cmd
```

Os dois pacotes precisam do Python 3.12+ e de um processador x86-64-v2. O Linux precisa de placa/driver com Vulkan 1.3; o Windows precisa do Windows 10 ou 11 x64 e de placa/driver com Vulkan 1.3. Na primeira abertura, ele pede a sua própria ISO americana compatível, confere e prepara uma instalação particular antes de abrir o jogo. Reserve pelo menos 5 GiB livres, mais espaço para caches. Siga [instalação e primeira abertura](https://github.com/djultra64/Fight-Night-Round-3-Recomp/blob/HEAD/docs/beta/installation.md) para os requisitos e os hashes exatos aceitos. Para compilar a partir do código, use o [guia de compilação](https://github.com/djultra64/Fight-Night-Round-3-Recomp/blob/HEAD/docs/beta/building.md).

Para uma extração compatível que você já tenha, indique onde ela está:

```
./launch.sh --game-data-root "/absolute/path/to/game-data"
```

```
.\launch.cmd --game-data-root "C:\Games\FNR3-data"
```

Use o launcher que vem junto, para a preparação dos arquivos e o **SAVE AND RESTART** funcionarem direito. Saves, configurações, registros e caches ficam fora do pacote.
