<!-- source: https://github.com/96dot/Dragon-Age-Origins-Recomp/blob/HEAD/README.md | fingerprint: 0a73bd5ae8d662c0 -->

Você precisa de:

- Um PC x86-64 com Windows 10/11 ou Linux, placa de vídeo com Direct3D 12 e processador com AVX (a maioria de 2011 em diante). O jogo é montado para o seu processador: com AVX2 (Intel de 2013, AMD de 2015 em diante), ele usa essas instruções mais novas. Uma cópia montada assim e levada para um PC sem elas avisa ao abrir; rode o instalador nesse PC para montar para o processador dele.

- A imagem de disco do Dragon Age: Origins de Xbox 360 (title ID 454108C0), da sua própria cópia: a edição em inglês, italiano e espanhol (media ID 186A1115), que também é o disco 1 da Ultimate Edition dessa edição. O instalador confere se é a versão aceita (as edições francesa e alemã são outros discos, não aceitos). O idioma do jogo é escolhido no launcher (Configure → Advanced → Game language, ou `language` na seção `[game]` do `userdata/config.toml`).

- Cerca de 25 GB livres: 8 GB para o jogo, o resto para as ferramentas e a montagem. A primeira montagem leva de 30 a 60 minutos, conforme o processador.

### Windows

Baixe o **DAORecomp.exe** na página de versões e abra: ele abre na página Install, com o visual do launcher (com a tela de título do seu disco depois que você o escolher). Escolha a ISO, os seus conteúdos para download se tiver (veja abaixo) e uma pasta de instalação, aceite a licença das ferramentas de compilação e clique em **Install**. Não precisa instalar mais nada antes: ele baixa o que precisa (inclusive um git portátil se você não tiver). Ele monta o jogo na pasta de instalação como um novo `DAORecomp.exe`, que então abre como launcher: **Play** abre o jogo. Ele também pode criar atalhos no menu Iniciar e na Área de Trabalho e adicionar o jogo à Steam.

A imagem do disco e a pasta podem ser passadas direto (ou a ISO arrastada para ele):

```
DAORecomp.exe --iso "D:\Games\Dragon Age Origins.iso" --dest "D:\Games\DAORecomp"
```

As ferramentas e a montagem vão para `%LOCALAPPDATA%\DAORecomp-build` (defina `DAO_WORK_DIR` para usar outra pasta). Mantenha-a, e um instalador mais novo só remonta o que mudou; apague para recuperar o espaço.

O Windows pode avisar sobre o `DAORecomp.exe` porque ele não tem assinatura digital (Mais informações → Executar assim mesmo).

### Atualizações do título e conteúdo para download

Awakening, Leliana's Song, The Darkspawn Chronicles, The Golems of Amgarrak e Witch Hunt também precisam da Title Update 6 ou 7 do jogo (as outras DLCs funcionam sem ela).

Onde ficam:

- **Ultimate Edition:** tudo está no disco 2, inclusive a Title Update 6. Use a imagem dele como está: não há nada para extrair.

