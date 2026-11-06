<!-- source: https://github.com/thefixinhixon/UltraCombo/blob/HEAD/README.md | fingerprint: ce637d417bf9dcab -->

Coloque os arquivos de um dos jogos, ou dos dois, em `run/`, como vêm nos conjuntos do MAME (tirados das suas próprias placas):

**Killer Instinct** (`kinst`):

- `ki-l15d.u98`: ROM de inicialização, versão L1.5D; `ki-l14.u98` (L1.4) é opcional

- `u10-l1` … `u13-l1`, `u33-l1` … `u36-l1`: ROMs de som

- `kinst.chd`: o disco rígido

**Killer Instinct 2** (`kinst2`):

- `ki2-l14.u98`: ROM de inicialização, versão L1.4

- `ki2_l1.u10` … `ki2_l1.u13`, `ki2_l1.u33` … `ki2_l1.u36`: ROMs de som

- `kinst2.chd`: o disco rígido

Os nomes dos arquivos dos dois jogos não se misturam. Um CHD também pode ficar numa pasta com o nome do conjunto (`run/kinst/kinst.chd`, `run/kinst2/kinst2.chd`), como o MAME guarda. Uma imagem bruta do disco (`kinst.raw`, `kinst2.raw`, do `chdman extractraw`) também funciona e é usada se houver as duas. Os caminhos de cada jogo também podem ser definidos no arquivo de configurações dele (`ki1.ini`, `ki2.ini`).

As imagens de disco são só lidas. As configurações, auditorias e recordes de um jogo (os primeiros 32 KB do disco) vão para `saves/<boot ROM>.settings`, um arquivo por versão do jogo. Uma imagem bruta em que uma versão anterior deste programa gravou mantém as configurações: elas são copiadas para esse arquivo na primeira abertura. `ultracombo --game ki2 --write-raw kinst2.raw` grava um disco como imagem bruta, igual ao `chdman extractraw`, se você precisar.

### Conferindo os seus arquivos

Estes são os arquivos dos conjuntos `kinst` e `kinst2` do MAME (as cópias que o MAME lista como boas). Ao abrir, o programa compara os seus arquivos com eles e avisa, dizendo o nome, se algum não bater. Para conferir você mesmo, use `md5sum` (Linux), `certutil -hashfile FILE MD5` (Windows) ou qualquer gerenciador de ROMs. Cada ROM tem 524.288 bytes.

**Killer Instinct:**

- Arquivo · O quê · MD5 · SHA1

- `ki-l15d.u98` · ROM de inicialização, L1.5D (padrão) · `6a63e4bbdef8ea9f0d26f7244341ca08` · `607394d4ba1713f38c2cb5159303cace9cde991e`

- `ki-l14.u98` · ROM de inicialização, L1.4 (opcional) · `aff07bdb5bf4763bf81e243077cf9761` · `07254f20707377f7195e64675eb6458e663c1a9a`

- `u10-l1` · som · `9ee2b2cdda7ede8811a57507ef61e07b` · `810d455df8f385d76143e9d7d048f2b555ff8bf0`

- `u11-l1` · som · `bce2d8cd20e94d4406c19fd55d798946` · `0595909cb667c38ac7c8c7bd0646b28899e27777`

- `u12-l1` · som · `14bb46cef4884f8df5df1fa985e6d3bf` · `7a8ee405c118fd176b66353fa7bfab888cc63cd2`

- `u13-l1` · som · `ca0904ee517845eed5509426cf5d8560` · `ea4d1f153eb284f1bcfc5295fbce316bba6083f4`

- `u33-l1` · som · `da9e7f5f70320b51e6fdc23e9337484c` · `b22e365bc8d58a80eaac226be14b4bb8d9a04844`

- `u34-l1` · som · `207395792342705bea307ecb321a95bf` · `ee439f5162a2b3379d3f802328017bb3c68547d2`

- `u35-l1` · som · `a1758b43b42b32c25c770dc3ac7badc3` · `48c4c954ac9db648f28ad64f9845e19ec432eec3`

