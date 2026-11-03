# TASK1 — 生產級契約

本版取代 2026-11-03 的保守草稿。執行時以本檔為準。

狀態以文末 checkpoint 為準。從 `step` 繼續。做完一個步驟就改進度。做完一個 milestone 才提交一次。

M0 完成前，根目錄 `*.md` 仍會忽略本檔。M0 把 `TASK*.md` 放進白名單，並在同一個 commit 裡納入版控。

NuGet 上已發布 **`0.0.1`**（`CHANGELOG` 的 `[0.0.1] - 2026-10-31` 即該基線）。本 TASK 的全部改動屬於 **`0.0.2`**。允許改寫既有 ADR，允許破壞性變更。刪掉的公開成員不留 `[Obsolete]`、不留轉發多載、不留雙軌實作。

## 進度

<!-- TASK1-CHECKPOINT
status: in-progress
milestone: M0
step: M0.2
last_commit: none
updated: 2026-11-03
blocked_reason:
-->

| 欄位 | 值 |
| --- | --- |
| status | `in-progress` |
| milestone | M0 |
| step | M0.2 |
| last_commit | none |
| updated | 2026-11-03 |

`status` 只用這四個值：`ready`、`in-progress`、`blocked`、`done`。

## 怎麼從斷點繼續

1. 讀 checkpoint。`status` 為 `done` 就停止。
2. 從 `step` 繼續。台帳裡已是 `done` 的步驟不要重做。本 milestone 尚未提交的工作區改動要留著，在其上接著做。
3. 動手前把 `status` 改成 `in-progress`，`updated` 改成當天日期。
4. 步驟做完：台帳改成 `done`，證據寫一行（命令和結果）。checkpoint 的 `step` 改成下一步。
5. 該 milestone 的最後一步才提交。一個 milestone 一個 commit，裡面包含程式變更和更新後的本檔。提交成功後把 `last_commit` 寫成完整 SHA，再把 `step` 推進到下一個 milestone 的第一步。
6. 中斷前先把 checkpoint 和台帳寫回本檔。沒寫回就沒有斷點。
7. 驗證失敗時把 `status` 設為 `blocked`，寫上命令、錯誤摘要、下次要改的檔案。測試紅著不要提交。規格撞上語言限制時同樣標 `blocked`，寫明限制。不要用 `[Obsolete]` 或多留一套多載來繞過。
8. 不要跳 milestone，不要把兩個 milestone 放進同一個 commit，不要 push。使用者當次明確要求 push 時才 push。
9. 不要改 git config，不要 `--no-verify`，不要 `commit --amend`，不要 force push。
10. 寫或改測試前先讀 `csharp-tunit` skill。跑 `dotnet test` 前先讀 `run-tests` skill；若倉庫有 `.agents/skill-overlays/dotnet-test/run-tests.md` 且 frontmatter 符合該 skill，套用它。
11. 程式、XML 註解、ADR、分析器訊息用英文。本檔用中文。
12. 公開簽章變了就在同一個 milestone 裡改完倉庫內呼叫點、XML 範例、README 裡因此失準的例子、sample。`DocExampleCompilationTests` 必須能過。

## 做完之後的系統

四個值，三種計算，一套由套件帶進消費端的分析器。

| 型別 | 問題 | 組合 |
| --- | --- | --- |
| `Result<TOk, TErr>` | 失敗了嗎 | 短路。`BindOk` 在第一個錯誤停下 |
| `Validation<T, TErr>` | 哪些規則沒過 | 累積。沒有 `Bind`。`Zip` / `Traverse` 收集全部錯誤 |
| `Option<T>` | 有沒有值 | `default` 仍是 `None` |
| `Either<TLeft, TRight>` | 兩個普通值是哪一個 | 同步 `Map` 與 `Bind`。沒有 `EitherTask` |

`ResultTask<TOk, TErr>`、`OptionTask<T>`、`ValidationTask<T, TErr>` 是唯一的非同步表面。它們可以被 `await`。取消權杖用 `WithCancellation` 設一次，存在計算上，不再出現在每個方法的可選參數裡。同步方法與非同步方法分名：`MapOk` 對 `MapOkAsync`。公開表面不再使用 `OverloadResolutionPriority`。

`default(Result<,>)`、`default(Either<,>)`、`default(Validation<,>)`、`default(ParseFailure)` 和三種 `*Task` 的 `default` 都是未初始化。讀取承載會拋 `InvalidOperationException`。實質型別的 `0` 不再被當成合法錯誤或合法右值。

`RopError.Equals` 比較整棵子錯誤樹。`Cause` 仍不參與相等，因為堆疊不穩定。`Try` 拒絕把 `Task` / `ValueTask` 當成成功值。

消費套件的專案會得到四條分析器診斷：禁止寫出這些 `default`、禁止把鐵路值當成陳述式丟掉、禁止把回傳工作的委派傳進同步方法、字面錯誤碼不符合 `E_*` 時提出警告。

## 版本 0.0.2

