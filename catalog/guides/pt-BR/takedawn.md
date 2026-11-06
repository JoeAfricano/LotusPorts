<!-- source: https://github.com/xenophobentx/takedawn/blob/HEAD/README.md | fingerprint: 7d5fc0867c5ddbf8 -->

1. Faça uma cópia do seu próprio disco. O Takedawn aceita três versões do jogo, os discos de Xbox original da América do Norte (NTSC-U), da Europa (França, Alemanha, Itália) e da Europa e Austrália (inglês, espanhol, holandês, sueco). Ele aceita um `.iso` no formato XISO, uma imagem de disco completa com a partição de vídeo primeiro ou uma pasta com os arquivos extraídos. O Takedawn não fornece imagens de disco nem indica onde obtê-las.

2. Pegue o Takedawn para o seu aparelho na seção Download e siga a instalação do seu sistema.

3. Abra. Enquanto faltarem os dados do jogo, o Takedawn mostra a janela de configuração. Escolha a imagem ou a pasta do seu disco, ou arraste para a janela. O Takedawn diz na hora se é o jogo e a versão certos. No Linux e no Steam Deck, ele também procura o disco nos lugares comuns e oferece o que encontrar. Clique em Install. Ele copia os dados para a sua pasta de usuário, ou para outra pasta escolhida em Change..., e confere cada um dos 716 arquivos com uma lista de tamanhos e somas SHA-256 conhecidos. Isso levou de 3 a 10 segundos nos computadores testados e no Steam Deck. Depois, você pode apagar a imagem do disco.

4. Clique em Play. Com a caixa "Prepare graphics" marcada (o padrão), a preparação dos shaders roda antes. No jogo, abra as configurações com Back e Start juntos no controle, ou F1 no teclado.

Saves e configurações ficam separados do programa e dos dados do jogo, então uma atualização não mexe neles.

### Como atualizar

As atualizações são manuais. No computador, Settings > System > Check for updates diz se há versão nova, e a página System mostra qual versão você tem. Baixe a nova na página de versões (releases).

- Mac: feche o Takedawn, arraste o novo para Aplicativos e escolha Substituir; depois, permita mais uma vez com Abrir Mesmo Assim.

- Windows: rode o instalador novo. Ele atualiza no lugar e guarda uma cópia da versão anterior.

Em todos os sistemas, uma atualização mantém saves, configurações e os dados do jogo instalados, e não precisa da imagem do disco de novo.

### Instalação no Windows

1. Rode o instalador. Ele não tem assinatura digital, então o SmartScreen mostra "O Windows protegeu o computador" com editor desconhecido. Clique em Mais informações e depois em Executar assim mesmo. O Windows pode perguntar o mesmo mais uma vez para o próprio programa. Alguns antivírus marcam o download pelo mesmo motivo; veja [Avisos de antivírus no Windows](https://github.com/xenophobentx/takedawn/blob/HEAD/docs/antivirus.md).

2. O instalador instala só para a sua conta de usuário e não precisa de administrador. Ele coloca o programa em `%LOCALAPPDATA%\Programs\Takedawn`, cria uma entrada no menu Iniciar e oferece um ícone na Área de Trabalho, desmarcado a não ser que você marque.

3. Abra o Takedawn pelo menu Iniciar. Na janela de configuração, escolha a imagem ou a pasta do seu disco, ou arraste para a janela, e clique em Install e depois em Play.

Não há zip portátil. Os dados do jogo vão para `%LOCALAPPDATA%\Takedawn\Game`, ou para a pasta escolhida em Change..., e os saves para `%APPDATA%\Takedawn`.

Para guardar tudo ao lado do programa, num pen drive ou num segundo disco, instale numa pasta à sua escolha (o instalador pergunta onde) e crie um arquivo vazio chamado `portable.txt` ao lado do `Takedawn.exe`, ou uma pasta chamada `data` ali. Na próxima abertura, saves, `settings.toml`, registros, cache de shaders e dados do jogo ficam em `data\Saves`, `data\cache` e `data\Game` ao lado do `Takedawn.exe`, e nada vai para `%APPDATA%` ou `%LOCALAPPDATA%`. O registro diz `portable mode` quando está ligado. O Takedawn não move o que você já tem: copie a sua pasta de saves antiga para `data\Saves` se quiser. Remova o marcador (e a pasta, se criou) para voltar.

Num modo de tela de 24 Hz ou 30 Hz, o jogo não mostra 60 fps. Use 60 Hz ou mais.

Para remover: desinstale o Takedawn em Configurações > Aplicativos. O desinstalador deixa os dados do jogo e os seus saves e diz onde estão. Apague `%LOCALAPPDATA%\Takedawn` e, se não quiser mais os saves, `%APPDATA%\Takedawn`.
