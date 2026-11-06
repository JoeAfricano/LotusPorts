<!-- source: https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/README.md | fingerprint: 28e53ff63fb9af39 -->

Os serviços online oficiais do Forza Horizon fecharam em 2023. O Forza Live, o servidor do próprio projeto, faz o papel deles: um público, que você usa sem configurar nada, e um que você mesmo pode rodar. O que funciona online:

- **Horizon World.** Mundo aberto online compartilhado, até 8 jogadores por mundo.

- **Co-op Festival.** Amigos correm juntos os eventos do festival; o launcher aplica os resultados de carreira válidos depois que o jogo fecha. [docs/coop_campaign.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/docs/coop_campaign.md)

- **Placares e Rivais.** Radares de velocidade, zonas de velocidade, eventos e Rivais, públicos no [site](https://forza.dyotson.dev/leaderboards.html).

- **Amigos, chat e clubes.** A lista de amigos do servidor, chat, clubes de carros e a Central de Mensagens.

- **Fotos e pinturas da comunidade.** Uma foto que você envia do modo foto aparece na [galeria](https://forza.dyotson.dev/gallery.html) com a sua gamertag. Pinturas e grupos de adesivos compartilhados na loja podem ser encontrados, baixados e avaliados no jogo, e os mais novos aparecem na [página de pinturas](https://forza.dyotson.dev/designs.html) com os códigos. O que é público e por quanto tempo: [PRIVACY.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/PRIVACY.md).

- **O seu próprio servidor.** O mesmo programa de servidor, com um [guia de hospedagem](https://github.com/FH1-ReImagined/forza-live/blob/main/docs/hosting.md).

Em **Settings > Online**, escolha um:

- **Public server.** Horizon World, placares e rivais. O endereço preenchido é `forza-reimagined.playit.plus:56677`. Não precisa abrir portas.

- **This PC.** Um servidor particular para esta sessão.

- **My own server.** [Guia de hospedagem](https://github.com/FH1-ReImagined/forza-live/blob/main/docs/hosting.md) no [forza-live](https://github.com/FH1-ReImagined/forza-live), o repositório do servidor. Python 3 só com a biblioteca padrão, ou Docker.

- **Offline.** Sem Xbox LIVE.

Servidores da comunidade são mantidos por voluntários em outras regiões, para um ping menor. Escolha um na página **Servers** do launcher, ou digite o endereço em **My own server**. Cada um tem a própria lista de amigos e placares: só os do servidor público são os oficiais do site.

- Região · Endereço

- Leste dos EUA · `reimagined-us-east.playit.plus:1819`

O Horizon World é a primeira lista em Multiplayer, Find Game. O servidor coloca você no mundo aberto mais cheio, até 8 jogadores (um mundo com alguém do seu clube primeiro, quando há vaga), ou você abre um e dirige. Quem entra depois recebe JUMP IN. Quando o anfitrião sai, os outros continuam dirigindo.

Amigos (F8, ou segurar o analógico esquerdo no controle) e chat (F9) usam a lista do servidor. Os clubes de carros e a Central de Mensagens funcionam. A garagem do clube pode compartilhar um carro, e o chat tem um canal Club para os membros online (`/club` lista quem está). [docs/playing.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/docs/playing.md)

Jogadores com a Title Update 4 e com o jogo base não se encontram. A Casa de Leilões não tem telas neste jogo.

### Instalar com o launcher

Este é o jeito de instalar. O launcher monta o jogo no seu PC a partir dos seus próprios arquivos. Nada do jogo é baixado.

1. Pegue o launcher na [versão mais recente](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/releases/latest). No Windows, o instalador. No Linux, o Flatpak (recomendado no SteamOS) ou o `.tar.gz`. Da v0.1.9 em diante, o launcher procura atualizações sozinho (**About > Launcher updates**). O Flatpak se instala com `flatpak install --user ForzaHorizon1-ReImagined-x86_64.flatpak`. Instale a extensão do compilador uma vez, senão o launcher baixa um compilador de 2 GB e avisa:

```
   flatpak install flathub org.freedesktop.Sdk.Extension.llvm22//26.08
```

**No SteamOS (Steam Deck):** no Modo Desktop, baixe o `.flatpak` e abra no Dolphin (o Discover instala) ou rode o comando acima, instale a extensão do compilador e, na Steam, escolha **Jogos > Adicionar jogo de fora da Steam** e escolha **Forza Horizon 1: ReImagined**. Aí ele abre pelo Modo Jogo. [docs/steam_deck.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/docs/steam_deck.md)

2. Em Setup, **Game files**: a imagem do seu disco, o pacote Games on Demand ou uma pasta extraída. **Updates & DLC**: a Title Update 4 e a pasta das suas DLCs, se tiver.

3. **Build.** O launcher gera o jogo a partir da sua cópia e compila. Uma montagem medida com 4 tarefas num PC de 8 núcleos levou cerca de 4,6 minutos. Dá para cancelar e continuar, e ela só roda de novo quando algo mudou.

4. **Prebuild shaders** (opcional, Vulkan). Monta os pipelines a partir dos seus arquivos antes de jogar, para a primeira visita a um lugar travar menos. O launcher oferece de novo depois de mudar driver, launcher ou configuração gráfica. [docs/shader_prebuild.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/docs/shader_prebuild.md)

5. **Play.** As configurações cobrem imagem, controles, áudio, online, perfil e opções avançadas. **Saves** é o editor de cópias e perfis.

Detalhes: [docs/launcher.md](https://github.com/FH1-ReImagined/ForzaHorizon1-ReImagined/blob/HEAD/docs/launcher.md).

Para conferir um download quando a versão traz `SHA256SUMS`, baixe esse arquivo da mesma versão. No Windows, rode `certutil -hashfile .\YourDownloadedSetup.exe SHA256` ou, no PowerShell, `Get-FileHash .\YourDownloadedSetup.exe -Algorithm SHA256`, e compare com a linha do arquivo correspondente em `SHA256SUMS`. No Linux, coloque os dois na mesma pasta e rode `sha256sum --ignore-missing -c SHA256SUMS`. Bater confirma que o download é o publicado; não garante que o programa é seguro nem assinado.

As versões para Windows também trazem `ForzaHorizon1-ReImagined-windows-files-<version>.json`, com os hashes dos arquivos instalados e um link para a compilação que os gerou. Se o antivírus colocar um arquivo em quarentena, informe o caminho exato, o nome da detecção e a versão do launcher, que aparecem no Histórico de proteção.
