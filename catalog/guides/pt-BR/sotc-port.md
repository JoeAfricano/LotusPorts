<!-- source: https://github.com/guichiapini/sotcport/blob/HEAD/README.md | fingerprint: c4a6206e89a60b67 -->

1. Baixe o zip na página de [versões (Releases)](https://github.com/Supermedo/bloodborne_pc/releases/latest) e extraia onde quiser.

2. Abra o `Bloodborne.exe`.

3. Em **Game & effects**, escolha a pasta do seu jogo (a que tem o `eboot.bin`).

4. Clique em **PLAY**.

**DLC.** A edição Game of the Year (CUSA03173) já inclui The Old Hunters. Com a cópia do jogo base, copie também os complementos: cada um é uma pasta com nome como `SPEXPANSIONDLC03`, só com `sce_sys` dentro, porque os dados vêm na atualização 1.09. Coloque todos numa pasta e escolha-a como **DLC folder** em **Game & effects**, ou copie para `user\addcont\<title id>\` (a mesma organização do shadPS4 e do bbport).

No jogo, **Insert** (ou **L3+R3** no controle) abre o menu de configurações. Teclado: WASD anda, setas giram a câmera, Espaço = X, Shift esquerdo = Círculo, E = Quadrado, V = Triângulo, 1/3 = L1/R1, R/F = L2/R2, Z = L3, Q/C = R3 (travar mira), I/K/J/L = direcional, Enter = Options, Tab = touchpad. O teclado agora funciona junto com um controle conectado. Tudo isso pode ser mudado no `input.ini` (mesma sintaxe do shadPS4: um arquivo de Bloodborne feito para o shadPS4 pode ser copiado por cima), inclusive a câmera pelo mouse: veja [docs/INPUT.md](https://github.com/guichiapini/sotcport/blob/HEAD/docs/INPUT.md).

### Jogar online (beta)

1. Crie uma conta shadNet em [shadnet.shadps4.net](https://shadnet.shadps4.net/).

2. No launcher, abra **Online co-op**, marque **Play online** e digite o nome da sua conta (não o e-mail) e a senha.

3. Clique em **Test connection**: as três linhas devem dizer "reachable". Depois, clique em **PLAY** e escolha online na tela de título. A mensagem de boas-vindas do Hunter's Dream mostra que você está online.

Todos com quem você quiser jogar precisam da versão 1.09 do jogo e do mesmo servidor de co-op. Um servidor shadNet particular (do projeto [shadp2p](https://github.com/Wozzardman/shadp2p)) também funciona: coloque o endereço em **Server address**.

Para jogar com os jogadores do bbhost (o port de PC do próprio The Hunter's Dream), escolha **The Hunter's Dream** em **Co-op pool** e clique em **Sign in with Discord**: não precisa de conta shadNet. O jogo então fica online pelo [bbbridge](https://github.com/MaskZeus/bbbridge-bbport-online-tool), de MaskZeus, que o launcher abre junto com o jogo. O arquivo da conta é o `user\bbbridge\bbbridge_state.json`: nunca compartilhe.
