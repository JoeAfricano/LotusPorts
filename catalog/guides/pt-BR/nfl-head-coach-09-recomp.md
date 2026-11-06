<!-- source: https://github.com/tools81/nfl-head-coach-09-static-recomp/blob/HEAD/README.md | fingerprint: 9c556f064b974a09 -->

Baixe o `HC09Setup.exe` na [página de versões (releases)](https://github.com/tools81/nfl-head-coach-09-static-recomp/releases) e abra. O assistente:

1. confere as ferramentas de compilação: Visual Studio 2022 ou mais novo (o Community serve) com "Desktop development with C++", "C++ Clang Compiler for Windows" e "C++ CMake tools for Windows", e o [Git para Windows](https://git-scm.com/download/win);

2. pede uma pasta de instalação (um caminho curto como `C:\Games\HC09Recomp` é melhor) e a imagem do seu disco do NFL Head Coach 09 (`.iso`), que ele confere se é o jogo certo;

3. oferece **conferência automática de atualizações**: ligada, o jogo confere a versão mais recente deste repositório a cada abertura e, se houver uma mais nova, pergunta antes de baixar e remontar;

4. baixa o código, o XenonRecomp e o FFmpeg, importa o executável do jogo da imagem do seu disco, recompila e monta o `HC09.exe` (cerca de 1 GB de downloads e de 10 a 20 minutos) e cria um atalho no menu Iniciar.

O jogo é montado no seu PC porque o executável recompilado é derivado do código do próprio jogo, que este projeto não distribui. A imagem do disco fica onde está: o jogo lê os dados dela enquanto você joga. As configurações ficam no `hc09.ini` da pasta de instalação (`IsoPath`, `AutoUpdate`). Rodar o instalador de novo e escolher a mesma pasta atualiza essa instalação para a versão mais recente (mantendo os saves), e também é o jeito de trocar a imagem do disco. O registro da instalação é o `%TEMP%\HC09Setup.log`.

### Jogar com os elencos de hoje (HC27.exe)

O instalador monta um segundo jogo ao lado do original: o **HC27.exe**, o NFL Head Coach 09 com a liga de hoje. Ele tem os jogadores atuais com notas e contratos, os técnicos atuais, fotos reais de jogadores e técnicos, nomes e logos atualizados dos times e temporadas começando em 2026. Tudo o que ele precisa vem no download (a pasta `hc27` na pasta de instalação), então não há nada para importar ou configurar.

1. Abra a pasta de instalação (por exemplo `C:\Games\HC09Recomp`) e entre em `build\hc09`.

2. Clique duas vezes no **HC27.exe**. Para abrir mais fácil depois, clique com o botão direito e escolha Enviar para > Área de Trabalho (criar atalho). O atalho do menu Iniciar abre o jogo original, HC09.exe.

3. Comece uma carreira nova ou um jogo Coach Now: os elencos novos já estão lá.

Bom saber:

- O HC27 usa a mesma imagem de disco do jogo original, e os dois mantêm saves separados (os do HC27 ficam em `build\userdata-hc27`), então uma carreira num nunca mexe no outro.

- Os elencos estão atualizados até a semana 8 da temporada 2026: trocas, dispensas, contratações e cortes desde que as notas foram tiradas estão aplicados. Jogadores que não têm notas (a maioria dos chamados do "practice squad") faltam.

- As atualizações (automáticas, ou rodando o instalador de novo) também trazem versões novas dos elencos e das imagens.

- Se o HC27 disser que o elenco padrão do disco não está onde ele espera, a sua imagem é de outra versão do jogo; o HC09.exe continua funcionando.

- As escolhas do draft recebem os contratos de calouro de hoje: quando o draft de uma carreira termina, cada escolha assina o contrato de quatro anos correspondente ao número dela, sem negociar com agentes. Os elencos podem passar de 53 jogadores até o dia dos cortes do training camp, como nas pré-temporadas da NFL.

- Escolhas de primeira rodada vêm com a opção de quinto ano. Na pré-temporada antes do quarto ano de uma escolha de primeira rodada, ele aparece na tela de propostas de agente livre restrito com duas opções: recusar a opção ou exercê-la por um quinto ano com o salário de opção da posição. Os times do computador decidem sozinhos. Uma carreira começada antes desta versão precisa definir uma vez as rodadas de draft dos jogadores: `python tools/roster_import/draft_rounds.py --save build/userdata-hc27/content/00000001/CAREER-<name>/CAREER-<name>`.

- O jogo original não muda: o HC09.exe ignora a pasta `hc27`.