- `u36-l1` · som · `57b73bec5d0e8c90d8898e102ce4066b` · `cc78070cc41701e9a91fde5cfbdc7e1e83354854`

- `kinst.raw` · disco, recém-extraído · `6d4c2f152c9a18ab3a9b05b8804306a8` · `a37a2c5e52ea936a715210d237874dd573bb002f`

**Killer Instinct 2:**

- Arquivo · O quê · MD5 · SHA1

- `ki2-l14.u98` · ROM de inicialização, L1.4 · `8681c9d373394b17f3499e8902055dab` · `aa7a2a9d72a47dd0ea2ee7b2776b79288060b179`

- `ki2_l1.u10` · som · `fa0c7f9dc7ee0995913a4769996a7d80` · `acfc9460cd5df01403b7f00b2f68c2a8734ad6d3`

- `ki2_l1.u11` · som · `dc87ed16f9d95cb0aa0ea0117ef4be09` · `fe7fc78f1c60b15f2bbdc4c455f55cdf30f48ed4`

- `ki2_l1.u12` · som · `0b034258a8561ac4bfa339a781777c4a` · `9997a83432cb720f65b40a8af46f31a5d0d16d8e`

- `ki2_l1.u13` · som · `6d42f7de396c7f9eb760e51af4fcdca2` · `7ff164fc2f079d039921594be92208973d43aa03`

- `ki2_l1.u33` · som · `aae93e81ac881c56d0aecab2f59c8797` · `0ab6dbfb76e9fa2db072e287864ad1f9d514dd9b`

- `ki2_l1.u34` · som · `116cbfd189879a3a601f1f7ccb64285c` · `48456a7b6592c40bc9c664dcd2ee2cfd91942811`

- `ki2_l1.u35` · som · `ec3103d67a528412da9cb91a2729df65` · `24a3ff009c8a7f5a0bfcb198b8dcb5df365770d3`

- `ki2_l1.u36` · som · `a03144702e28a9e89e50c5a377cb47b3` · `0fca72c40067034939b984b4bf32972a5a6c26af`

- `kinst2.raw` · disco, recém-extraído · `2563b089b316f2c8636d78af661ac656` · `ab0242233d2eaf9d907abe246a54e09a8a2561a5`

As somas do disco bruto valem para uma imagem tirada direto do CHD. Este programa nunca grava nela, mas versões anteriores gravavam, assim como outros programas que usam imagens brutas; uma imagem que já foi jogada não bate mais. Para conferir um disco, confira o CHD. O MD5 do próprio arquivo CHD depende da versão do chdman que o criou, então compare o SHA1 que o `chdman info -i FILE` mostra como "SHA1" (o que o MAME usa):

- CHD · SHA1 (do `chdman info`) · MD5 do arquivo atual do MAME

- `kinst2.chd` · `e7c9291b4648eae0012ea0cc230731ed4987d1d5` · `5970ca942c0c7c5f62f651bf93e15dc5`

- `kinst.chd` · `81d833236e994528d1482979261401b198d1ca53` · `a542547abdff0a28df440c3a4d1cfdde`

### Abrindo

Abra o `ultracombo` (`UltraCombo.exe` no Windows; o AppImage no Linux). Com os dois jogos instalados, ele pergunta qual jogar: esquerda/direita (ou um clique) escolhe, Enter ou o botão de seleção do controle começa (aparece na tela). Ele lembra a última escolha. Com um jogo só, ele abre direto. Menu → **Change game** volta à escolha, e `--game ki1` ou `--game ki2` pula a pergunta (para atalhos).

Na primeira vez que você joga cada jogo, e cada versão dele, o programa mostra "PREPARING THE GAME FOR THIS PC": ele inicia os arquivos do seu jogo no interpretador até o jogo carregar o código, tira uma cópia desse código, traduz para C, junto com o programa DSP da placa de som tirado das ROMs de som, e compila num módulo nativo. Isso leva cerca de um minuto, uma vez; o módulo fica guardado em `cache/` (um novo é feito se os arquivos do jogo mudarem). O compilador usado é o primeiro que funcionar da lista do repositório.
