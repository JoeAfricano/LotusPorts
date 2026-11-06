<!-- source: https://github.com/wivi514/Dead_Rising_2_Case_Zero_Xenon_Recomp/blob/HEAD/README.md | fingerprint: 19ead81c5606f07d -->

1. **Baixe** a versão para o seu sistema na página de [versões (Releases)](https://github.com/wivi514/Dead_Rising_2_Case_Zero_Xenon_Recomp/releases):

- Windows: `CaseZeroRecomp-windows-x86_64.zip`

- Mac (M1 ou mais novo): `CaseZeroRecomp-macos-arm64.zip`

2. **Extraia onde quiser** (Windows: botão direito → Extrair Tudo; Linux: `tar --zstd -xf CaseZeroRecomp-linux-x86_64.tar.zst`; Mac: clique duas vezes no zip e rode uma vez `xattr -dr com.apple.quarantine <the unzipped folder>` no Terminal; a versão para Mac não é notarizada pela Apple, e sem isso o macOS a recusa ou a roda de um local escondido onde ela não encontra a pasta `assets`).

3. **Acrescente a sua cópia do jogo.** Você precisa do arquivo de pacote XBLA que o seu Xbox 360 baixou, com cerca de **825 MB** e sem extensão. No armazenamento do console, ele fica em:

```
   Content/0000000000000000/58410A8D/000D0000/<a long string of letters and numbers>
```

Copie esse arquivo para a pasta `assets/package/`, dentro da pasta do jogo que você extraiu (copiar a pasta `58410A8D` inteira também funciona, ou arraste o arquivo para a janela do launcher no passo 4).

4. **Abra o jogo**: `cz_runtime.exe` no Windows, `./cz_runtime` no Linux, `CaseZeroRecomp.app` no Mac (mantenha na mesma pasta da `assets`). A primeira abertura prepara tudo sozinha, com uma barra de progresso: desempacota o seu pacote, prepara os 1.367 shaders do jogo (~10 segundos) e gera os arquivos de menu e de botões a partir dos seus dados. Nas próximas, vai direto ao jogo. A linha **SUBTITLES** do launcher escolhe o idioma do jogo, e o launcher e o painel de configurações de PC do jogo seguem essa escolha (inglês, francês, italiano, espanhol; os menus em japonês e coreano ficam em inglês, mas o texto do próprio jogo é traduzido de qualquer jeito).

Se faltar algo necessário, o jogo diz exatamente o quê e onde; ele nunca falha com uma tela vazia de propósito. Um `README.md` dentro do pacote tem uma seção de solução de problemas. **Cada abertura grava o `cz_runtime.log`** ao lado da `assets/` da pasta do jogo (no AppImage, ao lado do `.AppImage`), e `cz_runtime --diag` mostra o sistema, a placa de vídeo, o driver e a tela e fecha: anexe os dois em qualquer relato de erro. **Ou aperte F9 no jogo**: ele captura o quadro, os últimos 60 s do registro e os dados do seu computador num relatório que a aba Issues do launcher XenonLive envia para você.

Os seus **saves e configurações ficam fora da pasta do jogo** (Windows: `Jogos Salvos\Dead Rising 2 Case Zero\`; Linux: `~/.local/share/Dead Rising 2 Case Zero/`; Mac: `~/Library/Application Support/Dead Rising 2 Case Zero/`), então você pode apagar ou trocar a pasta do jogo quando quiser sem perder o progresso.
