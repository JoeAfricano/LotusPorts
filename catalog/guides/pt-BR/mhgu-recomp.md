<!-- source: https://github.com/morerokk/MHGURecomp/blob/HEAD/README.md | fingerprint: c524bea4663064e4 -->

[Baixe a versão pronta mais recente na página de versões (Releases). Confira se baixou o ZIP certo para o seu sistema.](https://github.com/morerokk/MHGURecomp/releases)

Você precisa das pastas `exefs` e `romfs` do Monster Hunter Generations Ultimate 1.4.0, tiradas da sua própria cópia. Se você vem do Ryujinx, clique com o botão direito no jogo (com a atualização 1.4.0 aplicada) e extraia os arquivos do jogo com os patches já aplicados. O Yuzu tem uma função parecida, ou você pode usar outras ferramentas para extrair os arquivos.

Com outra ferramenta, extraia tanto os arquivos do jogo base quanto a atualização 1.4.0. Os arquivos `exefs` e `romfs` da atualização devem ser copiados por cima dos do jogo base.

1. Extraia o zip da versão onde quiser, por exemplo `C:\Games\MHGURecomp\`.

2. Coloque as pastas `exefs` e `romfs` do jogo na subpasta `data`. Deve ficar assim:

```
   MHGURecomp\
     mhgurecomp.exe
     mhgugame.dll
     (other .dll files)
     data\
       exefs\
         main
         main.npdm
         rtld
         sdk
         subsdk0
       romfs\
         nativeNX\
         (the rest of the game's romfs)
```

O `exefs` precisa vir da atualização 1.4.0, e o `romfs` precisa ser o romfs do jogo base com os arquivos da atualização juntados por cima. São cerca de 35.500 arquivos, 12,3 GB.

3. Abra o `mhgurecomp.exe`.

### Mantendo atualizado

Versões novas podem sair com frequência. Para atualizar sozinho, abra o `mhgurecomp-updater.exe` (`mhgurecomp-updater` no Linux). O atualizador funciona com controle, então você pode usá-lo como atalho principal ou adicioná-lo como jogo de fora da Steam.

- O atualizador sempre confere a versão mais recente no GitHub. Versões de teste podem existir, mas não são oferecidas pelo atualizador porque podem ter problemas.

- Só os arquivos do recomp são substituídos; as outras pastas (inclusive `data` e `mods`) são mantidas.

- Dá para mudar o comportamento do atualizador nas configurações.

- Se tiver problemas, o `updater.log` traz mais informações.

### Onde as coisas ficam

- O quê · Onde

- Configurações · `mhgurecomp.ini` (pode apagar se precisar zerar as configurações)

- Saves · `%APPDATA%\MHGURecomp\save\` (Linux: `~/.local/share/MHGURecomp/save/`), ou a pasta escolhida como pasta de saves no menu de configurações (aba Menu Options, `SaveLocation` no `mhgurecomp.ini`)

- Registro · `log.txt`; anexe esse arquivo ao abrir uma issue

- Capturas de tela · `shots\` (aperte F12)

- Mods · `mods\`, uma pasta por mod

- Módulos do motor · `engine_modules\`

- Cheats · `cheats\`, arquivos `.txt` com códigos

- Cache de shaders · `cache\`

Para importar os seus saves do Switch, coloque os arquivos `system` e `system_backup` em `%APPDATA%\MHGURecomp\save\save_0\` (Linux: `~/.local/share/MHGURecomp/save/save_0/`).
