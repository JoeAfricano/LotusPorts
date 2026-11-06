<!-- source: https://github.com/tools81/link-faces-of-evil-static-recomp/blob/HEAD/README.md | fingerprint: 55a1470e0350e457 -->

O `package.bat` monta o jogo e cria dois arquivos de distribuição em `dist/`. Nenhum dos dois traz a imagem do disco.

- Arquivo · Uso

- `LinkFacesOfEvil-v<version>-portable.zip` · Extraia onde quiser e abra. Na primeira abertura, o jogo pede a imagem do disco.

- `LinkFacesOfEvil-v<version>-setup.exe` · Instalador de arquivo único. Ele pede a imagem do disco na instalação e cria atalhos no menu Iniciar e na Área de Trabalho.

- `package.bat -SkipBuild` empacota a versão atual sem recompilar.

- `package.bat -Publish` também cria uma versão `v<version>` no GitHub com os dois arquivos. Ele usa a ferramenta de linha de comando `gh`.

- O instalador precisa do Inno Setup 6 (`winget install JRSoftware.InnoSetup`). Sem ele, só o zip é criado.

- O número da versão fica em `project(... VERSION x.y.z)` no `CMakeLists.txt`.

- Para instalar sem perguntas, rode `setup.exe /VERYSILENT /DIR="D:\Games\Link" /ROM="D:\path\game.cue"`.

O instalador não precisa de administrador. O local padrão é `C:\Games\Link - The Faces of Evil`. Desinstalar mantém `saves\` e `linkfoe.ini`.

### Portátil por natureza

O jogo grava só dentro da própria pasta, nunca no AppData nem no registro:

- `saves\` guarda os saves (NVRAM), os estados salvos, as configurações de imagem e o registro.

- `linkfoe.ini` guarda o local da imagem do disco e as configurações de atualização.

- Os shaders ficam dentro dos executáveis.

- O runtime de C vem embutido, então não precisa do Visual C++ Redistributable. As únicas DLLs usadas são as que vêm no Windows 10 e 11.

Como o jogo é sempre portátil, ele não precisa da opção `--portable`. Os atalhos passam mesmo assim, e o programa aceita e ignora.

### linkfoe.ini

```
[game]
; the disc image; relative paths are relative to the game folder
rom=D:\CD-i\Link - The Faces of Evil (USA).cue

[update]
; ask (default), auto (install without asking) or off
mode=ask
; GitHub repository to check; the default is set at build time
repo=owner/name
; written when you choose "skip this version"
skip=v1.2.0
```

`rom=` é a imagem do disco (caminhos relativos partem da pasta do jogo); `mode=` pode ser `ask` (padrão, pergunta), `auto` (instala sem perguntar) ou `off`; `repo=` é o repositório conferido; `skip=` guarda a versão que você escolheu pular.

### Atualizações automáticas

A cada abertura, o jogo lê a versão mais recente do repositório configurado no GitHub. Se a tag (`vX.Y.Z`) for mais nova que a versão em uso e tiver um arquivo `*-portable.zip`, o jogo oferece instalar:

1. O jogo baixa o zip para `update\`.

2. O jogo fecha.

3. Um script do PowerShell substitui os arquivos do programa, mantendo `saves\`, `rom\` e `linkfoe.ini`.

4. O script reabre o jogo.

Sem internet ou sem versão publicada, ele abre normalmente sem mensagem. A conferência desiste depois de alguns segundos. `--no-update` pula, e as execuções sem janela também.

O repositório padrão é definido por `LINKFOE_UPDATE_REPO` no `CMakeLists.txt`. Para usar outro, configure com `cmake -DLINKFOE_UPDATE_REPO=owner/name` e recompile.

### Jogar

Abra o `linkfoe.exe`. Ele procura a imagem do disco nesta ordem:

1. um caminho para o `.cue` ou `.bin` passado como primeiro argumento;

2. a linha `rom=` do `linkfoe.ini`;

3. uma pasta `rom` ao lado do programa ou na pasta atual.

Se nada disso achar a imagem, o jogo pede para você escolher e guarda a escolha no `linkfoe.ini`.

- CD-i · Teclado · Controle estilo Xbox

- Direcional · Setas (Shift = mais rápido) · Analógico esquerdo / direcional

- Botão 1 · Z, Espaço ou A · A

- Botão 2 · X, Ctrl ou S · B

- Os dois botões · C ou Enter · X ou Start

- Inventário (baixo + botão 2) · I ou V · Y ou Back

- Outra tecla · Ação

- F2 · Salva o estado no espaço atual

- F3 · Próximo espaço de estado (1-10)

- F4 · Carrega o estado do espaço atual

- F5 · Cheat: invencibilidade liga/desliga

- F6 · Cheat: +100 rúpias (até 999)
