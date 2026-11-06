<!-- source: https://github.com/mrOt4/babel-recomp/blob/HEAD/README.md | fingerprint: 540c02c01fb555f5 -->

Baixe o pacote do seu sistema na página de [versões (Releases)](https://github.com/mrOt4/babel-recomp/releases), ou compile você mesmo. Em todos os casos, os arquivos que você acrescenta vão **ao lado do executável**:

```
babel-recomp(.exe / .nro)
assets/  i18n/  mods/       (come with the package)
rom.sfc                     your USA ROM (any name works if you pick it)
languages/                  your European ROMs (optional)
msu1/                       your MSU-1 pack's .pcm files (optional)
shaders/                    RetroArch GLSL shaders (optional)
```

`assets/`, `i18n/` e `mods/` vêm no pacote; `rom.sfc` é o seu arquivo americano (qualquer nome serve se você escolher); `languages/` os seus arquivos europeus, `msu1/` os `.pcm` de um pacote MSU-1 e `shaders/` shaders GLSL do RetroArch (todos opcionais).

### Onde fica cada coisa

- Sistema · Pasta do jogo (arquivo do jogo, `languages/`, `msu1/`, `mods/`, `shaders/`, `saves/`, `logs/`, `config.ini`)

- Windows · a pasta extraída, ao lado do `babel-recomp.exe`

- 3DS · `sdmc:/3ds/babel-recomp/` (firmware DSP: `sdmc:/3ds/dspfirm.cdc`)

- Vita · `ux0:data/babel-recomp/` (o app em si se instala como `ux0:app/BABL00001`)

Dentro dela:

- Caminho · Conteúdo

- `rom.sfc` · o seu arquivo americano

- `languages/` · os seus arquivos europeus, com qualquer nome

- `msu1/` · os `.pcm` de um pacote MSU-1

- `mods/` · mods extraídos (uma pasta com `manifest.toml`) e o catálogo embutido

- `shaders/` · shaders GLSL do RetroArch (computador, Switch, Android)

- `saves/save.srm` · o save de bateria do jogo (compatível com o original)

- `saves/save0.sav` … · estados salvos, com prévias `.thumb`

- `logs/` · o registro de depuração e relatórios de falha

- `config.ini`, `keybinds.ini` · configurações e controles

- `env.txt` · só nos consoles, opcional: chaves de diagnóstico, uma `KEY=VALUE` por linha

No PC, se a pasta for somente leitura, os registros vão para a pasta de dados do usuário (`%APPDATA%\babel-recomp`, `~/.local/share/babel-recomp`, `~/Library/Application Support/babel-recomp`); a segunda linha do registro mostra o caminho.

### Windows

1. Extraia o `babel-recomp-<version>-windows-x86_64.zip` num lugar onde dê para gravar.

2. Abra o `babel-recomp.exe`. O launcher aparece: escolha o seu arquivo do jogo e clique em **Play**. (Ou coloque-o ao lado do executável com o nome `rom.sfc`.)

O executável é independente (sem DLLs). O SmartScreen pode avisar sobre um programa sem assinatura: Mais informações → Executar assim mesmo.

### Desempenho em portáteis

Todas as versões emulam o hardware do SNES com exatidão (os mesmos quadros e o mesmo som do PC, conferidos quadro a quadro). O 3DS e o Vita têm bem menos processador que um celular ou um Switch: eles pulam o desenho de quadros quando preciso para manter a velocidade e usam sozinhos as configurações mais rápidas. Medido num emulador, um New 3DS deve rodar o jogo a cerca de três quartos da velocidade total; relatos de aparelhos reais são bem-vindos (ligue o contador de FPS no menu de configurações).
