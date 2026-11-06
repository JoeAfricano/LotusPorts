<!-- source: https://github.com/gamboozino/ngbrecomp/blob/HEAD/README.md | fingerprint: 9f2a5718cc4f2e5a -->

Em todos os sistemas, a primeira abertura depois de instalar demora mais para carregar (os shaders são preparados nela e guardados); as próximas são mais rápidas.

### Windows

1. Baixe o `NinjaGaidenBlack-windows-x64.zip` e extraia onde quiser.

2. Abra o `NinjaGaidenBlack.exe` e escolha a imagem do seu disco.

Precisa do Windows 10 ou 11 (64 bits) e de uma placa de vídeo com drivers Vulkan 1.3 (ou drivers mais antigos que tenham "dynamic rendering" e "synchronization2" como extensões).

**Avisos de antivírus:** o programa não tem assinatura digital, e um programa novo sem assinatura às vezes é marcado pela heurística de algum antivírus (o McAfee o colocou em quarentena no #88; o VirusTotal não mostra outras detecções). É um falso positivo. Cada versão lista o SHA-256 dos dois downloads nas notas e no `SHA256SUMS.txt`, para você conferir que o seu download é o publicado ali: no PowerShell, `Get-FileHash NinjaGaidenBlack-windows-x64.zip`; no Linux, `sha256sum -c SHA256SUMS.txt`. Se preferir não rodar um programa pronto, você pode montá-lo a partir do seu próprio disco.

### Onde as coisas ficam

- O quê · Linux · Windows · macOS · Android

- Jogo instalado, saves · `~/.local/share/NinjaGaidenBlack` · `%LOCALAPPDATA%\NinjaGaidenBlack` · `~/Library/Application Support/NinjaGaidenBlack/saves` · `Android/data/io.github.gamboozino.ngbrecomp/files`

- Configurações (`settings.ini`, `controls.ini`) · `~/.config/NinjaGaidenBlack` · `%APPDATA%\NinjaGaidenBlack` · `~/Library/Application Support/NinjaGaidenBlack/config` · `config` na pasta acima

- Cache de shaders · `~/.cache/NinjaGaidenBlack` · `%LOCALAPPDATA%\NinjaGaidenBlack\shaders` · `~/Library/Application Support/NinjaGaidenBlack/cache/NinjaGaidenBlack/shaders` · `cache` na pasta acima

- Registro · o terminal, ou `log.txt` ao lado dos saves quando aberto sem ele · `log.txt` ao lado dos saves (ou o console de onde foi aberto) · `log.txt` ao lado dos saves · `log.txt` ao lado dos saves

**Modo portátil:** com um arquivo chamado `portable.txt` ao lado do `NinjaGaidenBlack.exe` (ou do AppImage ou do app de Mac), tudo isso vai para uma pasta `userdata` ao lado. A configuração da primeira abertura oferece isso numa caixa de seleção. A pasta precisa ter permissão de gravação, então não dentro de Arquivos de Programas. Apague o `portable.txt` para voltar.

### Mods

Arquivos colocados numa pasta `mods` ao lado dos saves (a primeira linha acima; `userdata/mods` no modo portátil) são lidos no lugar dos do jogo: texturas, modelos, sons, vídeos, qualquer coisa que o jogo carregue dos arquivos. Use o mesmo caminho que o arquivo tem na pasta do jogo, por exemplo `mods/movie/cre.sfd` para `movie/cre.sfd`. Um mod substitui um arquivo inteiro (sem patches nem scripts), o jogo instalado nunca é alterado, e apagar o arquivo volta ao original. O registro lista cada arquivo tirado de `mods` (`[MODS]`).
