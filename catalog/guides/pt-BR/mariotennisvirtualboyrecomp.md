<!-- source: https://github.com/mstan/MarioTennisVirtualBoyRecomp/blob/HEAD/README.md | fingerprint: 35db4e9b04f09603 -->

A **versão v0.2.3 para Windows** inclui o launcher no estilo Virtual Boy, a arte da caixa e o mod opcional em cores. As cores vêm desligadas; o arquivo original do jogo nunca é alterado. Há scripts de compilação para Linux/macOS para quem compila a partir do código.

1. Baixe o `MarioTennisVirtualBoyRecomp-windows-x64.zip` na página de [versões (Releases)](https://github.com/mstan/MarioTennisVirtualBoyRecomp/releases).

2. Extraia o ZIP inteiro, incluindo `assets`, `licenses` e `SDL2.dll`.

3. Use a cópia do seu próprio cartucho do Mario's Tennis (o programa não roda com nenhum outro arquivo: ele confere o CRC32 ao abrir). Necessário:

- **CRC32:** `0x7CE7460D`

- **SHA-256:** `5dc5e6b5d5f538f56b3b9727db1c7931d9dfe1bbd0743f698897e0fd90e70101`

- **Tamanho:** 524.288 bytes

4. Abra:

```
   MarioTennisVirtualBoyRecomp.exe --rom path\to\marios_tennis.vb
```

Para ligar as cores, importe o `marios-tennis-full-color-0.2.2.vbmod` incluído em **Mods** e ligue **Full-color renderer**. Também dá para instalar e ligar pela linha de comando:

```
.\MarioTennisVirtualBoyRecomp.exe --install-mod .\marios-tennis-full-color-0.2.2.vbmod --enable-mod marios-tennis.full-color:full-color
```

O executável da versão não traz o depurador TCP nem o gravador interno de poses.
