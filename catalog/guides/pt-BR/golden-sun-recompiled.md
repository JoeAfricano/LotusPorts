<!-- source: https://github.com/Shmargus/GSRecomp/blob/HEAD/README.md | fingerprint: 65814f549f5ff9dd -->

1. Baixe o zip para Windows ou Linux na página de [versões (Releases)](https://github.com/Shmargus/GSRecomp/releases) e extraia.

2. Abra o `GoldenSunLauncher` (`GoldenSunLauncher.exe` no Windows), escolha The Broken Seal ou The Lost Age e indique o arquivo desse jogo. Ele lembra de cada um.

3. Na primeira abertura, e depois de uma atualização, o launcher monta o código do jogo a partir do seu arquivo, com uma barra de progresso. Um compilador vem junto; o Linux precisa do SDL2 e do OpenGL do sistema.

4. Escolha o cartucho para jogar. As configurações ficam na engrenagem dourada do launcher e no jogo, pelo F1.

Se algo der errado, aperte **F12** no jogo logo depois: o jogo guarda os últimos dois segundos, e o launcher monta um relatório de erro que você envia com **Send report**. Os arquivos dos jogos ficam em `ROMs/`, saves e estados salvos em `Saves/`. O mapeamento dos controles fica em `Controls/` e é compartilhado; as outras configurações do F1 são por jogo. Na primeira abertura, o mapeamento existente daquele jogo vira o compartilhado.

O README de cada jogo descreve os recursos e os passos de compilação específicos: [The Broken Seal](https://github.com/Shmargus/GSRecomp/blob/HEAD/The%20Broken%20Seal/README.md) e [The Lost Age](https://github.com/Shmargus/GSRecomp/blob/HEAD/The%20Lost%20Age/README.md). As [notas das versões](https://github.com/Shmargus/GSRecomp/releases) dizem o que cada versão mudou; atualizar pode fazer o launcher preparar o código do seu jogo de novo.
