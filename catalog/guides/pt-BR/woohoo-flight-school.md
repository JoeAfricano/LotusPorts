<!-- source: https://github.com/thefixinhixon/woohoo-flight-school/blob/HEAD/README.md | fingerprint: 3555e06048f33bc6 -->

**O mais fácil no Linux: o AppImage com o kit de compilação.** Baixe o `WoohooFlightSchool-Linux-kit-x86_64.AppImage` na [versão mais recente](https://github.com/thefixinhixon/woohoo-flight-school/releases), torne executável e abra (`chmod +x WoohooFlightSchool-Linux-kit-x86_64.AppImage`, depois clique duas vezes ou rode). Ele traz o launcher, o Python, um compilador C e as ferramentas de compilação (cerca de 140 MB), então você só precisa de um driver Vulkan e do SDL2, que a maioria dos sistemas já tem (glibc 2.29 ou mais novo: Ubuntu 20.04+, Debian 11+, Fedora, Arch, SteamOS). O jogo montado vai para a mesma pasta de instalação descrita abaixo. Jogadores de Windows: veja a seção Windows do repositório.

**A partir do código** (com as ferramentas de compilação listadas no repositório):

```
python3 launcher/pwr_launcher.py
```

1. **Arquivo do jogo:** indique a sua cópia. Ela é conferida na hora (título, criptografia, hash do código do jogo).

2. **Build:** recompila e compila na pasta de instalação (padrão `~/.local/share/pwr-port`). De um a dois minutos num PC com vários núcleos.

3. **Play:** escolha ou crie um perfil (cada um tem os próprios saves e configurações), ajuste a imagem e clique em Launch.

Sem a interface gráfica:

```
python3 tools/pipeline.py check                      # build tools present?
python3 tools/pipeline.py verify "My Dump.cia"
python3 tools/pipeline.py build "My Dump.cia"
python3 tools/pipeline.py run --profile default
python3 tools/pipeline.py profiles create alice
```
