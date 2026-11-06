<!-- source: https://github.com/SeveerJ/Orta_PC_Recomp/blob/HEAD/README.md | fingerprint: 207c1eb50bd87c81 -->

Baixe o `OrtaPC_Setup.exe` na página de versões (Releases) e abra.

- Ele procura sozinho a imagem do seu disco do Panzer Dragoon Orta (`.iso`) em Downloads, Área de Trabalho, Documentos, Vídeos, na sua pasta de usuário e depois em cada disco. Se não encontrar, clique em **Disc image...** (ou **Folder...** para uma cópia extraída com o `default.xbe`).

- Ele confere se o disco é o Panzer Dragoon Orta da versão norte-americana.

- Ele baixa o Python, o desmontador capstone e o compilador llvm-mingw (cerca de 300 MB) para `%LOCALAPPDATA%\Orta PC Setup`, traduz e compila o jogo (20 a 30 minutos), instala onde você escolheu e depois apaga essa pasta de trabalho.

- No fim, ele prepara os shaders gráficos que o jogo precisa para abrir (alguns minutos), para a primeira abertura ser rápida.

- A pasta de instalação recebe o `setup_log.txt`; envie esse arquivo junto com qualquer relato de problema. Se a instalação parar no meio, rode de novo: ela aproveita o que já baixou e compilou.
