<!-- source: https://github.com/VCKing101/Dissidia012-pc/blob/HEAD/README.md | fingerprint: 3d3cc06974924028 -->

**1. Baixe** o `Dissidia012-PC-Setup-<version>.exe` na [versão mais recente](https://github.com/VCKing101/Dissidia012-pc/releases). Não precisa instalar mais nada antes.

**2. Tenha o seu jogo pronto:** a imagem do seu próprio disco americano (**ULUS10566**) ou europeu (**ULES01505**), num arquivo `.iso`. As versões japonesa e asiática não funcionam.

**3. Rode o instalador.** Ele não tem assinatura digital, então o Windows pode dizer "O Windows protegeu o computador": escolha **Mais informações** e depois **Executar assim mesmo**. O assistente pergunta onde instalar (sem administrador) e pede a sua ISO; depois, prepara o jogo a partir dela: descriptografa o programa do jogo e compila o código para o seu PC com o compilador que vem junto. Leva alguns minutos (uns 4 num PC recente de 8 núcleos). Nada é baixado, e nada feito a partir do seu disco sai do seu PC. Marque **Battle BGM Maker** se quiser criar os seus próprios mods de música.

**4. Jogue** pelo atalho "Dissidia 012 (PC port)" no menu Iniciar (e na Área de Trabalho, se escolheu). Esc, ou L3 + R3 no controle, abre o menu do port. Os saves ficam em `data\ms0`, na pasta de instalação, e os mods vão em `data\mods` (o menu Iniciar tem um atalho para ela).

**Para atualizar:** rode o instalador da versão nova e instale na mesma pasta. Saves, configurações e mods ficam, e o código do jogo só é compilado de novo quando a versão nova precisa. O jogo avisa quando há uma.

**Se travar ou fechar sozinho,** [abra uma issue](https://github.com/VCKing101/Dissidia012-pc/issues/new/choose) e anexe o registro, `data\logs\D012Native.log` na pasta de instalação. Abrir o jogo de novo renomeia esse arquivo para `D012Native-previous.log`, então anexe esse se for o caso. Uma falha escreve onde aconteceu no fim do registro.

Você precisa do Windows 10 ou 11 (64 bits), de um driver de vídeo com Vulkan (qualquer placa de 2016 em diante) e de cerca de 2,5 GB livres.
