<!-- source: https://github.com/goodshepherd23/deSPIRIA-DreamRecomp/blob/HEAD/README.md | fingerprint: 0b0ce8c19871221f -->

O patch em inglês traduz a história do Capítulo 1. A partir do Capítulo 2, os diálogos voltam a ser em japonês (menus, itens e mapas continuam em inglês). Para ler, use o [LunaTranslator](https://github.com/HIllya51/LunaTranslator), uma ferramenta gratuita que lê o texto da tela e traduz automaticamente enquanto você joga:

1. Baixe o `LunaTranslator_x64.zip` na página de [versões](https://github.com/HIllya51/LunaTranslator/releases), extraia onde quiser e abra o `LunaTranslator.exe`.

2. Nas configurações, coloque o idioma de origem em **japonês** e o de destino em **inglês** (ou português), e escolha um tradutor (Google ou Bing funcionam sem conta).

4. Desenhe a região de OCR sobre **toda a imagem do jogo**: o deSPIRIA não tem uma caixa de diálogo fixa, e as falas aparecem no meio ou no alto da tela.

5. Opcional: clique com o botão direito na caixa da região para ligar a tradução no lugar e o clique através dela, e minimize a barra principal do LunaTranslator para o OCR não lê-la.

O LunaTranslator esquece a região de OCR ao fechar, então desenhe de novo a cada sessão. A tradução automática é mais tosca que a da equipe; ela serve para seguir a história até a tradução dos capítulos seguintes sair.

Também dá para acompanhar a história pela [jogatina completa traduzida de popomocco no YouTube](https://www.youtube.com/playlist?list=PLNWIo8BxEFzYPOp2jkPlwOrT1MoDREDCq) (23 vídeos, todos os capítulos): jogue um capítulo e assista à parte dele para pegar os diálogos que não conseguiu ler.

### Instalar

**Quiver Launcher:** adicione o deSPIRIA pelo catálogo e clique em Play.

**À mão:** extraia o zip da versão onde quiser e abra o `deSPIRIA.exe`.

Na primeira abertura, uma janela de configuração aparece:

1. Ela pede o seu `.gdi` com o patch em inglês e confere se é o deSPIRIA com esse patch.

2. Ela baixa as ferramentas de montagem para `tools\`, na pasta de dados: um clang portátil (llvm-mingw), CMake, Ninja, Python, SDL3 e os cabeçalhos do Vulkan, cerca de 1 GB extraído. Cada download tem versão fixa e é conferido.

3. Ela baixa o dream-recomp num commit fixo (`51a148c`), acrescenta as correções deste kit e a configuração do deSPIRIA, traduz o programa do seu disco e compila. Isso leva de 5 a 15 minutos.

4. O jogo abre. Nas próximas vezes, abre direto.

Para escolher outro disco, rode `deSPIRIA.exe --setup`. Para remontar, rode `deSPIRIA.exe --rebuild`.
