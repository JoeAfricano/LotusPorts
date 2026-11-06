<!-- source: https://github.com/luckvj/streetball-revival/blob/HEAD/README.md | fingerprint: 089409103e2d5fc9 -->

1. **Faça uma imagem de disco da sua própria cópia** do jogo: a versão americana de PlayStation 2, número de série **SLUS-20651** (o título completo está no [LEGAL.md](https://github.com/luckvj/streetball-revival/blob/HEAD/LEGAL.md)). Salve como arquivo `.iso` em qualquer lugar do PC.

2. **Baixe** o `StreetballRevival-…-windows.exe` na versão mais recente da página de [versões (Releases)](https://github.com/luckvj/streetball-revival/releases) deste repositório: o jogo inteiro num arquivo só. (Um zip do mesmo jogo, e um para Linux, também estão lá. Os arquivos "Source code" não são o jogo.)

3. **Abra**, clique em **Browse** e escolha o seu `.iso` (ou arraste para a janela). O launcher confere se é a versão certa e lembra dela.

4. Clique em **Play**.

Se o Windows disser "O Windows protegeu o computador", clique em **Mais informações** e depois em **Executar assim mesmo** (os arquivos deste projeto gratuito não têm assinatura). A página da versão cobre controles, configurações e solução de problemas.

O launcher guarda o local do disco em `Documentos\Streetball Revival`, ao lado do seu save. Troque quando quiser com **Change disc**.

Um controle é opcional: PlayStation, Xbox e a maioria dos controles reconhecidos pelo SDL funcionam e podem ser conectados a qualquer momento, e o teclado também funciona.

### Jogar online

> **Em desenvolvimento.** O jogo online é novo e ainda precisa de testes. Espere imperfeições: quedas de conexão, dessincronias ou problemas no lobby. Se acontecer, conte no [Discord](https://discord.com/invite/htu5pdxPQz) ou numa issue, dizendo o que estava fazendo e se você criou ou entrou na partida.

No menu principal, aperte **Select** (**P** no teclado) para **Online**.

- **Host 1 vs 1** ou **Host 3 vs 3**. O lobby mostra um **código** como `7QK3-M2X`; mande para os seus amigos. O computador joga nas vagas que ninguém pegar.

- Amigos na mesma rede veem a partida em **Join On Network**; os outros usam **Join With Code**.

- Cada um precisa da própria imagem de disco da mesma versão. Só os botões apertados (e uma conferência de que todos os discos batem) passam pela rede.

O jogo de quem cria a partida pede ao roteador para abrir a porta UDP 7375 (UPnP). Se os amigos não conseguirem conectar, o [docs/PLAYING.md](https://github.com/luckvj/streetball-revival/blob/HEAD/docs/PLAYING.md) explica o redirecionamento de portas e a alternativa por VPN.
