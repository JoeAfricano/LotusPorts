<!-- source: https://github.com/TheRunaway5/Phase-Distorter/blob/HEAD/README.md | fingerprint: b3c4297b3841b599 -->

### Baixou um ZIP de versão?

1. Baixe o ZIP para [Windows](https://github.com/TheRunaway5/Phase-Distorter/releases/download/0.5/Phase-Distorter-0.5-windows-x86_64.zip), [Linux](https://github.com/TheRunaway5/Phase-Distorter/releases/download/0.5/Phase-Distorter-0.5-linux-x86_64.zip) ou [macOS universal](https://github.com/TheRunaway5/Phase-Distorter/releases/download/0.5/Phase-Distorter-0.5-macos-universal.zip).

2. Use **Extrair Tudo** ou o seu gerenciador de arquivos para extrair a pasta inteira do aplicativo. **Não abra o aplicativo de dentro do ZIP.** Mantenha os arquivos juntos.

3. No Windows, abra o **`Phase Distorter.exe`** e mantenha o **`SDL2.dll`** ao lado. No Linux, abra o **`Phase Distorter`** e mantenha a pasta **`lib/`** ao lado; a versão para Linux precisa do **glibc 2.36 ou mais novo** e de OpenGL. No macOS 11 ou mais novo, arraste o **Phase Distorter.app** para Aplicativos; a primeira abertura pode exigir **Privacidade e Segurança → Abrir Mesmo Assim**.

4. Se a configuração do arquivo do jogo aparecer, importe o seu próprio arquivo do EarthBound ou do Mother 2 aceito. Uma importação anterior na sua pasta de dados de usuário pula este passo; nenhum arquivo do jogo nem pacote de recursos vem no download.

5. Durante o jogo, use **Settings (F1) → Assets** para trocar de jogo ou gerenciar os caches. Salve no jogo antes de trocar. **Tela cheia (F11)** e as outras opções de imagem ficam na barra do jogo.

6. Use o save normal do jogo e saia normalmente. Os saves ficam fora da pasta extraída. Faça cópia dos seus arquivos `.srm` antes de atualizar e extraia a versão seguinte numa pasta nova.

Cada ZIP traz uma pasta de aplicativo nova (`Phase-Distorter-0.5-windows-x86_64/` ou `Phase-Distorter-0.5-linux-x86_64/`). Extraia o ZIP inteiro e abra o aplicativo dentro dessa pasta. Esses pacotes trazem os arquivos do aplicativo, mas nenhum arquivo de jogo, pacote de recursos, save ou código-fonte. Indique o seu próprio arquivo do jogo na primeira abertura. Cada ZIP traz `README.txt`, uma lista de arquivos e somas de verificação.

Para o formato de pastas do repositório, baixe o [ZIP combinado de launchers da v0.5](https://github.com/TheRunaway5/Phase-Distorter/blob/HEAD/releases/Phase-Distorter-0.5-launchers-multiarch.zip) e extraia a pasta do aplicativo. Ele traz os três sistemas, `launch.sh` / `launch.bat`, os runtimes e a criação de atalhos.

### Rodando a partir do repositório

Baixe o repositório como ZIP e extraia, ou clone se também quiser o código completo. Mantenha a estrutura de pastas. Os aplicativos nativos ficam em **`launchers/linux/bin/eb_cpp`** e **`launchers/windows/bin/eb_cpp.exe`**. Abra o executável certo direto, ou use `./launch.sh` no Linux ou `launch.bat` no Windows; esses scripts preferem uma compilação local quando existe.

### Windows (x86-64)

1. Coloque a pasta extraída num lugar prático, como `C:\Games\PhaseDistorter`.

2. Clique duas vezes no **`Phase Distorter.exe`**.

3. Faça a importação do arquivo do jogo descrita abaixo.

Mantenha o **`SDL2.dll`** ao lado do **`Phase Distorter.exe`** na pasta principal. Para escolher o jogo direto, use `--game earthbound` para o inglês ou `--game mother2` para o japonês; por exemplo, no Prompt de Comando:

```
"Phase Distorter.exe" --game mother2
```

Você pode criar um atalho na Área de Trabalho direto para o executável. O **`install-shortcuts.vbs`** opcional também cria atalhos na Área de Trabalho e no menu Iniciar com o ícone do Saturn; mantenha a pasta do projeto no lugar, ou rode de novo depois de movê-la. O executável para Windows foi testado no Wine; testes no Windows nativo ainda faltam.

### Primeira abertura e importação do arquivo do jogo

1. Abra o **`Phase Distorter`** no Linux ou o **`Phase Distorter.exe`** no Windows.

2. Na janela de configuração, procure o seu próprio arquivo do jogo, arraste para a janela ou digite o caminho.

3. Clique em **Import**. O Phase Distorter confere o arquivo, extrai os recursos necessários para a sua pasta de dados de usuário e abre o jogo.

Importe cada jogo separadamente para jogar os dois. A versão japonesa usa o programa, textos, fontes e recursos originais do Mother 2. Os dois jogos têm pacotes de recursos e saves separados. Depois de uma importação bem-sucedida, as próximas aberturas usam o pacote local; o arquivo original não é mais necessário. A importação não altera o arquivo do jogo nem o executável.

Só estas imagens comerciais são aceitas. Cada uma tem **3.145.728 bytes** sem cabeçalho de copiadora; uma imagem `.smc` com um cabeçalho extra de 512 bytes também é aceita. Os hashes abaixo são da imagem sem esse cabeçalho.

- Jogo · Idioma · SHA-256

- EarthBound (US) · Inglês · `a8fe2226728002786d68c27ddddf0b90a894db52e4dfe268fdf72a68cae5f02e`

- Mother 2 (Japão) · Japonês · `1f8cfd13177d86b0eb2c8adcf9e1a4f0ec8966fa1583072b65a1b1c0e7961a5d`

Arquivos modificados, patches de tradução, protótipos e outras revisões não são aceitos nesta versão. Links para arquivos de jogos e recursos do jogo não são fornecidos.

A importação também funciona pela linha de comando. Por exemplo, no Linux:

```
"./Phase Distorter" --import-rom "/path/to/your/mother2.sfc" --import-only
"./Phase Distorter" --game mother2
```

Ou no Prompt de Comando do Windows:

```
start /wait "" "Phase Distorter.exe" --import-rom "C:\path\to\your\mother2.sfc" --import-only
"Phase Distorter.exe" --game mother2
```

O comando do Windows usa `start /wait` para o aplicativo terminar a importação antes do Prompt seguir para o próximo comando.

Use `--assets "/path/to/game.ebpak"` para escolher outro local do pacote de recursos ao importar ou jogar. Um jogo escolhido direto precisa bater com o arquivo ou pacote indicado. Importações inválidas mantêm intacto um pacote válido existente.
