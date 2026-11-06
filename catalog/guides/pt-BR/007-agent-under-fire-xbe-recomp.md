<!-- source: https://github.com/hk21x/007-agentunderfire-recomp/blob/HEAD/README.md | fingerprint: 663ea1b33f22c959 -->

O launcher prepara o jogo no seu PC a partir do seu próprio disco: sem Visual Studio, sem Git e sem linha de comando, nada instalado no Windows e sem precisar de administrador. A versão traz o runtime e um compilador, e nenhum código do jogo.

**Você precisa de:** Windows 10 ou 11, 64 bits, uma placa de vídeo com Direct3D 12 ou Direct3D 11, a sua própria cópia do jogo (uma imagem `.iso` do seu disco, ou uma pasta com os arquivos dele) e internet para o pacote.

1. Baixe o `AgentUnderFireLauncher.exe` na [versão mais recente](https://github.com/hk21x/007-agentunderfire-recomp/releases/latest).

2. Coloque numa pasta só dele e abra. O Windows pode avisar sobre um app não reconhecido: escolha **Mais informações** e depois **Executar assim mesmo**.

3. Escolha a imagem ou a pasta do seu disco e clique em **Get the game ready**. O launcher baixa o pacote para o seu disco e compila o código do jogo a partir dele; o tempo depende do seu PC e da conexão.

4. Clique em **Play**. Quando sai uma versão nova, o launcher oferece **Update**.

Para montar à mão, veja "Building" no repositório.

### O que vem numa versão

- Arquivo · Tamanho · O que é

- `AgentUnderFireLauncher.exe` · 2,5 MB · **O único arquivo para baixar à mão.** Ele busca o resto sozinho.

- `AgentUnderFire-PAL-package.zip` · 83,1 MB · Runtime e compilador para o disco PAL, sem código do jogo. O launcher busca para esse disco; não extraia à mão.

- `AgentUnderFire-USA-package.zip` · 78,3 MB · O mesmo para o disco americano.

- `*-package.zip.json` · O tamanho e o SHA-256 que o launcher confere em cada download. Pode ignorar.

Os tamanhos são da v1.0.0. Preparar o jogo levou 111 s num Windows Sandbox limpo com o disco PAL; no seu PC depende do computador e da conexão.
