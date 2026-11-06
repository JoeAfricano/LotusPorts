<!-- source: https://github.com/JonathanDC64/FFCCEOT-Recomp/blob/HEAD/README.md | fingerprint: 2985ec8e78ed9499 -->

1. Baixe o pacote do seu sistema na página de versões (Releases).

- **Windows:** extraia numa pasta onde dê para gravar (não em Arquivos de Programas) e abra o `ffcc.exe`.

2. Escolha o seu arquivo do jogo (ou arraste para a janela). Ele é conferido, uma cópia fica na pasta de dados e o jogo é montado com o compilador que vem no pacote. Isso leva cerca de um minuto, uma vez; uma atualização que muda o que é gerado remonta sozinha a partir da cópia guardada.

3. Escolha uma predefinição na janela de boas-vindas (Enhanced é o jeito recomendado) e clique em Play.

A tela inicial avisa quando sai uma versão nova (ela consulta o GitHub uma vez ao abrir e só envia o nome e a versão do programa; desligue em Settings, Interface). Ela nunca baixa nada sozinha: o botão Download abre a página da versão.

O programa **não tem assinatura digital**, então o SmartScreen do Windows ou o seu antivírus podem avisar na primeira vez, inclusive sobre o módulo do jogo que ele monta. Saves e configurações ficam em `%APPDATA%\FFCCEOT-Recomp\EchoesOfTime` (Linux: `~/.local/share/FFCCEOT-Recomp/EchoesOfTime`); coloque um arquivo chamado `portable.txt` ao lado do programa (ao lado do `.AppImage` no Linux) para guardá-los numa pasta `data` ali mesmo.
