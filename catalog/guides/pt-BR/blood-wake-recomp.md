<!-- source: https://github.com/minimalice92/bloodwake-recomp/blob/HEAD/README.md | fingerprint: f07a07092c952c95 -->

### Do que você precisa

- **Windows 10 ou 11, 64 bits**, e uma placa de vídeo com DirectX 11.

- **O seu próprio disco do Blood Wake**, copiado para o PC como pasta com os arquivos, ISO de Xbox (XISO) ou imagem de disco completa. A instalação reconhece a versão comercial norte-americana listada em [fingerprints/retail.json](https://github.com/minimalice92/bloodwake-recomp/blob/HEAD/fingerprints/retail.json). Outra versão, ou uma cópia modificada, também pode ser montada, depois de um aviso claro que você precisa aceitar.

- **Alguns GB livres** para a montagem (só os arquivos do jogo têm cerca de 2 GB).

- Um controle estilo Xbox (XInput), ou teclado e mouse: o teclado funciona como controle 1 direto, e no Remastered o mouse funciona em todos os menus.

A montagem também usa ferramentas gratuitas, que o instalador busca se faltarem, só dos sites oficiais: Python 3.13 com os pacotes fixados em [tools/requirements.txt](https://github.com/minimalice92/bloodwake-recomp/blob/HEAD/tools/requirements.txt), Visual Studio 2022 Build Tools (versão 17, carga C++; o 2026 ainda não é suportado), CMake 3.20 ou mais novo e o kit xboxrecomp num commit fixo, conferido por um hash fixo.

### Instalar

> **O Windows pode avisar:** o instalador não tem assinatura digital, então o SmartScreen pode dizer "O Windows protegeu o computador". Escolha **Mais informações** e depois **Executar assim mesmo**. Cada versão lista o SHA-256 do instalador, para você conferir que tem o arquivo certo.

1. Baixe o `BloodWakeRecomp-Setup-<version>.exe` na página de [versões (Releases)](https://github.com/minimalice92/bloodwake-recomp/releases) e abra. Ele instala só para o seu usuário do Windows; só a etapa que instala as ferramentas de compilação da Microsoft, se precisar, pede administrador.

2. Aceite o aviso, confirmando que o jogo é seu e que você o copiou do seu próprio disco.

3. Escolha a sua cópia do disco. O instalador diz em palavras simples se a reconhece. Se não (outra região, uma cópia modificada ou danificada), ele explica o que isso significa antes de você decidir continuar.

4. Escolha a pasta de instalação (por padrão `%LOCALAPPDATA%\Programs\Blood Wake Recomp`) e os atalhos.

5. Deixe montar: cerca de 15 minutos, com barra de progresso, e dá para cancelar a qualquer momento. A última etapa, "Preparing graphics", toca algumas cenas curtas escondidas e sem som (cerca de 2 minutos) para o primeiro jogo não travar enquanto os shaders compilam.

6. Jogue pelo menu Iniciar ou pelo atalho da Área de Trabalho.

**Atualizar:** rode o instalador novo por cima da versão atual, na mesma pasta. Saves, configurações e gráficos preparados ficam.

Para desinstalar, use **Aplicativos** nas configurações do Windows: ele remove a montagem e os atalhos e mantém saves e configurações, a não ser que você escolha o contrário.

**Ou monte à mão:**

```
git clone https://github.com/minimalice92/bloodwake-recomp.git
cd bloodwake-recomp
python -m pip install -r tools/requirements.txt
python tools/build_from_disc.py --disc "<your disc folder or image>" --install-dir "<where to install>"
```

Depois, abra o launcher na pasta de instalação. Cada opção, etapa e código de saída: [docs/BUILDING.md](https://github.com/minimalice92/bloodwake-recomp/blob/HEAD/docs/BUILDING.md).

### Jogar

O launcher: escolha uma edição e as configurações e clique em Play.

- O launcher tem uma aba para Edição, Imagem, Gráficos, Áudio, Correções, Controles, Cheats e Avançado. Passe o mouse sobre qualquer opção para uma explicação curta.

- **Controles:** remapeie qualquer tecla ou botão do mouse com um clique e um aperto; ajuste a direção no teclado e a zona morta dos analógicos.

- **Cheats:** marque os que quiser, com uma chave geral.

- **Se algo der errado:** depois de duas aberturas que falharam, o launcher oferece o modo seguro, e **Make a bug report** (aba Avançado) coloca um zip na Área de Trabalho com o registro e as configurações, nada do jogo.

**Predefinições (Remastered).** A aba Edição tem três predefinições que ajustam as abas Imagem e Gráficos de uma vez. Best Quality é o padrão. Você ainda pode mudar qualquer opção depois; aí a predefinição mostra Custom.

- **Best Quality:** um jogo moderno no máximo: todas as melhorias e extras ligados (oclusão de ambiente, bloom e nitidez suave incluídos).

- **Balanced and Original:** o jogo original fiel, com o melhor visual numa tela moderna, sem os extras que mudam a aparência.

- **Best Performance:** para um PC mais lento: as opções mais leves que mantêm o essencial (tela larga, taxa de quadros alta, anti-aliasing leve).

- O jogo abre em tela cheia sem bordas; Alt+Enter ou F11 alterna para janela e de volta durante o jogo. No Remastered, o último item do menu principal é Quit Game.

- Cada edição tem os próprios saves, em `%LOCALAPPDATA%\xboxrecomp\` (os do Remastered em `editions\remastered`). Na primeira vez no Remastered, ele começa com uma cópia dos saves do Original. As configurações ficam em `%LOCALAPPDATA%\BloodWake\settings.ini`.

### Solução de problemas

- **"not a supported unmodified retail copy" (código 2):** a cópia não é a versão do `fingerprints/retail.json`. Uma cópia com patch, reduzida ou reempacotada não bate: copie o disco de novo. Para montar mesmo assim, rode o instalador e aceite o aviso (ou acrescente `--allow-unverified` ao `build_from_disc.py`).

- **Falta uma dependência (código 3):** a mensagem diz qual. Instale, ou rode o instalador, que a busca.

- **A tradução falhou (código 4):** confira se os pacotes do Python estão exatamente nas versões fixadas (`python -m pip install -r tools/requirements.txt`).

- **"generated code doesn't match" (código 5):** geralmente outra versão de um pacote do Python ou do kit. Reinstale as versões fixadas e tente de novo.

- **A compilação falhou (código 6):** confira se o Visual Studio 2022 (17.x) Build Tools com a carga C++ está instalado.

- **Espaço em disco ou erro de arquivo (código 7):** libere espaço, ou escolha uma pasta de instalação onde dê para gravar.

- **O jogo não abre depois de instalar (o Windows diz que bloqueou um app):** o Controle Inteligente de Aplicativos está ligado. Ele bloqueia programas sem assinatura, e o jogo é montado no seu PC, então não pode ter. Desligue em Segurança do Windows > Controle de aplicativos e do navegador > Configurações do Controle Inteligente de Aplicativos. É uma configuração do PC inteiro, então a decisão é sua; o instalador avisa antes de montar.

- **O jogo está lento:** no launcher, escolha a predefinição Best Performance, ou reduza a resolução ou o limite de quadros.

- **Som chiando num PC lento:** aba Áudio, Sound processor: Simple (pula o eco e os efeitos 3D do chip de som).

Ainda travado? Abra uma issue: diga o que fez, o que viu e a sua versão do Windows e placa de vídeo. Não anexe arquivos do jogo, capturas dos textos do jogo nem a imagem do seu disco.
