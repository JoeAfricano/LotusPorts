<!-- source: https://github.com/Acranon/kf1-enhanced/blob/HEAD/README.md | fingerprint: bfba48b0cf41b447 -->

1. Baixe o zip mais recente na página de [versões (Releases)](https://github.com/Acranon/kf1-enhanced/releases) e extraia onde quiser.

2. Abra o `kings-field.exe`. Na primeira abertura, ele pede a imagem do seu disco e copia os arquivos do jogo para uma pasta `data` ao lado do exe. Leva alguns segundos e acontece uma vez só; a imagem em si não é alterada.

- Em vez de escolher a imagem, você pode arrastar o seu `.cue` (ou `.iso`) para o `kings-field.exe`, ou colocá-lo na mesma pasta antes da primeira abertura.

- Com o disco japonês, ele então oferece acrescentar o inglês (veja abaixo).

3. Aperte **F1** a qualquer momento para as configurações. O jogo continua rodando com o painel aberto.

Para usar outra imagem de disco depois, apague a pasta `data` e abra de novo. Versões anteriores à 0.1.0-pre.3 guardavam os arquivos direto em `data`; eles são movidos para `data\en` (ou `data\ja`) sozinhos.

### Idiomas

O jogo roda em inglês (a tradução de fã de John Osborne, v1.0) ou no japonês original, e troca entre eles quando quiser: no menu **Options** do próprio jogo (nova linha Language) ou em **F1 > Settings > Language**. Menus e nomes de itens mudam na hora; diálogos e placas mudam na próxima vez que aparecem. Os saves funcionam nos dois.

Cada idioma precisa dos próprios arquivos do jogo, guardados em `data\en` e `data\ja`:

- **Inglês a partir de um disco japonês:** nada traduzido vem com este projeto. O jogo baixa a versão original do tradutor (`Kings_Field_Jap_to_Eng_v1.0.rar`, cerca de 6 MB) da [cópia do site dele no Internet Archive](https://web.archive.org/web/2021/https://www.angelfire.com/art3/weissvulf/), confere, aplica nos seus arquivos japoneses e confere o resultado com os arquivos em inglês conhecidos. Se você já tem o `.rar` (ou o `.ppf` dentro dele), escolha-o, ou arraste para o `kings-field.exe`.

- **Japonês quando você só tem um disco com patch:** acrescente a imagem do seu disco japonês original em F1 > Settings > Language (ou arraste para o `kings-field.exe`). Um disco com patch não volta a ser japonês.

### Controles

- Ação · Teclado / mouse

- Andar / andar de lado · W A S D

- Olhar · Mouse (clique na janela para capturá-lo), Page Up / Page Down

- Virar · Setas esquerda / direita

- Correr · Shift (segurar, ou alternar em Settings > Controls)

- Atacar · Espaço, botão esquerdo do mouse

- Magia · Q, botão direito do mouse

- Interagir / confirmar · E, Enter

- Menu · Tab (Tab ou Backspace volta)

- Pausa · P, Esc

- Mapa completo · M

- Salvar / carregar rápido · F2 / F4 (o F4 também funciona na tela de título)

- Painel de configurações · F1

- Tela cheia · F11, Alt+Enter

Controles são aceitos (o analógico esquerdo anda; clique nele para correr).
