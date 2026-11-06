<!-- source: https://github.com/HossEz/fifa-world-cup-2014-recompiled/blob/HEAD/README.md | fingerprint: 64245c5b7c869cbf -->

1. Baixe o `fifa-world-cup-2014-recompiled-<version>-setup.exe` na [página de versões (Releases)](https://github.com/HossEz/fifa-world-cup-2014-recompiled/releases) e abra. Ele instala em Arquivos de Programas (o Windows pede permissão uma vez) e abre o launcher. Prefere não instalar? Pegue o `-portable.zip`, extraia onde quiser e abra o `Launcher.exe`. Ele guarda tudo, inclusive os saves, na própria pasta. Settings → Data → Desktop shortcut cria um atalho na Área de Trabalho com o ícone do jogo.

> O programa não tem assinatura digital, então o Windows pode mostrar "O Windows protegeu o computador" na primeira vez. Escolha **Mais informações** e depois **Executar assim mesmo**.

2. O launcher pede a sua cópia do jogo: a pasta (a que tem o `default.xex`) ou a imagem do disco (`.iso`). Escolha ou arraste para a janela, e ele confere se é o jogo certo.

3. Clique em **Play**.

Para sair do jogo, segure **Back + Start** (**Create + Options** no PlayStation) por 2 segundos, ou aperte **Alt+F4**.

As configurações do launcher definem como o jogo roda (janela, resolução, quadros por segundo, Show FPS, ...) e de onde ele é lido. Um controle é recomendado (Xbox, PlayStation e outros). O teclado também funciona, e as teclas estão na página Controls.

### Atualizações

Ao abrir, o launcher procura nesta página uma versão mais nova e oferece. **Update now** baixa, confere com as somas de verificação da versão e instala (a versão instalada pede permissão ao Windows, como na instalação, e a portátil substitui os próprios arquivos). Configurações e saves ficam. Settings → Data → Check for updates desliga isso.

### Requisitos

O jogo exige mais da placa de vídeo que do processador. Vídeo integrado roda mal, principalmente os da Intel.

- Requisito · Detalhes

- Windows · 10 ou 11, 64 bits

- Vídeo · Direct3D 12, com drivers atualizados

- Processador · Com SSE4.2 (Intel de 2008 em diante, AMD de 2011 em diante)

- O jogo · Não vem junto: a sua própria cópia do 2014 FIFA World Cup Brazil para Xbox 360, como imagem de disco (`.iso`) ou a pasta com o `default.xex` (o launcher confere se a sua cópia funciona)

### Onde as coisas ficam (por padrão)

- Item · Local

- O programa · `C:\Program Files\2014 FIFA World Cup Brazil Recompiled`

- Saves, configurações, registros, cache de shaders · `Documentos\2014 FIFA World Cup Brazil` (a versão portátil guarda tudo na própria pasta)

Problemas vão nas [Issues](https://github.com/HossEz/fifa-world-cup-2014-recompiled/issues). Inclua o `fifawc14.log` (Settings → Data → Logs abre a pasta dele). Ele lista o seu hardware e as configurações, sem nome de usuário nem caminhos de pastas.

### Problemas conhecidos

- Cenas, replays e comemorações ainda podem derrubar o FPS.

- As placas Intel UHD e Iris Xe ainda rodam o jogo mal. O jogo não desenha direito nessas placas por um problema do lado da Intel, então elas usam um modo mais lento que desenha certo, mas custa bem mais desempenho.
