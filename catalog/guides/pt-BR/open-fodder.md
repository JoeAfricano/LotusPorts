<!-- source: https://github.com/OpenFodder/openfodder/blob/HEAD/README.md | fingerprint: 44ca931299b19e0b -->

Veja o [INSTALL.md](https://github.com/OpenFodder/openfodder/blob/master/INSTALL.md).

Para a versão mais recente para Windows, você pode baixar o executável montado durante os testes [aqui](https://s3.amazonaws.com/openfodder-builds/OpenFodder-x86-Release-latest.zip).

### Como jogar

O Open Fodder aceita seis demos da plataforma Amiga e três versões comerciais (veja as versões aceitas no repositório).

### Parâmetros de linha de comando

```
  OpenFodder [OPTION...]

      --about                   About
  -h, --help                    Help
      --pc                      Default to PC platform data
      --amiga                   Default to Amiga platform data
      --engine cf1              Default to engine (single map/random)
                                (default: cf1)
      --columns 22              Number of camera columns (default: 22)
      --rows 16                 Number of camera rows (default: 16)
  -w, --window                  Start in window mode
      --window-scale arg        Set the window scale (default: 0)
      --integer-scaling         Set integer scaling (default: true)
      --cheats                  Enable cheat keys

      --max-sprite 45           Set the maximum sprites (default: 45)
      --max-spawn 10            Set the maximum spawn (default: 10)
      --sleep-delta 2           Set the engine speed (default: 2)
      --demo-record "Demo File"
                                Record Demo (default: )
      --demo-record-all         Record Demo
      --demo-play "Demo File"   Play Demo (default: )
      --unit-test               Run Tests
      --unit-test-headless      Run Tests, with no output

      --nosound                 Disable sound output
      --playground              Sprite playground
	  
      --skipintro               Skip all game intros
      --skipbriefing            Skip mission briefing
      --skipservice             Skip mission debriefing
      --skiphill                Skip the hill

      --list-campaigns          List available campaigns
  -c, --campaign "name"         Starting campaign (default: )
  -m, --mission 1               Starting mission (default: 0)
  -p, --phase 2                 Starting phase (default: 0)

      --single-map "MyMap"      Play a single map (default: )
  -r, --random                  Generate and play a random map
      --random-save "MyMap"     Generate and save a random map (default: )
      --script "script.js"      Name of script to execute (default: )
```

As mais úteis: `-w` abre em janela, `--skipintro` pula as aberturas, `-c`/`-m`/`-p` escolhem campanha, missão e fase iniciais e `-r` gera e joga um mapa aleatório.

### Atalhos

- F1: troca para a versão Amiga (se disponível)

- F2: troca para a versão PC (se disponível)

- F9: deixa o esquadrão atual invencível (com `--cheats`)

- F10: completa o mapa atual (com `--cheats`)

- F11: alterna a tela cheia

- F12: prende o cursor na janela

- + / -: aumenta / diminui a janela

- Esc: volta ao menu / abandona a fase

- M: mostra o mapa (só nas versões comerciais)

- P: pausa

- 1 / 2 / 3: escolhe o esquadrão 1, 2 ou 3

- Espaço: troca de arma
