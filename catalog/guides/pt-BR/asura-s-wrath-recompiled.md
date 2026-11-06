<!-- source: https://github.com/PauloUbirajara/asuras-wrath-recomp/blob/HEAD/README.md | fingerprint: e4ed273fe080ad3f -->

### Clonar o repositório

```
git clone --recursive https://github.com/PauloUbirajara/asuras-wrath-recomp.git
cd asuras-wrath-recomp
```

### 4. Preparar o jogo

Na mesma pasta do executável, rode:

```
# Extract Game ISO
mkdir -p extracted/
extract-xiso -x asura_wrath.iso -d extracted/

# (Optional) Install DLC Content (Not supported/tested yet)
```

Isso extrai a ISO do seu jogo para `extracted/`. Instalar DLC ainda não é suportado nem testado.

Se o jogo extraído estiver em outro lugar, indique com o argumento `--game_data_root`:

```
asura_wrath_recomp --game_data_root=/path/to/extracted/game
```

Opções de caminho do ReXGlue:

- Opção · Descrição · Padrão

- `--cache_root` · Pasta dos pipelines Vulkan e shaders compilados. · `cache`

- `--game_data_root` · Pasta com os arquivos extraídos do jogo. · `extracted` (se existir) ou a pasta atual

- `--metadata_root` · Pasta dos metadados do jogo. · `(vazio)`

- `--update_data_root` · Pasta com atualizações / patches de DLC. · `(vazio)`

- `--user_data_root` · Pasta dos saves e do perfil do jogador. · `%USERPROFILE%\Documents\asura_wrath_recomp` (Windows)

- `--user_data_root` · Pasta dos saves e do perfil do jogador. · `~/.local/share/asura_wrath_recomp` (Linux)
