<!-- source: https://github.com/OriginalSwayze/beyond-oasis-recomp/blob/HEAD/README.md | fingerprint: 916fe40df7209bf5 -->

**Você precisa da sua própria cópia do jogo:** Beyond Oasis (USA), ou The Story of Thor da Europa, França, Alemanha, Espanha ou Japão. Ela não vem junto e nunca virá, e não há um .exe do jogo pronto para baixar: o jogo montado traz o código do próprio jogo traduzido para C, então só pode ser feito no seu PC a partir da sua cópia. O instalador faz isso para você. Ele precisa do Windows 10 ou 11 de 64 bits e de internet na primeira instalação.

### Instalar

1. Baixe o `Beyond-Oasis-Setup-<version>.exe` na página de [versões (Releases)](https://github.com/OriginalSwayze/beyond-oasis-recomp/releases).

2. Abra. Se o SmartScreen disser que protegeu o computador (o instalador não tem assinatura digital), escolha **Mais informações** e depois **Executar assim mesmo**.

3. **Boas-vindas:** leia o que ele vai fazer e clique em **Next**.

4. **Pasta de instalação:** mantenha a padrão (`%LOCALAPPDATA%\Programs\Beyond Oasis`, sem precisar de administrador) ou escolha outra com **Browse** e clique em **Next**. Reserve cerca de 700 MB.

5. **Seus arquivos:** indique com **Browse** o seu arquivo do jogo (`.md`, `.bin` ou `.gen`, ou um `.zip` com um deles), ou arraste para a janela. Escolha mais de um, de países diferentes, para trocar de idioma no jogo; **Add** escolhe mais. Cada um é conferido na hora pelo SHA-1: um sinal verde com o idioma quer dizer que o instalador o conhece; senão, ele diz o motivo (um hack, uma versão desconhecida, uma cópia `.smd` com cabeçalho). Depois, clique em **Install**.

6. **Instalando:** o instalador baixa as ferramentas de compilação dos autores ([Node.js](https://nodejs.org/), [CMake](https://cmake.org/), [Ninja](https://ninja-build.org/) e [LLVM-MinGW](https://github.com/mstorsjo/llvm-mingw), cerca de 260 MB, cada uma conferida com um SHA-256 fixo) para a pasta de instalação, a não ser que já estejam no seu PC, depois recompila o seu arquivo para C e monta o jogo. Leva alguns minutos; a barra e as linhas embaixo mostram o andamento.

7. **Pronto para jogar:** escolha se quer um atalho na Área de Trabalho e abrir o jogo, e clique em **Finish**. O Beyond Oasis fica no menu Iniciar dali em diante.

O jogo, `BeyondOasis.exe`, fica na pasta de instalação com o `settings.ini`, os saves e os estados salvos. O instalador copia o seu arquivo para a pasta `rom` dela, então o jogo continua funcionando se você mover ou apagar o original.

**Atualizar:** rode o instalador mais novo. Ele encontra a instalação, oferece o arquivo usado e monta o jogo de novo; configurações, saves e estados salvos ficam como estão. (Configurações > Aplicativos > Beyond Oasis Recomp > Modificar faz o mesmo com o instalador que você tem.)

**Remover:** Configurações > Aplicativos > Beyond Oasis Recomp > Desinstalar. Ele remove o jogo, as ferramentas e os atalhos, e pergunta se quer manter configurações, saves e estados salvos. O seu arquivo do jogo em outro lugar nunca é tocado.

**Se a instalação parar**, o instalador mostra o motivo, e **Back** deixa tentar de novo. As causas comuns: sem internet enquanto ele baixa as ferramentas, ou um programa de segurança segurando um arquivo recém-gravado (tente de novo, ou crie uma exceção para a pasta de instalação).

### Sem o instalador

O `Beyond-Oasis-Recomp-<version>.zip`, na mesma página, traz os mesmos arquivos para montar à mão (não o "Source code": ele deixa de fora o código de terceiros que a montagem precisa). Extraia num lugar de caminho curto, clique duas vezes no **Build Beyond Oasis.bat** e escolha o seu arquivo quando pedir. Ele baixa as mesmas ferramentas para uma pasta `toolchain` ao lado (depois de perguntar), monta o jogo em `build/cmake/BeyondOasis.exe` e coloca um atalho **Beyond Oasis** na Área de Trabalho. Para atualizar, extraia a versão nova por cima da pasta antiga e rode de novo.

### Uso (montagem totalmente à mão)

Você precisa de:

- Windows 10 ou 11 (o launcher e os menus usam as fontes do Windows)

- [Git](https://git-scm.com/), [Node.js](https://nodejs.org/) 18 ou mais novo, [CMake](https://cmake.org/) 3.20 ou mais novo, [Ninja](https://ninja-build.org/) e [Clang](https://releases.llvm.org/)

- A cópia do seu próprio cartucho de uma ou mais versões (os SHA-1 estão em `config/versions.json`). Ela não vem junto e nunca virá.

Clone com os submódulos (o código de terceiros), senão a montagem não os encontra:

```
git clone --recursive <this repository's URL>
```

(Já clonou sem eles? Rode `git submodule update --init --recursive`.)

Depois, na pasta do repositório:

```
node tools/setup.mjs "<path to a ROM>" ["<another version's ROM>" ...]
node tools/disasm.mjs
cmake -S . -B build/cmake -G Ninja -DCMAKE_C_COMPILER=clang -DCMAKE_CXX_COMPILER=clang++
cmake --build build/cmake
build/cmake/BeyondOasis.exe "<path to ROM>"
```

O `setup.mjs` confere cada arquivo, descobre a versão e lembra onde ele está (`local.json`, nunca vai para o repositório); a montagem recompila cada versão configurada em `generated/<id>/` (também fora do repositório), tudo num jogo só.

Controles padrão (todos remapeáveis):

- Ação · Teclado · Controle (layout Xbox)

- Andar · Setas · Direcional (o analógico esquerdo sempre funciona também)

- A / B / C · Z / X / C · X / A / B

- Start · Enter · Start

- Voltar no tempo (segurar) · Backspace · LB

- Menu de pausa · Esc · Back (Guide e Esc sempre funcionam também)

Teclas fixas: F2 salva estado, F4 carrega, F6/F7 espaço anterior/próximo (1-9, arquivos em `states/`), F5 tela larga liga/desliga, F11 tela cheia. As configurações ficam no `settings.ini` ao lado do executável.
