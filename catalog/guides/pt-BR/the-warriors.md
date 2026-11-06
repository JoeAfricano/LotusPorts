<!-- source: https://github.com/Matt-code-d/TheWarriorsRecomp/blob/HEAD/README.md | fingerprint: ea385ab2ea99e2e0 -->

1. Instale o [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

2. Compile uma vez **ou** use uma pasta de versão que já tenha o `Warriors.exe` e o `WarriorsLauncher.exe`.

3. Coloque o [extract-xiso](https://github.com/XboxDev/extract-xiso) em `tools/extract-xiso/artifacts/extract-xiso.exe` se quiser o modo de extração.

4. Abra o launcher **a partir da saída da compilação EUR**:

```
.\warriors-recomp\out\build\release\WarriorsLauncher.exe
```

No launcher:

1. **SETUP / ISO** → escolha o seu `.iso` europeu (EUR) → **INSTALL INTO ASSETS**

2. **LANGUAGE** → os pacotes listados vêm do nome desse disco (por exemplo `En,Fr,De,Es,It`); a escolha é gravada antes do **PLAY**

3. **GRAPHICS** → FPS, resolução, modo de tela, D3D12 (Vulkan reservado, ainda não compilado)

4. **PLAY**

Configurações do launcher: `%USERPROFILE%\Documents\thewarriors\launcher_settings.json`. Configurações ao lado do exe: `settings.toml`, `language.cfg`. Registros: `logs\warriors.log`.
