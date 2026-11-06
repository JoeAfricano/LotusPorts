<!-- source: https://github.com/Vnnc1/gow-native-pc/blob/HEAD/README.md | fingerprint: 16540ef31a1bd0b6 -->

1. Pegue o launcher:

- **Instalador (o mais fácil):** baixe o `gow-native-pc-setup.exe` na página de [versões (Releases)](https://github.com/Vnnc1/gow-native-pc/releases), abra e clique em **INSTALL** (sem administrador, só para o seu usuário). Ele cria atalhos no menu Iniciar e na Área de Trabalho e uma entrada em Aplicativos do Windows para desinstalar.

2. No launcher → aba **Install**:

1. **Download tools**: cópias portáteis são baixadas uma vez, com conferência de SHA-256, sem mudar o seu PATH;

2. **Add ISO / CHD…**: escolha uma ou mais imagens de disco compatíveis; o launcher identifica o jogo e o idioma;

3. **Install and build**: baixa o runtime, prepara o disco e compila o jogo. Um perfil PGO correspondente é usado quando disponível; a 0.5.0 não traz um. Deixe **Prepare the first play (recommended)** ligado: uma cópia do jogo joga sozinha por quatro minutos, **escondida e sem som**, registrando o código VU1/IOP que usa, e esse código é compilado junto.

3. Escolha um **perfil gráfico** (página Graphics), confira **Controls** se for jogar no teclado e clique em **PLAY**.

A importação de CHD confere e extrai a imagem para um cache ISO local. Precisa de espaço extra. A imagem asiática com o executável idêntico ao americano foi montada e testada. As imagens japonesa e europeia exatas agora têm montagens próprias para Windows, testadas até o início do jogo; as montagens registradas podem ser escolhidas no launcher. Elas não usam a recompilação/PGO americana. Demos ficam de fora; a identificação segue o executável, não o nome do arquivo. Veja [formatos de disco e compatibilidade regional](https://github.com/Vnnc1/gow-native-pc/blob/HEAD/docs/disc-import.md).

O tempo de montagem depende do processador, da RAM, do armazenamento e dos downloads; as cerca de 6.400 funções recompiladas do jogo são compiladas no seu PC. No PC de teste (Xeon E5-2666 v3, 20 threads, 16 GB), a instalação toda levou **cerca de 30 minutos** quando ainda treinava o próprio perfil PGO (0.3.x). Um perfil correspondente pronto pode evitar esse treino, mas a v0.5.0 vem sem um. Num processador de notebook de 2 núcleos/4 threads (i3-10110U), uma instalação levou **cerca de 75 minutos**. São referências históricas, não prazos. Um jogo instalado com uma versão antiga mostra **"FPS optimisation recommended (PGO)"** na página inicial.

### Pelo terminal

Rode estes comandos na pasta do repositório (no PowerShell, use `.\build.cmd`):

```
build.cmd -Discs "D:\God of War (USA).iso" -Stages tools,fetch,discs,configure,recomp,sync,tune
```

É o que o launcher faz: ferramentas, runtime, preparação do disco, recompilação e a etapa `tune` (sessão de gravação + montagem final com o perfil PGO da versão; sem um perfil adequado, ele também treina um). Sem a preparação da primeira partida, use o padrão `build.cmd -Discs "D:\God of War (USA).iso"`. Para vários idiomas, passe `-Discs "D:\usa.iso|D:\ptbr.iso"`. Mais exemplos: [build.cmd](https://github.com/Vnnc1/gow-native-pc/blob/HEAD/build.cmd). `build.cmd -Stages verify` roda os testes de regressão sem disco.
