<!-- source: https://github.com/whompay/SaintsReborn/blob/HEAD/README.md | fingerprint: 13ac8ba85eb27008 -->

Baixe o **SaintsReborn-Setup.exe** na [versão mais recente](https://github.com/whompay/SaintsReborn/releases/latest), abra, escolha o seu `.iso` ou uma pasta com os arquivos do seu jogo (a que tem `default.xex` e `packfiles`) e clique em **Install**.

O instalador baixa as ferramentas de compilação que precisa: clang, CMake, Ninja e os cabeçalhos e bibliotecas C++ da Microsoft. São cerca de 1,5 GB, tudo numa pasta só, e não precisa de Visual Studio. O Windows só pede permissão se faltar o runtime do Visual C++. Depois, o instalador monta o jogo e cria um atalho do Saints Reborn. A primeira montagem leva de 10 a 60 minutos, conforme o processador.

Para atualizar, rode o instalador de novo ou use o atalho **Update Saints Reborn**. Só o que mudou é remontado, e os seus saves e a lista de mods são mantidos. Instalações feitas com o instalador antigo atualizam do mesmo jeito.

O co-op e o multijogador vêm embutidos, então não aparecem na lista do carregador de mods. Eles estão sempre ligados.

### Jogar

Abra o `dist\WhompaysModLoader.exe` para escolher mods e jogar, ou o `dist\saintsrow.exe` para jogar direto.

Um controle de Xbox funciona como no console. Os controles de teclado e mouse seguem o Saints Row 2 de PC:

- A pé · Num veículo

- Andar · W A S D · Acelerar / frear · W / S

- Câmera · Mouse · Virar · A / D

- Atacar / secundário · Botão esquerdo / direito do mouse · Atirar do carro · Botão esquerdo do mouse

- Pular / correr · Espaço / Shift · Freio de mão / nitro · Espaço / Shift

- Ação, entrar no veículo · E · Sair do veículo · E

- Recarregar, pegar arma · R · Olhar para esquerda / direita / trás · Z / C / X

- Chutar / agachar · F / C · Hidráulica · Ctrl

Em qualquer lugar: segure Q para a roda de armas (aponte com o mouse), Esc ou M para o menu de pausa e o mapa, Tab para voltar, as setas para o direcional, Enter e Backspace para A e B nos menus. No menu de pausa, o mouse arrasta o mapa, a roda dá zoom e o botão esquerdo marca um destino; Q e E trocam de aba. Um clique do mouse pula as cenas. Ao pichar, mova o mouse em círculos como as setas mostram.

- Tecla · Efeito

- F11 · Tela cheia / janela

- F10 · Limite de quadros: 30, 60, 90, 120 ou sem limite (acima de 30 precisa do mod de 60 FPS)

- F1 · Contador de quadros

- V · Primeira pessoa a pé, nadando e em veículos (volta sozinho em lojas, cenas e trechos roteirizados). Configurações: `dist\core\FirstPerson\mod.ini`

Os comandos são ignorados com a janela do jogo fora de foco. Saves e dados de perfil ficam em `dist\game`. O instalador faz as imagens dos botões do teclado a partir dos seus próprios arquivos do jogo (veja [tools/glyphgen](https://github.com/whompay/SaintsReborn/blob/HEAD/tools/glyphgen/README.md)); se essa etapa falhar, o jogo mostra os botões do controle.

Opções, criando um arquivo ao lado do `saintsrow.exe`:

- Arquivo · Efeito

- `res_scale.txt` · Escala da resolução interna: `1` (720p), `2` (padrão) ou `3`.

- `start_windowed` · Abre em janela em vez de tela cheia. O arquivo pode ficar vazio.

- `mouse_sensitivity.txt` · Sensibilidade do mouse, `1.0` por padrão.

- `fps_cap.txt` · Limite de quadros. O F10 grava para você.

- `ram_cache_mb.txt` · Memória para guardar leituras dos packfiles do jogo, em MB (`0` desliga). O padrão depende da sua RAM.

- `gpu_queue.txt` · Quantos buffers de comandos da placa de vídeo podem ficar na fila, `4` por padrão. `0` espera cada buffer (mais lento, para solução de problemas).

- `gpu_max_lag.txt` · Quanto a thread da placa de vídeo pode ficar atrás do jogo, em microssegundos, `4000` por padrão. `0` é sem limite.

- `gpu_timing` · Um arquivo vazio com esse nome registra o tempo da placa de vídeo por quadro e por tipo de trabalho a cada 2 segundos.

- `replay_thread.off`, `upload_batching.off`, `upload_check.off`, `bg_verify.off`, `direct_resolve_off`, `direct_resolve_skip_memory.off`, `rt_fast.off`, `texture_lookup_cache.off`, `keep_cross_transfers`, `msaa_tiling` · Solução de problemas: cada arquivo vazio desliga uma otimização de desempenho. Só serve para procurar a causa de um problema gráfico.
