<!-- source: https://github.com/Nezara/OpenCrown-PC/blob/HEAD/README.md | fingerprint: 2436377d8653d54e -->

Todos os downloads estão em **[opencrown.net](https://opencrown.net)** e na página de [versões (Releases)](https://github.com/Nezara/OpenCrown-PC/releases/latest).

### Windows

1. **Baixe** o `OpenCrownPC-*-win64.zip` mais recente em [opencrown.net](https://opencrown.net/) ou na página de [versões](https://github.com/Nezara/OpenCrown-PC/releases/latest).

2. **Extraia** onde quiser, por exemplo `C:\Games\OpenCrownPC`. Mantenha os três arquivos juntos: `OpenCrownPC Launcher.exe`, `dcp_run.exe` e `SDL3.dll`.

3. **Abra o `OpenCrownPC Launcher.exe`.** O SmartScreen do Windows pode avisar porque o programa não tem assinatura digital (é um projeto de hobby). Clique em Mais informações → Executar assim mesmo.

4. Na página **Game**, **indique a pasta da sua cópia** (a que tem o `eboot.bin`). O launcher confere a pasta e avisa se faltar algo.

5. Na primeira vez, ele pergunta **onde guardar os seus saves**. O padrão é `Documentos\Dragons Crown Pro\saves`.

6. Clique em **Play**. 🎉

Configurações, saves e registros ficam todos em `Documentos\Dragons Crown Pro\`, a não ser que você escolha outra pasta de saves.

### Jogar online

O jogo online usa **o próprio sistema de partidas e de rede do jogo**. O servidor de lobby só apresenta os jogadores uns aos outros:

1. **Crie ou entre numa sala.** Quando você fica online no jogo, ele abre uma sala no servidor de lobby. Os outros a encontram na lista de salas ou entram pela sua lista de **amigos**.

2. **Apresentação.** O servidor troca os endereços de rede dos jogadores para eles se alcançarem.

3. **Ponto a ponto.** Os jogadores se conectam **direto** por UDP. Todo o tráfego do jogo vai entre os jogadores, nunca pelo servidor. A travessia de NAT usa STUN e "hole punching", mais UPnP se o seu roteador permitir.

Os **ossos** (os registros de aliados que você encontra nas catacumbas e ressuscita no templo) são trocados pelo próprio jogo nessa mesma conexão quando uma fase começa, como no PS4/PS5. Não há banco central de ossos.

O lobby é um pequeno Cloudflare Worker de código aberto que fica em [server/lobby-worker](https://github.com/Nezara/OpenCrown-PC/blob/HEAD/server/lobby-worker). Qualquer um pode rodar o próprio e apontar a página **Online** para ele.

**Amigos:** cada jogador tem um código de amigo como `ABCD-1234`. Adicione alguém pelo código na página **Online**. Quando a pessoa aceitar, você vê quando ela está online e pode usar "Join Friend's Room" no jogo. Isso funciona entre versões: jogadores de Windows, Linux e Android entram uns com os outros assim. No Android, é preciso o OpenCrown Mobile 0.1.9 ou mais novo.

> O jogo online é liberado no mesmo ponto da história que no PS4/PS5. Pular a história com um personagem novo leva você lá na hora.

### Câmeras separadas

Novo na 1.0.8 (Windows, Linux) e no Mobile 0.1.6. No PS4/PS5, todos numa sala online dividem uma câmera. Com o OpenCrown, **a tela de cada jogador segue o próprio personagem** nas aventuras online, então dá para explorar um pouco separado.

- **As lutas mantêm o grupo junto.** Quando alguém começa uma luta, ninguém passa dela até os inimigos serem derrotados. Voltar é sempre permitido.

- **Quem cria a sala decide.** A opção é **Separate cameras when I host** na página **Online**. Ela vem **ligada por padrão** nesta versão; os autores querem saber como fica, e talvez não continue como padrão.

- **Todos precisam da versão nova.** Se alguém na sala estiver numa versão mais antiga, a sala inteira usa a câmera dividida como antes.

- A cidade e o jogo local (no sofá) não mudam.
