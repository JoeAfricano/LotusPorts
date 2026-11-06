<!-- source: https://github.com/alexbeavs-ps1-ports/resident-evil-3-recomp/blob/HEAD/README.md | fingerprint: 3318aaa440eaa371 -->

1. Extraia o ZIP completo de instalação numa pasta onde dê para gravar.

2. Abra o `Resident_Evil_3__Nemesis_Recompiled` (`.exe` no Windows).

3. No assistente de configuração, escolha o seu arquivo CUE e a BIOS necessária.

4. Clique em Generate & rebuild e espere o jogo abrir.

No Windows, o assistente pode baixar ferramentas de compilação portáteis. No Linux e no macOS, instale antes CMake, Ninja, Python 3 e um compilador C/C++. Mantenha o CUE e todos os arquivos que ele cita juntos. Esta revisão exata do disco também precisa do arquivo SBI correspondente ao lado do CUE, com o mesmo nome. A configuração confere esse arquivo e o mantém. Veja as [instruções do arquivo complementar do disco](https://github.com/alexbeavs-ps1-ports/resident-evil-3-recomp/blob/HEAD/psxrecomp/docs/DISC_COMPANIONS.md). A BIOS precisa ter 524288 bytes e SHA-256 `1faaa18fa820a0225e488d9f086296b8e6c46df739666093987ff7d8fd352c09`.
