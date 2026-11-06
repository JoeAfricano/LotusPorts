<!-- source: https://github.com/SilverwireGames/ReChan/blob/HEAD/README.md | fingerprint: 5271b42b301754d3 -->

Os dados do jogo **não vêm junto**. Coloque uma imagem NTSC-U do jogo, da sua própria cópia obtida legalmente (`.bin` ou `.iso`), numa pasta `discimage` ao lado do executável:

```
rechan/
|-- rechan.exe       # Windows (use "rechan" on Linux)
`-- discimage/
    `-- game.bin     # or game.iso, file name doesn't matter
```

Deixe só uma imagem de disco na `discimage`. Na primeira abertura, o ReChan confere a versão do disco e usa o extrator embutido para preparar os arquivos necessários. Não precisa de outro programa de extração. Depois que os arquivos forem extraídos, a pasta da imagem pode ser apagada.

Hoje o ReChan funciona em Windows e Linux 64 bits x86 (`x86_64`). Para compilar por conta própria, veja o [guia de compilação](https://github.com/SilverwireGames/ReChan/blob/HEAD/docs/BUILDING.md).
