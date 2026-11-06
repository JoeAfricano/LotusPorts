<!-- source: https://github.com/z3xox/Tenkaichi3Decomp/blob/HEAD/README.md | fingerprint: 3964439eed32eac2 -->

Extraia a versão, abra o `Tenkaichi3Decomp-setup` (`Tenkaichi3Decomp-setup.exe` no Windows), escolha a imagem do seu disco e clique em Play. A configuração confere se a imagem é a versão americana sem modificações e desempacota os dados do jogo ao lado do programa.

Num PC sem Vulkan funcionando, a janela de configuração e o jogo usam OpenGL sozinhos. Para pedir OpenGL desde o começo (um driver Vulkan que abre mas é incompleto, como na Intel Ivy Bridge), defina `BT3_GPU_API=gl`: `BT3_GPU_API=gl ./Tenkaichi3Decomp-setup`, e o mesmo para `./Tenkaichi3Decomp`. A configuração também roda sem janela: `./Tenkaichi3Decomp-setup --install <disc image>`.

**Atualizações.** A cada abertura, o jogo pergunta ao GitHub pela versão mais nova (F1 > Misc > "Check for updates" desliga isso). Uma versão nova é anunciada sobre o menu principal; o F1 tem Update now, Later e What's new. O download acontece enquanto você joga, a assinatura é conferida e "Restart now" substitui os arquivos do programa (os antigos ficam como `.old` até a próxima abertura e voltam se algo falhar) e abre a versão nova. Atualizar à mão, copiando os arquivos novos por cima, continua funcionando. O atualizador do jogo é baseado no PR #107 de JosephLlacchua.
