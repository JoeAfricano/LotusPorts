<!-- source: https://github.com/SashexSRB/SSNE-Native/blob/HEAD/README.md | fingerprint: f19080cb88b2c5d0 -->

1. **Baixe o pacote do launcher** para o seu sistema na página de [versões (Releases)](https://github.com/SashexSRB/SSNE-Native/releases) (`...-Linux-x86_64.tar.gz` ou `...-Windows-x64.zip`) e extraia numa pasta onde dê para gravar: os dados do jogo, os saves e as configurações ficam ao lado. O pacote traz o launcher e o kit de compilação, mas nenhum código do jogo. Para compilar você mesmo, veja o [docs/BUILDING.md](https://github.com/SashexSRB/SSNE-Native/blob/HEAD/docs/BUILDING.md).

2. **Abra o launcher**: `SSNE-Native` (Linux) ou `SSNE-Native.exe` (Windows).

3. **Select Disc**: escolha a imagem do seu disco ou arraste para a janela do launcher. Ele confere se é o jogo certo:

- Verified: uma cópia conhecida e boa.

- Recognized (unverified dump): o jogo certo, mas uma cópia diferente ou modificada (por exemplo um `.gcz` / `.ciso` reduzido). Instala e roda mesmo assim.

- Wrong game / Not a valid disc: recusado.

4. **Install Game**: copia os arquivos do jogo da imagem para `game_data` (cerca de 1,3 GB), depois traduz o programa do jogo e compila no seu PC. Leva cerca de um minuto com o clang, alguns minutos sem ele. Ele cria o `SSNE-Native-Game`.

5. **Play**. Dali em diante, também dá para abrir o jogo direto pela pasta dele:

```
   ./SSNE-Native-Game --data game_data
```

(Windows: `SSNE-Native-Game.exe --data game_data`.)

Depois de atualizar para um pacote de launcher mais novo, escolha **Update Game** no launcher uma vez: o jogo é montado de novo com o motor novo (dados do jogo, saves e configurações ficam).

Tudo o que é feito a partir do seu disco (`game_data`, `SSNE-Native-Game`) é só para você: não compartilhe.
