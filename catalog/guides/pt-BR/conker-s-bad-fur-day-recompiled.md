<!-- source: https://github.com/sciaschi/CBFD-Recompiled/blob/HEAD/README.md | fingerprint: 6cadc6181d72e928 -->

- [Git para Windows](https://git-scm.com/download/win).

- [Python 3](https://www.python.org/downloads/). No instalador, marque "Add python.exe to PATH".

- **Visual Studio 2022 ou mais novo** com a carga de trabalho "Desktop development with C++". A edição gratuita Build Tools for Visual Studio basta.

### 1. Instale as ferramentas (uma vez)

- **Xcode**, pela App Store. Só as Command Line Tools não bastam: o RT64 compila os shaders com o compilador Metal do Xcode. Abra o Xcode uma vez para terminar a configuração. Se `xcode-select -p` não mostrar um caminho dentro de `Xcode.app`, rode `sudo xcode-select -s /Applications/Xcode.app`.

- [Homebrew](https://brew.sh/).

- Opcional, para criar mods: `brew install llvm` (o clang da Apple não compila para o processador MIPS do N64).
