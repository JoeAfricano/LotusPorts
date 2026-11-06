<!-- source: https://github.com/tools81/flintstones-jetsons-timewarp-static-recomp/blob/HEAD/README.md | fingerprint: 583c7a056c4561bc -->

Baixe a versão mais recente na [página de versões (releases)](https://github.com/tools81/flintstones-jetsons-timewarp-static-recomp/releases). Você precisa da sua própria cópia do disco de CD-i como imagem (o `.cue` e o `.bin` dele, setores brutos de 2352 bytes); ela não vem junto.

- **Instalador** (`Timewarp-<version>-Setup.exe`): instala para o usuário atual, sem administrador, por padrão em `%USERPROFILE%\Games\Flintstones-Jetsons Timewarp`. Uma página pede a pasta com a imagem do disco. Atalhos no menu Iniciar são criados para as duas edições.

- **Portátil** (`Timewarp-<version>-portable.zip`): extraia onde quiser (num pen drive funciona) e abra o `timewarp_hd.exe` ou o `timewarp.exe`. Coloque a imagem do disco na pasta `rom`, ou escolha quando o jogo pedir na primeira abertura.

Nos dois casos o jogo é portátil: tudo o que ele grava fica na própria pasta, em `saves\` (saves, espaços de salvamento rápido, configurações de imagem e HD, o registro) e no `timewarp.ini`. Nada vai para o AppData nem para o registro, e os shaders gráficos ficam dentro dos executáveis. Para mover o jogo, mova a pasta. Os dois executáveis são independentes e precisam do Windows 10 ou mais novo, 64 bits.

`timewarp.ini` (ao lado dos executáveis):

- Configuração · Significado

- `rom=` · A imagem do disco: a pasta dela, ou o arquivo `.cue`/`.bin`

- `update_check=` · `1`: ao abrir, oferece uma versão mais nova quando houver (padrão); `auto`: instala sem perguntar; `0`: nunca confere

**Atualizações.** Ao abrir, o jogo pergunta ao GitHub pela versão mais recente. Se for mais nova, o jogo oferece instalar, baixa o zip portátil e fecha. Um pequeno script copia os arquivos novos por cima da pasta do programa, mantendo `saves\` e `timewarp.ini`, e abre o jogo de novo. `--no-update` pula a conferência numa abertura.

### Jogar

Abra o `timewarp.exe` ou o `timewarp_hd.exe`. Cada um encontra a imagem do disco numa pasta `rom` ao lado (ou até quatro níveis acima), ou recebe o caminho do `.cue`/`.bin` como argumento.

- Entrada · Ação

- Mouse · Move o ponteiro; botão esquerdo = botão 1, botão direito = botão 2

- Setas (Shift = mais rápido) · Movem o ponteiro

- Z / Espaço / A · Botão 1

- X / Ctrl / S · Botão 2

- Enter ou C · Os dois botões

- Controle · Analógico esquerdo / direcional move, A = botão 1, B = botão 2

- Tab (segurar) · Roda sem limitar os quadros

- F9 / F10 · Alterna o modo de proporção / filtro

- F11 ou Alt+Enter · Tela cheia

- Esc · Sair

O jogo é todo jogado com o ponteiro: escolha George ou Fred e aponte para as coisas de cada sala e aperte um botão.
