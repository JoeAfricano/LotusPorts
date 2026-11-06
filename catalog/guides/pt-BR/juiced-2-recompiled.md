<!-- source: https://github.com/anishchandra24/Juiced2-Recompiled/blob/HEAD/README.md | fingerprint: 193941e34f9c6066 -->

Esta é uma versão inicial, testada até agora num PC só. Relatos de outros PCs, bons ou ruins, são bem-vindos: [abra uma issue](https://github.com/anishchandra24/Juiced2-Recompiled/issues/new/choose).

### Instalação fácil (recomendada)

1. Baixe o `Juiced2-Recompiled-Setup.exe` na [página de versões (Releases)](https://github.com/anishchandra24/Juiced2-Recompiled/releases).

2. Abra. O Windows pode mostrar "O Windows protegeu o computador" porque o programa não tem assinatura digital. Clique em "Mais informações" e depois em "Executar assim mesmo". Dá para conferir o download com o SHA-256 das notas da versão.

3. Clique em "Browse..." ao lado de "Your game folder" e escolha a pasta que tem o `default.xex`. Mantenha a pasta de instalação sugerida ou escolha outro caminho curto e clique em "Install".

4. Espere. Se faltar o Git ou o Visual Studio 2022 Community, o programa os instala pelo instalador de pacotes do Windows (o Windows pede permissão), depois baixa o projeto e monta o jogo. Conte de 15 a 30 minutos, mais os downloads e uma eventual instalação do Visual Studio.

5. Quando disser que terminou, clique em "Play", ou use o atalho "Juiced 2 Recompiled" na Área de Trabalho.

Para remover, use Configurações, Aplicativos, "Juiced 2 Recompiled" (ou o botão "Uninstall" do instalador). Desinstalar nunca mexe nos arquivos do seu jogo e pergunta antes de apagar os saves.

A instalação automática do Visual Studio ainda não foi testada num PC sem ele. Se tentar, abra uma issue contando como foi, dando certo ou não. Se falhar, instale as ferramentas à mão e rode o instalador de novo.

### Instalação manual

Se preferir fazer à mão, ou se o instalador não funcionar:

1. Instale as ferramentas listadas no repositório.

2. Baixe o projeto numa pasta de caminho curto (caminhos longos podem quebrar a compilação). Abra um prompt de comando e rode:

```
git clone https://github.com/anishchandra24/Juiced2-Recompiled.git C:\Juiced2
```

3. Abra a pasta `C:\Juiced2` e clique duas vezes no `setup.bat`. Quando pedir, digite o caminho da pasta do seu jogo extraído (a que tem o `default.xex`) e aperte Enter.

4. Espere. O setup confere as ferramentas e os arquivos do jogo, baixa o ReXGlue SDK na versão exata que o projeto usa, aplica os patches, compila o SDK, gera o código do jogo a partir do seu `default.xex`, compila o jogo e cria um atalho na Área de Trabalho. Conte de 15 a 30 minutos, mais os downloads.

5. Abra o jogo pelo atalho "Juiced 2 Recompiled" na Área de Trabalho, ou rode `dist\Launch Juiced 2.bat`.

Observações:

- Se o Firewall do Windows perguntar sobre o `juiced2.exe`, pode clicar em Cancelar. O jogo não precisa de rede (recursos online não são suportados).

- Se o setup parar, ele mostra o que deu errado. Corrija e rode o `setup.bat` de novo; as etapas terminadas são puladas. Registros detalhados ficam em `.local\logs`.

- Opções: `setup.bat -NoShortcut` sem atalho na Área de Trabalho, `setup.bat -SkipIcon` pula o ícone do jogo, `setup.bat -Rebuild` recompila o SDK do zero.

- O ícone do jogo é feito a partir da sua própria cópia durante o setup. Para isso, a janela do jogo abre por alguns segundos e fecha sozinha. É normal. O ícone não faz parte do repositório.

- O setup cria uma pasta `assets` dentro do projeto. É um link para a pasta do seu jogo, não uma cópia. Antes de apagar a pasta do projeto à mão, abra um prompt de comando nela e rode `rmdir assets`, para os arquivos do seu jogo não serem tocados. O desinstalador faz isso por você.

- Saves, configurações e conquistas ficam em `Documentos\juiced2`, não na pasta do projeto. Faça uma cópia dessa pasta antes de testar versões novas.
