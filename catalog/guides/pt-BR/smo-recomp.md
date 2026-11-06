<!-- source: https://github.com/Coz-0/smo-recomp/blob/HEAD/README.md | fingerprint: 57c7950bf7fba53c -->

```
py -3.13 -m venv .venv
.venv\Scripts\python -m pip install lz4 capstone unicorn numpy Pillow
.venv\Scripts\python scripts\setup.py --dump "<folder of your dump>"
.venv\Scripts\python scripts\run.py play --exe smo_native.exe --seconds 0
```

A primeira compilação demora (o jogo é convertido e compilado); nas próximas vezes, o `setup.py` refaz só o que mudou. O [docs/building.md](https://github.com/Coz-0/smo-recomp/blob/HEAD/docs/building.md) traz os detalhes, as configurações e a solução de problemas.