- **De um console:** copie as DLCs da pasta `Content\0000000000000000\454108C0\00000002\` do disco rígido, e a atualização de `Content\0000000000000000\454108C0\000B0000\`.

Cada DLC é um arquivo com um nome longo de letras e números e sem extensão, por exemplo `35F2AA65A0CDDB099F21E3280F7CE1054B5E08024` (essa é a Leliana's Song). É normal; não renomeie nem acrescente extensão.

**Na instalação (o mais fácil):** no instalador, escolha em "Downloadable content and title update": a imagem do disco 2, ou os arquivos copiados de um console (File...), ou a pasta com eles (Folder...). Também dá para arrastar para a janela. As DLCs são instaladas antes, e a atualização é montada junto com o jogo.

**Depois:**

1. Abra o launcher, vá em Downloadable Content e arraste a imagem do disco 2, ou os arquivos, ou a pasta com eles, para a janela. Ele mostra cada um conforme instala.

2. Se você acrescentou uma atualização do título pela primeira vez, rode o instalador (`DAORecomp.exe` da página de versões) de novo na mesma pasta. O jogo é remontado uma vez para a atualização, o que leva quase o mesmo tempo da primeira montagem. Ir da 6 para a 7 depois não precisa disso.

Detalhes:

- A atualização precisa ser a do disco em inglês, italiano e espanhol (media ID 186A1115); no disco 2 da Ultimate, é o `ESI\tu00000001_00000000`. As das edições francesa e alemã são ignoradas. A 7, a última, acrescenta correções à 6 e a substitui.

- As DLCs são extraídas para `userdata/content/454108C0/00000002/`, onde o jogo as encontra. Os arquivos que você arrasta ficam como estão.

### Atualizar

O launcher procura uma versão mais nova ao abrir e, se houver, mostra um item **Update** com as notas da versão. Ele baixa o `DAORecomp.exe` da versão nova para `update/` na pasta de instalação, confere com o SHA-256 que o GitHub lista e roda na mesma pasta; ele remonta só o que mudou (a imagem do disco não é necessária de novo) e mantém saves, configurações e mods.

Há dois canais de atualização, escolhidos em Configure → Advanced (ou com `update_channel = "release"` ou `"dev"` em `[launcher]`):

- **Release** (o padrão): só versões oficiais.

- **Dev**: também as versões de desenvolvimento, de teste, publicadas entre as oficiais. Podem ter erros.

A conferência só envia a versão do programa ao GitHub. Desligue em Configure → Advanced ou com `check_updates = false` na seção `[launcher]` do `userdata/config.toml`. Para atualizar à mão, rode o `DAORecomp.exe` de uma versão mais nova e escolha a mesma pasta de instalação (ou passe `--dest`).

### O que ele baixa

Tudo tem versão fixa e é conferido: códigos de terceiros (bibliotecas do Xenia, XenonRecomp, SDL 3, AMD FidelityFX SDK, Intel XeSS SDK e partes do NVIDIA Streamline SDK com DLSS e Reflex), LLVM/Clang 22 (ou o LLVM 18+ do sistema), CMake e Ninja, MinGit no Windows sem git, e os cabeçalhos e bibliotecas do runtime C/C++ da Microsoft e o Windows SDK, buscados pelo [xwin](https://github.com/Jake-Shadle/xwin), sob a [licença da Microsoft](https://go.microsoft.com/fwlink/?LinkId=2086102), que você precisa aceitar. Não precisa instalar o Visual Studio.

A pasta de instalação recebe os arquivos do jogo (`game/`), o programa, `userdata/` (configurações, saves, cópias dos saves, DLCs instaladas, registros, cache de shaders), `mods/` e, com a atualização do título, `title_update/`.

Quando você salva por cima de um save, a versão substituída fica em `userdata/save_backups/`: uma por save, a anterior à última sobrescrita. Se a cópia não puder ser feita (disco quase cheio, por exemplo), o save não é sobrescrito: uma mensagem no jogo avisa que não salvou, e o antigo fica. Fechar a janela durante um salvamento espera (no máximo 10 segundos) o save terminar. Para recuperar um, abra Save Backups no launcher: ele lista por personagem, com as datas. Restore coloca a cópia de volta (depois de perguntar, e não com o jogo aberto), e o save substituído vira a cópia.

### Jogar

- **Windows:** abra o `DAORecomp.exe` na pasta de instalação. O launcher abre antes (segure Shift ao abrir para mostrá-lo quando estiver desligado). Ele é desenhado com a tela de título e as artes de menu da sua cópia do jogo (nada disso vem junto) e funciona com mouse, teclado ou controles que o SDL conhece (Xbox, DualSense, DualShock...): direcional ou analógico esquerdo movem, A/X escolhe, B/Círculo volta, LB/RB (L1/R1) trocam as abas de Configure, o analógico direito rola, Start joga. `--launcher` mostra o launcher, `--mods` abre na página Mods e `--play` pula o launcher.

Configure → Advanced → Add to Steam adiciona o jogo à Steam como jogo de fora da Steam, para cada conta Steam do PC, com artes da biblioteca feitas a partir da tela de título. A Steam o abre com `--play`, direto no jogo (para o Big Picture). A Steam precisa estar fechada para isso: o launcher pergunta antes e a abre de novo depois. O `shortcuts.vdf` antigo fica guardado ao lado como `shortcuts.vdf.daorecomp-backup`.

- Steam (no Linux): Adicionar jogo de fora da Steam → `DAORecomp.exe` e, nas propriedades, force uma versão do Proton (o Proton traz o vkd3d-proton para o Direct3D 12).

- Wine: instale o vkd3d-proton no prefixo (`winetricks vkd3d`) e rode `./run.sh`.

A maioria das configurações gráficas também muda no jogo: Options → Graphics. A maioria vale na hora; ao sair do menu, o jogo lista as que valem só na próxima abertura. Desligar as sombras dinâmicas e o pós-processamento ali (ou no launcher) deixa o jogo mais rápido em aparelhos fracos como o Steam Deck. O desfoque de movimento vem desligado; Low e High custam tempo da placa de vídeo enquanto a cena se move. A profundidade de campo nas conversas vem ligada e só custa tempo em conversas e cenas. Com o upscaling temporal desligado, o anti-aliasing SMAA é mais rápido que o MSAA 2x do jogo, mas pode deixar detalhes finos tremerem mais em movimento. F11 alterna entre tela cheia e janela até o jogo fechar; mudar Fullscreen na aba Graphics salva a escolha, o F11 sozinho não.
