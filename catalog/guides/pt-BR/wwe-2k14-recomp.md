<!-- source: https://github.com/PromethenDev/wwe2k14-recomp/blob/HEAD/README.md | fingerprint: 3d4a6c8c9c69ba74 -->

1. Baixe o `WWE2K14-Recomp-v1.4.10.zip` (o pacote com Windows e Linux juntos) e extraia numa pasta comum, como Documentos ou Área de Trabalho (não em `C:\Program Files`). Para baixar só o Linux, use o `Linux-WWE2K14-Recomp-v1.4.10.zip`.

2. Abra o **`WWE 2K14 Launcher.exe`** (Linux: `Linux-x86_64/wwe2k14-launcher`).

3. Em **Game files**, faça uma destas:

- coloque a pasta do seu jogo na pasta `WWE 2K14`, ao lado do launcher, ou

- clique em **Find Automatically** para procurar neste PC, ou

- clique em **Choose a Folder**, ou em **Use a Disc Image** para extrair o seu próprio `.iso` uma vez.

4. Acrescente a atualização do título (e os pacotes de DLC, se tiver) com **Add TU or DLC**. O launcher aceita os seus arquivos de pacote e instala onde o jogo espera.

5. Clique em **Play**. A primeira abertura leva alguns segundos a mais enquanto o jogo prepara os gráficos; depois, abre normalmente.

O launcher confere cada arquivo e mostra **Game files ready** quando está tudo certo.

```
WWE2K14-Recomp-x64/
├── WWE 2K14 Launcher.exe       # start here (Windows)
├── wwe2k14.exe                 # the game (started by the launcher)
├── wwe2k14.toml                # keyboard keys (change them on the launcher's Controls page)
├── avcodec-58.dll, avutil-56.dll
├── shaders/                    # Native renderer shaders (shared by Windows and Linux)
├── shaders32/                  # Native renderer shaders for GPUs without 64-bit shader support (Intel Ice Lake)
├── shadertools/                # converts rare missing shaders at run time (Linux: Linux-x86_64/shadertools)
├── Linux-x86_64/               # Linux launcher + game
├── README.md, CHANGELOG.md, DISCLAIMER.md, CONTRIBUTORS.md, SHA256SUMS
├── licenses/, third-party-licenses/, ffmpeg-source-*.zip
├── WWE 2K14/                   # your game folder (default.xex + default.xexp + data)
├── userdata/                   # saves, DLC and settings (created on first run)
└── logs/                       # game logs (created on first run)
```

Comece pelo `WWE 2K14 Launcher.exe`. A pasta `WWE 2K14/` é a do seu jogo (`default.xex` + `default.xexp` + dados); `userdata/` guarda saves, DLC e configurações, e `logs/` os registros (as duas são criadas na primeira abertura).
