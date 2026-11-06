<!-- source: https://github.com/theokyr/Project8Recomp/blob/HEAD/README.md | fingerprint: 7e08b603fa54de9b -->

1. Baixe a versão para o seu sistema e extraia num lugar onde dê para gravar.

2. Abra o `Project8Recomp` (`Project8Recomp.exe` no Windows). No Steam Deck, adicione esse arquivo como jogo de fora da Steam e não force o Proton na versão para Linux.

3. Ele pede a imagem do seu disco. Indique o seu `.iso`.

4. Ele confere a imagem. Isso leva bem menos de um segundo e ainda não copia nada. Um disco que não é a versão aceita é recusado aqui, antes de gravar 4,7 GB, e não depois.

5. Se bater, ele copia os dados do jogo para a mesma pasta. Leva alguns minutos, e dá para parar.

6. O jogo abre. Dali em diante, o `Project8Recomp` passa direto pelo launcher até o Play. Rode `Project8Recomp --gui` quando quiser configurações, o estado da instalação ou a tela inicial normal do launcher.

Os nomes dos executáveis da v0.1.0 continuam na pasta e mantêm o comportamento antigo, então atalhos existentes continuam funcionando. A nova entrada é um pequeno envoltório do `thps_p8_gui`; ela nunca abre o executável do jogo direto, então as configurações de abertura, a instância única, o supervisor e a limpeza após falhas seguem o mesmo caminho.

Tudo fica nessa pasta: dados do jogo, saves e configurações. Mova, copie entre os seus computadores ou apague. Nada é gravado em outro lugar do sistema.

**Windows:** o executável não tem assinatura, então o SmartScreen mostra "O Windows protegeu o computador". **Mais informações** → **Executar assim mesmo**.

Mais detalhes, inclusive o hash do disco aceito e onde fica cada coisa: o docs/SETUP.md do repositório.
