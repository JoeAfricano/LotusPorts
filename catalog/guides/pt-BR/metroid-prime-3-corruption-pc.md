<!-- source: https://github.com/bonllavier/Corruption_3-PC/blob/HEAD/README.md | fingerprint: 265d2efe98babe7f -->

1. Baixe o `Corruption_3-PC-vX.Y.Z-win64.zip` mais recente na página de [versões (Releases)](https://github.com/bonllavier/Corruption_3-PC/releases) e extraia onde quiser.

2. Abra o **`Corruption_3 PC.exe`**. Na primeira abertura, ele pede a imagem do seu disco: escolha o arquivo na janela. O caminho fica guardado em `UserData/Config.toml`, então ele só pergunta uma vez (dá para mudar depois em **F1 > Diagnostics > Select disc image**).

3. Escolha o seu espaço de save com o mouse e clique. **F1** abre as opções a qualquer momento.

**A primeira abertura de uma instalação nova compila os shaders do jogo em segundo plano.** Medido num i7-4790 de 4 núcleos: **cerca de 17 minutos, com o uso de memória chegando a uns 9 GB** (de 16 GB instalados). Essa memória continua em uso até você fechar o jogo, então reabra depois que a compilação terminar. Nas próximas vezes, ele fica pronto em **cerca de 7 segundos**.

Tudo o que o port grava (configurações, saves, registros, caches, relatórios de erro) fica na pasta `UserData`, ao lado do executável. A imagem do disco é só lida, onde quer que esteja.
