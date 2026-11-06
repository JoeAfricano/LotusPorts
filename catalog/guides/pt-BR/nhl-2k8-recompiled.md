<!-- source: https://github.com/anishchandra24/NHL2K8-Recompiled/blob/HEAD/README.md | fingerprint: 9c68aa083bd71f46 -->

Testado em vários PCs, inclusive por testadores de fora. Relatos de outros PCs, bons ou ruins, são bem-vindos: [abra uma issue](https://github.com/anishchandra24/NHL2K8-Recompiled/issues/new/choose).

### Instalação fácil (recomendada)

1. Baixe o `NHL2K8-Recompiled-Setup.exe` na [página de versões (Releases)](https://github.com/anishchandra24/NHL2K8-Recompiled/releases).

2. Abra. O Windows pode mostrar "O Windows protegeu o computador" porque o programa não tem assinatura digital. Clique em "Mais informações" e depois em "Executar assim mesmo". Dá para conferir o download com o SHA-256 das notas da versão. No Windows 11, o Controle Inteligente de Aplicativos pode bloquear de vez ("não está assinado"), sem opção de executar; ele só pode ser desligado em Segurança do Windows, e em muitos PCs desligá-lo não tem volta sem reinstalar o Windows. Se preferir mantê-lo ligado, use a instalação manual abaixo.

3. Clique em "Browse..." ao lado de "Your game folder" e escolha a pasta que tem o `default.xex`. Mantenha a pasta de instalação sugerida ou escolha outro caminho curto. Se quiser, digite uma gamertag para jogar online (senão você aparece como "User"). Depois, clique em "Install".

4. Espere. Se faltar o Git ou o Visual Studio 2022 Community, o programa os instala pelo instalador de pacotes do Windows (o Windows pede permissão), depois baixa o projeto e monta o jogo. Conte de 15 a 30 minutos, mais os downloads e uma eventual instalação do Visual Studio. No fim, uma janelinha explica que a instalação vai criar uma regra no Firewall do Windows para o jogo online; clique em OK e depois em Sim quando o Windows pedir permissão para o "Network Command Shell" (ou Cancelar se nunca for jogar online).

5. Quando disser que terminou, clique em "Play", ou use o atalho "NHL 2K8 Recompiled" na Área de Trabalho.

Se o antivírus marcar o instalador: alguns antivírus (por exemplo o Microsoft Defender, como "Trojan:Win32/Wacatac.C!ml") já marcaram o `NHL2K8-Recompiled-Setup.exe`. É um falso positivo. O "!ml" quer dizer um palpite de aprendizado de máquina, não um vírus conhecido: o programa é novo, não tem assinatura e faz o que um instalador faz, abrindo o PowerShell, o Git e o instalador de pacotes do Windows para baixar as ferramentas e montar o jogo. O código-fonte completo dele está na pasta `installer` do repositório, e as notas da versão trazem o SHA-256 para você conferir. Se preferir não rodar, use a instalação manual, que não usa o instalador.

Para remover, use Configurações, Aplicativos, "NHL 2K8 Recompiled" (ou o botão "Uninstall" do instalador). Desinstalar nunca mexe nos arquivos do seu jogo e pergunta antes de apagar os saves.

### Instalação manual

Se preferir fazer à mão, ou se o instalador não funcionar:

1. Instale as ferramentas listadas no repositório.

2. Baixe o projeto numa pasta de caminho curto (caminhos longos podem quebrar a compilação). Abra um prompt de comando e rode:

```
git clone https://github.com/anishchandra24/NHL2K8-Recompiled.git C:\NHL2K8
```

3. Abra a pasta `C:\NHL2K8` e clique duas vezes no `setup.bat`. Quando pedir, digite o caminho da pasta do seu jogo extraído (a que tem o `default.xex`) e aperte Enter.

4. Espere. O setup confere as ferramentas e os arquivos do jogo, baixa o ReXGlue SDK na versão exata que o projeto usa, aplica os cinco patches do SDK (música, correção do recompilador, correção de cores, trilhas personalizadas e jogo online), compila o SDK, gera o código do jogo a partir do seu `default.xex`, compila o jogo e cria um atalho na Área de Trabalho. Levou cerca de 12 minutos num notebook rápido de 8 núcleos e cerca de 21 minutos num desktop mais lento de 4 núcleos, com os downloads.

5. Abra o jogo pelo atalho "NHL 2K8 Recompiled" na Área de Trabalho, ou rode `dist\Launch NHL 2K8.bat`.

Observações:

- No fim, o setup oferece criar uma regra no Firewall do Windows chamada "NHL 2K8 Recompiled (online play)" para outros jogadores alcançarem o seu jogo: clique em OK e depois em Sim. Clique em Cancelar se nunca for jogar online; o jogo funciona sem ela. Rode `setup.bat -NoFirewall` para pular.

- Para definir uma gamertag para o jogo online, rode `setup.bat -Gamertag "SeuNome"` (letras, números e espaços, até 15). Ela fica guardada.

- Se o setup parar, ele mostra o que deu errado. Corrija e rode o `setup.bat` de novo; as etapas terminadas são puladas. Registros detalhados ficam em `.local\logs`.

- Opções: `setup.bat -NoShortcut` sem atalho na Área de Trabalho, `setup.bat -SkipIcon` pula o ícone do jogo, `setup.bat -Rebuild` recompila o SDK do zero.

- O ícone do jogo é lido da sua própria cópia (o seu `default.xex`) durante o setup. Ele não faz parte do repositório.

- O setup cria uma pasta `assets` dentro do projeto. É um link para a pasta do seu jogo, não uma cópia. Antes de apagar a pasta do projeto à mão, abra um prompt de comando nela e rode `rmdir assets`, para os arquivos do seu jogo não serem tocados. O desinstalador faz isso por você.

- Saves, configurações e conquistas ficam em `Documentos\nhl2k8`, não na pasta do projeto. Faça uma cópia dessa pasta antes de testar versões novas.

### Atualizar

Rode o `NHL2K8-Recompiled-Setup.exe` de novo e escolha a mesma pasta de instalação: ele baixa a versão mais recente do projeto e remonta o jogo. Na instalação manual, rode `git pull` na pasta do projeto e depois o `setup.bat`. Os seus saves não são tocados. A primeira atualização depois da v1.0 remonta mais que o normal, então conte com o mesmo tempo da primeira instalação.
