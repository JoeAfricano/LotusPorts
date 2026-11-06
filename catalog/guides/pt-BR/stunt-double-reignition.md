<!-- source: https://github.com/giangra/stuntdouble-reignition/blob/HEAD/README.md | fingerprint: c2904740aac7bad3 -->

1. Baixe o `StuntmanSetup-<version>.exe` na página de [versões (Releases)](https://github.com/giangra/stuntdouble-reignition/releases) e abra.

2. Escolha a ISO do seu Stuntman: Ignition e uma pasta de instalação (padrão `C:\Games\StuntmanIgnition`) e clique em **Install**.

3. Quando terminar, abra o jogo pelo atalho **Stuntman Ignition** na Área de Trabalho. Ele abre numa tela inicial onde você ajusta vídeo, controles e idioma, vê as suas conquistas e clica em **Play**.

O instalador confere se a ISO é do Stuntman: Ignition, extrai e busca o que a compilação precisa se faltar: o Microsoft Visual Studio Build Tools gratuito (cerca de 6 GB, pede administrador) e um Git portátil. Depois, ele baixa o código deste projeto e o ReXGlue SDK, traduz o código do jogo para C++ e compila. Conte com uns 12 GB livres e de 30 a 60 minutos na primeira vez. Se parar, rode de novo: as etapas terminadas ficam guardadas. O registro é o `install.log`, na pasta de instalação.

### Atualizar

A partir da 0.3.0, o launcher confere no GitHub se há versão nova ao abrir e mostra um botão **Update to x.y.z**. Ele baixa o instalador novo (conferindo o SHA-256 e a versão), que remonta o jogo na versão nova a partir dos arquivos já extraídos na pasta de instalação: sem precisar da ISO, cerca de 10 a 20 minutos. Saves, conquistas e o `stuntman.toml` são mantidos. Vindo da 0.2.x ou antes, baixe o instalador novo à mão uma vez e instale por cima na mesma pasta.
