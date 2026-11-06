<!-- source: https://github.com/TekRantGaming/king-kong-recompiled/blob/HEAD/README.md | fingerprint: 5463a984bfc396bd -->

**Você precisa de:** Windows 10 ou 11 (64 bits) ou Linux (x86 64 bits), uma placa de vídeo com DirectX 12 ou Vulkan, cerca de 7 GB livres e uma imagem que você fez do seu próprio disco de Xbox 360 do Peter Jackson's King Kong (veja abaixo qual versão).

1. Baixe o arquivo **KingKong-...-windows-x64.zip** na [versão mais recente](https://github.com/TekRantGaming/king-kong-recompiled/releases/latest) e extraia onde quiser. No Linux, veja a seção Linux e Steam Deck do repositório.

2. Abra o **king_kong.exe**. O launcher abre na primeira configuração.

3. Escolha **Select disc image** e indique a imagem do seu disco do King Kong. O launcher confere, copia os arquivos do jogo (cerca de 6,3 GB) para uma pasta `game` ao lado do exe e tira dali as artes.

4. Os gráficos são preparados uma vez, com uma barra de progresso. Depois, a configuração oferece **Auto detect**, o pacote de texturas HD e as cenas do PS2, e guia você pelas opções gráficas e pelos ícones dos botões.

5. Clique em **PLAY**.

O download traz só este port. **Nenhum arquivo do jogo vem junto**: eles vêm do seu próprio disco. Saves, configurações e downloads ficam todos dentro da pasta do King Kong, em `userdata`, então dá para copiar a pasta inteira para outro disco ou PC. Se você jogou uma versão anterior, os seus dados são tirados de `Documentos\king_kong` na primeira abertura desta. O port também aparece no [Quiver Launcher](https://quiverlauncher.com/apps/peter-jackson-s-king-kong-the-official-game-of-the-movie-king-kong-pc-port), onde dá para ver como roda para outros jogadores.

### Qual versão você precisa

O port foi feito só para este disco. A versão de PC de 2005 e discos de outras plataformas não funcionam.

- Jogo · Peter Jackson's King Kong: The Official Game of the Movie

- Disco · Xbox 360, EUA e Europa (inglês, francês, alemão, espanhol, italiano, holandês, sueco, norueguês, dinamarquês, finlandês)

- Title ID · `555307D3`

- Versão do jogo · `0.0.0.1` (o lançamento original em disco)

- Imagem do disco · `.iso`, 7.834.892.288 bytes

- SHA-1 · `075F43709E9C9A095E099AB5056CC04B768288D1`

- MD5 · `35D671ED4E9EAD9E6B288A3441B6C8F9`

O launcher confere o title ID antes de instalar. Para conferir a sua imagem você mesmo, rode isto no PowerShell e compare com o SHA-1 acima:

```
Get-FileHash "C:\Games\King Kong.iso" -Algorithm SHA1
```

O site do port explica como fazer a imagem do seu próprio disco. O projeto não fornece, não indica e não ajuda a encontrar arquivos de jogos.
