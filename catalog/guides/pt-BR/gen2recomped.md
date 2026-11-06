<!-- source: https://github.com/UNDERdecoded/Gen2Recomped/blob/HEAD/README.md | fingerprint: 07d2f7eb66c2c4fa -->

Abra o app. Na primeira abertura, escolha o seu arquivo `.gb` / `.gbc` / `.gba` / `.nds`, obtido legalmente, ou arraste para a janela. A importação leva alguns segundos para a 1ª geração, uns dois minutos para a 2ª e mais para os jogos de GBA e DS; depois, o jogo abre sozinho. Tem uma pasta cheia de cópias? **Tools → Import ROMs** aceita vários arquivos de uma vez e importa cada um que reconhecer.

Num console ou portátil não há seletor de arquivos: você copia a cópia para uma pasta que o launcher lê. Veja a seção Downloads do repositório para o caminho exato de cada aparelho.

### Jogos aceitos

Só cópias canônicas específicas são aceitas. O importador confere o SHA-1 antes de criar qualquer dado do jogo; uma cópia não aceita é identificada (jogo, região, revisão) para você saber o motivo. Veja [docs/rom-revisions.md](https://github.com/UNDERdecoded/Gen2Recomped/blob/HEAD/docs/rom-revisions.md).

- Jogo · Sistema · Estágio · SHA-1

- Red · GB · Estável · `ea9bcae617fdf159b045185467ae58b2e4a48b9a`

- Blue · GB · Estável · `d7037c83e1ae5b39bde3c30787637ba1d4c48ce2`

- Yellow · GB · Estável · `cc7d03262ebfaf2f06772c1a480c7d9d5f4a38e1`

- Gold · GBC · Estável · `d8b8a3600a465308c9953dfa04f0081c05bdcb94`

- Silver · GBC · Estável · `49b163f7e57702bc939d642a18f591de55d92dae`

- Crystal (Rev 1) · GBC · Estável · `f2f52230b536214ef7c9924f483392993e226cfb`

- Crystal (Rev 0) · GBC · Estável · `f4cd194bdee0d04ca4eac29e09b8e4e9d818c133`

- Polished Crystal 3.2.3 · GBC · Beta · `6930b48af5844d373e3c9130f26d6dd1084cf4ed`

- Prism · GBC · Experimental · `752076692ae3387cf426ce5f51a98c6b60e8df6a`

- FireRed (Rev 0) · GBA · Alpha · `41cb23d8dccc8ebd7c649cd8fbb58eeace6e2fdc`

- Emerald · GBA · Alpha · `f3ae088181bf583e55daf962a92bb46f4f1d07b7`

- Platinum (Rev 1) · DS · Pré-alpha · `0862ec35b24de5c7e2dcb88c9eea0873110d755c`

- Platinum (Rev 0) · DS · Pré-alpha · `ce81046eda7d232513069519cb2085349896dec7`

- HeartGold (USA) · DS · Pré-alpha · `4fcded0e2713dc03929845de631d0932ea2b5a37`

- SoulSilver (USA) · DS · Pré-alpha · `f8dc38ea20c17541a43b58c5e6d18c1732c7e582`

- Black (USA/EU) · DS · Pré-alpha · `26ad0b9967aa279c4a266ee69f52b9b2332399a5`

- White (USA/EU) · DS · Pré-alpha · `bc696a0dfb448c7b3a8a206f0f8214411a039208`

Cópias de HeartGold, SoulSilver, Black e White com o preenchimento `FF` final removido também são aceitas, desde que todos os bytes do jogo estejam intactos. Veja [docs/hgss-bw-import.md](https://github.com/UNDERdecoded/Gen2Recomped/blob/HEAD/docs/hgss-bw-import.md). O launcher mostra o estágio de cada jogo no cartão; jogos Alpha e Pré-alpha dá para jogar, mas espere falhas e erros.

O app não traz arquivo de jogo nem dados já extraídos. Músicas, efeitos sonoros e gritos são sintetizados durante o jogo a partir de pequenos programas de áudio copiados do arquivo conferido.

### Sobre os avisos do Windows Defender

O Windows Defender às vezes marca versões feitas com LÖVE com uma detecção genérica de aprendizado de máquina como `Trojan:Win32/Wacatac!ml`. É um falso positivo conhecido: o exe é o runtime oficial do LÖVE com o arquivo do jogo anexado (o jeito padrão de distribuir jogos LÖVE), e a heurística do Defender desconfia de executáveis sem assinatura com dados anexados. As versões publicam somas SHA-256 (`sha256sums.txt`) para você conferir o download, e você pode verificar um arquivo marcado no [VirusTotal](https://www.virustotal.com/), onde essas versões saem limpas em todos os antivírus, menos na heurística do Defender.
