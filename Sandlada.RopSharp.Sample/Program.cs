using Sandlada.RopSharp.Sample;
using Sandlada.RopSharp.Sample.Scenarios;

// Three orchestration-heavy tours. Each one runs several tracks at once: success and failure,
// cancellation, compensation, retries and aggregation. None of them throws for an expected failure.

await ScenarioRunner.RunAsync(
    "1. Fulfillment saga - multi-service tracks with reverse-order compensation",
    FulfillmentSagaScenario.RunAsync);

await ScenarioRunner.RunAsync(
    "2. Carrier quotes - eager fan-out, partial failure, retry and late selection failure",
    CarrierQuoteScenario.RunAsync);

await ScenarioRunner.RunAsync(
    "3. Settlement batch - per-row pipelines, grouped failures and recovery after a state change",
    SettlementScenario.RunAsync);

await ScenarioRunner.RunAsync(
    "4. New in 1.1 - parsing, Either, async aggregation and collection helpers",
    NewApiScenario.RunAsync);
