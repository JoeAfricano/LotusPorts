<!-- source: https://github.com/riotashley81-jpg/vagrantstory_recomp/blob/HEAD/README.md | fingerprint: 1ed60d5db7facd45 -->

A página de [versões (Releases)](https://github.com/riotashley81-jpg/vagrantstory_recomp/releases) tem uma versão para Windows que também não traz nada do código do jogo: ela lê o código do Vagrant Story do seu disco enquanto você joga. Na primeira abertura, ela compila em segundo plano o motor de batalha, os menus e a tela de título a partir do seu disco, com um gcc que vem junto (prioridade baixa, batalhas primeiro; pode levar uma hora ou mais e fica guardado para sempre). Até lá, e para todo o resto, o código roda pelo interpretador do psxrecomp e pelo TinyCC, como antes.

1. Extraia onde quiser.

2. Abra o `VagrantStory_Recompiled.exe` e indique no launcher a sua própria cópia do Vagrant Story (USA), SLUS-01040 (`.cue` + `.bin`).

3. Ajuste os extras na página Mods do launcher (todos ligados por padrão; a tela larga começa em 16:9).

Vagrant Story é © Square Enix. Este projeto não é afiliado nem aprovado pela Square Enix.
