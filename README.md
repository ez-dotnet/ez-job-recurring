# EZ.Job.Recurring

Módulo de **jobs recorrentes (cron)** para [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core).

Agenda execuções periódicas usando expressões cron, com recuperação automática de jobs pendentes no startup e 3 canais isolados (FF, Recurring, Recovery).

## Instalação

```bash
dotnet add package EZ.Job.Recurring
```

## Uso

```csharp
using EZ.Job.Core;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddEZJob(options =>
{
    options.WorkerCount = 4;
})
.AddRecurringJobs(options =>
{
    // Registro inline de jobs recorrentes
    options.Register<EmailService>(s => s.LimparCaixaAsync(), "0 */6 * * *");    // a cada 6h
    options.Register<RelatorioService>(s => s.GerarDiarioAsync(), "0 0 * * *");  // meia-noite
    options.WorkerCount = 1;
    options.PollIntervalSeconds = 30;
});

// Ou registre via DI a qualquer momento
public class MeuService
{
    private readonly IRecurringJobManager _recurring;

    public MeuService(IRecurringJobManager recurring)
    {
        _recurring = recurring;
    }

    public async Task IniciarAsync()
    {
        await _recurring.ScheduleAsync<EmailService>(
            s => s.EnviarNewsletterAsync(), "0 8 * * 1"); // segundas 8h
    }
}
```

## Workers

| Canal      | Default Workers | Descrição                                      |
|------------|----------------|------------------------------------------------|
| FF         | 4              | Processa jobs fire-and-forget imediatamente    |
| Recurring  | 1              | Processa jobs recorrentes agendados            |
| Recovery   | 1              | Reprocessa jobs pendentes ao iniciar           |

## Projetos relacionados

- [EZ.DotNet](https://github.com/ez-dotnet)
- [EZ.Job.Core](https://github.com/ez-dotnet/ez-job-core)
