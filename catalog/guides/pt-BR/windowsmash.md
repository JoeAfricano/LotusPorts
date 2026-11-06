<!-- source: https://github.com/OpenSmash-Ultimate/WindowSmash/blob/HEAD/README.md | fingerprint: daea9ac746e7bfef -->

**O `Install WindowSmash.bat` faz a instalação por você.** Ele instala as ferramentas que o WindowSmash precisa, encontra os arquivos do seu jogo e monta o jogo. A sua parte é ter os arquivos do jogo prontos, abrir o instalador e escolher algumas coisas numa janela. Não precisa saber programar.

O caminho todo de relance:

- Passo · O que acontece · Seu tempo

- **1** · **Conferir o seu PC** com o que é preciso. · 2 minutos

- **2** · **Ter os arquivos do jogo prontos**: o jogo, a atualização 13.0.5 e as DLCs, as suas chaves e o firmware, tirados do seu próprio Switch. · 10 minutos

- **3** · **Baixar o WindowSmash** e extrair. · 5 minutos

- **4** · **Abrir o instalador**: clique duas vezes no `Install WindowSmash.bat`. · 1 minuto

- **5** · **Deixar ele instalar as ferramentas** que faltam: Python, um compilador e o SDL3. · Alguns minutos, quase tudo esperando

- **6** · **Escolher as pastas** e o seu nome online na janela de configuração. · 5 minutos

- **7** · **Esperar a montagem do jogo.** · De 30 minutos a algumas horas

- **8** · **Jogar.** Clique duas vezes no atalho **Play SSBU** na Área de Trabalho.

### Passo 1: confira o seu PC

Compare o seu PC com os requisitos do repositório. (**Configurações → Sistema → Sobre** mostra o tipo de sistema, a RAM instalada e o processador.) O instalador também confere: se faltar algo ao seu PC, como um processador com AVX2 ou espaço livre suficiente, ele para e diz o quê.

### Passo 2: tenha os arquivos do jogo prontos

**Jogou no yuzu ou no Ryujinx neste PC, atualizado para a 13.0.5?** Então os seus arquivos estão prontos: o instalador encontra sozinho as suas chaves, o firmware e o jogo instalado, com a atualização e as DLCs.

**Senão**, coloque os seus arquivos em três pastas. Os nomes são exemplos; qualquer pasta serve:

- Pasta · O que vai nela

- `D:\MyDump` · O jogo: uma cópia do cartão (`.xci`), um `.nsp` ou os arquivos `.nca` dele. O `.nsp` da **atualização 13.0.5**, a não ser que a sua cópia já tenha a 13.0.5 junta. Os `.nsp` das suas DLCs, soltos ou numa pasta chamada `dlc`.

- `D:\MyKeys` · `prod.keys` e `title.keys`, do seu Switch.

- `D:\MyFirmware` · O firmware do seu Switch: algumas centenas de arquivos `.nca`. O WindowSmash tira deles as fontes do jogo.

- **O jogo precisa estar na versão 13.0.5.** Com a atualização 13.0.5 ao lado do jogo, o instalador a aplica para você; o jogo como foi lançado (1.0.0) é a melhor base.

- **Arquivos compactados** (`.nsz`, `.xcz`) precisam voltar a ser `.nsp` ou `.xci` antes.

- **Deixe os arquivos do jogo onde estão** depois da instalação: conforme o tipo, o jogo continua lendo os dados deles.

### Passo 3: baixe o WindowSmash

1. No topo da página do repositório, clique no botão verde **Code** e depois em **Download ZIP**.

2. Clique com o botão direito no `WindowSmash-main.zip`, escolha **Extrair Tudo…**, escolha um disco com uns 60 GB livres (por exemplo `C:\`) e clique em **Extrair**. Você recebe uma pasta `WindowSmash-main`; renomeie para `WindowSmash` se quiser.

> Mantenha o WindowSmash fora da Área de Trabalho e de Documentos: o OneDrive pode tentar enviar tudo, e a versão montada tem uns 30 GB. Com o [Git](https://git-scm.com/download/win), `git clone https://github.com/OpenSmash-Ultimate/WindowSmash.git` faz o mesmo que o download.

### Passo 4: abra o instalador

1. Abra a pasta do WindowSmash e clique duas vezes no **`Install WindowSmash.bat`**.

2. O Windows pergunta se deve rodar (**O editor não pôde ser verificado**): clique em **Executar**. Em alguns PCs aparece uma caixa azul **O Windows protegeu o computador**: clique em **Mais informações** e depois em **Executar assim mesmo**. O Windows pergunta isso uma vez para qualquer coisa baixada sem assinatura.

Uma janela abre e mostra cada etapa. Primeiro, ele busca o WindowSmash mais novo no GitHub, se houver, e confere o seu PC. Tudo o que ele mostra também fica no `install.log` da pasta do WindowSmash.

### Passo 5: deixe ele instalar as ferramentas

O instalador instala só o que falta no seu PC:

- **Python**, que roda os programas auxiliares do WindowSmash. Se o seu PC não tiver, o instalador coloca uma cópia na pasta do WindowSmash; nada mais muda no PC.

- **Um compilador**, que monta o jogo: o clang do LLVM com o runtime C e o Windows SDK da Microsoft, baixados para a pasta do WindowSmash (cerca de 570 MB). As partes da Microsoft seguem os termos de licença da Microsoft: o instalador mostra onde lê-los e pergunta se você aceita; digite `yes` e aperte Enter. Se esse download não funcionar, ele instala o Visual Studio Build Tools, as ferramentas gratuitas da Microsoft: alguns gigabytes, com uma janela de progresso. O Windows pergunta se deve permitir as mudanças: clique em **Sim**. Se pedir para reiniciar, reinicie e clique duas vezes no `Install WindowSmash.bat` de novo.

- **SDL3**, para controles de PlayStation, Switch Pro e a maioria dos outros.

Música e vídeos não precisam de nada a mais: os decodificadores vêm com o WindowSmash e são montados junto com o jogo.

### Passo 6: escolha as pastas

Com as ferramentas prontas, a janela de configuração abre:

- Caixa · O que escolher

- **Game files** · `D:\MyDump`, ou a pasta do yuzu ou do Ryujinx que ele encontrou.

- **Keys** · `D:\MyKeys`, ou a que ele encontrou.

- **Firmware** · `D:\MyFirmware`, ou a que ele encontrou.

As pastas encontradas no PC já vêm preenchidas; troque qualquer uma com **Browse…**. A linha embaixo de cada caixa diz o que há na pasta, por exemplo a atualização do jogo: 13.0.5 e DLC: 99 conteúdos adicionais. Verde quer dizer que está certo; vermelho diz o que falta. Se o jogo não estiver na 13.0.5, a linha avisa: coloque o `.nsp` da atualização 13.0.5 na pasta do jogo e clique em **Check files**.

Depois, siga os passos finais no repositório para montar o jogo e jogar.
