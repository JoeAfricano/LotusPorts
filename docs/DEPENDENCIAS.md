# Dependências

Versões centralizadas em `Directory.Packages.props`. Toda dependência nova precisa de licença MIT, Apache ou BSD e entra nesta lista.

| Pacote | Versão | Licença | Onde | Para quê |
|---|---|---|---|---|
| Microsoft.WindowsAppSDK | 2.5.1 | MIT | App, Design | WinUI 3 |
| Microsoft.Windows.SDK.BuildTools | 10.0.28000.2705 | MIT | App, Design | build WinUI |
| CommunityToolkit.Mvvm | 8.4.0 | MIT | App | ViewModels |
| Microsoft.Extensions.Hosting | 10.0.0 | MIT | App | Generic Host e DI |
| Microsoft.Extensions.Logging.Abstractions | 10.0.0 | MIT | Infrastructure | `ILogger` |
| Microsoft.Data.Sqlite | 10.0.13 | MIT | Infrastructure | banco local |
| Dapper | 2.1.66 | Apache-2.0 | Infrastructure | consultas |
| Serilog | 4.3.0 | Apache-2.0 | App | logs |
| Serilog.Extensions.Hosting | 9.0.0 | Apache-2.0 | App | Serilog no Host |
| Serilog.Sinks.File | 7.0.0 | Apache-2.0 | App | arquivo rotativo |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT | testes | |
| xunit | 2.9.3 | Apache-2.0 | testes | |
| xunit.runner.visualstudio | 3.1.4 | Apache-2.0 | testes | |
| AwesomeAssertions | 9.1.0 | Apache-2.0 | testes | fork aberto do FluentAssertions (a v8+ do FluentAssertions é comercial) |
| NSubstitute | 5.3.0 | BSD-3-Clause | testes | dublês |

Pacotes previstos nas próximas fases, que só entram quando forem usados: Polly ou Microsoft.Extensions.Http.Resilience, SharpCompress, System.IO.Hashing. Tomlyn e Vortice.Windows só com aprovação.
