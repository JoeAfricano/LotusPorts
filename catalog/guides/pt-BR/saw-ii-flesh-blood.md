<!-- source: https://github.com/Matt-code-d/Saw2Recomp/blob/HEAD/README.md | fingerprint: eedec20232812b72 -->

1. Instale o [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (e o .NET 8 SDK se for compilar a partir do código).

2. Compile o jogo uma vez. O CMake também coloca o **`Saw2Launcher.exe` na mesma pasta do `saw2.exe`**.

3. Coloque o [extract-xiso](https://github.com/XboxDev/extract-xiso) em `../tools/extract-xiso/artifacts/extract-xiso.exe` (ao lado deste repositório) e o ReXGlue SDK em `../tools/rexglue-sdk-bin/win-amd64`.

4. Abra o launcher a partir da saída da compilação (ou `dotnet run` em `launcher/` durante o desenvolvimento):

```
.\out\build\win-amd64-relwithdebinfo\Saw2Launcher.exe
```

No launcher:

1. **SETUP / ISO** → escolha o seu `.iso` → **EXTRACT & INSTALL**

2. **LANGUAGE** → só aparecem os pacotes que existem nesse disco

3. **VIDEO / AUDIO / INPUT** → ajuste do seu jeito

4. **PLAY**

As configurações ficam em `%USERPROFILE%\Documents\saw2\launcher_settings.json`. Os registros vão para `Documents\saw2\saw2.log`.

Prefira a versão **RelWithDebInfo** para jogar. A versão Debug pode parar com erro na parte gráfica (Xenos).
