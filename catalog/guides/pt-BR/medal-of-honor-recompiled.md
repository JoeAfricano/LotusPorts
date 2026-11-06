<!-- source: https://github.com/FractalEngineer/Medal-Of-Honor-PSX-Recomp/blob/HEAD/README.md | fingerprint: da85154b941b25df -->

Baixe a versão para Windows e extraia o ZIP inteiro. Conecte o seu headset e clique duas vezes no launcher do seu runtime: `RunVR-VDXR.bat` (Virtual Desktop VDXR), `RunVR-SteamVR.bat` ou `RunVR-Oculus.bat` (Meta Quest Link / Air Link). Cada um escolhe aquele runtime só para essa abertura e não muda o runtime ativo do sistema. Quando pedir, escolha a imagem CUE/BIN do seu próprio **SLUS-00974 (NTSC-U)**; o launcher lembra onde ela está. Deixe os arquivos BIN ao lado do CUE.

Os vídeos de abertura, os menus e o briefing aparecem numa tela dentro do VR; durante o jogo, a imagem passa a ser feita para cada olho. Não precisa de estado salvo, Python nem ferramentas de desenvolvimento. O jogo roda até você fechar, e os erros de abertura ficam visíveis no console. Guarde a sua pasta `saves/` ao atualizar.

Para escolher um disco direto:

```
.\RunVR-VDXR.bat -DiscPath "C:\Games\Medal of Honor\medal-of-honor.cue"
```

Use o `RunFlat.bat` para jogar normalmente, sem VR. Veja o [ALPHA_README.md](https://github.com/FractalEngineer/Medal-Of-Honor-PSX-Recomp/blob/HEAD/ALPHA_README.md) para mais detalhes de instalação e as limitações da versão alpha.
