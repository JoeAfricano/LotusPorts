<!-- source: https://github.com/hk21x/nightfire-recomp/blob/HEAD/README.md | fingerprint: aaac9c075ae54dd9 -->

O launcher prepara o jogo no seu PC a partir do seu próprio disco: sem Visual Studio, sem Git e sem linha de comando, nada instalado no Windows e sem precisar de administrador. A versão traz o runtime e um compilador, e nenhum código do jogo: ele é compilado no seu PC a partir do seu disco.

### O launcher (recomendado)

**Você precisa de:**

- Windows 10 ou 11, 64 bits, e uma placa de vídeo com Direct3D 12 ou Direct3D 11.

- A sua própria cópia do jogo (PAL ou USA): uma imagem `.iso` do seu disco, ou uma pasta com os arquivos do disco.

- Espaço para a sua cópia do jogo e internet para o pacote. A sua cópia ocupa 7,3 GB quando o launcher precisa copiar a imagem do disco, o que ele faz a partir de outro disco ou num disco como exFAT; nada a mais quando ele consegue ligar à imagem no mesmo disco NTFS, ou a uma pasta com os arquivos do disco.

**Passos:**

1. Baixe o `NightFireLauncher.exe` na [versão mais recente](https://github.com/hk21x/nightfire-recomp/releases/latest).

2. Coloque numa pasta só dele (por exemplo `C:\Games\NightFire`) e abra. Tudo o que ele cria vai para uma pasta `NightFire` ao lado. O Windows pode avisar sobre um app não reconhecido: escolha **Mais informações** e depois **Executar assim mesmo**.

3. **Seu jogo:** escolha a imagem do seu disco ou a pasta com os arquivos do disco. O launcher confere se é a versão aceita.

4. Clique em **Get the game ready**. O launcher baixa o pacote e compila o código do jogo a partir do seu disco; o tempo depende do seu PC e da conexão.

5. **Jogar:** clique em **Play**. Quando sai uma versão nova, o launcher oferece **Update**.

Instalou com um launcher anterior, que montava o jogo a partir do código? O launcher oferece **A faster way to play**: clique em **Update**, e os seus saves e configurações continuam como estão.

### O que vem numa versão

- Arquivo · O que é

- `NightFireLauncher.exe` · **O único arquivo para baixar à mão.** Ele busca o resto sozinho.

- `NightFire-package.zip` · O runtime e o compilador, para os discos PAL e USA, sem código do jogo. O launcher busca; não extraia à mão.

- `NightFire-package.zip.json` · O tamanho e o SHA-256 que o launcher confere no download. Pode ignorar.

Arquivos, todos na pasta `NightFire` ao lado do launcher:

- `play\`: o jogo (`Nightfire.exe`, `NightfireDriving.exe`, o código compilado deles `Nightfire.dll` e `NightfireDriving.dll`, `settings.toml`, `hdd\` com os saves e `logs\` com os registros do jogo)

- `launcher.log`: o registro do launcher (envie junto com qualquer problema)

Para remover tudo, apague o launcher e a pasta `NightFire` dele (guarde a `play\hdd` se quiser os seus saves).
