<!-- source: https://github.com/mr-finster/vf1-recomp/blob/HEAD/README.md | fingerprint: bff2a3ff577ba262 -->

Abra o menu (Esc, ou View / Select no controle) e escolha **Online**. Na primeira vez, ele pede o seu nome (até 10 letras). O nome aparece embaixo da sua barra de energia, com a bandeira do seu país.

**RANDOM: jogar com qualquer um**

- Escolha **RANDOM**. O cartão mostra quantas pessoas estão online agora.

- Ele encontra quem mais estiver procurando, os mais próximos primeiro (a melhor conexão), e amplia a busca conforme você espera. **Stop** desiste.

- Dá para jogar sozinho enquanto espera. A partida assume assim que alguém é encontrado.

- Depois de uma partida, **New opponent** procura de novo.

- **Random: max ping** (Online > Settings): qualquer um, ou de 80 a 250 ms. O ping de um adversário aleatório é medido ao conectar, antes da partida; acima do seu limite, o jogo sai e procura outra pessoa (e vocês dois não se encontram de novo por 10 minutos). Códigos de sala e endereços jogam com qualquer ping.

- **Match found chime** (Online > Settings): ligado (o padrão) toca um aviso quando uma partida online é encontrada, e a janela do jogo pisca na barra de tarefas ou no Dock se não estiver na frente, para você esperar um adversário em outra janela.

**HOST / JOIN: jogar com um amigo**

1. Um jogador escolhe **HOST**. Aparece um código de sala de 6 letras como `FR7-W25`. **Copy code** copia: mande para o seu amigo.

2. O outro escolhe **JOIN** e digita ou cola o código. Ele entra sozinho na 6ª letra, e os passos na tela mostram o andamento.

3. Com os dois dentro, o jogo começa na tela de título: os dois apertam Start.

No controle, as telas de código e de nome mostram letras para escolher com A (X apaga, Y cola). No teclado, é só digitar.

**Your fighter** (Online > Settings)

- **Normal:** escolha na seleção, como sempre.

- **Random:** um lutador aleatório novo a cada seleção: cabeça, corpo e golpes misturados do elenco, como o Menu > Cheats faz.

- **Seed:** um lutador com nome em toda partida, com a mesma montagem e os mesmos golpes em qualquer máquina. Digite o nome dele (da barra de energia, como `SAJACWOLF`) na linha **Seed** abaixo. Todos os nomes: o livro de sementes, [docs/SEEDS.md](https://github.com/mr-finster/vf1-recomp/blob/HEAD/docs/SEEDS.md).

- **Secret:** um dos personagens que sobraram no jogo (Siba, Jeff, Takeru, os irmãos Koskol, Dural).

- Ele é seu de qualquer lado, e o adversário vê embaixo do seu nome na tela de VS antes da luta. Os seus Cheats ficam desligados numa partida: os dois lados jogam o mesmo jogo.

**Na partida**

- Cada jogador usa os próprios controles de **Player 1**.

- As partidas seguem o jeito do PC (jogo livre), com as configurações de arcade de quem criou a sala (rounds, dificuldade, energia).

- **Partidas online são sempre de 3 rounds vencidos.** Toda partida online vai até alguém vencer 3 rounds, como no Virtua Fighter 3, nos torneios da SEGA e nos fliperamas do Japão, independente do Menu > Arcade settings > **Rounds to win (vs player)** de cada um, que vale para jogar offline (também 3 de fábrica). Quem cria a sala define isso para a partida, então fica em 3 mesmo contra uma versão mais antiga da v1.5.0 quando esta cria a sala.

- O cenário é aleatório, como um fliperama ligado o dia todo: um aleatório na primeira luta e depois a ordem do próprio jogo.

- Quem perde é convidado a apertar Start para a revanche. Como no fliperama, o vencedor continua jogando contra o computador até isso acontecer.

- Depois de sair, **Play again** reabre o mesmo código de sala, e o seu amigo volta sozinho.
