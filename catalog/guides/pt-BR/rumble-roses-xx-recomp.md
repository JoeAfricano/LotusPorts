<!-- source: https://github.com/JesusTaylo/rumble-roses-xx-recomp/blob/HEAD/README.md | fingerprint: c544f006759e1a3d -->

O `rrxx-launcher.exe` é uma pequena interface gráfica para a instalação (um programa .NET Framework de ~2,7 MB, quase tudo imagem de fundo e fontes; o framework já faz parte do Windows 10/11, nada para instalar). Ele tem visual de menu de jogo e funciona com mouse, teclado e controle (XInput). Ele **não traz arquivos do jogo nem código recompilado**: só roda o `setup.ps1` e depois o `jugar.bat` da pasta onde está, e o código dele está em [launcher/](https://github.com/JesusTaylo/rumble-roses-xx-recomp/blob/HEAD/launcher/).

1. Pegue a pasta do projeto num **caminho curto** sem espaços, fora de pastas protegidas:

```
   git clone https://github.com/JesusTaylo/rumble-roses-xx-recomp.git C:\rrxx\rrxx-recomp
```

(não precisa de `--recursive`: a instalação busca o SDK sozinha). Se o "Acesso controlado a pastas" do Windows Defender estiver ligado, evite `Documentos`/`Área de Trabalho`: ele bloqueia a saída do compilador ali.

2. Baixe o `rrxx-launcher.exe` na página de [versões (Releases)](https://github.com/JesusTaylo/rumble-roses-xx-recomp/releases) e coloque **dentro dessa pasta**, ao lado do `setup.ps1`. (O SmartScreen pode avisar sobre um app sem assinatura: Mais informações → Executar assim mesmo. Você também pode montá-lo com o `launcher\build.bat`.)

3. Abra e escolha **Start Game**. Se o jogo ainda não estiver instalado, ele pede o `.iso` da **sua própria** cópia (e lembra o caminho) e roda a instalação. A barra mostra a etapa atual (1 a 6 do `setup.ps1`) com a saída ao vivo. O Windows pode pedir permissão de administrador quando ele instala ferramentas (com o `winget`).

4. Quando terminar, **Start Game** abre o jogo. Se algo quebrar, escolher **Start Game** de novo roda a instalação outra vez (as etapas terminadas são puladas).

Outros itens do menu: **Controls** (mostra o teclado como controle e deixa trocar as teclas; o controle usa o layout nativo de Xbox, que o SDK não deixa remapear) e **Settings** (edita o `rrxx.toml`: caminho de renderização RTV/ROV, escala de resolução, tela cheia/janela, VSync, margem do pool de envio, registro de FPS; cada opção tem uma descrição curta e um botão **Save**). Se você colocar o seu próprio `splash.png` ao lado do exe, ele substitui o fundo.

Se algo falhar, o launcher diz qual etapa e onde estão os registros (`logs\setup.log`, `build_sdk.log`, `build.log`, `codegen.log`). Rodar de novo é seguro: as etapas terminadas são puladas.

**Tempo da primeira instalação:** cerca de **20 minutos** no PC de teste com as ferramentas já instaladas (extração da ISO ≈1 min, SDK ≈13 min, recompilação + montagem ≈8 min). Num Windows novo, some o download e a instalação do Visual Studio Build Tools, do LLVM etc. (≈10–30 min, conforme a conexão), então **conte com 30 a 90 minutos**. É uma vez só; as próximas aberturas são imediatas.

### Alternativa: setup.bat

A mesma instalação sem a janela gráfica.

1. Clone como acima (`git clone ... C:\rrxx\rrxx-recomp`; não precisa de `--recursive`).

2. Clique duas vezes no **`setup.bat`** (ou rode `setup.bat "D:\path\to\your-game.iso"`; também dá para arrastar a ISO para ele).

3. Siga as perguntas. Ele, em ordem:

1. pede a sua ISO (ou aceita como argumento);

2. confere Git, Visual Studio Build Tools (C++), LLVM/clang, CMake, Ninja e Python, e instala o que faltar com o `winget`, **perguntando antes** (o Windows pode pedir administrador);

3. extrai a ISO para `..\rrxx_data\extraido\`;

4. busca o ReXGlue SDK, aplica os patches do projeto (`third_party/patches`) e o compila;

5. deixa o SDK recompilar o `default.xex` para C++ (`generated\`, nunca compartilhado) e monta o `rrxx.exe`;

6. cria o **`jugar.bat`** e um atalho **`Rumble Roses XX.lnk`** com o ícone do jogo.

4. Jogue pelo `jugar.bat` (ou pelo atalho).

Rodar de novo é seguro: as etapas terminadas são puladas. Os registros vão para `logs\`. Nada é enviado e nenhum dado do jogo fica no repositório.

Se uma etapa falhar, o script diz qual, onde está o registro e como continuar. Correções comuns: falta o winget → atualize o "Instalador de Aplicativo" pela Microsoft Store; "ferramenta instalada mas não encontrada" → feche a janela e rode o `setup.bat` de novo; erros de compilação em `Documentos` → mova o repositório para, por exemplo, `C:\rrxx\`.
