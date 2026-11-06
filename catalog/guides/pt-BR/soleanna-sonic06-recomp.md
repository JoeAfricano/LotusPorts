<!-- source: https://github.com/DrummingBird1/sonic06-recomp/blob/HEAD/README.md | fingerprint: 09955a2ad9e7c8ff -->

**Você precisa de**

- Windows 10 ou 11, 64 bits, com 8 GB de RAM ou mais e cerca de 10 GB livres

- [RPCS3](https://rpcs3.net/), para descriptografar o `EBOOT.BIN` do jogo no seu PC

- O seu próprio disco BLES00028, como ISO ou pasta

**Depois**

1. Baixe o `Soleanna-0.1.0.exe` na [versão](https://github.com/DrummingBird1/sonic06-recomp/releases/tag/v0.1.0) e abra. Ele pergunta onde instalar (uma pasta `Soleanna` ao lado dele, ou uma que você escolher), desempacota os scripts de montagem ali, cria um atalho na Área de Trabalho e abre o launcher. O arquivo não tem assinatura, então o Windows pode avisar antes: **Mais informações** e depois **Executar assim mesmo**. Abrir de novo abre a sua instalação, e uma versão mais nova oferece atualizar. Num clone do git, rode o `Sonic06.cmd`, que monta o launcher antes (precisa do .NET SDK).

2. Abra **Setup** e clique em **Set up everything**. Ele lista o que vai fazer e pergunta antes de instalar ou baixar qualquer coisa. Depois ele:

- instala as ferramentas de montagem que faltam: LLVM, CMake, Ninja, Git, uv e PowerShell 7 pelo winget, e o runtime C e o Windows SDK da Microsoft (cerca de 630 MB, com o [xwin](https://github.com/Jake-Shadle/xwin)) depois que você aceita a licença da Microsoft. Não precisa do Visual Studio; se você tiver com as ferramentas C++, a montagem usa.

- copia o seu disco para `game/` e descriptografa com o seu RPCS3,

- baixa o FFmpeg 8 (LGPL) para os vídeos, o SDL2 2.32.10 (zlib) para os controles e o ps3recomp,

- traduz o código do jogo e monta. Esta é a parte longa: cerca de 17 minutos num PC rápido com 32 GB, mais num PC mais lento ou com menos memória.

3. Clique em **Play**.

Pelo console, o `tools/launcher.ps1` faz os mesmos passos sem a janela.

Quanto tempo a montagem leva e quanta memória usa: o launcher monta quantos arquivos a memória permitir ao mesmo tempo (um com 8 GB, quatro com 16 GB, dez com 32 GB). No PC de 20 threads onde foi feito, dez por vez levaram 14 minutos, e dois por vez levaram 23 minutos com pico de 5 GB. Um PC com menos memória ainda não foi testado.
