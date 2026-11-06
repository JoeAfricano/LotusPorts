<!-- source: https://github.com/nfroggy/openmadoola/blob/HEAD/README.md | fingerprint: 6eacec35eaad2a4b -->

Para funcionar, o OpenMadoola precisa dos arquivos de dados font.bin, 2c04.pal e nes.pal e das pastas demo e mml do repositório. Eles vêm junto com os executáveis prontos para Windows da página de versões (Releases). O OpenMadoola também precisa de uma imagem do Wing of Madoola. Pode ser um arquivo comum de NES chamado "madoola.nes" ou o arquivo "sharedassets0.assets" do [Sunsoft is Back! Retro Game Selection](https://store.steampowered.com/app/2821290/SUNSOFT_is_Back/) na Steam. O OpenMadoola lê todos os gráficos e fases dessa imagem e não abre se ela não estiver lá.

Se você não tem o cartucho original ou não quer copiá-lo, o autor recomenda comprar a coletânea na Steam. Ela custa pouco e tem uma interface bonita com digitalizações do manual em alta qualidade e artes conceituais. Para pegar o arquivo "sharedassets0.assets", vá à sua biblioteca da Steam, clique com o botão direito na coletânea da Sunsoft e escolha "Gerenciar → Ver arquivos locais". Ele fica na pasta "SUNSOFT is Back! Retro Game Selection_Data".

### Windows

O OpenMadoola tenta ler a imagem do jogo e os arquivos de dados da pasta atual (normalmente a pasta do executável). Se não encontrar a imagem ali, ele também tenta a pasta padrão de instalação da Steam (`C:\Program Files (x86)\Steam\steamapps\common\SUNSOFT is Back! レトロゲームセレクション`). O OpenMadoola grava configurações e saves na pasta atual.

### Controles padrão

- Teclado · Controle (layout Xbox) · Botão equivalente do NES

- Cima / Baixo / Esquerda / Direita · Direcional · Direcional

- X · X · B

- Z · A · A

- Shift direito · Back · Select

- Enter · Start · Start

Dá para mudar os controles no menu de opções.

### Tipos de jogo

No menu de opções também há a configuração "game type". O tipo "Original" tenta imitar o jogo de NES, mas com correções importantes de erros. O tipo "Plus" (o padrão) traz correções de erros menores e ajustes que o autor acha que melhoram o jogo. O tipo "Arcade" recria a versão do VS. System.

### Mudanças em todos os tipos

- O jogo roda sem as limitações do NES (sem lentidão, sem sprites piscando, sem lixo na borda da tela).

- O progresso é salvo em disco.

- As telas de status e de fase podem ser puladas com Start.

- Corrigido: pegar a Wing of Madoola e depois entrar numa porta fazia o Daltos não aparecer.

- Corrigido: matar o Daltos e morrer para uma das bolas de fogo dele deixava pular a luta depois de continuar.

- Corrigido: efeitos sonoros às vezes ficavam presos ou com defeito.

- Corrigido: pausar e despausar numa fase de castelo fazia o canal de ruído sumir da música.

- Corrigido: o espelhamento da bola de fogo de alguns inimigos não era iniciado.

- Corrigido: a Lucia não morria se estivesse na frente de uma porta.

- Corrigido: a tela tremia para cima e para baixo quando a Lucia caía numa escada.

### Mudanças só do Plus

- O texto de abertura e de final da versão arcade.

- Um segundo chip de som emulado só para a música (os canais não cortam para tocar efeitos).

- A música não recomeça ao se teletransportar dentro da mesma fase.

- A música não recomeça ao pausar e despausar.

- A música de chefe da versão arcade no lugar da do console.

- A arma escolhida aparece durante o jogo, e dá para trocar de arma sem pausar.

- O Daltos aparece mesmo quando está só parcialmente visível, ficando mais fácil de ver.

- Dá para matar vários inimigos de uma vez quando estão um em cima do outro (deixa os Nyurus bem menos chatos).

- Corrigido: o orbe do fim da fase não era animado direito.

- Corrigido: o Nipata travava se aparecesse num vão estreito.

### Mudanças só do Arcade

Para a lista de todas as diferenças conhecidas entre as versões de console e arcade, veja [aqui](https://www.infochunk.com/madoola/arcade.html). Diferenças entre a versão arcade real e a do OpenMadoola:

- As mudanças do Plus (acima) estão incluídas.

- Nada relacionado a colocar fichas. O sistema de continuar é o mesmo dos outros tipos.

- O "Press Start" da tela de título pisca mais devagar que o original, porque o autor acha mais bonito.

- A opção "Arcade difficulty" controla a dificuldade assim:

- **Easy**: a vida da Lucia cai 1 a cada 32 quadros.

- **Normal**: não há cronômetro de vida, mas o MP da Lucia só volta até 1000 quando ela morre ou entre fases. Dá para recarregar o MP fazendo múltiplos de 10.000 pontos. Este modo segue [este](https://www.youtube.com/watch?v=C9T8ad12eO8) vídeo de jogo.

- **Hard**: junta o cronômetro de vida do Easy com a recarga de MP do Normal. Este modo segue [este](https://www.youtube.com/watch?v=FKKC1KHHi4Q) vídeo de jogo.

- Há colisão com inimigos no modo de demonstração. O autor acrescentou outras fases além da Fase 1 à demonstração para variar.

- A Lucia começa com 3 vidas em vez de 2. Isso pode ter sido uma chave DIP no original.

- Como o OpenMadoola tem botões start/select, nenhuma das mudanças de controle foi trazida.

- A mudança da Fase 4 não foi acrescentada porque parece um erro.