| 項目 | 規則 |
| --- | --- |
| 已發布基線 | NuGet / git tag 上的 **`0.0.1`**。`PublicAPI.Shipped.txt` 必須與該版本一致，直到你依下面流程更新它。 |
| 本 TASK 目標版本 | **`0.0.2`**。每個 milestone 的變更都記在 `CHANGELOG` 的 **`## [Unreleased]`** 下，標題旁註明「將於 0.0.2 發布」。 |
| 專案版本號 | **M1.1** 把 `Sandlada.RopSharp.csproj` 的 `<Version>` 改成 **`0.0.2`**。`<PackageReleaseNotes>` 改成一句指向 `CHANGELOG` 的 0.0.2 / Unreleased 摘要，不要仍寫「initial release」。 |
| 套件相容性 | `EnablePackageValidation` 保持開啟。若破壞性變更導致 pack 與 NuGet 上的 `0.0.1` 比對失敗，在 csproj 加上 `<PackageValidationBaselineVersion>0.0.1</PackageValidationBaselineVersion>`（或 SDK 文件當前推薦的等價屬性），讓驗證以已發布包為基線、允許 0.0.2 的破壞性 diff。 |
| 發布 cut（M8） | 把 `## [Unreleased]` 改為 **`## [0.0.2] - YYYY-MM-DD`**，寫 **Migration** 小節列出必改呼叫點（刪除的 API、`Task<Result>` 組合子 → `ResultTask` 等）。把 `PublicAPI.Unshipped.txt` 全部併入 `PublicAPI.Shipped.txt`（序數排序），`Unshipped` 只留檔頭。README 若仍描述 0.0.1 表面，一併改到 0.0.2。 |
| 本 TASK 內 | 不執行發佈 workflow，不 `dotnet nuget push`，除非使用者當次明確要求。 |

## 破壞性變更怎麼落地

- 刪除就是刪除。替代品是新名字或新型別，舊名字不再存在。NuGet 消費者從 `0.0.1` 升級到 `0.0.2` 必須改呼叫點；不提供 shim。
- 改決定時，直接改那份 ADR 的正文，並在標題下加一行 `- **Revised:** YYYY-MM-DD. ...`。不要留兩份互相矛盾的 ADR。新子系統才新增 ADR。
- M1 先改 `AGENTS.md` 和 `CONTRIBUTING.md` 裡「既有 ADR 結論永不改寫」那句，改成上一節的 ADR 改寫規則。否則後面的 milestone 會和倉庫規則衝突。
- `CHANGELOG.md`：在 `## [0.0.1]` 之上維護 `## [Unreleased]`（內容屬於 0.0.2）。M4 要改寫 M3 留下的非同步句子，讓 Unreleased 只描述最終的 `*Task` 表面。
- **公開 API 基線（已發布 0.0.1）：**
  - **新增**成員：只加進 `PublicAPI.Unshipped.txt`（`RS0016` 給出精確簽章）。
  - **刪除**成員：從 `PublicAPI.Shipped.txt` 刪掉對應行（`RS0017`）。
  - **簽章變更**：舊簽章從 `Shipped` 刪除，新簽章進 `Unshipped`。
  - **M8 發布 cut** 之前：`Unshipped` 可以累積多個 milestone 的條目；不要在每個 milestone 結束時清空 `Unshipped`（那是未發布 repo 的流程，不適用於已發布 0.0.1）。
  - **M8** 做一次合併：`Unshipped` → `Shipped`，然後清空 `Unshipped` 條目。
- 推斷用的 `RopError` 非泛型多載留下來。它們不是舊 API 的轉接層。
- `Partition` 留在 `Result`。它回傳的是兩份清單，不是累積驗證。
- `Option` 的 `default == None` 不變。

程式有改動的 milestone 在提交前跑：

```powershell
dotnet format RopSharp.sln --verify-no-changes
dotnet test -c Release
```

格式不乾淨時先 `dotnet format RopSharp.sln`，再重跑驗證。單測用 `--treenode-filter`，例如 `dotnet test -- --treenode-filter "/*/*/EitherTests/*"`。提交前必須是上面的全量 Release 測試。

提交：

```powershell
git status
git diff
git log -8 --oneline
git add -- <本 milestone 的路徑>
git commit -m "<本 milestone 的單行訊息>"
git status
git rev-parse HEAD
```

訊息用倉庫既有的 `type: 說明`。SHA 寫回 `last_commit`。不要把無關的本地改動帶進 commit。

新的公開成員保留一份可編譯的 `<code><![CDATA[ ... ]]></code>`，範例沿用 `OrderFixtures`。註解寫契約：哪一條路徑會執行、未初始化時拋什麼。

## 範圍外

核心保持零執行期依賴。不加入 HTTP、Polly、`IAsyncEnumerable<T>`、函式庫內的並發節流、四元以上的 `Zip`、`Choice`、隱式轉換、`Map` / `Bind` 別名、第二套 `ValueTask` 多載。`RopError` 維持 sealed class。`Either` 不取得非同步計算型別；要進入非同步鐵路時先 `ToResult`。

## Milestone

| ID | 結果 | commit |
| --- | --- | --- |
| M0 | Markdown 白名單，本檔進版控 | `chore: widen the markdown gitignore whitelist` |
| M1 | 未初始化不再偽裝成值；`Either` 可 `Bind` | `fix: reject unset unions and add Either bind` |
| M2 | 會吞掉失敗的展開方法消失，只留 `Expect(string)` | `refactor: replace silent unwraps with Expect` |
| M3 | 累積驗證成為獨立型別 | `feat: split accumulating validation from Result` |
| M4 | 非同步多載矩陣收成三種計算 | `feat: replace async overloads with computation types` |
| M5 | 錯誤按樹比較；`Try` 拒絕工作物件 | `fix: compare errors by structure and reject tasks from Try` |
| M6 | 分析器隨套件送達 | `feat: ship contract analyzers in the package` |
| M7 | 原始碼按型別歸位，公開表面被測試鎖住 | `refactor: group the core by type and lock the surface` |
| M8 | SourceLink + **0.0.2** 發布 cut（PublicAPI 合併、CHANGELOG 定稿） | `build: prepare 0.0.2 with SourceLink and release metadata` |

