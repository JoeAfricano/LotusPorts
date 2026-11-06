<!-- source: https://github.com/wivi514/Dead_Rising_2_Case_West_Xenon_Recomp/blob/HEAD/README.md | fingerprint: 662b3eabd03c3f5b -->

1. **Baixe** a versão para o seu sistema na página de [versões (Releases)](https://github.com/wivi514/Dead_Rising_2_Case_West_Xenon_Recomp/releases):

- Windows: `CaseWestRecomp-windows-x86_64.zip`

- Mac (Apple Silicon): `CaseWestRecomp-macos-arm64.zip`

2. **Extraia onde quiser** (Windows: botão direito → Extrair Tudo; Linux: `tar --zstd -xf CaseWestRecomp-linux-x86_64.tar.zst`; Steam Deck: `tar -xzf CaseWestRecomp-steamdeck-x86_64.tar.gz`; Mac: clique duas vezes no zip). A versão para Mac não é notarizada; antes da primeira abertura, rode `xattr -dr com.apple.quarantine <the unzipped folder>` no Terminal.

3. **Acrescente a sua cópia do jogo.** Você precisa do arquivo de pacote XBLA que o seu Xbox 360 baixou, com cerca de **1,2 GB** e sem extensão. No armazenamento do console, ele fica em:

```
   Content/0000000000000000/58410B00/000D0000/<a long string of letters and numbers>
```

Copie esse arquivo para a pasta `assets/package/`, dentro da pasta do jogo que você extraiu (copiar a pasta `58410B00` inteira também funciona, ou arraste o arquivo para a janela do launcher no passo 4).

4. **Abra o jogo**: `cw_runtime.exe` no Windows, `./cw_runtime` no Linux ou Steam Deck, ou `CaseWestRecomp.app` no Mac (mantenha ao lado da pasta `assets`). A primeira abertura prepara tudo sozinha, com uma barra de progresso: desempacota o seu pacote, prepara os 1.322 shaders do jogo (7,5 s num desktop de 16 threads, mais com menos núcleos; ele usa todos) e gera as imagens das teclas a partir dos seus dados. Nas próximas, vai direto ao jogo.

Se faltar algo necessário, o jogo diz exatamente o quê e onde; ele nunca falha com uma tela vazia de propósito. Um `README.md` dentro do pacote tem uma seção de solução de problemas.

Os seus **saves e configurações ficam fora da pasta do jogo** (Windows: `Jogos Salvos\Dead Rising 2 Case West\`; Linux: `~/.local/share/Dead Rising 2 Case West/`), então você pode apagar ou trocar a pasta do jogo quando quiser sem perder o progresso.
