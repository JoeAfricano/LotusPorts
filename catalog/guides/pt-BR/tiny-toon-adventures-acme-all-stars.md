<!-- source: https://github.com/rafarvns/Tiny-Toon-Acme-All-Stars---Native/blob/HEAD/README.md | fingerprint: da0e154e6fd4f85b -->

### Nativo, com um comando

```
./build.sh                  # uses ./rom.bin
./build.sh /path/to/rom.bin
./build.sh --release        # windowed build: no console, log to file, icon
```

O `build.sh` confere antes as suas ferramentas e para com uma mensagem clara se faltar algo. Ele não gera um executável pela metade para você descobrir depois.

Você precisa do Python 3.9+, do módulo `capstone`, de um compilador C, do `make` e do SDL3 (3.2 ou mais novo). As instruções completas por sistema e a solução de problemas estão no [BUILDING.md](https://github.com/rafarvns/Tiny-Toon-Acme-All-Stars---Native/blob/HEAD/BUILDING.md).
