<!-- source: https://github.com/Spark198rus/black-pc/blob/HEAD/README.md | fingerprint: 6b3353f028d0ab72 -->

1. Copie a sua versão NTSC-U ou PAL europeia do jogo para uma ISO e extraia com o [extract-xiso](https://github.com/XboxDev/extract-xiso) (`extract-xiso -x Black.iso`). Renomeie a pasta extraída para `game`.

2. Confira se você tem a versão aceita. No PowerShell:

```
   Get-FileHash game\default.xbe -Algorithm SHA256
```

O resultado precisa ser um destes:

- NTSC-U: `DF2739C372D254A90AEECCF5971D12097F22D89FBB90DB021FDA96DD4DEB68F6`

- PAL Europa: `67F35121CCF622E7A293C402219596FE6456CA4C3D27E6F8139E449264DA30F0`

3. Baixe o zip na página de [versões (Releases)](https://github.com/Spark198rus/black-pc/releases) e extraia ao lado da pasta `game`, para a pasta `port` ficar do lado dela:

```
   BLACK\
     game\
       default.xbe
       ...
     port\
       black.exe
       blackrt.dll
```

4. Abra o `port\black.exe`.

O port guarda tudo o que grava na pasta `port`: `savedata\` tem o disco rígido do Xbox (saves e configurações do jogo), `black.ini` as configurações do port, `black.log` o registro da última vez e `cache\menus` os arquivos de menu do jogo com as páginas PC SETTINGS acrescentadas.

### Fase 2: instalar o WineHQ 9+ (Staging) no Linux

O Wine 9 moderno elimina erros crônicos de shaders das versões antigas.

1. Ative a arquitetura de 32 bits:

```
   sudo dpkg --add-architecture i386
```

2. Baixe as chaves oficiais e adicione o repositório do Ubuntu Jammy:

```
   sudo mkdir -pm755 /etc/apt/keyrings
   sudo wget -O /etc/apt/keyrings/winehq-archive.key https://dl.winehq.org/wine-builds/winehq.key
   sudo wget -NP /etc/apt/sources.list.d/ https://dl.winehq.org/wine-builds/ubuntu/dists/jammy/winehq-jammy.sources
```

3. Atualize os pacotes e instale o WineHQ Staging:

```
   sudo apt update
   sudo apt install --install-recommends winehq-staging -y
```
