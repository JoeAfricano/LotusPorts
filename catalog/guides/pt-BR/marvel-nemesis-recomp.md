<!-- source: https://github.com/15Cyber/marvel-nemesis-recomp/blob/HEAD/README.md | fingerprint: 4e81d6d16b75fc67 -->

1. Baixe o **MarvelNemesis-Windows-x64.zip** na página de [versões (Releases)](https://github.com/15Cyber/marvel-nemesis-recomp/releases). Os arquivos automáticos "Source code" são para desenvolvedores.

2. Extraia o ZIP completo numa pasta onde dê para gravar. Mantenha a pasta `compiler/` ao lado do `MarvelNemesis.exe`.

3. Coloque os arquivos do seu próprio jogo de Xbox compatível em `assets/`: o `default.xbe` extraído com todas as pastas do jogo, ou a ISO do seu jogo.

4. Abra o `MarvelNemesis.exe`. A primeira abertura traduz e compila o seu XBE no seu PC e depois abre o jogo. Uma janela mostra o progresso; isso pode levar alguns minutos. Nas próximas vezes, ele usa o `MarvelNemesis.dll` já gerado.

Este pacote para jogadores não precisa de Visual Studio, CMake nem Ninja. Ele traz o recompilador, Clang/LLD e as DLLs necessárias. Mantenha a pasta `compiler/` para preparações e reparos futuros. Arquivos de compilação já terminados são guardados se a preparação for interrompida. Veja o `README.txt` do pacote e o [guia de uso](https://github.com/15Cyber/marvel-nemesis-recomp/blob/HEAD/docs/USAGE.md).

A DLL com o código do jogo preparado e a pasta `compiler/work/` ficam só no seu computador; não redistribua. O tempo da primeira abertura e o uso de memória dependem do PC. Use uma pasta nova para testar uma atualização e guarde os seus saves à parte.
