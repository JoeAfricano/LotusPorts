<!-- source: https://github.com/tools81/laser-lords-static-recomp/blob/HEAD/README.md | fingerprint: d2183b1f8605fe7a -->

Baixe a versão mais recente na [página de versões (releases)](https://github.com/tools81/laser-lords-static-recomp/releases). Você precisa da sua própria cópia do disco de CD-i como imagem (o `.cue` e o `.bin` dele, setores brutos de 2352 bytes). Ela não vem junto.

- **Instalador** (`LaserLords-<version>-Setup.exe`): instala para o usuário atual, sem administrador, por padrão em `%USERPROFILE%\Games\Laser Lords`. Uma página pede a pasta com a imagem do disco. Atalhos no menu Iniciar são criados para as duas edições.

- **Portátil** (`LaserLords-<version>-portable.zip`): extraia onde quiser (num pen drive funciona) e abra o `laserlords_hd.exe` ou o `laserlords.exe`. Coloque a imagem do disco na pasta `rom`, ou escolha quando o jogo pedir na primeira abertura.

Nos dois casos o jogo é portátil. Tudo o que ele grava fica na própria pasta: `saves\` (saves da NVRAM, espaços de salvamento rápido, configurações de imagem e HD, o registro) e `laserlords.ini`. Nada vai para o AppData nem para o registro, e os shaders gráficos ficam dentro dos executáveis. Para mover o jogo, mova a pasta. Os atalhos passam `--portable`, que não muda nada porque o jogo é sempre portátil; a opção é aceita para outros programas poderem informá-la. Os dois executáveis são independentes e precisam do Windows 10 ou mais novo, 64 bits.

`laserlords.ini` (ao lado dos executáveis):

- Configuração · Significado

- `rom=` · A imagem do disco: a pasta dela, ou o arquivo `.cue`/`.bin`

- `update_check=` · `1`: ao abrir, oferece uma versão mais nova quando houver (padrão); `auto`: instala sem perguntar; `0`: nunca confere

**Atualizações.** Ao abrir, o jogo pergunta ao GitHub pela versão mais recente. Se for mais nova, o jogo oferece instalar, baixa o zip portátil e fecha. Um pequeno script copia os arquivos novos por cima da pasta do programa, mantendo `saves\` e `laserlords.ini`, e abre o jogo de novo. `--no-update` pula a conferência numa abertura.
