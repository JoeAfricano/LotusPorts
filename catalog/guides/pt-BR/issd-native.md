<!-- source: https://github.com/sergiomanzur/issd-native/blob/HEAD/README.md | fingerprint: 22c6b49f270e6a21 -->

O arquivo do jogo não vem junto. Use a cópia do seu próprio cartucho que bata com a revisão aceita:

- Propriedade · Valor esperado

- Jogo · International Superstar Soccer Deluxe (USA)

- Título interno · `SUPERSTAR SOCCER 2`

- Formato · `.sfc` sem cabeçalho, 2.097.152 bytes

- MD5 · `345ddedcd63412b9373dabb67c11fc05`

- SHA-256 · `d2fe66c1ce66c65ce14e478c94be2e616f9e2cad374b5783a6a64d3c1a99cfa9`

Confira uma cópia com:

```
python tools/verify_rom.py "path/to/International Superstar Soccer Deluxe (USA).sfc"
```

No Windows, extraia o pacote da versão e abra o `ISSDNative.exe`, ou abra a sua compilação:

```
.\build\ISSDNative.exe --rom "path/to/International Superstar Soccer Deluxe (USA).sfc" --mods-dir mods
```

Mantenha o `aot_boot_deny.txt` ao lado do executável; ele traz ajustes de abertura que o programa precisa. O Windows também precisa do `SDL2.dll` correspondente ao lado do executável ou no `PATH`. Inclua a pasta `mods/` para usar os pacotes que vêm junto.

Se nenhum arquivo do jogo válido estiver configurado, o launcher abre um seletor de arquivo. O Linux precisa do `zenity` ou do `kdialog` para esse seletor; `--rom` funciona sem eles. O Android usa o seletor do sistema para importar o arquivo e os mods.

As configurações ficam no `issd_native.cfg`. Por padrão, em `%APPDATA%/ISSDNative` no Windows, `$XDG_CONFIG_HOME/ISSDNative` ou `~/.config/ISSDNative` no Linux e no armazenamento do app no Android. Um `issd_native.cfg` ou `issd_config.json` na pasta de trabalho tem prioridade. Use `--config <path>` para escolher um arquivo. Uma configuração mínima:

```
rom_path=path/to/International Superstar Soccer Deluxe (USA).sfc
mods_dir=mods
```

Os saves ficam em `%LOCALAPPDATA%/ISSDNative/saves` no Windows, `$XDG_DATA_HOME/issd-native/saves` ou `~/.local/share/issd-native/saves` no Linux e no armazenamento interno do app no Android. Use `--save-dir <directory>` para saves portáteis. Arquivos antigos em `saves/` na pasta de trabalho continuam disponíveis carregando à mão, com um aviso de compatibilidade.
