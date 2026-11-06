<!-- source: https://github.com/raulzrrs/swsh-recomp/blob/HEAD/README.md | fingerprint: bedb7d164a13435e -->

1. Baixe o `SwShRecomp-<version>-windows.zip` na página de [versões (Releases)](https://github.com/raulzrrs/swsh-recomp/releases) e extraia numa pasta onde dê para gravar (não dentro de Arquivos de Programas).

2. Abra o **`SwShRecomp.exe`** (se o Windows avisar, **Mais informações** → **Executar assim mesmo**).

3. Clique em **Select game folder...** e escolha a pasta com o seu jogo extraído (`exefs` e `romfs`).

4. Espere enquanto ele copia os arquivos e recompila o jogo para o seu PC. Isso acontece uma vez só e demora (cerca de 40 minutos num processador de 20 threads, numa instalação nova; mais com menos threads).

5. Clique em **Play**.

Tudo fica nessa pasta (configurações em `larecomp.ini`, saves em `save-sword` / `save-shield`, caches de shaders, o registro `lasrecomp.log`). Para atualizar, extraia uma versão nova por cima.

**Na primeira abertura**, os shaders do jogo são preparados para a sua placa de vídeo em segundo plano (cerca de 6 minutos num processador de 8 núcleos; a tela de título fica mais lenta enquanto isso, mas dá para jogar). Depois, a primeira visita a um lugar quase só aproveita o que foi preparado; o que não está nos arquivos do jogo ainda é compilado na primeira vez, o que pode dar uma travadinha.
