# Sandlada.RopSharp 使用指南（繁體中文）

> 本檔是 [`README.md`](../../README.md) 的中文摘要，程式碼範例與 1.1 實作一致。
> 設計理由見 [`ADR 0001`](../adr/0001-core-design-decisions.md) 與 [`ADR 0002`](../adr/0002-api-gap-remediation.md)。

## 安裝

```bash
dotnet add package Sandlada.RopSharp
```

目標框架：`net10.0`、`net11.0`。無外部相依。核心型別為 `readonly struct`，AOT/trim 安全，熱路徑無反射。

## 訂單履約主線

本文件、README 與程式庫的 XML 文件說明共用同一條「訂單履約」主線，因此讀者一路看著同一個程式長大，而不是
一堆互不相干的片段：

| 型別 | 角色 |
| --- | --- |
| `Customer` | 買家。收件地址可為空，因此能示範「缺少值」的路徑。 |
| `Address` | 郵遞地址。 |
| `Order` | 待履約的訂單，內含訂單行；`Total` 與 `ItemCount` 為衍生值。 |
| `OrderLine` | 單一訂單行，`Amount` 為衍生值。 |
| `Payment` | 對應訂單的付款嘗試。 |
| `PaymentStatus` | 付款的生命週期狀態。 |
| `Shipment` | 交給承運商的包裹。 |

這些型別位於
[`Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs`](../../Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs)，
並編譯進測試組件。`OrderFixtures` 代替外部系統：`Charge` 對遭拒付的訂單會丟出例外，因此建立在它之上的邊界
可以確定性地展示兩條軌道。

## 三個型別，三個問題

| 問題 | 型別 | 成功 | 缺少／失敗 |
| --- | --- | --- | --- |
| 「失敗了嗎？為什麼？」 | `Result<TOk, TErr>` | `Ok(value)` | `Err(error)` |
| 「有值嗎？」 | `Option<T>` | `Some(value)` | `None` |
| 「兩個都是一般值時選哪一個？」 | `Either<TLeft, TRight>` | `Left(left)` | `Right(right)` |

`Option` 不攜帶原因。需要原因時，在邊界顯式轉換：

```csharp
Result<Address, RopError> address =
    Opt.FromNullable(order.Customer.Address)
       .BindSome(shipping => Opt.Some(shipping))
       .RequireSome(() => RopError.Create("E_ADDRESS", "the customer has no shipping address"));
```

## Result 快速上手

```csharp
// 生產默認：Err 固定為 RopError
Result<Payment, RopError> ok = Rop.Ok(payment);
Result<Payment, RopError> err = Rop.Err<Payment>(RopError.Create("E_CHARGE", "the provider refused"));

// 自訂 Err 型別（測試／interop）
Result<Payment, string> custom = Rop.Ok<Payment, string>(payment);

// 不拋例外的分支查詢
if (result.TryGetOk(out var value)) { /* ... */ }
if (result.TryGetErr(out var error)) { /* ... */ }

// 唯一全函數出口
string text = result.Match(ok: v => v.ToString(), err: e => e.Code);

// 明確默認值的輕量出口
int fallback = result.BindErrValue(0);
int lazily  = result.BindErrValue(() => ComputeDefault());
```

`OrThrow()`／`OkOrThrow()`／`ErrOrThrow()` 僅供測試與邊界使用。

## 錯誤模型

```csharp
RopError error = RopError.Create(
    code: "E_ORDER_NOT_FOUND",     // 穩定、可 switch
    message: "order 42 does not exist", // 只做展示
    details: "id=42");             // 選用

RopError boundary = RopError.FromException(exception, "E_IO"); // 只有這裡會帶 Cause
RopError retagged = error.WithCode("E_ORDER_404");

RopError aggregate = RopError.Combine([notFound, outOfStock]);
IReadOnlyList<RopError> leaves = aggregate.InnerErrors; // 原始錯誤，依傳入順序
```

- `Code`／`Message` 不可為空白；空白屬於程式錯誤（`ArgumentException`）。
- `Details` 可為 `null`，但不可為空白字串。
- `Kind` 是粗略分類（`Failure`、`Validation`、`NotFound`、`Conflict`、`Unauthorized`、`Forbidden`、`Unexpected`、
  `Timeout`、`Unavailable`、`RateLimited`），供傳輸層對映使用；精確分支請用 `Code`。新值只會附加，請把 enum 視為開放集合。
- 相等性比 `Kind`、`Code`、`Message`、`Details`，**不比較 `Cause` 與 `InnerErrors`**（避免堆疊與結構差異）。
- `InnerErrors` 永不為 `null`：一般錯誤是空清單，`Combine` 的聚合則保留每個原始錯誤（不攤平，巢狀聚合維持巢狀）；
  `WithCode` 等 `With*` 方法會一併保留。
