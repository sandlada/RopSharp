# Sandlada.RopSharp 使用指南（繁體中文）

> 本檔是 [`README.md`](../../README.md) 的中文摘要，程式碼範例與 1.1 實作一致。
> 設計理由見 [`ADR 0001`](../adr/0001-core-design-decisions.md) 與 [`ADR 0002`](../adr/0002-api-gap-remediation.md)。

## 安裝

```bash
dotnet add package Sandlada.RopSharp
```

目標框架：`net10.0`、`net11.0`。無外部相依。核心型別為 `readonly struct`，AOT/trim 安全，熱路徑無反射。

## 兩個型別，兩個問題

| 問題 | 型別 | 成功 | 缺少／失敗 |
| --- | --- | --- | --- |
| 「失敗了嗎？為什麼？」 | `Result<TOk, TErr>` | `Ok(value)` | `Err(error)` |
| 「有值嗎？」 | `Option<T>` | `Some(value)` | `None` |

`Option` 不攜帶原因。需要原因時，在邊界顯式轉換：

```csharp
Result<Address, RopError> address =
    Opt.FromNullable(user.Profile)
       .BindSome(profile => Opt.FromNullable(profile.Address))
       .RequireSome(() => RopError.Create("E_ADDRESS", "address required"));
```

## Result 快速上手

```csharp
// 生產默認：Err 固定為 RopError
Result<int, RopError> ok = Rop.Ok(42);
Result<int, RopError> err = Rop.Err<int>(RopError.Create("E_X", "bad"));

// 自訂 Err 型別（測試／interop）
Result<int, string> custom = Rop.Ok<int, string>(42);

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
    code: "E_USER_NOT_FOUND",      // 穩定、可 switch
    message: "user 42 not found",  // 只做展示
    details: "id=42");             // 選用

RopError boundary = RopError.FromException(exception, "E_IO"); // 只有這裡會帶 Cause
RopError retagged = error.WithCode("E_USER");

RopError aggregate = RopError.Combine([nameError, amountError]);
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
       .BindOk(CheckName)
       .BindOk(CheckAmount);

// 聚合：全部檢查，收集所有錯誤（依參數順序）
Result<Order, IReadOnlyList<RopError>> validated =
    Rop.Validate(CheckName(raw), CheckAmount(raw), CheckStock(raw),
                 (name, amount, stock) => new Order(name, amount, stock));

// 顯式轉換
Result<Order, IReadOnlyList<RopError>> lifted = order.ToValidation();
Result<Order, RopError> first  = validated.ToSingleError();
Result<Order, RopError> merged = validated.ToSingleError(RopError.Combine);
```

`Sequence`／`Traverse`／`Partition` 接受 `IEnumerable<T>`，但**立即物化一次**。空集合回傳 `Ok([])`／`Some([])`，永不回傳 `null`。
`SequenceValidation`／`TraverseValidation` 遵循相同的集合規則，但採聚合語義：每個元素都會被映射，所有錯誤依元素順序回傳。

## Option 快速上手

```csharp
Option<string> some = Opt.Some("x");
Option<string> none = Opt.None<string>();

string? maybeName = null;
Option<string> lifted = Opt.FromNullable(maybeName);       // 引用型別
Option<int> age       = Opt.FromNullableValue(maybeAge);   // Nullable<T>

string text = some.Match(some: v => v, none: () => "anonymous");

// default(Option<T>) 契約為 None，可安全放陣列／欄位
Option<Order>[] slots = new Option<Order>[3];              // 全部 None
```

Option 沒有 `MapNone`：None 沒有值可映射；None 側只有恢復（`BindNone`／`BindNoneValue`）與觀察（`TeeNone`）。

## 跨域轉換（全部顯式）

```csharp
// Option -> Result：唯一橋樑
Result<int, RopError> required = Opt.None<int>().RequireSome(RopError.Create("E_NONE", "missing"));

// Result -> Option：丟棄錯誤
Option<int> optional = Rop.Err<int>(error).ToOption();

// Result/Option -> Either：Ok/Some -> Right，Err/None -> Left
Either<RopError, int> fromResult = Rop.Ok(42).ToEither();
Either<string, int>  fromOption = Opt.None<int>().ToEither("missing");
Result<int, RopError> roundTrip = fromResult.ToResult(reason => reason);

// Either -> Option：只保留 Right，Left 會被丟棄
Option<int> present = Either.Right<string, int>(1).ToOption();

// Option <-> Nullable
string? back = Opt.Some("x").ToNullable();          // 引用型別
int? count  = Opt.Some(1).ToNullableValue();        // 值型別

// Option<->Result 翻轉
Result<Option<int>, RopError> flipped = Opt.Some(Rop.Ok(1)).Transpose();
Option<Result<int, RopError>> back2   = Rop.Ok(Opt.Some(1)).Transpose();
```

## 非同步與取消

每個同步組合子都有 `...Async` 對應；`CancellationToken` 一律可選、放最後。取消一律拋
`OperationCanceledException`，**不會**轉成 `Err`／`None`。

```csharp
Task<Result<string, RopError>> chained =
    GetUserAsync(id, ct)
        .MapOkAsync(user => user.Name, ct)
        .BindOkAsync(user => GetOrderAsync(user.Id, ct), ct);

// CT-aware 重載：同一個 token 透傳
Task<Result<string, RopError>> ctAware =
    GetUserAsync(id, ct).MapOkAsync((user, token) => FetchNameAsync(user, token), ct);
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
Result<int, RopError> parsed = Rop.Try(() => int.Parse(text), ex => RopError.FromException(ex, "E_PARSE"));
Result<int, RopError> byCode = Rop.Try(() => int.Parse(text), code: "E_PARSE");

// 沒有回傳值的邊界：用 Unit 保持結果可組合
Result<Unit, RopError> saved = Rop.Try(() => repository.Save(order), ex => RopError.FromException(ex, "E_SAVE"));
Task<Result<Unit, RopError>> sent = Rop.TryAsync(() => gateway.SendAsync(order, ct), "E_SEND", ct);

Result<string, RopError> name =
    Rop.FromNullable(maybeName, () => RopError.Create("E_NULL", "name required"));

Result<int, RopError> age =
    Rop.FromNullableValue(maybeAge, () => RopError.Create("E_AGE", "age required"));
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
Result<int, RopError> quantity = Rop.Parse<int>(raw, "E_QUANTITY");
// Err: Code=E_QUANTITY, Message='abc' is not a valid Int32., Details=input="abc"
```

回顯上限為 64 個 code point，會在不切開 surrogate pair 的前提下截斷，並轉義引號、反斜線與控制字元；
`null` 會顯示為字面 `null`，與空字串區分。

## LINQ

```csharp
// Result：select + from（沒有 where，因為謂詞失敗沒有錯誤值可產生）
var total = from user in GetUser(id)
            from order in GetOrder(user.Id)
            select order.Total;

// Option：select + from + where（where = Filter）
var sum = from a in Opt.Some(1)
          from b in Opt.Some(2)
          where a < b
          select a + b;
```

## 可空性規則

- `Rop.Ok(null)`、`Rop.Err(null)`、`Opt.Some(null)` 拋 `ArgumentNullException`（輸入守衛）。
- mapper 可以回傳可空 payload：`Result<string?, RopError>`；用 `RequireNotNull` 收斂：

```csharp
Result<string, RopError> name =
    GetUser(id)
        .MapOk(user => user.Name)   // string?
        .RequireNotNull(() => RopError.Create("E_NULL", "name required"));
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