## 刪除清單

| 刪除 | 替代 | 哪個 milestone |
| --- | --- | --- |
| `OkOrDefault` | `Match` 或 `BindErrValue` | M2 |
| `Result.OrThrow`、`OkOrThrow`、`ErrOrThrow`、`Option.OrThrow` | `Expect(string reason)` | M2 |
| `ToValidation`、`ToSingleError`、`Result.Validate`、`Result.SequenceValidation`、`Result.TraverseValidation` 及其 `Async` | `Validation` / `ValidationTask` | M3 刪同步與清單通道，M4 刪 `Task<Validation<,>>` |
| `Task<Result<,>>`、`Task<Option<>>` 上的組合子多載 | `ResultTask`、`OptionTask` | M4 |
| 公開方法上的 `OverloadResolutionPriority` | 同步與非同步分名，非同步委派只有一種形狀 | M4 |

## M0 — Markdown 白名單

沒有產品程式。提交前不要跑測試。

### M0.1 改 `.gitignore`

把「Markdown: ignore by default」到 `!docs/adr/*.md` 那一段換成：

```gitignore
# Markdown: ignore by default, then whitelist essential docs.
# LICENSE has no extension, so the *.md rule does not apply to it.
*.md
!README.md
!CHANGELOG.md
!AGENTS.md
!CONTRIBUTING.md
!CONTRIBUTORS.md
!CODE_OF_CONDUCT.md
!SECURITY.md
!LICENSE.md
!docs/**/*.md
!Sandlada.*/*.md
!perf/README.md
!TASK*.md
```

`bin/`、`obj/`、套件輸出和 `# AI` 以下的規則保持原樣。不要新增空的 `CODE_OF_CONDUCT.md` 或 `CONTRIBUTORS.md`。

### M0.2 驗證

`git check-ignore` 結束碼 0 代表被忽略，1 代表沒被忽略。檔案不必存在：

```powershell
$paths = @(
  'README.md', 'CHANGELOG.md', 'AGENTS.md', 'CONTRIBUTING.md', 'CONTRIBUTORS.md',
  'CODE_OF_CONDUCT.md', 'SECURITY.md', 'LICENSE.md', 'LICENSE',
  'docs/adr/0001-core-design-decisions.md', 'docs/guide.md',
  'Sandlada.RopSharp.Sample/README.md', 'Sandlada.RopSharp.Sample/NOTES.md',
  'Sandlada.RopSharp/Internals.md', 'perf/README.md', 'TASK1.md',
  'scratch.md', 'notes/local.md',
  'Sandlada.RopSharp.Sample/Scenarios/NOTES.md',
  'Sandlada.RopSharp.Tests/Documentation/Notes.md'
)
git check-ignore -v --non-matching -- @paths
```

必須沒被忽略：`README.md`、`CHANGELOG.md`、`AGENTS.md`、`CONTRIBUTING.md`、`CONTRIBUTORS.md`、`CODE_OF_CONDUCT.md`、`SECURITY.md`、`LICENSE.md`、`LICENSE`、兩份 `docs/` 路徑、`Sandlada.RopSharp.Sample/README.md`、`Sandlada.RopSharp.Sample/NOTES.md`、`Sandlada.RopSharp/Internals.md`、`perf/README.md`、`TASK1.md`。

必須被忽略：`scratch.md`、`notes/local.md`、`Sandlada.RopSharp.Sample/Scenarios/NOTES.md`、`Sandlada.RopSharp.Tests/Documentation/Notes.md`。

`docs/adr/0001-core-design-decisions.md` 若被忽略，先修規則再繼續。不要為了驗證去新增這些檔。

### M0.3 提交

```powershell
git add -- .gitignore TASK1.md
git commit -m "chore: widen the markdown gitignore whitelist"
```

提交後 `git check-ignore -v TASK1.md` 的結束碼應為 1。SHA 寫進 checkpoint，`step` 改為 `M1.1`。

## M1 — 未初始化是第三態

`Result`、`Either`、`ParseFailure` 各用一個 `byte` 標籤。不要同時留著舊的 `bool` 旗標。標籤 `0` 就是 `default`。

| 標籤 | `Result` | `Either` | `Validation`（M3 才新增型別，規則先寫在這裡） |
| --- | --- | --- | --- |
| 0 | 未初始化 | 未初始化 | 未初始化 |
| 1 | Ok | Left | Success |
| 2 | Err | Right | Failure，錯誤清單非空 |

`Result` 的觀察：

| 成員 | 0 | 1 | 2 |
| --- | --- | --- | --- |
| `IsOk` | false | true | false |
| `IsErr` | true | false | true |
| `TryGetOk` | false，不拋 | true | false |
| `TryGetErr`、`Match`、`MapErr`、`BindErr`、`ErrUnsafe` | 拋 `InvalidOperationException` | 既有行為 | 既有行為 |
| `Equals` / `GetHashCode` / `==` | 兩個 0 相等。0 不等於任何 Ok，也不等於承載剛好是 `default(TErr)` 的 Err | 既有承載相等 | 既有承載相等 |
| `ToString` | `Err(uninitialized)`，不拋 | 既有格式 | 既有格式 |

刪掉 `default(TErr) is not null` 這條判斷。雜湊把標籤算進去。

`Either` 的觀察：

