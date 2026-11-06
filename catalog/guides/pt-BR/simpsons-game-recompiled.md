<!-- source: https://github.com/YesterMester/TheSimpsonsGameRecomp/blob/HEAD/README.md | fingerprint: 1086a700040d3ef4 -->

1. Baixe o arquivo para o seu sistema na [página de versões (Releases)](https://github.com/YesterMester/TheSimpsonsGameRecomp/releases). Use o pacote `-NoAVX2` se o seu processador for anterior ao AVX2 (senão, o pacote padrão não abre; no Windows, com o erro 0xc0000142).

2. Extraia onde quiser.

3. Abra o launcher:

- Windows: abra o `simpsons-launcher.exe`. É um programa independente e não precisa de Python.

4. Na aba **Install**, escolha a sua ISO de Xbox 360. O launcher extrai e instala os dados do jogo.

5. Clique em **Play**.

Para abrir o jogo sem a janela do launcher, por exemplo por um atalho seu na Steam, rode `Play.sh --play` no Linux ou `simpsons-launcher.exe --play` no Windows. Ele usa as mesmas configurações do botão Play.

O launcher pode se atualizar pela aba **About**. As atualizações substituem só os arquivos do motor e do launcher. Configurações, saves e mods são mantidos. Se o pacote instalado não combinar com o seu processador, o launcher oferece o certo.

**Avisos de antivírus.** Os programas para Windows da versão não têm assinatura digital, então o Microsoft Defender e navegadores como o Edge às vezes marcam ou bloqueiam o download, de vez em quando com uma detecção genérica como `Trojan:Win32/Suschil!rfn`. São falsos positivos: cada versão é compilada a partir do código deste repositório pelo workflow público do GitHub Actions em `.github/workflows/release.yml`. Se o download for bloqueado, você pode compilar o jogo você mesmo ou restaurar o arquivo pelo histórico de proteção do Defender.
