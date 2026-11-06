<!-- source: https://github.com/gymzatan/smt-nine-recomp/blob/HEAD/README.md | fingerprint: 383cf2918aeebf91 -->

Precisa do Windows x64, do Python 3.14, do Visual Studio 2022 C++ Build Tools com Windows SDK/CMake/Ninja e do Rustup. Só a versão opcional em chinês precisa também do FFmpeg/FFprobe no PATH e das fontes suplementares de chinês simplificado do Windows (SimSun e SimHei). Veja o guia de compilação para a identificação do disco aceito e a configuração das ferramentas.

```
git clone https://github.com/gymzatan/smt-nine-recomp.git
cd smt-nine-recomp
setup.cmd
build.cmd --iso "D:\Games\Shin Megami Tensei - Nine (Japan).iso"
Config.exe
smt9_recomp.exe
```

O `--iso` aceita sozinho XISO puro (`.xiso.iso` ou `.iso`), cópias de disco completas em ISO ou uma pasta extraída do jogo original com `default.xbe` e `Media`. Não precisa converter; os arquivos originais do jogo são conferidos um a um, sem exigir uma soma do disco inteiro. O conteúdo da pasta é conferido antes de ser copiado para a pasta de compilação; a pasta de origem não muda.

Para incluir o chinês, acrescente a opção no primeiro comando de compilação:

```
build.cmd --iso "D:\Games\Shin Megami Tensei - Nine (Japan).iso" --with-chinese
```

A compilação extrai e confere a ISO, prepara o código nativo e os recursos do título, traduz as funções do jogo com o conversor de versão fixa, monta o DSP/runtime e compila o jogo e o Config. O `--with-chinese` também monta scripts, fontes e vídeos legendados em chinês. Sem essa opção, essas etapas são puladas. O áudio original dos vídeos é mantido. Nenhum serviço de geração de imagens ou tradução é chamado durante a compilação.

Os dois executáveis ficam na raiz do projeto. Clique duas vezes no `smt9_recomp.exe` para jogar e no `Config.exe` para as configurações; não precisa de script. Os caminhos seguem o executável, então a pasta pode ir para outro disco mantendo os recursos juntos. A versão padrão começa com as configurações em inglês e o texto do jogo em japonês; uma versão com chinês começa com configurações e texto em chinês. Configurações já salvas continuam valendo.

O Config oferece **Vertical synchronization (VSync)** ao lado da taxa de atualização. Ela vem desligada. Ligue para sincronizar a imagem com o monitor; a configuração de 30/60 Hz do jogo continua independente. Salve e reabra o jogo para aplicar.

O Config também oferece **Graphics processing: Multithreaded (padrão) / Single-threaded**. O modo multithread roda o processamento ordenado dos comandos gráficos junto com o jogo. O single-thread roda o mesmo processamento na thread do jogo, para comparação e solução de problemas; ele não fixa a afinidade de processador do programa. Salve e reabra o jogo para aplicar.

No Config, o idioma da interface e o do jogo são independentes. O japonês vem embutido; o chinês só aparece se ligado na compilação. Para jogar só em japonês e inglês, use a versão padrão e depois **Import or update language ISO…**, digite `English` como identificador e nome e escolha a sua ISO já com o patch. A importação compila um pacote de idioma separado; ela não inclui nem aplica um patch de tradução para inglês por você. Para atualizar a tradução depois, importe a ISO nova com o mesmo identificador `English`. Ela substitui o pacote inglês ativo só depois de uma compilação bem-sucedida; configurações e saves comuns são mantidos.