| 成員 | 0 | 1 | 2 |
| --- | --- | --- | --- |
| `IsLeft` / `IsRight` | 都是 false | Left | Right |
| `TryGetLeft` / `TryGetRight` | 都回傳 false，不拋 | 只有左支 | 只有右支 |
| `Match`、`MapLeft`、`MapRight`、`BindLeft`、`BindRight`、`Swap`、`ToResult`、`ToOption` | 先拋 `InvalidOperationException`，再也不呼叫委派 | 既有行為 | 既有行為 |
| `Equals` / `==` | 兩個 0 相等，且不等於 `Left(default)` 或 `Right(default)` | 承載相等 | 承載相等 |
| `ToString` | `Either(uninitialized)` | 既有格式 | 既有格式 |

工廠仍拒絕 null 承載。

`Bind` 加在 `Either<TLeft, TRight>` 上，binder 在最後：

```csharp
public Either<TLeftOut, TRight> BindLeft<TLeftOut>(Func<TLeft, Either<TLeftOut, TRight>> binder)
public Either<TLeft, TRightOut> BindRight<TRightOut>(Func<TRight, Either<TLeft, TRightOut>> binder)
```

另一支不呼叫 binder。binder 回傳標籤 0 時，結果保持標籤 0。null binder 拋 `ArgumentNullException`。

`ParseFailure` 從 `readonly record struct` 改成 `readonly struct`，自己實作相等性和 `ToString`。標籤 0 時，`Code`、`Message`、`Input`、`TypeName`、`Details` 的讀取都拋 `InvalidOperationException`。搜尋 `ParseFailure` 的 `with` 運算式，改成建構式。

### M1.1 規則與 ADR

改 `AGENTS.md`、`CONTRIBUTING.md`：決定變了就改原 ADR，並加上日期；已發布 `0.0.1` 的成員在 `0.0.2` 中直接刪除，不加 `[Obsolete]` 或轉發多載。

把 `Sandlada.RopSharp.csproj` 的 `<Version>` 改為 **`0.0.2`**，更新 `<PackageReleaseNotes>`。必要時設定 `PackageValidationBaselineVersion` 為 **`0.0.1`**。

在 `CHANGELOG.md` 的 `## [0.0.1]` 之上新增 **`## [Unreleased]`**（註明：以下將納入 **0.0.2**）。

改 ADR 0001 D1 的正文。標題下加 `- **Revised:**` 行。新正文寫明三種標籤，以及實質型別 `TErr` 的 `default(Result<,>)` 也是未初始化。`Option` 那句維持 `default` 是 `None`。

改 ADR 0002 裡描述 `Either` 的那條決定（D20）：補上 `BindLeft` / `BindRight`，並寫明仍然沒有 `Choice`、沒有 `Either` 的非同步表面。

不要新增 `IsInitialized` 公開屬性。未初始化只透過既有成員的行為暴露。

### M1.2 `Result`

改欄位、相等性、雜湊、`ToString`、`TryGetErr`、`ThrowIfUninitialized`。已經呼叫 `ThrowIfUninitialized` 或 `ErrUnsafe` 的組合子跟著新的標籤走。

### M1.3 `Either`

加上標籤和兩個 `Bind`。XML 範例用訂單領域。新簽章照 `RS0016` 寫進 **`PublicAPI.Unshipped.txt`**（0.0.1 已在 `Shipped` 鎖定，不在本 milestone 併入 Shipped）。

### M1.4 `ParseFailure`

改儲存和讀取契約。更新 `MaintenanceHardeningTests` 裡把 `default(ParseFailure).Input` 當成可讀 null 的斷言。

### M1.5 測試、變更紀錄、提交

至少這些行為要有 TUnit 測試：

- `default(Result<string, int>)` 的 `TryGetErr` 和 `Match` 拋 `InvalidOperationException`。
- 它不等於 `Result.Err<string, int>(0)`。
- `default(Result<int, RopError>)` 仍是 `IsErr`，`TryGetErr` 仍拋。
- `default(Either<int, string>)` 的 `IsLeft` 和 `IsRight` 都是 false，`TryGetRight` 為 false。
- 它不等於 `Either.Right<string, int>(0)`。兩個 `default(Either<...>)` 相等。
- `default` 的 `Match` 拋，而且委派呼叫次數是 0。
- `BindRight` 只在右支呼叫 binder；binder 回傳 `default` 時結果仍是未初始化。
- `default(ParseFailure).Code` 拋。

`DefaultValues_FollowTheirDocumentedContracts` 裡 `Either` 那一行改成兩支都是 false。`Either_GetHashCode_CoversTheDefaultRightValue` 改名，斷言改成未初始化。`Option` 仍等於 `None`。

`## [Unreleased]` 的 `### Changed` 記錄這三個 `default` 契約。`### Added` 記錄兩個 `Bind`。

搜尋註解和 README 裡把 `default(Either)` 說成 `Right` 的句子，改成新契約。ADR 0001 的正文已在 M1.1 改寫，不要在別處留舊句。

```powershell
git add -- Sandlada.RopSharp Sandlada.RopSharp.Tests AGENTS.md CONTRIBUTING.md CHANGELOG.md docs/adr/0001-core-design-decisions.md docs/adr/0002-api-gap-remediation.md README.md TASK1.md
git commit -m "fix: reject unset unions and add Either bind"
```

實際改到的路徑以 `git status` 為準，放進同一個 commit。

## M2 — 展開必須說出理由

刪除 `Result.OkOrDefault`、`Result.OrThrow`、`Result.OkOrThrow`、`Result.ErrOrThrow`、`Option.OrThrow`。

只加這兩個方法：

