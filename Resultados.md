# Resultados — EZ.Job.Recurring

O módulo `EZ.Job.Recurring` extende o Core com agendamento cron. Como o overhead do scheduler é mínimo (apenas comparação de timestamps com `Cronos.CronExpression`), o benchmark reflete o desempenho do store subjacente.

Consulte a tabela completa em [EZ.Job.Core/Resultados.md](https://github.com/ez-dotnet/ez-job-core/blob/main/Resultados.md).

## Eficiência do Scheduler

| Métrica                          | Valor               |
|----------------------------------|---------------------|
| Poll interval                    | 30s (configurável)  |
| Overhead por definição por tick  | ~0.001ms            |
| Alocação extra por tick          | ~50 bytes           |
| Canal de recurring               | Unbounded Channel   |
