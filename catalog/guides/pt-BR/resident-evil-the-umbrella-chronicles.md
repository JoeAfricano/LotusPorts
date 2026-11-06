<!-- source: https://github.com/solleo95/reuc-recomp/blob/HEAD/README.md | fingerprint: c185f22790b4817f -->

Esta seção é para **Windows**. No **Steam Deck** (ou outro Linux), siga a seção Steam Deck e Linux do repositório: há um script de instalação que faz tudo.

### 1. Baixe o port

1. Abra a [página de versões (Releases)](https://github.com/solleo95/reuc-recomp/releases/latest) e baixe o **`UC-Windows-X.Y.Z.zip`**.

2. Extraia numa pasta onde você tenha permissão de gravação (por exemplo `C:\Games\Umbrella Chronicles`; **não** em `Arquivos de Programas`). O conteúdo vai direto para essa pasta: você vê `uc_launcher.exe`, `uc.exe` e várias DLLs.

### 2. Extraia a imagem do seu jogo (pelo launcher ou com o Wit)

O port não traz o jogo. Ele lê os arquivos do jogo de uma pasta onde a imagem do **seu próprio** disco está extraída (a pasta que tem `DATA`). Há dois jeitos, escolha um: o launcher extrai para você (**jeito A**, o mais fácil), ou você extrai com o Wit (**jeito B**).

**Do que você precisa (nos dois):** uma imagem do seu próprio disco da versão NTSC-U (`RBUE08`) em `.iso`, `.wbfs`, `.ciso`, `.wdf` ou `.wia`, e cerca de 4 GB livres. (O formato `.rvz` do Dolphin não é aceito: converta para ISO no Dolphin antes, botão direito no jogo → Convert File…. Discos num leitor também não: use uma imagem.) Você pode copiar o seu disco com um Wii e software caseiro.

### Jeito A: o launcher extrai a imagem

A ferramenta de extração **Wit** (Wiimms ISO Tools) já vem no port (a pasta `tools\wit`), então não há nada para baixar nem comando para digitar.

1. Abra o **`uc_launcher.exe`**. Se o jogo ainda não for encontrado, o launcher pede para escolher a pasta do jogo extraído.

2. Clique no **botão de disco** (o ícone redondo, dica "Select a disc image…") à direita do botão de pasta e escolha o arquivo da imagem. Também dá para colar o caminho no campo **Game folder**. A mensagem embaixo muda para "Disc image selected. It is unpacked into the game folder before the game starts."

3. Clique no botão vermelho **Singleplayer**. A janela **Unpacking the game** abre com uma barra de progresso e um botão **Cancel**. Leva cerca de um minuto.

4. Quando terminar, o launcher define a **Game folder** como a nova pasta `game` ao lado do `uc.exe` e abre o jogo sozinho. Da próxima vez, é só clicar em **Singleplayer**: a imagem não é mais necessária.

O que saber:

- **Só a imagem NTSC-U (`RBUE08`) é aceita.** O launcher lê o ID do disco antes. Uma imagem de outra região, ou um arquivo que não é imagem de disco, é recusado com uma mensagem, e nada muda no seu disco.

- **A pasta `game` ao lado do `uc.exe` é substituída.** Quando a imagem é aceita, o launcher **apaga a pasta `game` antiga** (se houver) e extrai o jogo numa nova. Se você colocou arquivos seus nela, faça uma cópia antes.

- **A imagem não pode estar dentro da pasta `game`** (seria apagada junto): mova para outro lugar. O launcher avisa se você escolher um arquivo assim.

- **Nunca fica um jogo pela metade.** Se você cancelar ou a extração falhar (imagem danificada, disco cheio), a pasta `game` incompleta é removida.

- **Multiplayer** fica indisponível até o jogo ser extraído: clique em **Singleplayer** uma vez antes.

- O jogo é sempre extraído em `game` ao lado do `uc.exe`. Se quiser em outra pasta, use o jeito B.

### Jeito B: extrair à mão com o Wit

Use se quiser o jogo em outra pasta ou não quiser que o launcher mexa na pasta `game`.

1. Baixe o **Wit** para Windows em <[https://wit.wiimm.de/wit/](https://wit.wiimm.de/wit/)> e extraia, por exemplo em `C:\Tools\wit`.

2. Abra o **PowerShell** ou o prompt de comando na pasta do port (onde está o `uc.exe`) e rode (troque pelos seus caminhos):

```
   C:\Tools\wit\bin\wit.exe extract "C:\Images\Umbrella Chronicles.iso" ".\game"
```

O `wit extract` grava o disco inteiro na pasta `game`. Leva um minuto e precisa de cerca de 4 GB livres.

3. Confira o resultado. O arquivo **`game\DATA\sys\main.dol`** precisa existir. A estrutura fica assim:

```
   uc.exe
   uc_launcher.exe
   game\
   ├─ DATA\
   │  ├─ files\        (the game's data)
   │  └─ sys\
   │     └─ main.dol
   └─ UPDATE\
```

4. (Opcional) Confira se a imagem é a certa. O SHA-1 do arquivo deve ser `7ed67efe2ba9b0de1cef0bbb2ea157d7a43d7f25`:

```
   certutil -hashfile "game\DATA\sys\main.dol" SHA1
```

Se você extraiu em outra pasta, escolha-a no launcher no próximo passo. A pasta escolhida precisa ser a que **contém `DATA`**.

### 3. Primeira abertura e configurações

1. Abra o **`uc_launcher.exe`**. Se ele disser "Select the extracted game folder", clique no campo **Game folder** e escolha a pasta do passo 2 (a que tem `DATA`), ou escolha a imagem do seu disco com o botão de disco e clique em **Singleplayer**. Um sinal verde quer dizer que o jogo foi encontrado.

2. Escolha o idioma do launcher (no topo da janela), a resolução e o modo de janela.

3. Ajuste as opções gráficas. Um bom ponto de partida:

- **Upscaler:** DLSS (placa RTX) ou FSR (qualquer placa), modo Quality. Sem upscaler, o jogo renderiza na sua resolução.

- **Geração de quadros:** deixe desligada até o jogo rodar bem e depois teste. Escolher um gerador também liga o upscaler correspondente.

- **Ray tracing, DLSS Ray Reconstruction, HDR:** só se a sua placa suportar; o que não estiver disponível fica cinza com uma explicação.

- **Limite de FPS:** o limite também define o ritmo da simulação do jogo. Com geração de quadros, o FPS mostrado é o limite vezes o multiplicador do gerador.

4. Abra a página **Controls** se quiser mudar teclas ou a sensibilidade do mouse. Vai jogar com controle? Conecte e abra a página **Controllers**: escolha o modelo se não for detectado direito e clique em **Gamepad** para ajustar. Com uma pistola de luz, coloque **Pointing device** em **Light gun** na página **Controls**. Com um Wii Remote, conecte antes (veja [Pistolas de luz e o Wii Remote](https://github.com/solleo95/reuc-recomp/blob/HEAD/docs/lightguns.md)) e clique em **Wii Remote** na página **Controllers**.

5. Clique no botão vermelho **Singleplayer** para abrir o jogo (**Multiplayer** fica ao lado).

Na **primeira abertura**, o launcher faz uma calibração rápida do processador e o jogo compila os shaders antes de mostrar a imagem. A primeira abertura demora mais; as próximas são mais rápidas.

Tudo o que o launcher muda é salvo na hora no `config.json`, ao lado do `uc.exe`. Os saves ficam na pasta `saves`. Não rode o jogo de um local somente leitura.
