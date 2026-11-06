<!-- source: https://github.com/xtamqispeak/KirbyRecomp/blob/HEAD/README.md | fingerprint: c8dc89a0fd17e27b -->

Baixe o **KirbyRecompSetup-v0.3.0-alpha.exe** e o arquivo SHA-256 dele na página da versão. O instalador traz um Python portátil; não precisa de Python instalado no sistema nem de administrador. Por enquanto ele não tem assinatura digital.

1. Abra o instalador e escolha uma pasta de instalação vazia. O padrão é `%LOCALAPPDATA%\KirbyRecomp`. Uma instalação existente pode ser atualizada no lugar.

2. Indique as suas próprias pastas **ExeFS** e **RomFS** extraídas.

3. Clique em **Build game**. O instalador baixa do PyPI o compilador e as dependências de compilação, baixa bibliotecas nativas do repositório oficial do MSYS2, confere a sua cópia, traduz o código do jogo e compila o executável no seu PC. A primeira compilação precisa de internet, de alguns GB livres e de alguns minutos.

4. Clique em **Play game**. Para as próximas vezes, abra o `KirbyRecompSetup.exe` na pasta de instalação. As pastas escolhidas ficam guardadas depois de uma compilação bem-sucedida.

**Install tools** prepara o ambiente antes de escolher a cópia. **Cancel** para o processo atual de ferramentas/compilador; objetos gerados e conferidos podem ser aproveitados na próxima tentativa. A saída de diagnóstico também vai para o `setup.log`. Mantenha a sua RomFS no local escolhido. Feche o jogo antes de recompilar.

Precisa do Windows 10/11 x64 com o `tar.exe` do sistema, de um processador com AVX2/FMA e de uma placa de vídeo com Vulkan 1.3. Só a RTX 4070 SUPER foi conferida. O instalador vem pré-compilado; o executável do jogo é montado no seu PC a partir dos seus arquivos. Importar NSP/XCI não está incluído.
