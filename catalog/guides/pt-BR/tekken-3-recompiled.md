<!-- source: https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/README.md | fingerprint: 0bbfe1bb72ff25d9 -->

Baixe o `Tekken3Recompiled-v0.1.4-Easy-Setup.zip` na [versão v0.1.4](https://github.com/FishB0nes98/Tekken3Recompiled/releases/tag/v0.1.4), extraia o ZIP inteiro numa pasta onde dê para gravar e abra o `Play Tekken 3.exe`. Escolha a imagem CUE/BIN do seu Tekken 3 americano de PlayStation e clique em **Set up & play**. A Jun também precisa dos conjuntos de arcade não mesclados `tektagt.zip` e `tekken3.zip` aceitos. A primeira configuração baixa ferramentas de compilação em versões fixas e prepara o jogo no seu PC; nas próximas aberturas, abre o launcher nativo com Mods, controles, biblioteca de discos e Patches & Updates. É preciso pelo menos 4 GB livres de espaço de trabalho.

Quem tem a v0.1.2 precisa extrair este pacote completo uma vez. Quem está na v0.1.3 com a configuração completa pode atualizar pelo launcher. Se a configuração da Jun falhou na v0.1.3, extraia a v0.1.4 por cima da mesma pasta e tente de novo. A partir da v0.1.4, o launcher instala versões completas mais novas no lugar. Veja o [RELEASE-NOTES.md](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/RELEASE-NOTES.md) para as mudanças e o [LAUNCHER.md](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/LAUNCHER.md) para o fluxo de atualização.

Port nativo e pesquisa de recompilação estática do **Tekken 3 (USA, SLUS-00402)**, feitos sobre o [psxrecomp](https://github.com/mstan/psxrecomp) e o [recomp-ui](https://github.com/mstan/recomp-ui).

O repositório tem dois produtos para a versão americana de PlayStation:

- `Tekken_3_PC_Port.exe`: o port nativo independente, em `native/`;

- `Tekken_3_Recompiled.exe`: a recompilação híbrida, mantida como referência de comportamento e imagem enquanto os sistemas são migrados.

Os dois precisam de uma imagem de disco de uma cópia legal. O port nativo é um recorte jogável inicial, ainda sem todo o conteúdo.

### Começo rápido (desenvolvimento)

Para o jogo recompilado, abra o `Play-Tekken3-Recompiled.cmd`. O launcher nativo novo sempre aparece primeiro, com mods, mapeamento separado de teclado e controle e uma biblioteca local de discos. Veja o [LAUNCHER.md](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/LAUNCHER.md).

Para o recorte do port nativo, compile uma vez e depois clique duas vezes no [Play-Tekken3-PC-Port.cmd](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/Play-Tekken3-PC-Port.cmd):

```
.\scripts\dev.ps1 native-build
.\scripts\dev.ps1 native-test
.\scripts\dev.ps1 native-capture
```

O renderizador nativo usa a proporção real da janela: o campo de visão vertical fica fixo e o 16:9 mostra mais do cenário na horizontal. Veja o [native/README.md](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/native/README.md) para controles e arquitetura.

Para a referência híbrida já compilada, clique duas vezes no [Play-Tekken3-Recompiled.cmd](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/Play-Tekken3-Recompiled.cmd). Ele abre o executável nativo com o BIN/CUE conferido e nunca altera os arquivos do disco. Clique duas vezes no [Configure-Tekken3-Recompiled.cmd](https://github.com/FishB0nes98/Tekken3Recompiled/blob/HEAD/Configure-Tekken3-Recompiled.cmd) para abrir antes as configurações gráficas e o gerenciador de Mods.

O mod **True Widescreen** da referência híbrida vem ligado. Ele mostra mais cenário na horizontal e dá layouts 16:9 nativos ao menu principal, às seleções de personagem e às telas de carregamento do Arcade, Team Battle e Tekken Force. Retratos e textos mantêm as proporções. Menus e vídeos não reconhecidos continuam 4:3. A opção **Wider fight framing (experimental)** também vem ligada: as lutas comuns podem usar a visão mais larga antes da câmera se afastar, com a visão de perto, o movimento e a colisão originais. Desligue essa opção para comparar a câmera original, ou desligue **True 16:9 Widescreen** para voltar à imagem 4:3 original.

O menu **Options → Sounds** do jogo tem controles separados de **Music Volume** e **SFX Volume** (0–100, de 5 em 5). Esquerda/direita testam a mudança; escolha **Save** para guardar. **Back** descarta mudanças não salvas. As preferências ficam no `tekken3-sound.ini` da pasta de saves ativa.

Neste ambiente Windows, o script repetível confere o disco, gera de novo o código do jogo, compila, roda os testes e confere as sobreposições capturadas:

```
.\scripts\dev.ps1 all
.\scripts\dev.ps1 run
```

O fluxo híbrido de mais baixo nível é:

```
git submodule update --init --recursive
./psxrecomp/tools/ci/build_emitters.sh
python3 psxrecomp/psxrecomp_cli.py generate \
  --config game.toml --project-root . --disc disc/<your>.cue
cmake -S . -B build-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build build-release --target psx-runtime
```

Prefixo do zip dos artefatos de CI: `tekken3`.
