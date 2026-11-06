<!-- source: https://github.com/danisandoval/fukami/blob/HEAD/README.md | fingerprint: e64381bb3122f62c -->

Baixe a versão mais recente na página de [versões (Releases)](https://github.com/danisandoval/fukami/releases) (e só de lá):

- **Windows:** extraia o `Fukami-<version>-windows-x86_64.zip` onde quiser e abra o **Fukami.exe**. Configurações e saves ficam em `%LOCALAPPDATA%\Fukami`.

Na primeira abertura, o Fukami pede o seu arquivo CHD ou `.cue`, confere se é o disco americano e desempacota os arquivos do jogo uma vez para a própria pasta. Instruções completas, controles e configurações estão no [guia do usuário](https://github.com/danisandoval/fukami/blob/HEAD/docs/USER_GUIDE.md).

### Como ter o seu CHD

Uma cópia `.cue`/`.bin` funciona direto: escolha o `.cue` (deixe o `.bin` ao lado). Senão, faça a imagem do seu próprio disco do Ridge Racer V (USA) com qualquer ferramenta comum e converta com o `chdman` (parte do MAME): `chdman createcd -i RidgeRacerV.cue -o RidgeRacerV.chd`. O projeto não ajuda a encontrar arquivos de jogos e não indica onde obtê-los.
