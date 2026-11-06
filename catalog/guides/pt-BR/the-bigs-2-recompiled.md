<!-- source: https://github.com/mdomangue26/The-Bigs-2-Recompiled/blob/HEAD/README.md | fingerprint: 8900b7b78499de7c -->

1. Baixe a versão mais recente na página de **[versões (Releases)](https://github.com/mdomangue26/thebigs2/releases/latest)**:

- Windows: `TheBigs2-windows-x64.zip` (extraia)

Ou compile você mesmo (veja "Building" no repositório).

2. Coloque numa pasta só dele, de preferência com o seu `.iso` ao lado.

3. Abra. A tela de configuração pede a imagem do disco (um `.iso` na mesma pasta já vem escolhido), extrai uma vez e o jogo abre.

Tudo o que o jogo grava fica nessa pasta:

```
TheBigs2-x86_64.AppImage   (or thebigs2.exe)
game/      the extracted game files
config/    thebigs2.toml (settings)
saves/     profile, save games, achievements
cache/     shader cache
logs/
```

`game/` guarda os arquivos extraídos do jogo, `config/` as configurações (`thebigs2.toml`), `saves/` o perfil, os saves e as conquistas, `cache/` o cache de shaders e `logs/` os registros.

Mova ou faça cópia da pasta inteira. Se a pasta não puder ser gravada, o jogo usa `~/.config/thebigs2` e `~/.local/share/thebigs2` (Linux) ou `%APPDATA%\thebigs2` (Windows).

**Requisitos:** uma placa de vídeo com Vulkan e driver atualizado. O Linux precisa do glibc 2.38 ou mais novo (Ubuntu 24.04, Fedora 39, Arch ou SteamOS atuais). O Windows precisa ser o 10 ou 11 de 64 bits.

### Jogar com o teclado

O teclado funciona como o controle do jogador 1, junto com um controle:

- Controle · Teclas

- A / B / X / Y · `;` ou Espaço / `'` ou Backspace / L / P

- LB / RB · 1 / 3

- LT / RT · Q ou I / E ou O

- Back / Start · Z ou Tab / X ou Enter

- Analógico esquerdo (clique) · W A S D (F)

- Analógico direito (clique) · setas (K)

- Direcional · Shift + setas

Mude as teclas na aba **Keyboard** do menu de configurações e remapeie os botões do controle na aba **Controller** (por exemplo, trocar A e B). O remapeamento de botões vale depois de reabrir.
