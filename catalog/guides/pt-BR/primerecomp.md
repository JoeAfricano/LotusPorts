<!-- source: https://github.com/gminustwo/primerecomp/blob/HEAD/README.md | fingerprint: 31a7d3583ec78b73 -->

### Windows

1. Baixe o `PrimeRecomp-v0.0.7-windows.zip` na página de [versões (Releases)](https://github.com/gminustwo/primerecomp/releases) e extraia num lugar onde dê para gravar (por exemplo Documentos ou uma pasta de jogos, não em Arquivos de Programas).

2. Abra o `primerecomp.exe`. A tela inicial pede os arquivos do seu jogo: escolha a pasta que tem `exefs` e `romfs` (ou, em **Or its game package**, o arquivo NSP / XCI do jogo) e clique em **Set up**. Ele copia o que precisa e recompila o jogo no seu PC. Isso acontece uma vez e leva alguns minutos. Um pacote de jogo não é copiado: o jogo roda a partir dele, então deixe-o onde está (se mover, o PrimeRecomp pede para configurar de novo).

3. Clique em **Play**.

Tudo o que o PrimeRecomp grava (arquivos do jogo, saves, configurações em `primerecomp.ini`, registros) fica na própria pasta, menos uma nota curta de onde essa pasta está (`%LOCALAPPDATA%\PrimeRecomp\copies.txt`), para as versões novas a encontrarem.

### Atualizar

A partir da v0.0.8, a tela inicial confere se há versão nova e oferece: **UPDATE TO ...** abaixo do PLAY. No Windows e no AppImage (Linux, Steam Deck), **Update now** baixa e instala no lugar (os seus atalhos na Steam e no Playnite continuam funcionando) e o PrimeRecomp reabre sozinho; no macOS, ele abre a página de download. A conferência só pergunta ao GitHub pela versão mais recente; desligue em Settings > Prelaunch (Check for updates).

À mão, uma versão nova não precisa dos arquivos do jogo de novo e mantém saves e configurações:

- **Windows:** extraia o zip novo onde quiser, como da primeira vez, e abra. Numa pasta nova, ele encontra sozinho a sua cópia anterior (v0.0.3 ou mais nova) e usa os arquivos do jogo, saves e configurações dela. Os arquivos do jogo não são copiados de novo: no mesmo disco, as duas cópias os compartilham, então a atualização não ocupa espaço extra, e você pode apagar a pasta antiga depois. Extrair o zip novo por cima da pasta antiga do PrimeRecomp (substituindo os arquivos) também funciona. **Da v0.0.2 para uma pasta nova:** na tela inicial, escolha a pasta antiga do PrimeRecomp como pasta dos arquivos do jogo; ela é usada do mesmo jeito (os saves vêm junto, as configurações não).

Quando uma versão nova muda o jeito de recompilar o jogo, a tela inicial recompila sozinha uma vez na primeira abertura da versão nova (alguns minutos; mais no Steam Deck). Nada precisa ser escolhido ou configurado de novo.

### Abrir por um launcher (Playnite, Steam, ...)

Dois argumentos abrem o PrimeRecomp sem a tela inicial, para o atalho de um launcher de jogos (Playnite: as Actions do jogo; Steam: Propriedades → Opções de inicialização; o AppImage também aceita):

- `--play` vai direto ao jogo. Com **Pre-load shaders** ligado (aba Video), os shaders carregam junto com o jogo em vez de antes.

- `--preload-shaders` monta antes os shaders do jogo, com uma barra de progresso (B / Esc: joga na hora), e depois abre o jogo, com o Pre-load shaders ligado ou não. Use no lugar do `--play` para uma primeira visita a qualquer lugar sem travadinhas.

Enquanto os arquivos do jogo não estiverem configurados, os dois abrem a tela inicial normalmente.
