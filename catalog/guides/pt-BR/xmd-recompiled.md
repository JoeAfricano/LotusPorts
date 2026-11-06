<!-- source: https://github.com/florinp93/xmd-recompiled/blob/HEAD/README.md | fingerprint: 4fd9d4eeb7084ea3 -->

### 1. Prepare o SDK

```
.\setup.ps1
```

Isso clona o ReXGlue SDK (fixo na `v0.10.0`) em `thirdparty/rexglue-sdk` e inicia os submódulos dele.

### 2. Coloque os arquivos do jogo

Extraia a ISO do seu Xbox 360 em `game/`. O executável de entrada precisa ficar em `game/default.xex` (o caminho definido em `xmd_manifest.toml`). Mantenha a organização original das pastas para todos os outros arquivos.

> **Não envie nada de `game/` para o repositório**: lá ficam arquivos protegidos por direitos autorais, usados só no seu PC para a recompilação.

### 4. Gere de novo os arquivos do SDK

Com o `game/default.xex` no lugar, gere de novo a estrutura do SDK, para ela levar a versão e a marca exatas:

```
rexglue init --force --project_name xmd --project_root . --xex_path game\default.xex --game_root game
```

### 6. Abrir

```
.\out\win-amd64\RelWithDebInfo\xmd.exe
# Useful flags:
#   --log_level=trace     verbose logging
#   --log_file=run.log    write logs to file
```

Opções úteis: `--log_level=trace` para registro detalhado e `--log_file=run.log` para gravar o registro num arquivo.
