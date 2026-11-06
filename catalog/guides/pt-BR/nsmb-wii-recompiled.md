<!-- source: https://github.com/WitherBlitz/NSMB-Wii-Multiplayer-Recompilation/blob/HEAD/README.md | fingerprint: 7c84fe7b1627fd7c -->

Baixe na página de [versões (Releases)](https://github.com/WitherBlitz/NSMB-Wii-Multiplayer-Recompilation/releases/latest):

- **Windows:** `NSMBW-0.4.0-windows-x64.zip`. Extraia onde quiser e abra o `NSMBW.exe`. As configurações (inclusive onde estão os arquivos do seu jogo), os saves e os caches ficam em `Documentos\MarioWiiSaveData`, então uma versão nova, extraída em qualquer lugar, continua de onde a anterior parou. A primeira abertura da 0.0.8 ou mais nova assume os dados de uma versão antiga extraída na mesma pasta (ou da versão que ela substitui). Num processador sem AVX2 (anterior ao Intel Haswell ou AMD Excavator), o `NSMBW.exe` abre sozinho o `compat\NSMBW-compat.exe`: o mesmo jogo, com as contas feitas do jeito longo, que continua jogando em rede local com todo mundo. Mantenha a pasta `compat` ao lado do `NSMBW.exe`.

- **Newer Super Mario Bros. Wii:** no Android, abra a aba Newer no menu inicial, baixe o módulo de suporte ali e importe a sua própria cópia do Newer v1.30 (USA, SMNE03). No PC e no Linux, por enquanto ele é compilado a partir do código: veja [projects/nsmbw/BUILDING.md](https://github.com/WitherBlitz/NSMB-Wii-Multiplayer-Recompilation/blob/HEAD/projects/nsmbw/BUILDING.md).

- **Outro jogo, como um mod:** os saves são guardados por jogo. Extraia uma segunda cópia da versão e coloque a pasta extraída desse jogo ao lado do `NSMBW.exe` dela, com o nome `game`. Essa cópia roda o jogo com os próprios saves, e a primeira continua com o original.

> Só use versões da página de [versões (Releases)](https://github.com/WitherBlitz/NSMB-Wii-Multiplayer-Recompilation/releases) deste repositório. Se alguém estiver compartilhando um APK ou zip em outro lugar, não use.
