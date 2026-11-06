<!-- source: https://github.com/wipeout-phantom-edition/wipeout-phantom-edition/blob/HEAD/README.md | fingerprint: efbd3df9e6744119 -->

> **Resumo**: baixe a [versão mais recente](https://github.com/wipeout-phantom-edition/wipeout-phantom-edition/releases/latest), coloque os arquivos `.bin` e `.cue` da sua versão americana de PlayStation em `wipeout/diskimages` e abra o jogo.

Baixe a [versão mais recente](https://github.com/wipeout-phantom-edition/wipeout-phantom-edition/releases/latest) e descompacte a pasta `wipeout` onde quiser no seu disco.

Você precisa dos dados do jogo da versão americana original de Wipeout para PlayStation. Dá para fornecer os arquivos à mão ou usar a imagem do disco em bin/cue, feita a partir de um disco que você tem. O método da imagem do disco é o preferido, porque extrai as músicas em arquivos wav automaticamente.

**IMPORTANTE:** os dados precisam ser da versão **americana de PlayStation** de Wipeout. Os dados das versões oficiais de PC não funcionam.

### Método da imagem do disco

- **Coloque os arquivos da imagem**: abra a pasta `wipeout/diskimages` e coloque ali os arquivos da imagem do disco do Wipeout.

- **Confira o formato**: a imagem precisa estar em vários `.bin` com um `.cue`. São 9 arquivos `.bin` e um `.cue`. Exemplo:

```
  WipeOut USA (Track 1).bin
  WipeOut USA (Track 2).bin
  WipeOut USA (Track 3).bin
  WipeOut USA (Track 4).bin
  WipeOut USA (Track 5).bin
  WipeOut USA (Track 6).bin
  WipeOut USA (Track 7).bin
  WipeOut USA (Track 8).bin
  WipeOut USA (Track 9).bin
  WipeOut USA.cue
```

Os dados do jogo devem estar no formato "MODE2/2352" na primeira faixa do cue; as outras faixas, no formato "AUDIO".

**Extração ao abrir**: ao abrir, o jogo confere se faltam arquivos de dados e tenta extraí-los da imagem do disco.

**Pode apagar a imagem depois**: quando o jogo chegar uma vez ao menu principal, os arquivos da imagem do disco não são mais necessários e podem ser removidos.

**OBSERVAÇÃO: como ler o sistema de arquivos da imagem do disco não é simples, o jogo faz uma busca por hash na faixa de dados. Isso pode demorar em PCs com menos de 8 núcleos.**

### Método dos arquivos soltos

**Copiar os arquivos do jogo**: se você já tem todos os arquivos do jogo (517 no total), copie-os direto para a pasta `wipeout/wipeoutgame`. Eles podem ser copiados do disco de PlayStation pelo Explorador de Arquivos.

**Músicas**: a desvantagem deste método é que as músicas, gravadas como faixas de áudio no CD e não como arquivos, não podem ser copiadas assim.

Para as músicas, use o método da imagem do disco ou, se tiver os arquivos de música separados, coloque-os na pasta `wipeout/music`. Eles precisam seguir um padrão de nome, com números de 2 dígitos entre 01 e 32. Mais detalhes em `wipeout/music/musicgoeshere.txt`.
