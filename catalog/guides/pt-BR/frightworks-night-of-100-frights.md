<!-- source: https://github.com/FrightWorks/FrightWorks-no100f/blob/HEAD/README.md | fingerprint: e3dd8ff7a083fd4f -->

Precisa do Windows 10/11 (64 bits) e do **Night of 100 Frights de GameCube, USA v1.01 (GIHE78, revisão 1)** em `.iso`, `.gcm` ou `.ciso`.

1. Na [versão mais recente](https://github.com/FrightWorks/FrightWorks-no100f/releases/latest), rode o `FrightWorks-win-Setup.exe` (instala para o seu usuário do Windows, sem administrador; atalhos no menu Iniciar e na Área de Trabalho) ou extraia o `FrightWorks-<version>.zip` onde quiser, se preferir tudo numa pasta só.

2. Abra o **FrightWorks** e clique em **Play**. Na primeira vez, ele pede a imagem do seu disco (**Extras**).

O Windows pode avisar que o instalador é de um editor desconhecido (não tem assinatura digital): **Mais informações** > **Executar assim mesmo**.

**Atualizações.** O jogo instalado procura versão nova quando o launcher abre e pergunta antes de atualizar (**Extras > Updates** desliga isso ou confere na hora). Atualizar mantém saves e configurações. O zip não se atualiza sozinho: o launcher dele avisa quando sai uma versão nova; extraia por cima da pasta antiga.

**Onde ficam os seus arquivos**

- O quê · Instalado · Zip

- Saves · `Jogos Salvos\FrightWorks` · `data\UserData\Saves`

- Configurações, mods, registros, arquivos do jogo do seu disco · `%LOCALAPPDATA%\FrightWorks-Data` · `data\UserData`, `data\Disc`

- Capturas (**F2**) / vídeos (**F3**) · `Imagens\FrightWorks` / `Vídeos\FrightWorks` · `Screenshots` / `Videos` ao lado do launcher

**Extras > Folders** abre essas pastas, pode colocar os saves em outra pasta (uma com backup, por exemplo) e copia saves e configurações de uma pasta anterior do FrightWorks ou do Refrightened. Desinstalar (Configurações do Windows > Aplicativos) remove só o programa; saves e configurações ficam.

As configurações ficam no launcher (**Settings**) e no jogo (**F10**, ou o botão **Back** / **Select** do controle, desde que nenhum mapeamento o use). **F11** alterna a tela cheia, **F2** salva uma captura e **F1** abre esta página no navegador. A tela de título e o menu principal mostram essas teclas no canto. Com mais de um monitor, **Settings** > **Display** > **Fullscreen monitor** (ou **F10** > Video) escolhe qual a tela cheia usa.

Para testes: **Extras > Developer tools** no launcher liga o menu de desenvolvedor no **F8** (ir para qualquer fase, pontos salvos, câmera livre, modo deus, poderes liga/desliga, localizador de texturas), a gravação de vídeo do jogo no **F3** (precisa do [ffmpeg](https://ffmpeg.org/): o launcher oferece baixar quando você liga as ferramentas) e uma marca **DEV** com a versão no canto do jogo. A versão também aparece na página **About** do F10 e no topo do `console.log`.

Vindo de um zip anterior do FrightWorks ou do Scooby-Doo: Refrightened? Na primeira abertura, o launcher procura a pasta antiga (ao lado do zip novo, ou o jogo instalado em Downloads, Área de Trabalho e Documentos) e oferece copiar saves, configurações, controles, mods e arquivos do jogo; a pasta antiga fica como está. Se não encontrar, use **Extras > Folders > Copy from an earlier version**.

**Os números de versão** continuam os do Scooby-Doo: Refrightened. A última versão dele, v1.0.2, é o commit de onde o FrightWorks se separou, então o FrightWorks começa na v1.0.3.
