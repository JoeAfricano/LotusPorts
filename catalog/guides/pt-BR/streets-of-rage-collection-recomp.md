<!-- source: https://github.com/OriginalSwayze/Streets-Of-Rage-Recomp/blob/HEAD/README.md | fingerprint: 1368a59eb531a6d5 -->

**Você precisa dos seus próprios arquivos dos jogos.** Eles não vêm junto e nunca virão, e não há programas de jogo prontos para baixar: um jogo montado traz o código do próprio jogo traduzido para C, então só pode ser feito no seu PC a partir da sua cópia. O instalador faz isso para você:

1. Baixe o `Streets-of-Rage-Collection-Setup-<version>.exe` na página de [versões (Releases)](https://github.com/OriginalSwayze/Streets-Of-Rage-Recomp/releases) e abra. Se o Windows avisar sobre um programa desconhecido, escolha Mais informações e depois Executar assim mesmo.

2. Escolha a pasta de instalação (por padrão, a pasta Programas do seu usuário: não precisa de administrador).

3. Escolha os seus arquivos dos jogos: `.md`, `.bin`, `.gen` ou um `.zip` com um deles dentro. Cada um é conferido na hora (um arquivo escolhido para o jogo errado vai para o certo). Pule os jogos que você não tem; rode o instalador de novo depois para acrescentar.

4. O instalador baixa as ferramentas de compilação para a pasta de instalação, a não ser que já estejam no seu PC: [Node.js](https://nodejs.org/), [CMake](https://cmake.org/), [Ninja](https://ninja-build.org/) e [LLVM-MinGW](https://github.com/mstorsjo/llvm-mingw) (Clang), cerca de 260 MB, cada um da versão oficial do autor e conferido com um SHA-256 fixo. Depois, ele monta cada jogo a partir do seu arquivo (alguns minutos cada) e coloca a coleção no menu Iniciar (e na Área de Trabalho, se quiser).

O **Streets of Rage Collection** abre nos três jogos. Cada um tem o próprio launcher, com o tema do jogo, as configurações, a trilha sonora e as cores dos lutadores; configurações, saves e estados salvos ficam na pasta `games\<game>` da instalação. Para acrescentar um jogo, remontar ou remover a coleção: Configurações > Aplicativos > Streets of Rage Collection (Modificar roda o instalador de novo; Desinstalar pode manter os seus saves).
