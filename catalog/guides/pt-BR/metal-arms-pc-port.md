<!-- source: https://github.com/tvall43/metal-arms-pc-port/blob/HEAD/README.md | fingerprint: 61de4765f3443184 -->

O exe é portátil: ele cria os dados ao lado dele e nunca grava fora da própria pasta:

```
MetalArmsPC/
  ma_win.exe          port build (in-tree Bink decoder)
  ma_win.ini          created on first run; settings only
  LICENSE-unbikit.txt  in-tree decoder license
  assets/             your own extracted Xbox disc contents
    mettlearms_xb.mst master file (auto-detected here)
    MOVIES/ ...
  saves/              profile files (created automatically: saves/profile-*)
  ma_win_log.txt      only when debug logging is enabled
```

`ma_win.exe` é o port; `ma_win.ini` guarda só as configurações (criado na primeira abertura); `assets/` recebe o conteúdo extraído do seu próprio disco de Xbox (com o arquivo mestre `mettlearms_xb.mst`); `saves/` guarda os perfis; `ma_win_log.txt` só existe com o registro de depuração ligado.

Primeira abertura:

1. Extraia a versão (ou coloque um `ma_win.exe` compilado por você). A versão é independente; não há outros arquivos para instalar.

2. Abra o `ma_win.exe` e use **Extract from ISO...** no launcher para ler o jogo da imagem do seu próprio disco de Xbox (imagens `.xiso` só do jogo e `.iso` de disco completo funcionam; a extração também define o arquivo mestre para você). Use a versão americana de Xbox: é com ela que o port foi feito e testado; discos PAL/europeus ainda não são aceitos. O extrator lê o certificado do `default.xbe` da imagem antes de copiar e avisa se não for a versão americana testada, ou se não houver `default.xbe`. Ou coloque o conteúdo do disco já extraído em `assets/`, com o `mettlearms_xb.mst` no topo.

3. Jogue. Os perfis ficam em `saves/profile-*`; um `profile-*` antigo ao lado do exe é movido para `saves/` na primeira abertura.

Ordem de busca do arquivo mestre no `ma_win.ini`: primeiro `assets/mettlearms_xb.mst`, depois o `[Paths] MasterFile` guardado e, por fim, o antigo `<exe>/mettlearms_xb.mst` de instalações anteriores ao formato portátil.

### Modos de abertura (linha de comando, sem diferença de maiúsculas; - ou / opcional)

- Opção · Comportamento

- (nenhuma) · Janela de abertura no modo normal (linhas de desenvolvimento escondidas, sem registro de depuração)

- `play` · Vai direto à campanha no modo normal

- `dev` · Modo de desenvolvimento: escolha do modo de abertura, menu e cheats de desenvolvimento, pular de fase na janela de abertura

- `autorun` · Modo de desenvolvimento, vai direto (o comportamento do projeto original)

O modo normal nunca grava arquivo de registro, ignora as opções só de desenvolvimento e começa limpo. Um registrador de falhas está sempre ativo e grava o rastro ao lado do exe numa falha grave.

As versões públicas trazem um `ma_win.exe` enxuto (cerca de 7 MB; a versão completa tem 70 MB por causa dos dados de depuração). Os dados de depuração de cada versão publicada ficam arquivados como `ma_win_<build>.dbg` em `releasedbg/`; é o único jeito de traduzir um endereço de falha:

```
addr2line -f -e ma_win_<build>.dbg 0x<address from the crash log>
```

Enxugar não move o código, então os endereços no exe publicado e no `.dbg` são idênticos.

### Informar um problema

A versão registra tudo o que pode precisar; um relato sem o registro quase nunca dá para diagnosticar. Clique em **Start Dev** no launcher (modo de desenvolvimento com o registro já ligado), ou rode `ma_win.exe dev` e marque **Debug log file** na janela de abertura. Jogue até acontecer de novo numa mesma sessão, se puder, feche e anexe o `ma_win_log.txt` da pasta do jogo. Falhas gravam o próprio rastro ao lado do exe sem nenhuma opção. Abra uma issue no GitHub em vez de comentar numa página de download: issues permitem respostas e detalhes. Informe o número da versão que aparece no cabeçalho do registro (por exemplo R153) e confira antes qual versão você tem, porque as correções saem o tempo todo.

Se a abertura parar em `_GameInit: FAILED at gameloop.cpp:779`, inclua a linha logo acima. Ela diz qual sistema do jogo falhou; a linha 779 sozinha não identifica a causa. Um disco PAL/europeu ainda não é aceito.
