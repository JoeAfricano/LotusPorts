<!-- source: https://github.com/saulob/odin-sphere-leifthrasir-pc/blob/HEAD/README.md | fingerprint: b83d71a47d2b19e3 -->

**Nenhum arquivo do jogo vem** na versão, e o port não baixa o jogo.

### Windows

1. Baixe o **OdinSphereLeifthrasirPC-v0.2.3-windows-x64.zip** na [versão mais recente](https://github.com/saulob/odin-sphere-leifthrasir-pc/releases/latest) e extraia num lugar onde dê para gravar (não em `Program Files`). Ele traz uma pasta, `OdinSphereLeifthrasirPC-v0.2.3-windows-x64`.

2. Abra o **odin_sphere_leifthrasir.exe** dessa pasta. O launcher aparece.

3. Na página **Play**, clique em **Install game...** e escolha o seu próprio arquivo `.pkg` do Odin Sphere Leifthrasir.

4. Espere aparecer **Ready to play**. O pacote é desempacotado no seu PC na pasta `game`, ao lado do programa (cerca de 3,3 GB). O `.pkg` é só lido, e você pode apagá-lo depois.

5. Americana 1.01, japonesa 1.02 ou europeia 1.01: instale primeiro o pacote base daquela região e depois o pacote de atualização do mesmo jeito. A atualização é aplicada sobre a base e vira a versão ativa. Um pacote americano 1.01 completo (`CATEGORY` `gd`, `APP_VER` `01.01`) instala sozinho, sem a base.

6. Clique em **PLAY**.

**Reinstall game...** na página Play repara os arquivos do jogo e mantém configurações e saves.

### SmartScreen do Windows

O Windows pode avisar na primeira vez, porque as versões não têm assinatura digital. Se você baixou da página de versões deste projeto no GitHub, escolha **Mais informações** e depois **Executar assim mesmo**. Cada versão lista o SHA-256 dos downloads; compare com o seu.

### Saves e configurações

Tudo fica ao lado do programa (ao lado do AppImage no Linux):

- Pasta · Conteúdo

- `user\osl.ini` · Configurações do launcher e do jogo

- `user\savedata\` · Os seus saves

- `user\trophy\` · O seu progresso de troféus e a lista e os ícones dos troféus tirados do seu jogo

- `user\keys.json` · A sua Trophy Key, se você configurar uma

- `user\cache\<Title ID>\` · Cache de shaders, para as próximas aberturas carregarem mais rápido

- `logs\` · Um `odin_sphere_leifthrasir_NNN.log` por abertura, launcher e jogo juntos; os 50 mais novos ficam guardados

- `game\` · O jogo instalado (desempacotado do seu próprio pacote)