- `RopError.Combine(errors)` 將非空清單合併為 `E_MULTIPLE`；只傳一個元素時直接回傳該錯誤，不再包一層。
- 慣例：`E_<DOMAIN>[_<REASON>]`，全大寫加底線。

## 短路 vs 聚合（兩種語義，永不混用）

```csharp
// 短路：首錯即停，後續函數不執行
Result<Order, RopError> order =
    Rop.Ok(raw)
       .BindOk(CheckCustomer)
       .BindOk(CheckAmount);

// 聚合：全部檢查，收集所有錯誤（依參數順序）
Result<Order, IReadOnlyList<RopError>> validated =
    Rop.Validate(CheckCustomer(raw), CheckAmount(raw), CheckStock(raw),
                 (customer, amount, stock) => new Order("ord-1001", customer, stock));

// 顯式轉換
Result<Order, IReadOnlyList<RopError>> lifted = order.ToValidation();
Result<Order, RopError> first  = validated.ToSingleError();
Result<Order, RopError> merged = validated.ToSingleError(RopError.Combine);
```

`Sequence`／`Traverse`／`Partition` 接受 `IEnumerable<T>`，但**立即物化一次**。空集合回傳 `Ok([])`／`Some([])`，永不回傳 `null`。
`SequenceValidation`／`TraverseValidation` 遵循相同的集合規則，但採聚合語義：每個元素都會被映射，所有錯誤依元素順序回傳。

## Option 快速上手

```csharp
Option<Address> some = Opt.Some(order.Customer.Address!);
Option<Address> none = Opt.None<Address>();

Address? maybeAddress = null;
Option<Address> lifted = Opt.FromNullable(maybeAddress);       // 引用型別
Option<int> count      = Opt.FromNullableValue(maybeItemCount); // Nullable<T>

string city = some.Match(some: a => a.City, none: () => "unknown");

// default(Option<T>) 契約為 None，可安全放陣列／欄位
Option<Order>[] slots = new Option<Order>[3];                  // 全部 None
```

Option 沒有 `MapNone`：None 沒有值可映射；None 側只有恢復（`BindNone`／`BindNoneValue`）與觀察（`TeeNone`）。

## 跨域轉換（全部顯式）

```csharp
// Option -> Result：唯一橋樑
Result<Payment, RopError> required = Opt.None<Payment>().RequireSome(RopError.Create("E_NONE", "missing"));

// Result -> Option：丟棄錯誤
Option<Payment> optional = Rop.Err<Payment>(error).ToOption();

// Result/Option -> Either：Ok/Some -> Right，Err/None -> Left
Either<RopError, Payment> fromResult = Rop.Ok(payment).ToEither();
Either<string, Payment>  fromOption = Opt.None<Payment>().ToEither("missing");
Result<Payment, RopError> roundTrip = fromResult.ToResult(reason => reason);

// Either -> Option：只保留 Right，Left 會被丟棄
Option<Payment> present = Either.Right<string, Payment>(payment).ToOption();

// Option <-> Nullable
Address? back = Opt.Some(address).ToNullable();          // 引用型別
int? count  = Opt.Some(1).ToNullableValue();             // 值型別

// Option<->Result 翻轉
Result<Option<Payment>, RopError> flipped = Opt.Some(Rop.Ok(payment)).Transpose();
Option<Result<Payment, RopError>> back2   = Rop.Ok(Opt.Some(payment)).Transpose();
```

## 非同步與取消

每個同步組合子都有 `...Async` 對應；`CancellationToken` 一律可選、放最後。取消一律拋
`OperationCanceledException`，**不會**轉成 `Err`／`None`。

```csharp
Task<Result<string, RopError>> chained =
    ChargeAsync(order, ct)
        .MapOkAsync(payment => payment.Id, ct)
        .BindOkAsync(id => GetShipmentAsync(id, ct), ct);

// CT-aware 重載：同一個 token 透傳
Task<Result<string, RopError>> ctAware =
    ChargeAsync(order, ct).MapOkAsync((payment, token) => FetchReceiptAsync(payment, token), ct);
```

語義：

- token 已取消且前置 Task 尚未完成 → 回傳的 Task 被取消，mapper/binder 不執行。
- 成功軌 + token 已取消 → `OperationCanceledException`。
- 失敗軌 → 直接穿透（不額外觀察取消）。
- 程式庫內部一律 `ConfigureAwait(false)`。
- async 方法的參數驗證透過回傳的 Task 觀察。

`Rop.TryAsync`、`Rop.SequenceAsync`、`Rop.TraverseAsync` 規則相同；`SequenceAsync` 依列舉順序逐一
await，確保回報的錯誤具決定性（要並行請在呼叫端先啟動 Task）。

