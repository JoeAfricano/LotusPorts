<!-- source: https://github.com/alexbeavs-ps1-ports/suikoden-ii-recomp/blob/HEAD/README.md | fingerprint: 8c1592967a3abcb1 -->

1. Baixe o ZIP de instalação para o seu sistema na página de [versões (Releases)](https://github.com/Alexbeav/suikoden-ii-recomp/releases) e extraia numa pasta onde dê para gravar.

2. Abra o `Suikoden_II` (`.exe` no Windows).

3. No assistente de configuração, escolha a imagem do seu disco (o CUE no padrão Redump, ou um CHD da mesma cópia) e o arquivo da sua BIOS.

4. Clique em Generate & rebuild e espere o jogo abrir. A primeira vez compila o jogo e leva alguns minutos.

No Windows, o assistente baixa as ferramentas de compilação portáteis (cmake-clang-v1) ou usa o cmake/ninja que já estiverem no PATH. No Linux e no macOS, instale antes CMake, Ninja, Python 3 e um compilador C/C++. Mantenha o CUE e todos os arquivos que ele cita juntos. A instalação gera uma versão normal e uma de diagnóstico; veja "Diagnostic mode" no repositório.
