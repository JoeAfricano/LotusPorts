<!-- source: https://github.com/Comfubar/KnowYourRoleRecomp/blob/HEAD/README.md | fingerprint: f2be8c7c3f62fa11 -->

**[Baixe a versão mais recente (KnowYourRole-win-x64.zip)](https://github.com/Comfubar/KnowYourRoleRecomp/releases/latest/download/KnowYourRole-win-x64.zip)**

1. Extraia o zip numa pasta onde dê para gravar, por exemplo `C:\Games\KnowYourRole`.

2. Clique duas vezes no **KnowYourRole.exe**.

3. Escolha o arquivo `.cue` da imagem do seu disco (o `.bin` ao lado é encontrado sozinho). O disco é conferido, o jogo é montado (cerca de três minutos) e depois abre. Nas próximas vezes, o KnowYourRole.exe abre o launcher: clique em **PLAY**.

Você precisa da imagem do seu próprio disco NTSC-U, número de série SLUS-01234.

Instalação em detalhes:

- **Conferir o download** (opcional): a versão também traz o `SHA256SUMS.txt`. Num prompt de comando na pasta do download, rode `certutil -hashfile KnowYourRole-win-x64.zip SHA256` e compare com a linha do arquivo.

- **Onde extrair**: uma pasta onde dê para gravar, por exemplo `C:\Games\KnowYourRole`. Não em `Arquivos de Programas`: o Windows não deixa programas gravarem lá, e o jogo guarda os saves na própria pasta. Nem numa pasta sincronizada pelo OneDrive (muitas vezes `Documentos` e `Área de Trabalho`): o OneDrive enviaria o jogo montado (cerca de 250 MB, com o código montado a partir do seu disco, que não pode ser compartilhado). A instalação avisa quando a pasta está dentro do OneDrive.

- **A pasta**: `KnowYourRole.exe` (o programa que você abre) e `README.txt`; todo o resto fica em `KnowYourRole-files\` (`runtime\` com o jogo, `user\` com saves, configurações e registros, e os arquivos de licença).

- **Primeira abertura**: a instalação pede a imagem do disco, confere o disco e o PC, monta o jogo (com barra de progresso) e abre. Quando você fecha o jogo, o launcher aparece.

- **SmartScreen**: o Windows pode dizer "O Windows protegeu o computador" porque o programa não tem assinatura de um editor conhecido. Escolha "Mais informações" e "Executar assim mesmo".

- Deixe a imagem do disco onde está: o jogo lê dela enquanto roda. Se você mover, o launcher pede de novo.

### O launcher

- **PLAY** abre o jogo. O launcher volta quando o jogo fecha.

- **Controllers**: um cartão por jogador.

- **Display**: tela cheia ou janela, VSync.

- **Game files**: trocar o disco / montar o jogo de novo, abrir a pasta dos saves, reunir diagnósticos, criar um atalho na Área de Trabalho e "abrir o jogo direto da próxima vez" (pula o launcher; `KnowYourRole.exe --launcher` abre de novo). O atalho da Área de Trabalho abre o jogo direto (`KnowYourRole.exe --play`).
