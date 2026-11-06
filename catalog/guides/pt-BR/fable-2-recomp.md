<!-- source: https://github.com/himdo/Fable-2-Recomp/blob/HEAD/README.md | fingerprint: 9ad1ad9fe9632c28 -->

Você precisa da imagem do seu próprio disco do Fable 2 GOTY (`.iso`) e de cerca de 7 GB livres.

1. Baixe o zip `windows` na [versão mais recente](https://github.com/himdo/Fable-2-Recomp/releases/latest) e extraia onde quiser.

2. Abra o `Fable2Launcher.exe`.

3. Clique em **Extract from ISO** e escolha o seu `.iso`. Os arquivos do jogo vão para a pasta do launcher.

4. Clique em **Play Fable II**.

**Atualizar:** extraia a versão nova numa pasta própria, copie a sua pasta `saves` para lá e extraia o seu `.iso` de novo.

**Precisa de ajuda?** Pergunte no [Discord](https://discord.gg/8TQUZpPNeR). Para compilar o jogo você mesmo, veja "Building" no repositório.

### Observações para rodar o jogo

- **Como extrair (embutido, recomendado):** o launcher tem o botão **Extract from ISO**. Clique e escolha a imagem do seu disco (`.iso`). Ele confere antes a ISO com a lista de versões aceitas e, só se bater, extrai o disco (`default.xex` e `data/`) para a pasta do próprio launcher. Não precisa de outra ferramenta.

- **Como extrair (ferramenta externa):** ou copie o disco para uma ISO e abra com o **[XboxImageExtractor](https://github.com/dromex1/XboxImageExtractor)**, uma ferramenta gráfica para imagens de Xbox 360. Ele lista os arquivos da imagem; escolha `default.xex` e `data` e extraia na raiz do projeto (`data` como pasta).

- **Você precisa fornecer o conteúdo do jogo** (ele não está no repositório): copie o seu disco do Fable 2 GOTY (EUA/Europa) (veja os hashes em [German GOTY support](https://github.com/himdo/Fable-2-Recomp/blob/HEAD/docs/GERMAN_GOTY_SUPPORT.md)) e coloque o `default.xex` e a `data/` na raiz do projeto.

- **Os saves ficam em `saves`**: faça uma cópia dessa pasta para guardar o progresso, e copie entre versões ou computadores para levar um save.

- O `--game_data_root <path>` ainda aponta o conteúdo para outra pasta; saves e cache continuam ao lado do exe.

- Embora tenha sido feito para a versão GOTY EUA/Europa, o autor conseguiu rodar estas regiões: Alemanha, França, Itália, Rússia e Espanha. Os resultados podem variar nelas.

### Rodar

Para rodar o jogo:

1. Extraia uma versão baixada do port.

2. Extraia a "data" e o "default.xex" da sua cópia do jogo (o mais fácil é com o botão **Extract from ISO** do launcher).

3. Abra o `fable_2.exe`; se nenhum erro aparecer, o jogo abre e está tudo certo.

- Se aparecer um erro dizendo que o hash está errado, tente extrair outra versão do jogo e tente de novo.

Opções de linha de comando:

- Argumento · Efeito

- `--game_data_root <path>` · Usa outra pasta de conteúdo

- `--update_data_root <path>` · Pasta opcional de atualização

- `--window_width` / `--window_height` / `--fullscreen` · Opções de janela

- `--keyboard_gamepad_map <map>` · Mapeamento de teclas do teclado para botões do controle

- `--skipHash` · Pula a conferência SHA-256 do `default.xex`. O hash ainda vai para o registro, mas uma diferença não bloqueia mais a abertura (útil para um XEX modificado ou fora da lista)

### Configuração do usuário (fable2_config.toml)

O recomp tem o próprio arquivo de configuração legível, o `fable2_config.toml`, ao lado do exe, separado do `fable_2.toml` (a configuração do ReXGlue SDK). Ele é criado uma vez (as suas edições sobrevivem às recompilações) e o jogo o recria com os padrões se ele sumir.

- Chaves que faltam usam os padrões; tipos errados ou seções desconhecidas geram avisos no registro e usam os padrões; um erro de sintaxe mostra uma janela e usa os padrões (nunca bloqueia a abertura).

- As configurações da seção `[input]` (`keyboard_gamepad_map`, `mouse_look`, `mouse_look_scale`) só valem quando nenhuma fonte de prioridade maior as definiu: o `fable_2.toml`, as variáveis de ambiente `REX_*` e a linha de comando continuam ganhando, e o console do F3 ainda muda o valor ao vivo.

- A seção `[patches]` tem chaves para ligar e desligar os patches do recomp a cada abertura, sem recompilar. Isso inclui `high_tick_rate` e `higher_hf_tick_rate` (ligue os dois juntos).

### Controles de teclado

O jogo normalmente lê um controle pela API do Xbox 360. Um "controle de teclado" sintético é acrescentado, então as teclas do teclado comandam o jogo junto com um controle de verdade, que continua funcionando; o teclado só acrescenta botões.

O mapeamento é a opção `keyboard_gamepad_map`, no formato `Tecla:Botão,Tecla:Botão,...`. O padrão vem de `[input] keyboard_gamepad_map` no `fable2_config.toml`, que você pode editar para mudar de vez. A linha de comando e o console do F3 ainda mudam por abertura ou ao vivo.

- **Tecla**: um nome de tecla (`E`, `Space`, `LeftShift`, `F1`, ...) ou um botão do mouse: `LMB` (esquerdo), `RMB` (direito), `MMB` (meio) e `XMB1`/`XMB2` (laterais).

- **Botão**: um comando do controle: `A`, `B`, `X`, `Y`, `LB`/`RB` (ombros), `LT`/`RT` (gatilhos), `Up`/`Down`/`Left`/`Right` (direcional), `Pause` (Start), `Select` (Back), `L3`/`R3` (cliques dos analógicos) e `StickUp`/`StickDown`/`StickLeft`/`StickRight` (analógico esquerdo, todo inclinado enquanto segura).

O layout padrão (mouse mais teclado):

- Tecla(s) · Comando

- `LMB` · `X`

- `RMB` · `Y`

- `MMB` / `B` · `B`

- `Shift` / `E` · `A`

- `W` `A` `S` `D` · Analógico esquerdo (cima / esquerda / baixo / direita)