```csharp
public static TOk Expect<TOk, TErr>(this Result<TOk, TErr> result, string reason)
public static T Expect<T>(this Option<T> option, string reason)
```

`reason` 為 null、空字串或空白時拋 `ArgumentException`。`Result` 在未初始化或 `Err` 時拋 `InvalidOperationException`，訊息同時包含 `reason` 和值的 `ToString()`。`Ok` 回傳承載。`Option` 在 `None` 時拋，訊息包含 `reason`。`Either` 不加 `Expect`，兩邊都用 `Match`。

這是邊界出口，不是領域失敗的表達方式。預期失敗仍然是值。

### M2.1 實作與文件

用搜尋把倉庫裡的舊呼叫全部改掉，包含測試、sample、XML、README。測試需要成功值時寫 `Expect("test")`。需要預設值時改成 `Match` 或 `TryGetOk`，不要新增另一個「失敗就回 default」的方法。

ADR 0001、0002 裡把 `OrThrow` 寫成拋出 API 的句子改成 `Expect`。加上 `- **Revised:**` 行。

從 **`PublicAPI.Shipped.txt`** 刪掉 `OrThrow` / `OkOrDefault` 等已移除簽章；**`Expect`** 加進 **`PublicAPI.Unshipped.txt`**。`## [Unreleased]` 用 `### Removed` 和 `### Added`（0.0.2）。

### M2.2 提交

全量驗證通過後：

```powershell
git commit -m "refactor: replace silent unwraps with Expect"
```

## M3 — `Validation<T, TErr>`

累積和短路分成兩個型別。`Validation<T, TErr>` 是 `readonly struct`，標籤規則同 M1。失敗清單由工廠保證非空：空清單拋 `ArgumentException`，null 元素拋 `ArgumentNullException`。

```csharp
public readonly struct Validation<T, TErr>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }          // 標籤 0 和 2 都是 true
    public bool TryGetSuccess(out T value); // 標籤 0 回傳 false，不拋
    public bool TryGetErrors(out IReadOnlyList<TErr> errors); // 只有標籤 2 回傳 true；標籤 0 拋

    public Validation<TOut, TErr> Map(Func<T, TOut> mapper);

    public Result<T, TErr> Collapse(Func<IReadOnlyList<TErr>, TErr> merge);
}

public static class Validation
{
    public static Validation<T, TErr> Success<T, TErr>(T value);
    public static Validation<T, TErr> Failure<T, TErr>(TErr error);
    public static Validation<T, TErr> Failure<T, TErr>(IReadOnlyList<TErr> errors);

    public static Validation<T, TErr> From<T, TErr>(Result<T, TErr> result);

    public static Validation<TOut, TErr> Zip<T1, T2, TOut, TErr>(...);
    public static Validation<TOut, TErr> Zip<T1, T2, T3, TOut, TErr>(...);
    public static Validation<IReadOnlyList<T>, TErr> Sequence<T, TErr>(...);
    public static Validation<IReadOnlyList<TOut>, TErr> Traverse<TIn, TOut, TErr>(...);
}
```

`Zip` 的參數順序是資料在前、合併函式在最後，元數停在 2 和 3。兩個失敗會把錯誤清單依參數順序接起來，不把 `TErr` 再展平。`Traverse` 對每個元素都呼叫 mapper，前一個失敗不會讓後面的元素跳過。mapper 拋出的例外繼續往外拋，不收進錯誤清單。

`Collapse` 在成功時回傳 `Ok`，不呼叫 `merge`。失敗時把非空清單交給 `merge`。`merge` 回傳 null 時拋 `InvalidOperationException`。未初始化時拋，不呼叫 `merge`。

沒有 `Bind`。沒有「只留第一個錯誤」的無參數折疊。

從 `Result` 刪掉：`Validate`、`SequenceValidation`、`TraverseValidation`、`ToValidation`、`ToSingleError`，以及這些方法的 `Async` 版本裡以 `Result<T, IReadOnlyList<TErr>>` 為通道的簽章。

同步累積改由 `Validation.Zip`、`Sequence`、`Traverse`、`From` 承擔。既有的非同步累積方法先改成回傳 `Task<Validation<T, TErr>>`，讓清單通道在這個 milestone 就消失。M4 會刪掉這個 `Task<Validation<,>>` 回傳型別。

`Result.Combine`、`Zip`、`Sequence`、`Traverse`、`Partition` 保持短路，留在 `Result`。`Result.Zip` 不是 `Validation.Zip`。

### M3.1 ADR

改寫 ADR 0001 D5 和 ADR 0004 裡仍把累積描述成 `Result<T, IReadOnlyList<TErr>>` 的決定。正文改成獨立的 `Validation<T, TErr>`。標題下加 `- **Revised:**`。等待策略的句子留到 M4 再對到 `ValidationTask`：M3 的非同步方法仍是「先等待，再累積」。

### M3.2 測試必須守住的行為

- 兩個失敗的 `Zip` 依參數順序含有兩邊的錯誤。
- `Traverse` 在第一個元素失敗後仍然呼叫後面的 mapper。用計數斷言。
- `Result` 的短路 `Traverse` 仍然在第一個錯誤後停止呼叫 mapper。既有測試要改到新名字上，不要放寬。
- `Validation<,>` 的公開方法裡沒有名字以 `Bind` 開頭的成員。用 `typeof` 反射即可，反射只出現在測試。
- 標籤 0 的 `TryGetErrors` 拋，`Collapse` 不呼叫 merge。
- `Failure` 拒絕空清單和 null 元素。
- sample 裡的 `ToSingleError(RopError.Combine)` 改成 `Collapse(RopError.Combine)`。

