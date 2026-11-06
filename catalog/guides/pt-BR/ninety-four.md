<!-- source: https://github.com/1nputs/ninety-four/blob/HEAD/README.md | fingerprint: ecf4055df01f467a -->

### O menu

Esc (ou o botão Back do controle) abre o menu; setas e Enter, ou o direcional, A e B, navegam.

- **Game ROM**: a cópia do seu cartucho, num navegador de arquivos.

- **Reset** e **Save state** / **Load state**: oito espaços, cada um com uma imagem do momento, em `states/` ao lado do jogo. Os estados são salvos durante o jogo (não nos menus ou numa pausa). Sair salva o **Autosave** (onde você estava), primeiro na página Load: o jogo sempre começa do zero e só o carrega quando você escolhe.

- **Video**: um ampliador de pixel art (Scale2x, Scale3x ou MMPX, que suavizam os degraus sem borrar nem criar cores, ou xBR, que funde as bordas); escala nítida, inteira ou ajustada; pixels quadrados ou 4:3; os níveis de cor (padrão, faixa completa ou os do próprio console); pontilhado misturado como numa TV; linhas de varredura e grade ou máscara de tubo; tela cheia; e **Vsync**, para nada rasgar (ligado a não ser que você desligue).

- **Capture**: os últimos 20 segundos ficam sempre guardados; F10 (ou esta página) os salva, e F11 grava até você apertar de novo. Vão para `videos/` como GIFs (cores exatas, 30 quadros por segundo, sem som).

- **Sound**: volume, e o núcleo e o chip FM (ouça a diferença jogando).

- **Players**: qual controle cada jogador usa, os botões de um controle (aperte o novo), turbo A / B / C, a zona morta do analógico e o **4 Way Play** da EA para quatro jogadores (como no cartucho: ligue, Reset e escolha Players: Three ou Four na tela de configuração do jogo).

- **Rosters**: quem joga.

- **Game rules**: como o jogo funciona (abaixo).

- **Online play**: de duas a quatro máquinas, com quantos jogadores quiser em cada (abaixo).

- **Options** e **Keys**: velocidade do acelerar, contador de FPS e todas as teclas dos controles (escolha uma e aperte a tecla nova, com Shift, Ctrl ou Alt se quiser).

Tudo fica no `settings.ini`, na pasta de onde você roda o jogo, ao lado dos saves; dá para editar com o jogo fechado.

### Regras do jogo

Coisas que o cartucho nunca ofereceu, para todos os jogadores e o jogo inteiro. Elas são definidas antes de uma partida começar (uma mudança no meio vale a partir do próximo Reset), e um estado salvo guarda as regras da sua partida. **Classic**, o padrão, é exatamente o original.

- **Correções e estatísticas**: o erro de peso corrigido (o original soma os pesos de um choque num byte que dá a volta, então um jogador pesado derrubado por um mais leve cai fácil), a segunda assistência mantida numa tabela, e +/- com os choques nas estatísticas dos jogadores.

- **Tempo**: mais durações de período na configuração (2, 3, 7 e 15 minutos além de 5, 10 e 20) e a duração da prorrogação na temporada regular.

- **Como joga**: velocidade da patinação, a arrancada, a força do chute, o quanto o disco desliza, cansaço, descanso no banco e no banco de penalidades, quantas penalidades são marcadas e as fases boas e ruins.

- **Predefinições** ajustam tudo: Classic, Simulation (as correções e estatísticas, arrancada mais fraca, menos penalidades, fases mais suaves) e Arcade (mais rápido, mais forte, mais animado).

- **A visão do rinque**: 320 de largura em vez dos 256 do cartucho, mostrando a largura inteira do gelo com a câmera se movendo menos, com o placar e as sobreposições no meio.

As regras que são quantidades são números por baixo. A página alterna entre escolhas com nome, cada uma mostrando o número (Skating speed: classic, faster 110%, fastest 125%, slower 90%), e o `settings.ini` aceita um nome ou qualquer número na faixa da regra (`rules.skating = 115`), que a página mostra como "custom (115%)". O `--rule` aceita o mesmo (`--rule skating=115`).

- Regra · O número · Com nome · Faixa

- `overtime` · minutos de prorrogação na temporada regular · classic 10, 5min, 3min · 1 a 20

- `skating` · % da aceleração e velocidade máxima · classic 100, faster 110, fastest 125, slower 90 · 50 a 150

- `burst` · % do empurrão da arrancada · classic 100, weaker 50, weakest 25, stronger 150 · 0 a 200

- `shot_power` · % da velocidade do chute · classic 100, harder 115, softer 85 · 50 a 150

- `puck_slide` · % da desaceleração do disco no gelo (menos desliza mais longe) · classic 100, farther 67, shorter 150 · 25 a 200

- `tiring` · % da energia gasta patinando e arrancando · classic 100, less 50, more 150 · 0 a 200

- `bench_rest` · % da energia que o banco devolve · classic 100, faster 150, slower 67 · 0 a 200

- `penalties` · % das penalidades marcadas (icing e impedimento sempre) · classic 100, fewer 50, rare 25 · 0 a 100

- `streaks` · % do efeito das fases boas e ruins nas notas · classic 100, milder 50, off 0 · 0 a 200

As regras de ligar / desligar e as durações de período usam `on` / `off` e `classic` / `more`; a visão do rinque é `classic` / `wide`.

### Jogo online

De duas a quatro máquinas, pela internet ou rede local, com rollback: cada jogo usa na hora os botões dos próprios jogadores e adivinha os dos outros, e quando os deles chegam diferentes, ele rejoga os últimos quadros com eles antes de desenhar o próximo, então ninguém espera. A página **Online play** do menu: **Name**; **Role**: criar, entrar ou (duas máquinas) os dois conectam ao mesmo tempo (a maioria dos roteadores domésticos deixa passar sem redirecionar porta); o endereço e a porta; o endereço desta máquina na rede local; o atraso de entrada (quadros entre apertar e o jogo ver, 2 por padrão: mais quer dizer menos rollbacks em distâncias longas); **Players here** (quantos jogam nesta máquina); e, para quem cria, **Total players**. Quem cria espera todos entrarem e então o jogo liga em todas as máquinas. Se alguém sair, a partida acaba para todos, e cada um é avisado de quem saiu. Pela linha de comando: `--host [PORT]`, `--join ADDR[:PORT]`, `--peer ADDR[:PORT]`, `--delay N`, `--name NAME`, `--here N`, `--players N`.

A partida é de quem cria: as escolhas de elencos e regras dele vão para as outras máquinas antes de começar, então ninguém mais precisa ter cópia delas. Todo jogo online começa de um cartucho novo (escalações padrão, sem recordes): temporadas, escalações e recordes vão online por um estado salvo, que o **Load state** de quem cria carrega para todos no mesmo quadro. Pausa, câmera lenta, acelerar e voltar no tempo não estão disponíveis jogando junto, e só quem cria pode reiniciar. Todas as máquinas precisam da mesma versão do jogo e do cartucho EUA/Europa, senão são recusadas.
