<!-- source: https://github.com/n0madic/Soulcalibur2HD-recomp/blob/HEAD/README.md | fingerprint: 2fd894f8156fb85d -->

```
unzip sc2hd-0.1.0-macos-arm64.zip
cd sc2hd
xattr -dr com.apple.quarantine .      # only if macOS refuses to open the downloaded program

# first run: give it the game (the zip is fine as it is) and, optionally, its update; the .rap is found inside the zip
./sc2hd.py --pkg "Soulcalibur II HD Online (USA).zip" --pkg UP0700-NPUB31366_00-SC2HDONLINEGAME1-A0101-V0100-PE.pkg

# every later run
./sc2hd.py
```

Na primeira vez, indique o jogo (o zip serve como está) e, se quiser, a atualização; o `.rap` é encontrado dentro do zip. Nas próximas vezes, é só rodar `./sc2hd.py`.

O `./sc2hd.py` é um script Python 3.10+: no Windows (ou se a permissão de execução se perder), abra com `py sc2hd.py` ou `python3 sc2hd.py`.

Se a licença não estiver no zip, acrescente `--rap <file>.rap`. Indique primeiro o jogo e depois a atualização (se quiser). Para acrescentar a atualização mais tarde, rode de novo com os dois pacotes: o jogo é reinstalado e traduzido de novo (os seus saves são mantidos).

A primeira vez leva alguns minutos (uns dois num PC rápido):

1. **instalar** (segundos): desempacota os pacotes e os descriptografa com a sua licença;

2. **converter** (cerca de 10 s): traduz o código PPU do jogo e os cinco programas SPU para C/C++;

3. **compilar** (cerca de 30 s em oito núcleos): monta tudo numa biblioteca, `game.dylib`;

4. **jogar**.

Nas próximas vezes, vai direto ao passo 4. Nada é refeito se as entradas não mudarem.
