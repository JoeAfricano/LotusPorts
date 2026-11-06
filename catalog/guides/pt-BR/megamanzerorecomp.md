<!-- source: https://github.com/mstan/MegaManZeroRecomp/blob/HEAD/README.md | fingerprint: 9f5ca3c8f9b493fe -->

1. Baixe o `MegaManZeroRecomp-windows-x64-v0.0.6.zip` na página de [versões (Releases)](https://github.com/mstan/MegaManZeroRecomp/releases) e extraia a pasta inteira.

2. Abra o `MegaManZeroRecomp.exe`.

3. Quando pedir, escolha o seu arquivo do Mega Man Zero (USA), obtido legalmente, e a cópia da BIOS do seu GBA. Os caminhos ficam guardados para as próximas vezes.

4. Jogue. Saves (SRAM), estados salvos e caches de código nativo ficam ao lado do programa extraído.

O logo e o som de abertura da BIOS são pulados por padrão, então o jogo abre direto. Isso só pula a abertura: as chamadas do sistema continuam sendo atendidas pela BIOS real recompilada (LLE), que segue como referência. A cópia da BIOS do GBA continua obrigatória. Para ver a animação original de abertura, configure

```
[bios]
skip_intro = false
```

ou abra com `MegaManZeroRecomp.exe --no-bios-skip-intro`.
