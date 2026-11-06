<!-- source: https://github.com/goodshepherd23/JetMoto2-Recomp/blob/HEAD/README.md | fingerprint: ac197aa94065168e -->

Você precisa do Windows x64, do [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`winget install Microsoft.DotNet.SDK.10`) e da sua própria cópia do disco americano Championship Edition.

1. Baixe o zip da versão e extraia.

2. Copie a imagem do seu disco (`.cue` + 17 faixas `.bin`, com os nomes do Redump) para `disc\`.

3. Abra o `JetMoto2.exe`. Na primeira vez, ele recompila o jogo a partir do seu disco e monta tudo, o que leva alguns minutos; depois, o mesmo exe só abre o jogo. `JetMoto2.exe --rebuild` força uma nova montagem.

Depois de uma atualização, o launcher percebe a versão nova e remonta uma vez sozinho, mantendo configurações, cheats e cartões de memória.

Com o [Quiver Launcher](https://quiverlauncher.com/), instale o bloco do jogo, abra a pasta dele, faça o passo 2 e clique em Play.
