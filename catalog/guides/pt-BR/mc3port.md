<!-- source: https://github.com/SickSartori/mc3port/blob/HEAD/README.md | fingerprint: 7629bef16ab09e23 -->

**Com o Quiver Launcher**: encontre o mc3port no catálogo e instale. O Quiver mantém atualizado.

**Com o instalador**: baixe o `mc3port-<version>-windows-x86-setup.exe` na [página de versões (releases)](https://github.com/SickSartori/mc3port/releases) e abra. Ele instala só para o seu usuário (sem administrador), cria atalhos no menu Iniciar e na Área de Trabalho e pode ser removido pelos aplicativos instalados do Windows. Quando sai uma versão nova, o launcher oferece atualizar: ele baixa o instalador novo, confere o SHA-256 e mostra no Explorador para você abrir.

**Portátil**: baixe o `mc3port-<version>-windows-x86.zip` e extraia onde quiser: ele tem uma pasta `mc3port`, e o `mc3.exe` dentro dela abre o jogo. Para atualizar, extraia a versão nova no mesmo lugar, substituindo os arquivos.

Todos os pacotes trazem o `README.txt`, um guia curto do que você precisa, da primeira abertura e de onde ficam os seus arquivos, e o `CHANGELOG.md`, com o que mudou em cada versão.

Para conferir um download, compare o SHA-256 com o arquivo `SHA256SUMS` da versão. Os arquivos não têm assinatura digital, então o SmartScreen do Windows pode perguntar uma vez se você confia ("Mais informações" e depois "Executar assim mesmo").

As suas configurações (`mc3port.ini`), saves (`xbox_hdd`), registro e cache de shaders ficam em `%LOCALAPPDATA%\mc3port`, então atualizar ou desinstalar nunca mexe neles. Para guardá-los ao lado do `mc3.exe` (num pen drive, por exemplo), crie ali um arquivo vazio chamado `portable.txt`.
