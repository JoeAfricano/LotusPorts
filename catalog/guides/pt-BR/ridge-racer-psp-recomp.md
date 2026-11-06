<!-- source: https://github.com/SpeedyJustice2/Ridge-Racer-PSP-Static-Recomp/blob/HEAD/README.md | fingerprint: 495c2b3156cfba0f -->

1. Baixe o **Setup.exe** na página de [versões (Releases)](https://github.com/SpeedyJustice2/Ridge-Racer-PSP-Static-Recomp/releases) e abra. (O Windows pode avisar "editor desconhecido" porque o arquivo não tem assinatura digital; escolha Mais informações → Executar assim mesmo.)

2. Escolha o seu jogo: uma imagem de disco **descriptografada** (`.iso`) ou a pasta que tem `PSP_GAME`.

3. Escolha onde instalar (qualquer disco). A instalação monta o jogo em segundo plano; isso pode levar 20 minutos ou mais na primeira vez e não precisa de outros downloads.

4. Clique em Finish. As configurações ficam no jogo: **Options > RECOMP OPTIONS** (segure Shift ao abrir o jogo para a janela antiga de configurações).

O `README-BETA.md` deste repositório é o guia do jogador (instalação, controles, relatos de erro). A instalação copia esse arquivo para a pasta do jogo como `README.txt`.

### A sua cópia precisa estar descriptografada

O port lê o executável do jogo (`PSP_GAME/SYSDIR/BOOT.BIN`). Ele precisa estar descriptografado (começa com `ELF`); um criptografado (começa com `~PSP`) é recusado com uma mensagem clara. Este projeto não fornece, não indica e não explica como copiar ou descriptografar um jogo; isso fica por sua conta. Nenhuma ferramenta ou chave de descriptografia é fornecida aqui.
