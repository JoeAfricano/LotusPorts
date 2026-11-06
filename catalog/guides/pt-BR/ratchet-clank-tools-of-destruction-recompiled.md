<!-- source: https://github.com/Re-TeRo17/ratchet-tod-recomp/blob/HEAD/README.md | fingerprint: c16647a08446e935 -->

- **Lista de espaços de save (1.1):** salvar e carregar mostram a lista de espaços do jogo, então você escolhe o espaço. `TOD_SAVE_PICKER = 0` desliga a lista: aí carregar pega o save mais novo e um save manual cria um espaço novo.

- Os saves ficam ao lado do jogo, em `gamedata\dev_hdd0\home\00000001\savedata`, gravados pelo próprio código de save do jogo. F1 > System > **Saves folder** abre a pasta; `PS3_SAVEDATA_ROOT` no `tod.ini` muda o local. Uma pasta `BCES00052_SAVE_<n>` por save, a mesma organização do RPCS3, então copiar saves entre os dois deve funcionar (ainda não testado).

- O idioma do jogo (texto, vozes, legendas) pode ser mudado na página de configurações.

- O `Install.bat` / `tod_setup.exe` instala a partir de um `.iso` descriptografado ou de uma pasta do disco; o `Play.bat` abre o jogo e guarda os dois últimos registros (`log\tod.log`, `log\tod.previous.log`).

- Mostra o Discord Rich Presence (fase atual e tempo de jogo, com um botão do GitHub) enquanto o Discord estiver aberto; esconda nas configurações do Discord > Privacidade de atividade.

- Pasta de mods (ligada por padrão, `TOD_MODS = 0` desliga): um arquivo em `mods\`, ao lado do `tod.exe`, é lido no lugar do arquivo do jogo no mesmo caminho em `game\disc\`; por exemplo, `mods\PS3_GAME\USRDIR\built\movies\l01_s01_ntro_fmv\l01_s01_ntro_fmv.bik` substitui o vídeo de introdução (o nível `PS3_GAME\` é opcional). Só leitura: saves e arquivos do jogo nunca são tocados, e apagar o arquivo do mod volta ao original. Os mods são seus; o projeto não traz nenhum.

### Instalar

1. Baixe o `tod-recomp-1.1-win64.zip` na página de [versões (Releases)](https://github.com/Re-TeRo17/ratchet-tod-recomp/releases) e extraia onde quiser, **menos** em `Arquivos de Programas` (o jogo grava saves e cache de shaders ao lado do `tod.exe`). É preciso cerca de 24 GB livres nesse disco.

2. Rode o **`Install.bat`**. (Também dá para arrastar o `.iso` ou a pasta do disco para o `Install.bat`.)

- Escolha o seu `.iso` descriptografado (botão ".iso...") ou a pasta do disco ("Folder...").

- O instalador confere se é o disco certo e avisa em verde.

- Escolha o **idioma do jogo** (padrão inglês; dá para mudar depois no jogo com F1).

- Clique em **Install**. Ele copia o disco para a pasta `game` e cria o `elf\EBOOT.ELF` a partir do `EBOOT.BIN` do disco. Leva alguns minutos. Nada sai do seu PC.

3. Clique em **Play** (ou rode o **`Play.bat`** quando quiser).

O SmartScreen do Windows pode avisar na primeira vez porque o programa não tem assinatura: "Mais informações" e depois "Executar assim mesmo". Alguns antivírus marcam executáveis novos sem assinatura; o código-fonte é público se você quiser conferir ou compilar.

O instalador pode ser rodado de novo quando quiser (ele substitui só os arquivos do jogo que instalou). Com o jogo já instalado, ele também pode só trocar o idioma.

**Atualizar de uma versão antiga:**

A partir da 1.1, rode o **`Update.bat`** na pasta do jogo: ele confere o GitHub, pergunta antes de fazer qualquer coisa, baixa a versão nova e substitui só os arquivos do port. Para atualizar à mão (e para ir da 1.0 para a 1.1, já que a 1.0 não tem `Update.bat`):

1. Feche o jogo.

2. Abra o zip da 1.1. Dentro há uma pasta `tod-recomp-1.1`: copie **tudo o que está dentro dela** para a pasta do seu jogo (a que tem o `tod.exe`). Quando o Windows perguntar, escolha **Substituir os arquivos no destino**.

3. Abra o jogo pelo `Play.bat` como sempre.

Você **não** precisa rodar o `Install.bat` de novo: o jogo instalado (`game\`, `elf\`) fica como está. As suas configurações (`tod.ini`), saves (`gamedata\`) e cache de shaders (`cache\`) são mantidos, porque o download traz o `tod.default.ini` em vez do `tod.ini` e não tem saves nem cache.

O jogo avisa na tela de título e em F1 > System > Updates quando sai uma versão mais nova (`TOD_UPDATE_CHECK = 0` desliga a conferência; nada é baixado sozinho). Para guardar o jogo instalado (`game\disc`) em outro disco: feche o jogo, mova a pasta `game\disc` para lá e escreva o caminho novo no `tod.ini`, por exemplo `PS3_VFS_ROOT = D:\Games\ToD\disc`.

Pela linha de comando: `tod_setup.exe --install --base <folder> <iso or disc folder>` (e `tod_setup.exe --check <file>` para ver o que o instalador acha de um arquivo).

### O que fica onde

```
tod.exe, SDL2.dll, z.dll   the port (released with this download)
tod.default.ini            the default settings (from the download)
tod.ini                    your settings, made from tod.default.ini at the first start (also written by F1)
game\disc\                 a copy of your disc (PS3_GAME, PS3_DISC.SFB)   <- from your disc
elf\EBOOT.ELF              the decrypted game executable                  <- from your disc
cache\                     shader cache, built by the game while you play
gamedata\                  your saves
mods\                      optional, yours: files the game reads instead of its own (see Features)
log\tod.log                the log of the last run (Play.bat), tod.previous.log the one before
```

O `tod.exe` e as DLLs são o port; `game\disc\` é a cópia do seu disco e `elf\EBOOT.ELF` o executável descriptografado a partir dele; `cache\` é o cache de shaders, `gamedata\` os seus saves, `mods\` os seus mods opcionais e `log\` os registros.

O executável do jogo é descriptografado no seu PC a partir do seu próprio disco. Este download não traz nenhuma parte do jogo: nem executável, nem dados, nem shaders, nem saves.