聚合遍歷在非同步軌也維持「不短路」契約：`Rop.SequenceValidationAsync` 與 `Rop.TraverseValidationAsync`
（含 CT-aware mapper 重載）會等待所有 Task，並依來源順序回傳所有錯誤。Task 擲出的例外一律往外拋，
不視為領域錯誤；多個 Task 同時失敗時以第一個例外為準，其餘仍會被觀察。

`Option` 也補齊了同樣的非同步表面：同步 `Option<T>` 可直接使用 `MapSomeAsync`／`BindSomeAsync`／
`BindNoneAsync`／`TeeSomeAsync`／`TeeNoneAsync`／`FilterAsync`；`Opt.CombineAsync`／`ZipAsync` 會等待
所有輸入，`Opt.SequenceAsync`／`TraverseAsync` 依序等待並在第一個 `None` 短路。`Task<Option<T>>` 也
支援 `Select`／`SelectMany`／`Where` 查詢語法。

## 邊界：例外只在此處變成值

```csharp
Result<decimal, RopError> parsed = Rop.Try(() => decimal.Parse(text), ex => RopError.FromException(ex, "E_PARSE"));
Result<decimal, RopError> byCode = Rop.Try(() => decimal.Parse(text), code: "E_PARSE");

// 沒有回傳值的邊界：用 Unit 保持結果可組合
Result<Unit, RopError> saved = Rop.Try(() => repository.Save(order), ex => RopError.FromException(ex, "E_SAVE"));
Task<Result<Unit, RopError>> sent = Rop.TryAsync(() => gateway.SendAsync(order, ct), "E_SEND", ct);

Result<Address, RopError> address =
    Rop.FromNullable(customer.Address, () => RopError.Create("E_ADDRESS", "the customer has no shipping address"));

Result<int, RopError> count =
    Rop.FromNullableValue(maybeItemCount, () => RopError.Create("E_COUNT", "the order has no item count"));
```

- `OperationCanceledException` 永遠原樣拋出。
- 邊界函數回傳 `null` → `Err`，代碼 `E_NULL_RESULT`；`Try` 永不產生 `Ok(null)`。
- `Rop.Switch` 純抬升（不捕例外）；`Rop.TrySwitch` 抬升並轉例外；`Rop.SwitchAsync`／`Rop.TrySwitchAsync` 是等待版本。
  它們只負責「抬升函數」，不是 `Result` 上的 `Switch`／`SwitchAsync` 終端。
- `Rop.Try(Action, …)`／`Rop.TryAsync(Func<Task>, …)` 處理沒有回傳值的邊界，回傳 `Result<Unit, RopError>`。
- 不要把回傳 `Task` 的 lambda 傳給同步 `Rop.Try<T>`：`Task` 會變成成功值；非同步工作請用 `Rop.TryAsync`。

## 解析

`Rop.Parse<T>`／`Rop.TryParse<T>` 使用 invariant culture 與 `IParsable<T>`。解析失敗時，錯誤會同時在訊息與
`Details`（`input=<quoted>`）回顯原始輸入，讓錯誤本身就能定位問題，不必另外記錄原始 payload。

```csharp
Result<decimal, RopError> amount = Rop.Parse<decimal>(raw, "E_AMOUNT");
// Err: Code=E_AMOUNT, Message='19,50' is not a valid Decimal., Details=input="19,50"
```

回顯上限為 64 個 code point，會在不切開 surrogate pair 的前提下截斷，並轉義引號、反斜線與控制字元；
`null` 會顯示為字面 `null`，與空字串區分。

## LINQ

```csharp
// Result：select + from（沒有 where，因為謂詞失敗沒有錯誤值可產生）
var total = from customer in GetCustomer(id)
            from order in GetOrder(customer.Id)
            select order.Total;

// Option：select + from + where（where = Filter）
var total = from address in Opt.FromNullable(customer.Address)
            from line in Opt.Some(order.Lines[0])
            where line.Quantity > 0
            select line.Amount;
```

## 可空性規則

- `Rop.Ok(null)`、`Rop.Err(null)`、`Opt.Some(null)` 拋 `ArgumentNullException`（輸入守衛）。
- mapper 可以回傳可空 payload：`Result<string?, RopError>`；用 `RequireNotNull` 收斂：

```csharp
Result<Address, RopError> address =
    GetOrder(id)
        .MapOk(order => order.Customer.Address)   // Address?
        .RequireNotNull(() => RopError.Create("E_ADDRESS", "the customer has no shipping address"));
```