`## [Unreleased]` 記錄移除的方法和 `Validation`。非同步那一句寫成目前的 `Task<Validation<,>>`，M4 會改寫它。

### M3.3 提交

```powershell
git commit -m "feat: split accumulating validation from Result"
```

## M4 — 三種計算

這是一個 milestone。中間步驟可以編譯失敗，但不要在舊多載還活著的時候提交。

刪掉 `ResultAsyncExtensions`、`OptionAsyncExtensions` 的公開方法，以及 `Result` / `Option` 上所有回傳 `Task<Result<,>>`、`Task<Option<>>`、`Task<Validation<,>>` 的組合子。也刪掉為這些多載服務的 `OverloadResolutionPriority`。LINQ 的 `Select` / `SelectMany` / `Where` 從 `Task<...>` 搬到對應的計算型別上。

`Result.TryAsync` 回傳 `ResultTask<T, RopError>`。直接 `await Result.TryAsync(...)` 仍然成立，因為計算型別有 `GetAwaiter`。

```csharp
public readonly struct ResultTask<TOk, TErr>
{
    public static ResultTask<TOk, TErr> From(Result<TOk, TErr> result);
    public static ResultTask<TOk, TErr> From(Task<Result<TOk, TErr>> task);

    public ResultTask<TOk, TErr> WithCancellation(CancellationToken cancellationToken);

    public ResultTask<TOut, TErr> MapOk(Func<TOk, TOut> mapper);
    public ResultTask<TOut, TErr> MapOkAsync(Func<TOk, CancellationToken, Task<TOut>> mapper);
    public ResultTask<TOut, TErr> BindOk(Func<TOk, Result<TOut, TErr>> binder);
    public ResultTask<TOut, TErr> BindOkAsync(Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder);

    public ResultTaskAwaiter<TOk, TErr> GetAwaiter();
}
```

`OptionTask<T>` 與 `ValidationTask<T, TErr>` 用同一套形狀。`ValidationTask` 有 `Map` / `MapAsync` / `Zip` / `Sequence` / `Traverse`，沒有 `Bind`。

規範：

- `From(null)` 拋 `ArgumentNullException`。`default` 的 `GetAwaiter().GetResult()` 拋 `InvalidOperationException`，不要讓它變成 `NullReferenceException`。
- `WithCancellation` 更換權杖，不建立 `CancellationTokenSource`，不把兩個權杖連起來。呼叫端要合併權杖時自己做。
- 單值組合子使用存在該計算上的權杖：等待時用 `WaitAsync`，並把同一枚權杖傳給 `*Async` 委派。`ConfigureAwait(false)`。
- `MapOkAsync` 只有 `Func<TOk, CancellationToken, Task<TOut>>` 這一種委派。不需要權杖的呼叫端寫 `static (value, _) => ...`。不要再提供「同步委派的 Async 方法」或「不帶權杖的 Async 委派」。
- 同步 `MapOk`、`BindOk`、`Option.MapSome`、`BindSome`、`Validation.Map` 若成功型別是 `Task`、`Task<T>`、`ValueTask` 或 `ValueTask<T>`，拋 `InvalidOperationException`，訊息指向上面對應的 `*Async`。這個判斷按封閉型別快取一次，做法對齊 `ArgumentGuards.Nullability<T>`。
- 多個計算的靜態方法（`Zip`、`Sequence`、`Traverse`）另帶一枚最後參數 `CancellationToken`，蓋到結果上。不要去讀每個輸入計算內部已經存著的權杖。
- `ResultTask.Zip` 保持今天 `CombineAsync` 的等待策略：先等待全部輸入，再依參數順序取第一個錯誤，合併函式不執行。不要在這裡取消其他輸入。
- 循序的 `Sequence` 在第一個錯誤後不再等待後面的工作。
- `ValidationTask.Traverse` 對每個元素都啟動映射；取消仍然是 `OperationCanceledException`，不是一筆驗證錯誤。工作本身拋出的例外也不是驗證錯誤。
- `GetResult` 回傳鐵路值。不要把 `Err` 或 `Failure` 解開成例外。工作本身 fault 時，例外照常傳播。
- `Result.Ok(task)` 仍然合法。拒絕 `Task` 成功值的是同步映射和 M5 的 `Try`，不是 `Ok` 工廠。
- 外部 API 回傳的 `Task<Result<,>>` 由 `ResultTask.From` 進入。這是邊界，不是舊擴充方法的轉接層。

`await` 器是 `readonly struct`，實作 `ICriticalNotifyCompletion`。

### M4.1 ADR

改寫 ADR 0001 D8：刪掉 `OverloadResolutionPriority` 的決定，改成計算型別和分名方法。改寫 ADR 0003：`Option` 的非同步對等是 `OptionTask`。ADR 0010 加一句：`ResultTask` 不引入節流或第二套取消模型。ADR 0004 的非同步句子改成 `ValidationTask`。各加 `- **Revised:**`。

### M4.2 到 M4.5

依序做 `ResultTask`、`OptionTask`、`ValidationTask`，然後刪舊的公開非同步組合子並改完全部呼叫點、XML、README、sample、benchmarks。基準專案必須能編譯。配置預算若因為新的等待層而變化，把量到的數字寫進 `AllocationBudgets.cs`，旁邊用一行註解說明原因。不要刪掉配置測試，也不要無說明地把預算放大。

`## [Unreleased]` 改寫 M3 的非同步句子，只保留 `ResultTask`、`OptionTask`、`ValidationTask`。

### M4.6 提交

