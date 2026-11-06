<!-- source: https://github.com/TekRantGaming/outpost-kaloki-x-recompiled/blob/HEAD/README.md | fingerprint: fa252dc11db49964 -->

**Você precisa de:** Windows 10 ou 11 (64 bits) e uma placa de vídeo com DirectX 12, **ou** Linux 64 bits com Vulkan, mais o seu próprio pacote Xbox Live Arcade do Outpost Kaloki X, exatamente esta versão:

- Title ID · `584107DB` (Xbox Live Arcade, tipo de conteúdo `000D0000`)

- Tamanho do pacote · 21.557.248 bytes (o arquivo sem extensão da pasta de conteúdo do seu Xbox 360 ou emulador)

- Versão do jogo · `default.xex` 0.0.1.1 (04/11/2005), 3.252.224 bytes, CRC32 `CCB0ACE9`

O launcher confere os dois e avisa se o seu pacote for de outra versão.

1. Baixe o **OutpostKalokiX-v...-windows-x64.zip** na [versão mais recente](https://github.com/TekRantGaming/outpost-kaloki-x-recompiled/releases/latest) e extraia onde quiser.

2. Abra o **outpost_kaloki_x.exe**. O launcher aparece.

3. Na página **Play**, clique em **Install from package...** e escolha o seu pacote. Os arquivos dele são copiados para uma pasta `game` ao lado do programa.

4. Clique em **PLAY**.

**Atualizações:** quando o launcher abre, ele confere as versões deste repositório e oferece as novas (página About: **Updates**). Os arquivos do jogo, saves e configurações são mantidos.

**Nenhum arquivo do jogo vem junto.** O download é só o port: os arquivos do jogo vêm do seu próprio pacote.

Compilar à mão (para desenvolvedores):

```
.\setup.ps1 -Package "C:\path\to\Outpost Kaloki X"   # downloads the SDK, unpacks your game, translates the code
okx\build.bat okx-release                             # compiles
.\tools\package_windows.ps1 -Version v1.1.0           # optional: the release zip in dist\
```

O `Build Outpost Kaloki X.bat` faz os dois primeiros passos de uma vez, instalando as ferramentas se precisar. Para Linux, rode `tools/build_appimage.sh v1.1.0` no Linux ou no WSL depois do `setup.ps1`.

O jogo pronto fica em `okx\out\build\okx-release`. Indique os arquivos do seu jogo com `--game_data_root`, ou copie para uma pasta `game` ao lado do exe. As notas técnicas de cada correção estão no [NOTES.md](https://github.com/TekRantGaming/outpost-kaloki-x-recompiled/blob/HEAD/NOTES.md).
