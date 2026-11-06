<!-- source: https://github.com/zdiemer/haze-recomp/blob/HEAD/README.md | fingerprint: 6d51a686f3b569ea -->

Rode o `HazeRecomp-<version>-Setup.exe` da página de [versões (Releases)](https://github.com/zdiemer/haze-recomp/releases), escolha a sua ISO e clique em **Install**. O instalador baixa o auxiliar do RPCS3 e o firmware da Sony necessários, prepara o jogo e instala o launcher. Quando terminar, clique em **Play Haze**. Nas próximas vezes, use o **Haze Recomp** no menu Iniciar. Não precisa de outras ferramentas nem de ambiente de desenvolvimento.

Depois que o instalador prepara o jogo, o launcher está pronto para usar.

A imagem testada (já descriptografada) é `Haze (USA) (En,Fr,Es).iso` (8.498.184.192 bytes), BLUS30094, versão do disco 01.02 / versão do aplicativo 01.00. O **SHA-256** dela é:

```
e778fff687b4a122146d23160fe1fefdff14dddb5d5709b257cff76832dc2520
```

Confira a sua imagem pelo PowerShell:

```
Get-FileHash -LiteralPath 'D:\PS3\Haze (USA) (En,Fr,Es).iso' -Algorithm SHA256
```

Isso identifica a ISO testada byte a byte. A instalação também confere os dados do disco e os hashes do executável; outras regiões, cópias criptografadas e executáveis atualizados não são aceitos. Use a imagem do seu próprio disco; nenhuma vem nas versões.
