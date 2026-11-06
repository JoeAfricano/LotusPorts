<!-- source: https://github.com/mateussmeneses/theorder1886-port/blob/HEAD/README.md | fingerprint: 8c11b7241d849693 -->

A pasta do jogo vem de `THEORDER_GAME_DIR`; sem ela, o `run.py` procura uma pasta `CUSA00035` ao lado da pasta do projeto.

```
$env:THEORDER_GAME_DIR = 'D:\path\CUSA00035'
python run.py
```

### Opções do motor do jogo

Estas opções existem no executável do jogo e o `run.py` repassa. O que elas fazem na versão de varejo ainda não foi verificado: com `--level`, o jogo ainda para na tela de título.

- `run.py` · Opção do jogo

- `--level <name>` · `-startlevel <name>` (por exemplo `cat_010_streets`, `emb_020_tunnels`, `air_010_rappel`, `pro_020_westminster`)

- `--checkpoint <name>` · `-startcheckpoint <name>`

- `--no-cloth` · `-disablecloth`

- `--no-cam-springs`, `--no-cam-additive` · `-nocamsprings`, `-nocamadditive`

- `--msaa <n>` · `-msaa <n>`

- `--frontend` · `-frontend`

- `--perf` · `-disablecloth -nocamsprings -nocamadditive -msaa 1`
