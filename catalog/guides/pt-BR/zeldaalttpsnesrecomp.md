<!-- source: https://github.com/mstan/ZeldaAlttPSNESRecomp/blob/HEAD/README.md | fingerprint: 7956d3407da2ed68 -->

1. Baixe o zip da versão mais recente na página de [versões (Releases)](https://github.com/mstan/ZeldaAlttPSNESRecomp/releases) e extraia.

2. Abra o `ZeldaALttPSNESRecomp.exe`. Na primeira vez, uma janela pede o seu arquivo do The Legend of Zelda: A Link to the Past (USA), **obtido legalmente** (`.sfc` / `.smc`). O CRC32 esperado é `0x777AAC2F` (cópia americana canônica no-intro, 1 MiB, LoROM). Cabeçalhos SMC de 512 bytes são removidos sozinhos antes da conferência, então funciona com ou sem cabeçalho.

3. Edite o `keybinds.ini` (criado ao lado do exe na primeira vez) para trocar as teclas e reabra.

O caminho escolhido fica guardado no `rom.cfg`, ao lado do exe, e as próximas vezes não perguntam de novo.
