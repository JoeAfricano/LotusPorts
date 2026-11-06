<!-- source: https://github.com/CompaLarios18/MH4UltimateDC/blob/HEAD/README.md | fingerprint: d47e1a0bdc039cf2 -->

```
# 1. Python tooling (3.10+)
pip install -r requirements.txt

# 2. Place your own decrypted dump's ExeFS code + exheader in baserom/  (see docs/SETUP.md)
#    baserom/exheader.bin
#    baserom/code.bin      (compressed or decompressed, both work)

# 3. Produce an ELF for your disassembler
python3 tools/codebin2elf.py --exheader baserom/exheader.bin --code baserom/code.bin -o build/mh4u.elf

# 4. Run the tool tests
python3 -m pytest

# 5. Recompile the game into C (build/recomp/, never committed) and build it
#    (recompile.py takes the USA v1.3.0 update's code, whose addresses config/ holds;
#    for Europe's v1.1.0 update, use tools/build_game.py below)
python3 tools/recompile.py --exheader baserom/exheader.bin --code baserom/code.bin -o build/recomp
(cd tools && python3 -m recomp.build ../build/recomp --harness --opt O0 -D RC_CHECK_RETURNS)

# Or, to play: the update .cia (USA v1.3.0 or Europe's v1.1.0) straight to an installed code library (see docs/PLAYING.md)
python3 tools/build_game.py --dump "MH4U v1.3.0 update.cia" --install --fetch-compiler
```

Em resumo: instale as ferramentas de Python, coloque o código (ExeFS) e o exheader da sua própria cópia descriptografada em `baserom/`, recompile o jogo para C e compile. Para só jogar, o último comando monta e instala o jogo a partir do `.cia` da atualização (USA v1.3.0 ou Europa v1.1.0); veja o docs/PLAYING.md do repositório.
