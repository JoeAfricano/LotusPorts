<!-- source: https://github.com/chasem-dev/sms-launcher/blob/HEAD/README.md | fingerprint: a62595a563309648 -->

### Windows

Abra o instalador `.exe` e siga os passos; depois, abra o SMS Launcher.

**Versão portátil:** coloque o `SMS-Launcher-<version>-win-portable.exe` em qualquer pasta, como num pen drive, e abra. Nada é instalado. Na primeira vez, ele cria uma pasta `SMS-Launcher-Portable` ao lado do `.exe` e guarda tudo ali: configurações, arquivos de instalação, ferramentas de montagem, o jogo, os caches de shaders e texturas HD, saves e cópias dos saves. Ele não pergunta onde instalar. Você pode mover ou copiar o `.exe` e essa pasta juntos, para outro disco ou outro computador, e continuar jogando sem instalar de novo. O jogo lê o seu disco sempre que roda, então mantenha o arquivo do disco ao lado do `.exe` ou na `SMS-Launcher-Portable` e leve junto: se o disco escolhido não estiver no computador novo, o launcher usa o que encontrar ali. Senão, escolha o disco de novo em Settings. As versões portáteis não se atualizam sozinhas. Para atualizar, baixe o `.exe` portátil novo e coloque no lugar do antigo, ao lado da mesma pasta `SMS-Launcher-Portable`. Cada abertura portátil leva alguns segundos enquanto o launcher se desempacota.

### Prepare o seu jogo

O launcher guia você por três passos:

1. **Baixar os arquivos de instalação.** Use o local sugerido, ou escolha outra pasta antes de baixar.

2. **Escolher a imagem do disco.** Escolha uma cópia do seu próprio disco original norte-americano do Super Mario Sunshine (**GMSE01, revisão 0**). Arquivos aceitos: `.iso` (inclusive NKit), `.gcm` e `.ciso` do Dolphin. O launcher confere o conteúdo ao escolher: ele identifica imagens RVZ, WIA e GCZ (converta para um `.iso` completo antes), arquivos compactados, outras regiões e outros jogos, e pega uma cópia incompleta antes de montar qualquer coisa.

3. **Começar a instalação.** O launcher baixa as texturas HD e prepara o jogo no seu computador. Essa primeira preparação pode demorar.

No Mac, a instalação pode pedir as Command Line Tools da Apple e, no Apple Silicon, o Rosetta. Siga o **Mac setup help** do launcher.

A barra de progresso mostra o que está acontecendo. **View build log** mostra mais detalhes; fechar essa tela não para a instalação.

Quando terminar, clique em **Play**.