- `Option<T>` 永不含 `null`；可空映射請用 `BindSomeNullable`／`BindSomeNullableValue`。
- `ToNullable` 是程式庫唯一產生 `null` 的地方（引用型別）；回到 Option 請用 `Opt.FromNullable`。
- 集合輔助方法把 `null` 視為程式錯誤：`TryFirst`、`TryLast`、`TryFind` 與帶 mapper 的 `Choose` 遇到
  `null` 元素、字典值或映射結果時拋 `ArgumentNullException`，訊息會指出來源（`source` 或 `mapper`）；
  `Option` 不會產生 `Some(null)`，也不會把 `null` 放進結果清單。

## 命名對照

| Result 域 | Option 域 | 語義 |
| --- | --- | --- |
| `MapOk` | `MapSome` | 成功側變換 |
| `MapErr` | —（None 無值） | 失敗側變換 |
| `BindOk` | `BindSome` | 成功側展開 |
| `BindErr` | `BindNone` | 失敗側恢復（函數） |
| `BindErrValue` | `BindNoneValue` | 失敗側恢復（值／工廠） |
| `TeeOk` | `TeeSome` | 成功側觀察 |
| `TeeErr` | `TeeNone` | 失敗側觀察 |
| `Ensure` | `Filter` | 斷言 |
| `Match` | `Match` | 全函數出口 |
| `RequireNotNull` | `RequireSome` | 補 null／補 Some，抬升為 Result |
| `Combine`／`Validate` | `Zip`／`Sequence` | 同層合併（Option 無錯誤可聚合） |

## 反模式

- 用 `try/catch` 包住組合子鏈 → 用 `Rop.Try` 包邊界。
- 生產路徑用 `OrThrow` → 用 `Match`、`BindErrValue`、`TryGet*`。
- 用 `null` 表示失敗 → 用 `Err`。
- 用 `Option` 表達需要原因的領域失敗 → 用 `Result`。
- 用 `Option` 包集合 → 空集合就是空集合。
- 在同一個公開簽名混用 `Option` 與 `Nullable<T>` → 只在邊界轉換一次。
- 用 `Message` 做分支 → 用 `Code`。
- 需要短路卻用 `Validate` → 用 `BindOk`。
- 在 `Tee*` 內改外部狀態並依賴執行順序 → 只做日誌／度量。

## 1.1 新增（摘要）

- `OptionEnumerableExtensions`：`TryFirst`、`TryLast`、`TryFind`、`Choose`、`Execute`、`ExecuteNoValue`。
- `Rop.SuccessIf`／`Rop.FailureIf`、`Rop.Parse<T>`／`Rop.TryParse<T>`（`IParsable<T>`、invariant culture）。
- `Switch`／`SwitchAsync`／`MatchFirst`／`SwitchFirst`、`FailIf`／`ThenEnsure`。
- 非同步聚合：`CombineAsync`、`ValidateAsync`、`ZipAsync`、`PartitionAsync`（皆支援取消）。
- 驗證遍歷：`Rop.SequenceValidation`、`Rop.TraverseValidation`，以及
  `Rop.SequenceValidationAsync`、`Rop.TraverseValidationAsync`（支援取消與 CT-aware mapper）。
- 非同步 LINQ：`Task<Result<,>>` 上的 `Select`／`SelectMany`。
- `Either<TLeft,TRight>` 與 `RopErrorKind`（`RopError.Kind`）。
- `RopError.InnerErrors`：`RopError.Combine` 保留每個原始錯誤，不再只留下摘要。
- 非同步邊界與抬升：`Rop.Try(Action)`、`Rop.TryAsync(Func<Task>)`、`Rop.SwitchAsync`、`Rop.TrySwitchAsync`。
- 診斷品質：`RopErrorKind` 新增 `Timeout`／`Unavailable`／`RateLimited`；`Rop.Parse` 在 `Details` 回顯輸入。
- 模型橋接：`Result.ToEither`、`Option.ToEither`、`Either.ToOption` 補齊三種模型間的轉換。
- 集合輔助方法統一 null 元素政策：一律拋 `ArgumentNullException`，訊息指出來源。
- 範圍決策（ADR 0010）：高元數 `Combine`/`Validate`、有界並發、`IAsyncEnumerable` 皆留在呼叫端組合，核心不新增 API。

## 與初稿的已知差異

C# 無法宣告「只差泛型約束」的兩個重載，因此可空值型別版本以 `Value` 後綴命名：

```text
FromNullableValue / ToNullableValue / BindSomeNullableValue
```

class 版本維持 `FromNullable`／`ToNullable`／`BindSomeNullable`。`TryGetOk`／`TryGetErr`／`TryGetValue`
以 struct 實例方法提供。完整 API 與設計理由見 [`README.md`](../../README.md) 與
[`ADR 0001`](../adr/0001-core-design-decisions.md)、[`ADR 0002`](../adr/0002-api-gap-remediation.md)。

XML 文件範例的撰寫規則（以及保證它們真的能編譯的守門測試）見
[`docs/doc-examples.md`](../doc-examples.md)。
