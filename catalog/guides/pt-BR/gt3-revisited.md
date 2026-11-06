<!-- source: https://github.com/matt-pulsipher/GT3-revisited/blob/HEAD/README.md | fingerprint: fa474efd9de2a8fb -->

Baixe o pacote para o seu sistema na página de [versões (Releases)](https://github.com/matt-pulsipher/GT3-revisited/releases). Cada versão lista os `SHA256SUMS`, e cada arquivo tem uma atestação de compilação: `gh attestation verify <file> --repo matt-pulsipher/GT3-revisited` confere se ele foi montado pelo workflow deste repositório a partir do código marcado. Os pacotes não têm certificado de desenvolvedor, então a primeira abertura tem um passo a mais:

- **Windows**: extraia e abra o `gt3r.exe`. Se o SmartScreen disser "O Windows protegeu o computador", escolha **Mais informações → Executar assim mesmo**. O 3D roda em Vulkan ou, em placas sem ele (por exemplo GeForce 400/500, Intel HD 2000–4000), em OpenGL ES pelo ANGLE, que vem no pacote.

- **Meta Quest** (conferido no Quest 3; Quest 2, 3S e Pro sem teste): ligue o modo de desenvolvedor do headset no app Meta Horizon do celular e instale o `gt3r-quest.apk` a partir de um computador com `adb install gt3r-quest.apk` (ou SideQuest / Meta Quest Developer Hub). Copie a imagem do seu disco para a pasta **Download** do headset por USB. O jogo fica na biblioteca, em **Unknown Sources**. Detalhes: [VR](https://github.com/matt-pulsipher/GT3-revisited/blob/HEAD/docs/dev/VR.md).

- **iPhone e iPad** (iOS 16 ou mais novo): não há pacote, porque um app de iOS precisa ser assinado para o aparelho onde roda. Monte num Mac e instale com o Xcode e o seu próprio Apple ID; uma conta gratuita basta. Com a conta gratuita, o app instalado para de abrir depois de 7 dias: instalar de novo renova, e saves e configurações continuam.

Links diretos para a versão mais nova, sempre os mesmos: [gt3r-windows-x64.zip](https://github.com/matt-pulsipher/GT3-revisited/releases/latest/download/gt3r-windows-x64.zip), [gt3r-macos-arm64.zip](https://github.com/matt-pulsipher/GT3-revisited/releases/latest/download/gt3r-macos-arm64.zip), [gt3r-linux-x86_64.tar.gz](https://github.com/matt-pulsipher/GT3-revisited/releases/latest/download/gt3r-linux-x86_64.tar.gz), [gt3r-android.apk](https://github.com/matt-pulsipher/GT3-revisited/releases/latest/download/gt3r-android.apk), [gt3r-quest.apk](https://github.com/matt-pulsipher/GT3-revisited/releases/latest/download/gt3r-quest.apk). O **Download update** do jogo usa esses links.

Ou compile você mesmo; uma compilação a partir do código não precisa de nenhum dos passos acima.
