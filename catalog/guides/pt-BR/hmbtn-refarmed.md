<!-- source: https://github.com/d4rksp4rt4n/HMBTN-ReFarmed/blob/HEAD/README.md | fingerprint: 127ae2d3fedbc584 -->

Abra o `HarvestMoonBTN.exe` (Windows) ou o `./HarvestMoonBTN` (Linux) na pasta `dist`. O jogo precisa dos seus dados (gráficos, música, texto), que vêm da sua própria cópia, de um destes dois jeitos:

- **Imagem do disco (padrão).** O jogo lê o seu `.cue`/`.bin`. O `hmbtn.ini` lembra a imagem usada para montar a pasta; se você mover a imagem, coloque o `.cue` e o `.bin` na pasta `disc`, ao lado do executável, ajuste `disc =` no `hmbtn.ini` ou passe o caminho na linha de comando.

- **Pacote de dados (`python build.py --pak`).** A imagem do seu disco é reempacotada em `HarvestMoonBTN.pak` (58 MB em vez de 214 MB) ao lado do executável, e a imagem não é mais necessária. O pacote mantém a organização exata dos setores do disco (o jogo toca a música direto do disco, intercalada com outros dados), e cada bloco é conferido com uma soma de verificação do seu disco original ao carregar, então o jogo vê exatamente os mesmos dados. `HarvestMoonBTN --verify-data` confere o arquivo inteiro.