全量測試通過，而且公開原始碼裡不再有 `OverloadResolutionPriority`。

```powershell
git commit -m "feat: replace async overloads with computation types"
```

## M5 — 錯誤樹與 `Try` 邊界

### 相等性

`RopError.Equals`、`GetHashCode`、`==` 納入 `InnerErrors`，依順序遞迴。`Cause` 仍排除。兩個摘要相同、子清單不同的聚合不再相等。

用明確堆疊走訪，不要用遞迴呼叫。深的聚合不應該把呼叫堆疊撐爆。`Equals` 要能處理惡意循環：走訪時看到已經在目前路徑上的參考就視為不相等的結構並結束，不要無限迴圈。`GetHashCode` 同樣要終止。

改寫 ADR 0005 D38 的正文。摘要相等這條決定撤回。

新增：

```csharp
public IReadOnlyList<RopError> Flatten()
```

回傳葉節點，順序與 `Combine` 的參數順序一致，巢狀聚合展開，聚合節點本身不出現在清單裡。原子錯誤回傳只含自己的單元素清單。走訪同樣要終止。

### `Try`

`Result.Try` 與 `Result.TryAsync` 在即將回傳 `Ok` 時，若成功型別是 `Task`、`Task<T>`、`ValueTask` 或 `ValueTask<T>`，拋 `InvalidOperationException`，訊息要求改用會等待的邊界。判斷按封閉型別快取。

改寫 ADR 0006 D43。`Result.Ok(task)` 的合法性在 ADR 裡寫清楚，避免實作者把工廠一起封掉。

### 測試與提交

- 兩個 `Combine` 結果摘要相同、子錯誤不同時，`Equals` 為 false。
- 只差在 `Cause` 時 `Equals` 為 true。
- 巢狀 `Combine` 的 `Flatten` 順序正確。
- 造一個把自己放進子清單的循環（測試可以靠反射寫進非公開欄位，或在測試子類別無法達成時，在 `internal` 測試鉤子裡建立；優先用 `InternalsVisibleTo` 已有的可見性，不要為了測試把欄位改成 public）。循環輸入上 `Equals` 和 `Flatten` 都會結束。
- `Result.Try(() => Task.CompletedTask, ...)` 拋 `InvalidOperationException`。
- `Result.Ok(Task.CompletedTask)` 仍然是 `Ok`。

```powershell
git commit -m "fix: compare errors by structure and reject tasks from Try"
```

## M6 — 分析器進套件

新增專案 `Sandlada.RopSharp.Analyzers`，目標框架只許 `netstandard2.0`。這是 Roslyn 的載入契約。函式庫、測試、sample、benchmarks 維持 `net10.0;net11.0`。不要把 `netstandard2.0` 加進函式庫。

分析器引用集中管理的 `Microsoft.CodeAnalysis.CSharp`，`PrivateAssets="all"`。啟用 `EnforceExtendedAnalyzerRules` 和 `IsRoslynComponent`。為這個專案開啟 lock file，並提交 `packages.lock.json`。若因此要改 `Directory.Packages.props`，測試與基準的 lock 也要能 `dotnet restore --locked-mode`。

把專案加進 `RopSharp.sln`。

診斷用英文：

| ID | 嚴重度 | 條件 |
| --- | --- | --- |
| `ROP0001` | Error | 語意模型能看出目標型別是 `Result<,>`、`Either<,>`、`Validation<,>`、`ParseFailure`、`ResultTask<,>`、`OptionTask<>`、`ValidationTask<,>` 的 `default` 運算式，以及這些型別的無參數建構。不標記陣列配置 |
| `ROP0002` | Error | 以上型別出現在表達式陳述式，或被賦值給捨棄元 `_` |
| `ROP0003` | Error | 同步參數（`Func<T>`、`Func<T, TOut>` 這類不以 `Task` 為回傳的委派）實際接到回傳 `Task`、`Task<T>`、`ValueTask`、`ValueTask<T>` 的 lambda 或方法群組。參數本身就要求工作的 `*Async` 方法不標記 |
| `ROP0004` | Warning | 傳給 `RopError.Create` 錯誤碼參數的編譯期字串不符合 `^E_[A-Z0-9_]+$` |

第一版不做未使用區域變數的完整資料流分析。

新 ADR `docs/adr/0013-analyzers.md`，狀態 Accepted。寫明四個 ID、嚴重度，以及測試專案不把分析器裝成編譯器分析器的原因：測試必須能構造 `default` 來鎖住執行期契約。

測試專案用普通 `ProjectReference` 參考分析器組件，在 TUnit 裡建立帶分析器的 `CSharpCompilation`，對每個 ID 断言有診斷和沒有診斷的各一份片段。做法對齊現有的 `DocExampleCompilationTests`，不要引入另一個測試框架。

sample 用 `ReferenceOutputAssembly="false"`、`OutputItemType="Analyzer"` 參考分析器，讓範例在編譯時就受到這四條規則約束。sample 裡因此失敗的寫法直接改掉。

函式庫打包後，分析器 DLL 必須出現在 nupkg 的 `analyzers/dotnet/cs/Sandlada.RopSharp.Analyzers.dll`。它不是執行期依賴，函式庫的 AOT 表面不參考 Roslyn。先 `dotnet pack -c Release -o artifacts/pkg`，再把 nupkg 當 zip 檢查。路徑不對就改打包項目，直到這個路徑存在。

```powershell
git commit -m "feat: ship contract analyzers in the package"
```

## M7 — 檔案布局與表面鎖

