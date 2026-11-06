<!-- source: https://github.com/Yokimitsuro/khdays-port/blob/HEAD/README.md | fingerprint: 05bdaae2dc46eca8 -->

No Windows, o instalador da página de versões é tudo o que você precisa além da sua própria cópia do jogo: ele pede o seu arquivo do jogo (a versão europeia ou norte-americana do KINGDOM HEARTS 358/2 Days; ele encontra sozinho onde o KH Melon Mix guarda uma, na sua pasta do KINGDOM HEARTS HD 1.5+2.5 ReMIX), prepara os dados do jogo a partir dele e também pode fazer o pacote HD a partir das suas coleções HD, adicionar o port à Steam (PC e Steam Deck) e instalar o estúdio de mods. Nada do jogo vem junto. O port avisa quando sai uma versão nova, e o Download dele atualiza a sua cópia sozinho: baixa o instalador novo, confere, fecha o jogo, atualiza com as suas escolhas (o Windows pede permissão de administrador) e abre de novo. Mais: [installer/README.md](https://github.com/Yokimitsuro/khdays-port/blob/HEAD/installer/README.md).

### Começando

Para montar o port a partir do código:

1. Confira e extraia a sua cópia do jogo (Python 3.11+):

```
   py .\tools\verify_rom.py "E:\path\game.nds"
   py .\tools\extract_data.py "E:\path\game.nds"
```

Os dados vão para `data/extracted/<hash>/`, que nunca vai para o repositório.

2. Compile (Windows, Visual Studio 2026 com a carga de trabalho C++, CMake, Ninja e, para a preparação, o `pycparser` do Python):

```
   py -m pip install pycparser
   py .\port\build.py --config Release
```

O `port/tools/prepare.py` gera os códigos da compilação a partir da decompilação (o C do jogo reescrito com cada ponteiro como referência, em `build/port/gen-x64`); o executável é `build/port/obj-release-x64/khdays-native.exe` (`--config Debug`: `build/port/obj-x64/`).

3. Rode a partir da raiz do repositório, para ele encontrar `data/` e guardar o save em `saves/khdays.sav`:

```
   .\build\port\obj-release-x64\khdays-native.exe 2> khdays.log
```

O programa escreve o registro no stderr; o `2>` guarda para um relato de erro. Aberto sem os dados (ou com eles incompletos), ele avisa numa caixa de mensagem no idioma do sistema, com a pasta onde procurou.

O save é a memória de backup do DS num arquivo, no mesmo formato de um save de DS (64 KB brutos), então um save do DS carrega. Ele é gravado inteiro a cada salvamento (um arquivo novo ao lado, que depois toma o lugar), então uma falha do jogo ou do PC durante o salvamento deixa o save como estava antes ou como fica depois, nunca pela metade.

4. Quando algo parecer errado (música atrasada, o jogo parando por um momento), o registro da sessão diz o que o jogo fazia: a versão Debug grava um por sessão em `saves/logs/` (na Release, ligue **Session log** nas configurações, ou `KHDAYS_LOG=1`): as cenas, os arquivos lidos e quanto tempo levaram, quando cada música foi pedida e quando tocou, e cada trecho de mais de 50 ms sem quadro. Aperte **F12** quando acontecer: o registro ganha uma marca ali, e o que as duas telas do DS mostram nesse momento é salvo em `saves/marks/` (`mark_<n>_frame_<frame>.bmp`). Envie o registro e as imagens com o relato de erro.
