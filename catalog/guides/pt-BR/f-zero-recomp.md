<!-- source: https://github.com/craigshaw/FZeroRecomp/blob/HEAD/README.md | fingerprint: bde12800b7598043 -->

A [versão 0.6.0](https://github.com/craigshaw/FZeroRecomp/releases/tag/v0.6.0) está disponível para Linux, Windows e macOS Apple Silicon. Os downloads trazem o SDL; não precisa de compilador nem de geração de código para jogar. Você usa o seu próprio arquivo do F-Zero (USA). A opção de pasta de saves personalizada precisa da 0.6.0.

- Sistema · Download · Requisitos

- Windows · [ZIP para Windows x64 (0.6.0)](https://github.com/craigshaw/FZeroRecomp/releases/download/v0.6.0/FZeroRecomp-v0.6.0-Windows-x64.zip) · Windows 10/11 x64; Direct3D 12 com Shader Model 6.0 para os filtros visuais

### Windows

1. Baixe o ZIP para Windows x64 e extraia o arquivo inteiro numa pasta onde dê para gravar, como uma pasta do seu usuário. Não abra de dentro do ZIP.

2. Abra o `fzero_recomp.exe`, escolha o seu arquivo do F-Zero (USA) e clique em **Play**.

3. Aperte **F1** no jogo para mudar tela larga, estilo visual, áudio e controles.

Mantenha o executável, as DLLs e a pasta `assets/` juntos. O SDL e o runtime do Visual C++ vêm junto; não precisa instalar nada à parte. Se a placa de vídeo não rodar os filtros, o jogo usa as cores originais e continua com tela larga.

Configurações e saves ficam ao lado do executável: `config.ini`, `keybinds.ini`, `rom.cfg` e `saves/save.srm`. Mantenha essa pasta com permissão de gravação. Para atualizar, feche o jogo, faça uma cópia desses arquivos e substitua os arquivos do programa pelos do download novo, mantendo as configurações e as pastas `saves/` e `screenshots/`. As configurações existentes têm prioridade sobre os padrões novos. A versão traz um arquivo `.zip.sha256` para conferir cada download.

Na área Save do launcher, clique em **Browse** ao lado de **Folder** para guardar o `save.srm` em outra pasta. **Default** volta para a pasta `saves/` ao lado do executável. A escolha fica no `config.ini`, em `[Save]`; mantenha esse arquivo ao atualizar se usar uma pasta personalizada.

Para usar um save de bateria que você já tem, clique em **Import** no launcher e escolha o seu `.srm` ou `.sav`. Aparece uma confirmação quando a importação dá certo; clique em **Play** para carregar. A importação cria a pasta de saves na primeira vez e guarda o save existente como `saves/save.srm.bak` antes de substituir.