把 `Sandlada.RopSharp` 的原始碼移到這些目錄，命名空間維持 `Sandlada.RopSharp`：

```text
Result/
Option/
Either/
Validation/
Error/
Parsing/
Internal/
```

`RopError` 與 `RopErrorKind` 進 `Error/`。`ParseFailure` 與 `DiagnosticText` 進 `Parsing/`。`ArgumentGuards` 與 `AsyncGuards` 進 `Internal/`。一個公開類別可以拆成多個 `partial` 檔，類名不變。

這個 milestone 只搬檔、不改簽章。該 commit 對 **`PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`** 的 diff 必須是空的。不是空的就停，把 API 變更移出這個 commit。

新增 `SurfaceLockTests`：

- 公開方法上不再有 `OverloadResolutionPriority`。
- 沒有擴充方法的第一個參數是 `Task<Result<,>>`、`Task<Option<>>` 或 `Task<Validation<,>>`。
- `Validation<,>` 與 `ValidationTask<,>` 沒有以 `Bind` 開頭的公開方法。
- 組件裡的每個公開型別都在測試的允許清單上。清單寫死現有公開型別加上這份計劃新增的 `Validation`、`Validation<,>`、`ResultTask<,>`、`OptionTask<>`、`ValidationTask<,>` 和對應的 awaiter。新的公開型別要讓這支測試失敗，直到清單被有意識地更新。

```powershell
git commit -m "refactor: group the core by type and lock the surface"
```

## M8 — SourceLink 與 0.0.2 定稿

本 milestone 產出的 NuGet 包版本為 **`0.0.2`**（`<Version>` 應已在 M1.1 設好）。

### M8.1 SourceLink

先用 Context7 查目前 SDK 的 SourceLink 做法，再改專案檔。目標是 pack 出來的 **`0.0.2`** 套件帶有 `https://github.com/sandlada/RopSharp`，符號能回到該儲存庫的提交。

設定 `RepositoryUrl`、`RepositoryType`、`PublishRepositoryUrl`。`PackageProjectUrl` 用同一個 GitHub 儲存庫。已經存在的 `EmbedUntrackedSources`、`IncludeSymbols`、`SymbolPackageFormat`、`Deterministic`、`ContinuousIntegrationBuild` 不要設成衝突值。

只有在 `dotnet pack -c Release` 的 nupkg 裡看不到 SourceLink 時，才加入 `Microsoft.SourceLink.GitHub`，並更新 `Directory.Packages.props` 和因此改變的 lock file。

### M8.2 驗證 nupkg

nuspec 含 repository url；`analyzers/dotnet/cs/Sandlada.RopSharp.Analyzers.dll` 仍在；包版本為 **0.0.2**。把看到的 url 寫進台帳。

ADR 0001 末尾「有正式遠端之後補 SourceLink」那條標成已由本 milestone 完成，加上 `- **Revised:**`。`## [Unreleased]` 記一筆套件中繼資料（定稿在 M8.3）。

不執行發佈 workflow，不 `dotnet nuget push`，除非使用者當次明確要求。

### M8.3 0.0.2 發布 cut

- `CHANGELOG`：`## [Unreleased]` → **`## [0.0.2] - <date>`**；新增 **`### Migration`**（從 0.0.1 升級必做項）。
- `PublicAPI.Unshipped.txt` 全部移入 `PublicAPI.Shipped.txt`，排序後清空 `Unshipped` 條目。
- 確認 `<Version>0.0.2</Version>`、`dotnet pack -c Release` 通過、`EnablePackageValidation` 以 **0.0.1** 為基線時可接受破壞性 diff。
- README / sample 不再描述已刪除的 0.0.1 API。

### M8.4 提交

```powershell
git commit -m "build: prepare 0.0.2 with SourceLink and release metadata"
```

提交後把 checkpoint 設為 `status: done`，`step` 留在 `M8.4`，`last_commit` 為這個 SHA。

## 步驟台帳

只改狀態、證據和日期。規格要變時，在證據裡寫「規格調整」，先改對應小節，再繼續。

| 步驟 | 狀態 | 證據 |
| --- | --- | --- |
| M0.1 | done | `.gitignore` markdown whitelist updated |
| M0.2 | done | `git check-ignore`: TASK1/adr not ignored; scratch/Scenarios/Notes ignored |
| M0.3 | in-progress | |
| M1.1 | pending | |
| M1.2 | pending | |
| M1.3 | pending | |
| M1.4 | pending | |
| M1.5 | pending | |
| M2.1 | pending | |
| M2.2 | pending | |
| M3.1 | pending | |
| M3.2 | pending | |
| M3.3 | pending | |
| M4.1 | pending | |
| M4.2 | pending | |
| M4.3 | pending | |
| M4.4 | pending | |
| M4.5 | pending | |
| M4.6 | pending | |
| M5.1 | pending | |
| M5.2 | pending | |
| M5.3 | pending | |
| M5.4 | pending | |
| M6.1 | pending | |
| M6.2 | pending | |
| M6.3 | pending | |
| M6.4 | pending | |
| M7.1 | pending | |
| M7.2 | pending | |
| M7.3 | pending | |
| M8.1 | pending | |
| M8.2 | pending | |
| M8.3 | pending | |
| M8.4 | pending | |

M5 的四步對應：ADR、相等性與 `Flatten`、`Try`、提交。M6 的四步對應：ADR、分析器本體、編譯測試與 sample、打包驗證並提交。M8 的四步對應：SourceLink、nupkg 驗證、**0.0.2 發布 cut**、提交。其餘 milestone 的最後一步都包含提交，證據寫 SHA。
