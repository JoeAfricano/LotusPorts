<!-- source: https://github.com/tools81/hotel-mario-static-recomp/blob/HEAD/README.md | fingerprint: de16f16e0055f7db -->

Cada [versão](https://github.com/tools81/hotel-mario-static-recomp/releases) tem um instalador pronto (`HotelMarioRecomp-<version>-setup.exe`) e um zip portátil (`HotelMarioRecomp-<version>-win64.zip`). Os jogadores só precisam disso e da imagem do próprio disco; não precisa de ferramentas de compilação. A imagem do disco nunca vem junto.

Para montar os pacotes você mesmo a partir de uma compilação, rode:

```
package.bat
```

Ele roda o `build.bat` e grava em `dist\`:

- Arquivo · O que é

- `HotelMarioRecomp-<version>-win64.zip` · Portátil: extraia onde quiser e abra. A primeira abertura pede a imagem do disco.

- `HotelMarioRecomp-<version>-setup.exe` · Um instalador de arquivo único, feito só se o [Inno Setup 6](https://jrsoftware.org/isinfo.php) estiver instalado (`winget install JRSoftware.InnoSetup`).

`-SkipBuild` empacota os executáveis existentes, e `-NoInstaller` faz só o zip.

**O instalador** (`installer\hotelmario.iss`):

- Instala sem administrador, por padrão em `C:\Games\Hotel Mario Recomp`.

- Pede a imagem do disco do Hotel Mario (.cue ou .bin) e confere se é uma imagem de CD-i. A imagem não é copiada: o caminho vai para o `config.ini` na pasta do programa.

- Cria atalhos no menu Iniciar (e na Área de Trabalho, se você quiser).

- Desinstalar remove o programa, mas mantém `saves\` e `config.ini`.

- Instalação sem perguntas: `setup.exe /VERYSILENT /DIR="D:\Games\HM" /ROM="D:\images\Hotel Mario (USA).cue"`.

**Portabilidade.** O programa grava só na própria pasta: `config.ini` ao lado dos executáveis, e saves, configurações, capturas e registros em `saves\`. Nada vai para o AppData nem para o registro, então uma instalação (ou a pasta extraída) pode ser movida ou copiada, por exemplo para um pen drive. Os shaders ficam dentro do executável. `--portable` é aceito (os atalhos passam), mas não muda nada, porque é sempre assim.

**Dependências.** Os executáveis trazem o runtime de C embutido e usam só DLLs que fazem parte do Windows 10 e 11 (Direct3D 11 e o compilador de shaders dele, WinHTTP, XInput). Nada mais precisa ser instalado.

**config.ini**

```
[paths]
rom=D:\Games\Hotel Mario (USA).cue   ; the disc image
[update]
check=1                              ; 0: never look for new versions
skip=v1.0.1                          ; a version you chose not to install
```

`rom=` é a imagem do disco; `check=0` desliga a procura por versões novas; `skip=` guarda uma versão que você escolheu não instalar.

**Atualizações.** Ao abrir (não em execuções `--headless`, nem com `--no-update-check` ou `check=0`), o programa pergunta ao GitHub pela versão mais recente deste repositório (no máximo alguns segundos; sem conexão, ele só abre). Se a versão for mais nova, ele oferece instalar:

- **Sim:** o `update.ps1` espera o jogo fechar, baixa o zip portátil da versão, substitui os arquivos do programa (os executáveis antigos ficam como `*.old`; `config.ini` e `saves\` não são tocados) e reabre o jogo. Se uma versão não tiver zip, ele compila o código dessa versão com a imagem do seu disco. Isso precisa do Visual Studio 2022 (C++), CMake e Python; sem eles, ele abre a página da versão e reabre a versão atual. O registro fica em `saves\update.log`.

- **Não:** pula essa versão.

- **Cancelar:** pergunta de novo na próxima vez.

Para publicar uma versão nova: aumente o `VERSION`, faça commit e push, rode o `package.bat` e crie uma versão no GitHub com a tag `v<version>` e os dois arquivos de `dist\` anexados:

```
gh release create v1.0.1 dist\HotelMarioRecomp-1.0.1-setup.exe dist\HotelMarioRecomp-1.0.1-win64.zip
```
