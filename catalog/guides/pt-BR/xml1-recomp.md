<!-- source: https://github.com/GTTeancum/XML1-recomp/blob/HEAD/README.md | fingerprint: 14c5e18c18ece25f -->

Precisa do Windows 10/11 64 bits e da sua própria ISO de Xbox do **X-Men Legends (World)**.

1. Extraia os arquivos da sua ISO numa pasta onde dê para gravar, com uma ferramenta de extração de ISO de Xbox.

2. Extraia o `XML1-recomp-1.0.1-win64.zip` nessa mesma pasta, substituindo os arquivos. Mantenha a pasta `runtime` e o arquivo oculto `.xml1-player-layout`.

3. Abra o **X-Men Legends.exe**. Na primeira abertura, a configuração extrai o arquivo compactado em arquivos soltos e gera os manifestos PKGB.

Na primeira abertura, o jogo também compila os dados XML da Raven para o formato XMLB dos jogos seguintes (`.xmlb`, `.engb` e outras variantes, `.chrb`, `.navb`). Instalações soltas que já existem são atualizadas uma vez ao abrir. Os arquivos de texto originais e os saves são mantidos; no modo solto, o jogo lê as versões compiladas.

O SHA-256 da ISO World testada é: `0a1ef03e57458144609906bbc2d44d2c26028cf704f4698ce0f1e61c34030b44`.
