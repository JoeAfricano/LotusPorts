<!-- source: https://github.com/Zorkats/G-Diffuser/blob/HEAD/README.md | fingerprint: 58e2013e82b4e90e -->

> **Você precisa usar a sua própria cópia legal do F-Zero X. O G-Diffuser não distribui e nunca vai distribuir arquivos de jogos protegidos por direitos autorais.**

1. **Tenha os arquivos do jogo que você possui.** O G-Diffuser precisa da cópia do F-Zero X **US rev0**. Ela precisa estar na ordem de bytes **big-endian (`.z64`)**: o carregador não converte, então uma cópia `.n64` ou `.v64` precisa ser convertida para `.z64` antes, senão é recusada. A configuração encontra sozinha um arquivo chamado `baserom.us.rev0.z64` ao lado do executável; outro nome também funciona, desde que você o indique no assistente da primeira abertura com **Browse**; ele é copiado com o nome padrão.

2. **Baixe** uma versão do G-Diffuser para o seu sistema, ou compile você mesmo.

3. **Abra** o executável. Na primeira abertura, a tela de configuração encontra os arquivos que já estão ao lado do executável e deixa você revisar, trocar ou escolher o arquivo do jogo, a imagem de disco do Expansion Kit e a ROM IPL do 64DD. A extração só começa depois de você clicar em **Build game data and continue**. Tudo fica ao lado do executável, então as próximas aberturas vão direto ao jogo.

4. **Jogue.** Aperte **F1** a qualquer momento para abrir o menu de melhorias.

A abertura cria as pastas aceitas, sem substituir os seus arquivos: `mods/~romhacks/` na pasta de dados do jogo, e `music/` e `exports/machines/` ao lado do executável. Use `music/` para trocar músicas do jogo e para músicas escolhíveis no Course Edit. A exportação de máquinas fica em **F1 > Workshop > Tools**.

**Atualizar.** A partir da 1.2, as versões para Windows, Linux e macOS se atualizam pelo menu de melhorias. O Android instala versões novas pelo APK. Vindo da 1.1.0 ou anterior, atualize à mão: os saves ficam em `saves/` ao lado do executável, e toda versão extrai numa pasta chamada `G-Diffuser`. Para atualizar, extraia a versão nova no mesmo lugar da instalação atual, substituindo os arquivos: `saves/`, o arquivo do jogo e os dados ficam onde estão. No Windows, o **Extrair Tudo** sugere uma pasta nova com o nome do zip; troque o destino para a pasta que contém a sua `G-Diffuser`.

### Arquivos do Expansion Kit (obrigatórios)

O G-Diffuser é um port da experiência completa do Expansion Kit (Course Edit, as copas do DD e o sistema de saves em disco são recursos centrais, não complementos), então estes dois arquivos, da sua própria cópia, são obrigatórios junto com o arquivo do jogo:

- Arquivo · O que é

- `baserom.translated.ek.ndd` · A imagem de disco do Expansion Kit traduzida por fãs (tradução para inglês de Zoinkity, adaptada à imagem de disco do 64DD por LuigiBlood). O carregador também aceita `baserom.jp.ek.ndd` para o disco japonês original. Uma imagem comercial do 64DD tem ~64,9 MB.

- `N64DDIPLROM.n64` · Uma cópia da ROM IPL / do leitor do 64DD (~4 MB). Traz a fonte embutida do leitor. A cópia do protótipo americano também é aceita com o próprio nome, `64DD_IPL_US_MJR.n64`, sem precisar renomear.

Coloque-os ao lado do arquivo do jogo (ou indique no assistente da primeira abertura). A configuração não termina sem eles.

### Sobre o uso de IA

O autor conta que usou modelos de linguagem (IA) para entender melhor o funcionamento do F-Zero X e para escrever parte do código: cerca de 70% foi escrito por ele e 30% por IA, sempre revisado e testado por ele, com cada versão passando por testes. Todas as decisões de projeto foram dele, a partir de um documento de requisitos feito à mão, e o port foi testado por ele e pelo irmão em jogatinas completas. O projeto só foi possível graças à decompilação de inspectrdc. Um projeto 100% feito à mão deve aparecer quando a decompilação chegar a 100%. O autor é contra arte ou recursos gerados por IA e pede que quem não quiser jogar um projeto feito com ajuda de IA simplesmente não jogue, sem hostilidade.
