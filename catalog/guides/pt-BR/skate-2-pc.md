<!-- source: https://github.com/christianortel/skate2-recompile-pc/blob/HEAD/README.md | fingerprint: 895594a8ed301ce0 -->

1. Baixe o **Skate2-PC-v0.1.1-Windows-x64.zip** na página de [versões (Releases)](https://github.com/christianortel/skate2-recompile-pc/releases/latest) e extraia o ZIP inteiro numa pasta onde dê para gravar.

2. Rode o **Import Game Files.cmd** e escolha a pasta do seu jogo de Xbox 360 extraído, com `default.xex` e `data`. É uma cópia e conferência feita uma vez só.

3. Conecte o controle e abra o **Launch Skate 2.exe**.

Se o launcher pedir o Microsoft Visual C++ x64 Runtime, abra o **Install Visual C++ Runtime.url** incluído, rode o instalador da Microsoft e abra de novo. O [guia de instalação](https://github.com/christianortel/skate2-recompile-pc/blob/HEAD/docs/INSTALL.md) cobre cada passo, a revisão do jogo aceita, os controles e a solução de problemas.

Você vai precisar de:

- Um PC com Windows 10/11 x64 e placa de vídeo com Direct3D 12.

- Windows PowerShell 5.1, que já vem no Windows 10/11; o launcher nativo usa o script de abertura existente.

- Os seus próprios arquivos extraídos da [revisão aceita do jogo de Xbox 360](https://github.com/christianortel/skate2-recompile-pc/blob/HEAD/docs/INSTALL.md#supported-game-version).

- Um controle. O DualSense por USB funciona direto, sem o DS4Windows; a compatibilidade com outros controles ainda está em teste.

Use o ZIP para Windows com nome, em **Releases**. O **Code → Download ZIP** do GitHub e os arquivos automáticos **Source code** são para desenvolvedores; eles não trazem o executável do jogador.

Os arquivos originais do jogo são obrigatórios e o projeto não os inclui nem baixa. Uma ISO ou arquivo compactado precisa ser extraído antes de importar.
