<!-- source: https://github.com/tolotratlt/rival-schools-nekketsu-seishun-nikki-2-recomp/blob/HEAD/README.md | fingerprint: ed317fafd9402cb4 -->

1. Baixe o ZIP de instalação para Windows (`rival-schools-sjgnsn-<version>-windows-x64.zip`) na página de [versões (Releases)](https://github.com/tolotratlt/rival-schools-nekketsu-seishun-nikki-2-recomp/releases) e extraia numa pasta onde dê para gravar.

2. Instale o Python 3, se ainda não tiver.

3. Abra o `Shiritsu_Justice_Gakuen__Nekketsu_Seishun_Nikki_2.exe`.

4. No assistente de configuração, escolha a imagem do seu disco (o `.cue`) e o arquivo da sua BIOS SCPH-1001.

5. Clique em **Generate & rebuild** e espere o jogo abrir. A primeira vez gera e compila o jogo e leva alguns minutos.

6. Opcional, tradução para inglês: feche o jogo e rode o `Install-English-Translation.cmd`. Ele monta um mod em inglês a partir da imagem do seu próprio disco (cerca de um minuto, com Python 3; o Pillow é instalado numa pasta própria `.localization-venv` se faltar). Abra o jogo de novo: **English Translation** aparece ligado na aba Mods do launcher. Desligue lá para jogar em japonês.

O assistente usa o CMake e o Ninja do `PATH` quando existem; senão, baixa as ferramentas de compilação portáteis (cmake-clang-v1). Mantenha o `.cue` e o `.bin` que ele cita juntos. O ZIP não traz dados do jogo: nem imagem de disco, nem executável do jogo, nem BIOS, nem código do jogo gerado.
