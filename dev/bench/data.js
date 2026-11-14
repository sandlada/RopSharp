window.BENCHMARK_DATA = {
  "lastUpdate": 1794653182561,
  "repoUrl": "https://github.com/sandlada/RopSharp",
  "entries": {
    "RopSharp": [
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "f9885f73222ffa664890e2438ac3b6a19712634c",
          "message": "perf(bench): extend coverage, add nightly trend and AOT size gate\n\nAdd parsing, async, collection and equality benchmark classes plus the Sequence failure paths. Use OperationsPerInvoke for sub-nanosecond methods so no benchmark reports ZeroMeasurement. Fix the Parse_Int_Success harness input, which overflowed Int32 and silently measured the failure path.\n\nAdd the nightly benchmarks.yml workflow that merges the per-class reports and stores the time trend in gh-pages (alerts only, never blocking), and an AOT size gate in ci.yml against perf/aot-size.baseline.json. Rewrite docs/benchmarks.md around the pinned configuration and regenerate its tables with tools/Update-BenchmarkTables.ps1.",
          "timestamp": "2026-10-29T05:28:45Z",
          "url": "https://github.com/sandlada/RopSharp/commit/f9885f73222ffa664890e2438ac3b6a19712634c"
        },
        "date": 1793260138605,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 58.27967619895935,
            "unit": "ns",
            "range": "± 0.7234728297750751"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 53.0508074760437,
            "unit": "ns",
            "range": "± 1.241476904795686"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 54.27845553557078,
            "unit": "ns",
            "range": "± 1.4744844025675454"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 125.36717947324117,
            "unit": "ns",
            "range": "± 0.80411564609409"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 61.28165580828985,
            "unit": "ns",
            "range": "± 1.7280798457004838"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 51.59373929103216,
            "unit": "ns",
            "range": "± 0.6400294740014578"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 38.91965721050898,
            "unit": "ns",
            "range": "± 0.22531388294246849"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 77.77918457984924,
            "unit": "ns",
            "range": "± 0.5515561383993367"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 16.312308261791866,
            "unit": "ns",
            "range": "± 0.360471316185843"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 91.57313545544942,
            "unit": "ns",
            "range": "± 1.0571779843897395"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 50.82593187689781,
            "unit": "ns",
            "range": "± 1.040171477680226"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 401.39938084284466,
            "unit": "ns",
            "range": "± 6.293852652308206"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 700.3279425303141,
            "unit": "ns",
            "range": "± 19.093569565485634"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 16.510163346926372,
            "unit": "ns",
            "range": "± 0.42183344753501373"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 616.495540936788,
            "unit": "ns",
            "range": "± 8.907706499933894"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 546.740603129069,
            "unit": "ns",
            "range": "± 9.61216499384139"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5325.526184082031,
            "unit": "ns",
            "range": "± 69.42891581134288"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 14367.085255940756,
            "unit": "ns",
            "range": "± 266.6702001612969"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 16.49613011876742,
            "unit": "ns",
            "range": "± 0.2305299801250266"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7919.526596069336,
            "unit": "ns",
            "range": "± 116.78308901658238"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7321.684336344401,
            "unit": "ns",
            "range": "± 32.04785529836382"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.7366999536752701,
            "unit": "ns",
            "range": "± 0.0039379873632486555"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6072080520292124,
            "unit": "ns",
            "range": "± 0.001718059885020069"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0,
            "unit": "ns",
            "range": "± 0"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.226084217429161,
            "unit": "ns",
            "range": "± 0.005430845926189139"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 10.359582106272379,
            "unit": "ns",
            "range": "± 0.16358695044031368"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 1.2535288016001382,
            "unit": "ns",
            "range": "± 0.002446182061653941"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 0.9431574834187826,
            "unit": "ns",
            "range": "± 0.0031599312393707428"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.058384668986003,
            "unit": "ns",
            "range": "± 0.0014895581915434892"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9420823907852173,
            "unit": "ns",
            "range": "± 0.0016709056308532953"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6297277205785116,
            "unit": "ns",
            "range": "± 0.0017400752744705828"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8041804161071777,
            "unit": "ns",
            "range": "± 0.007090768028416364"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 0.9412762463887533,
            "unit": "ns",
            "range": "± 0.0009356112248331461"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.0569094429016115,
            "unit": "ns",
            "range": "± 0.0012215405230072952"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 48.58551688989004,
            "unit": "ns",
            "range": "± 0.2704397607840822"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6302671960194905,
            "unit": "ns",
            "range": "± 0.0021160464966946692"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 37.91380981604258,
            "unit": "ns",
            "range": "± 0.15086019798026157"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9410025075276692,
            "unit": "ns",
            "range": "± 0.00013202808070045054"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9417595869700115,
            "unit": "ns",
            "range": "± 0.0007187368668552006"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9994494158426921,
            "unit": "ns",
            "range": "± 0.0037958417715715023"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9437647616068521,
            "unit": "ns",
            "range": "± 0.002123847695486241"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9427815847396851,
            "unit": "ns",
            "range": "± 0.0004574847794113985"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.143391927083334,
            "unit": "ns",
            "range": "± 0.13127494087004238"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 189.83067433039346,
            "unit": "ns",
            "range": "± 1.620785533627807"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 1013.1124210357666,
            "unit": "ns",
            "range": "± 13.75557521660177"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.831653227408726,
            "unit": "ns",
            "range": "± 0.04210346969898473"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.816302383939425,
            "unit": "ns",
            "range": "± 0.014668822834733272"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.281553342938423,
            "unit": "ns",
            "range": "± 0.00945393660302672"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 33.38224641482035,
            "unit": "ns",
            "range": "± 0.014190520876386229"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 203.08764489491782,
            "unit": "ns",
            "range": "± 0.09993134513305421"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.937552637110154,
            "unit": "ns",
            "range": "± 0.0023321667310570566"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.110986818869909,
            "unit": "ns",
            "range": "± 0.004556095919973959"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0352119045952957,
            "unit": "ns",
            "range": "± 0.002358233932156506"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.214269558588663,
            "unit": "ns",
            "range": "± 0.27415068391064473"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.523274784286817,
            "unit": "ns",
            "range": "± 0.00790070582356073"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.4295164669553437,
            "unit": "ns",
            "range": "± 0.0052093133734931645"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.982579479614895,
            "unit": "ns",
            "range": "± 0.2839015922707538"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9591712901989617,
            "unit": "ns",
            "range": "± 0.014752463983578134"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 29.15164299805959,
            "unit": "ns",
            "range": "± 1.0449994427503062"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 603.5973545710245,
            "unit": "ns",
            "range": "± 4.117885860444657"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 74.72644231716792,
            "unit": "ns",
            "range": "± 1.2795817459429275"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4894.715461730957,
            "unit": "ns",
            "range": "± 80.77771804016501"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 489.6479667027791,
            "unit": "ns",
            "range": "± 14.957041643853922"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 46141.35282389323,
            "unit": "ns",
            "range": "± 502.24808013010426"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 31.461321542660396,
            "unit": "ns",
            "range": "± 1.0382564120024345"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 80.83189725875854,
            "unit": "ns",
            "range": "± 0.10418397914864146"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 43.6492872039477,
            "unit": "ns",
            "range": "± 0.24777483616230755"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 172.67656819025675,
            "unit": "ns",
            "range": "± 2.36042313214965"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 89.09914290904999,
            "unit": "ns",
            "range": "± 0.44053980315794455"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 110.56957246859868,
            "unit": "ns",
            "range": "± 1.77346032977651"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 336.8983138402303,
            "unit": "ns",
            "range": "± 6.737705514211673"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 727.2857084274292,
            "unit": "ns",
            "range": "± 12.417920433469467"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 322.63165950775146,
            "unit": "ns",
            "range": "± 8.512391232656725"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1466.5539925893147,
            "unit": "ns",
            "range": "± 15.102490694571717"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 874.0320418675741,
            "unit": "ns",
            "range": "± 1.6813215166208355"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 1008.3147417704264,
            "unit": "ns",
            "range": "± 36.647349113074426"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5166.162246704102,
            "unit": "ns",
            "range": "± 32.88337887132522"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8536.481562296549,
            "unit": "ns",
            "range": "± 107.52645642565126"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3774.203222910563,
            "unit": "ns",
            "range": "± 70.54262009931617"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 19380.706685384113,
            "unit": "ns",
            "range": "± 244.72602138637248"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 13160.452026367188,
            "unit": "ns",
            "range": "± 109.52928905332045"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7776.116737365723,
            "unit": "ns",
            "range": "± 152.2580835172194"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "f9885f73222ffa664890e2438ac3b6a19712634c",
          "message": "perf(bench): extend coverage, add nightly trend and AOT size gate\n\nAdd parsing, async, collection and equality benchmark classes plus the Sequence failure paths. Use OperationsPerInvoke for sub-nanosecond methods so no benchmark reports ZeroMeasurement. Fix the Parse_Int_Success harness input, which overflowed Int32 and silently measured the failure path.\n\nAdd the nightly benchmarks.yml workflow that merges the per-class reports and stores the time trend in gh-pages (alerts only, never blocking), and an AOT size gate in ci.yml against perf/aot-size.baseline.json. Rewrite docs/benchmarks.md around the pinned configuration and regenerate its tables with tools/Update-BenchmarkTables.ps1.",
          "timestamp": "2026-10-29T05:28:45Z",
          "url": "https://github.com/sandlada/RopSharp/commit/f9885f73222ffa664890e2438ac3b6a19712634c"
        },
        "date": 1793355590736,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 23.838274319966633,
            "unit": "ns",
            "range": "± 0.691112555996735"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 24.813932478427887,
            "unit": "ns",
            "range": "± 1.1511931616437194"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 28.533541053533554,
            "unit": "ns",
            "range": "± 0.3820853194745567"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 62.70643877983093,
            "unit": "ns",
            "range": "± 0.27966836893330216"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 34.42005052169164,
            "unit": "ns",
            "range": "± 2.7245311665801584"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 19.51591321825981,
            "unit": "ns",
            "range": "± 0.21731531676554228"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 19.616225729386013,
            "unit": "ns",
            "range": "± 1.485563050845277"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 36.574148972829185,
            "unit": "ns",
            "range": "± 0.6647630098729291"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 9.572161614894867,
            "unit": "ns",
            "range": "± 0.09901156098201266"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 47.962208499511085,
            "unit": "ns",
            "range": "± 0.7906551237234002"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 25.841817716757458,
            "unit": "ns",
            "range": "± 0.24452640753383822"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 201.9359554449717,
            "unit": "ns",
            "range": "± 3.2891212181260845"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 265.4721001784007,
            "unit": "ns",
            "range": "± 0.9008876465531488"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 9.267369662721952,
            "unit": "ns",
            "range": "± 0.11706740390250235"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 310.7036666870117,
            "unit": "ns",
            "range": "± 5.433432656561349"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 254.73243776957193,
            "unit": "ns",
            "range": "± 4.5461951946776455"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 3879.7944564819336,
            "unit": "ns",
            "range": "± 8.033355155710796"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 3149.383014678955,
            "unit": "ns",
            "range": "± 21.993978099094058"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 8.80470615128676,
            "unit": "ns",
            "range": "± 0.14922179501932875"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 4053.140080769857,
            "unit": "ns",
            "range": "± 334.65703245350903"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 4246.991714477539,
            "unit": "ns",
            "range": "± 132.2777332258105"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.027637335161368053,
            "unit": "ns",
            "range": "± 0.0035210982568261405"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.0507250955949227,
            "unit": "ns",
            "range": "± 0.08785844278919437"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.0005351311216751734,
            "unit": "ns",
            "range": "± 0.0009268742914527233"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 0.42562449537217617,
            "unit": "ns",
            "range": "± 0.060948088020791345"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 5.84842470784982,
            "unit": "ns",
            "range": "± 0.20695162068234677"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 0.5597290140787761,
            "unit": "ns",
            "range": "± 0.0017965336207198031"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 0.35387416823705037,
            "unit": "ns",
            "range": "± 0.002283713752476745"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 1.0415527483622233,
            "unit": "ns",
            "range": "± 0.004030630156932334"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.5299441248575846,
            "unit": "ns",
            "range": "± 0.007759204035445724"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.3650480297406515,
            "unit": "ns",
            "range": "± 0.0031212175087645066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.044421496073405,
            "unit": "ns",
            "range": "± 0.0156790575261382"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 0.542078119913737,
            "unit": "ns",
            "range": "± 0.0032443177079792477"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 1.0215572268168132,
            "unit": "ns",
            "range": "± 0.004109976891605797"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 22.377190341552097,
            "unit": "ns",
            "range": "± 0.12497134920449339"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.33482377529144286,
            "unit": "ns",
            "range": "± 0.0015353960673374317"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 16.819609105587006,
            "unit": "ns",
            "range": "± 0.0322023117795479"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.5005129512151082,
            "unit": "ns",
            "range": "± 0.002628413201446097"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.5261550258000692,
            "unit": "ns",
            "range": "± 0.01173811964853509"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.4658469886779785,
            "unit": "ns",
            "range": "± 0.014456545755194587"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.5672602965037028,
            "unit": "ns",
            "range": "± 0.0015792627381704283"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.6160107227961222,
            "unit": "ns",
            "range": "± 0.0020211641417398228"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 8.336261769135794,
            "unit": "ns",
            "range": "± 0.03429982085681021"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 94.27615598837535,
            "unit": "ns",
            "range": "± 1.029423721152549"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 554.7761452992758,
            "unit": "ns",
            "range": "± 14.044111229177968"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 14.779286503791809,
            "unit": "ns",
            "range": "± 0.4307199287844548"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 7.096938828627269,
            "unit": "ns",
            "range": "± 0.010345090709167509"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 4.99745795627435,
            "unit": "ns",
            "range": "± 0.015064495707082933"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 18.71384225289027,
            "unit": "ns",
            "range": "± 0.046148716377805106"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 82.13401937484741,
            "unit": "ns",
            "range": "± 3.3198119617125528"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 0.7989594961206118,
            "unit": "ns",
            "range": "± 0.004074614110675294"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 0.9049414694309235,
            "unit": "ns",
            "range": "± 0.07991465435639741"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 0.11974335896472137,
            "unit": "ns",
            "range": "± 0.003002026736847731"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 7.667987450957298,
            "unit": "ns",
            "range": "± 0.13889519163722674"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 1.0180452056229115,
            "unit": "ns",
            "range": "± 0.007116840607677165"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 0.941432865957419,
            "unit": "ns",
            "range": "± 0.006049055320304485"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 12.494578739007315,
            "unit": "ns",
            "range": "± 0.1817315000272377"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 0.9057952016592026,
            "unit": "ns",
            "range": "± 0.0063751111460304795"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 14.789269477128983,
            "unit": "ns",
            "range": "± 0.20312918020985615"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 306.77909024556476,
            "unit": "ns",
            "range": "± 0.7801926485144645"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 34.26285254955292,
            "unit": "ns",
            "range": "± 0.9590426851126401"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 2373.6722679138184,
            "unit": "ns",
            "range": "± 170.4999204592831"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 256.26896715164185,
            "unit": "ns",
            "range": "± 2.3283524833412126"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 21800.77569580078,
            "unit": "ns",
            "range": "± 108.1568527720848"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 16.45536059141159,
            "unit": "ns",
            "range": "± 0.15126428612506113"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 39.554546217123665,
            "unit": "ns",
            "range": "± 0.05519150170332746"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 22.279665450255077,
            "unit": "ns",
            "range": "± 0.14812385222547844"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 99.53216143449147,
            "unit": "ns",
            "range": "± 4.071472734915566"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 38.27235628167788,
            "unit": "ns",
            "range": "± 0.8350752049554397"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 48.39978482325872,
            "unit": "ns",
            "range": "± 0.6150212217080235"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 183.60891230901083,
            "unit": "ns",
            "range": "± 10.939845141598775"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 301.00017261505127,
            "unit": "ns",
            "range": "± 1.9945414060810733"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 161.01815740267435,
            "unit": "ns",
            "range": "± 2.4709478683288943"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 905.2725836435953,
            "unit": "ns",
            "range": "± 34.79617580387169"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 276.29268248875934,
            "unit": "ns",
            "range": "± 3.0503754619499985"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 289.26831754048663,
            "unit": "ns",
            "range": "± 2.5854538157641818"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 2762.3221956888833,
            "unit": "ns",
            "range": "± 26.096118301692705"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 3502.4082056681314,
            "unit": "ns",
            "range": "± 32.46224365455723"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 1868.9495029449463,
            "unit": "ns",
            "range": "± 15.717386714233387"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 11074.528345743814,
            "unit": "ns",
            "range": "± 241.436428424409"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 3561.111764272054,
            "unit": "ns",
            "range": "± 49.03676679201426"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 3556.7369397481284,
            "unit": "ns",
            "range": "± 6.572389068764662"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "74d09ffc531d06f4fc3e811d0692523149dbeb21",
          "message": "docs: tell one story across the documentation\n\nThe README, the Traditional Chinese guide and the XML documentation of the\nlibrary now all draw from the same order-fulfillment domain, so a reader who\nmoves between them watches one program grow instead of three unrelated ones.\n\nAdds the domain table to both guides and rewrites the examples around\nCustomer / Address / Order / OrderLine / Payment / Shipment.",
          "timestamp": "2026-10-30T15:13:02Z",
          "url": "https://github.com/sandlada/RopSharp/commit/74d09ffc531d06f4fc3e811d0692523149dbeb21"
        },
        "date": 1793440584031,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 57.411526481310524,
            "unit": "ns",
            "range": "± 0.17592612290237608"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 57.14544655879339,
            "unit": "ns",
            "range": "± 0.058986556446198664"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 59.380648831526436,
            "unit": "ns",
            "range": "± 0.26193166222058345"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 133.00798296928406,
            "unit": "ns",
            "range": "± 1.0392964454936149"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 68.36099716027577,
            "unit": "ns",
            "range": "± 0.6329706557110234"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 55.218913008769356,
            "unit": "ns",
            "range": "± 0.4314329689846359"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 40.86295348405838,
            "unit": "ns",
            "range": "± 0.09049848578630604"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 79.07918693621953,
            "unit": "ns",
            "range": "± 0.17984187625249784"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 18.390819936990738,
            "unit": "ns",
            "range": "± 0.21601420051867967"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 98.29245946804683,
            "unit": "ns",
            "range": "± 0.3084579631252445"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 53.57121741771698,
            "unit": "ns",
            "range": "± 0.21941859558380591"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 423.5891275405884,
            "unit": "ns",
            "range": "± 0.8832927723231012"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 734.5847590764364,
            "unit": "ns",
            "range": "± 10.432867151271825"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 17.523382822672527,
            "unit": "ns",
            "range": "± 0.2510481751461659"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 667.991584777832,
            "unit": "ns",
            "range": "± 12.61853926668016"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 564.65895652771,
            "unit": "ns",
            "range": "± 1.0588254039714022"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 12564.612594604492,
            "unit": "ns",
            "range": "± 81.581158291112"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 15076.8935546875,
            "unit": "ns",
            "range": "± 16.088102818907483"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 17.989159514506657,
            "unit": "ns",
            "range": "± 0.018400656408999013"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 8482.169357299805,
            "unit": "ns",
            "range": "± 96.104672269931"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 8442.175354003906,
            "unit": "ns",
            "range": "± 44.61686462250877"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.7166044476131598,
            "unit": "ns",
            "range": "± 0.002085764924897923"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6156288608908653,
            "unit": "ns",
            "range": "± 0.0033640925926072397"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.0022339516629775367,
            "unit": "ns",
            "range": "± 0.003869317781930078"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2275805957615376,
            "unit": "ns",
            "range": "± 0.004400398870284232"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 10.989756519595781,
            "unit": "ns",
            "range": "± 0.2926224122776546"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 1.2518688735961916,
            "unit": "ns",
            "range": "± 0.0017832965201723312"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 0.9411438032786051,
            "unit": "ns",
            "range": "± 0.0019375125663551342"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.05914758682251,
            "unit": "ns",
            "range": "± 0.0030944528165860736"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9428798850377401,
            "unit": "ns",
            "range": "± 0.0021147227911266337"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6291898247400919,
            "unit": "ns",
            "range": "± 0.00014457840000500248"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.800738260269165,
            "unit": "ns",
            "range": "± 0.00006615674152251244"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 0.9420476090113321,
            "unit": "ns",
            "range": "± 0.001490504740442218"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.0595475959777834,
            "unit": "ns",
            "range": "± 0.0005680494815879271"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 51.307113468647,
            "unit": "ns",
            "range": "± 0.281317618138079"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.629125465075175,
            "unit": "ns",
            "range": "± 0.0005714676213898859"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.80715799331665,
            "unit": "ns",
            "range": "± 0.2687596436221811"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9417211249669393,
            "unit": "ns",
            "range": "± 0.00047546148945387076"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9425617688496907,
            "unit": "ns",
            "range": "± 0.001146266008016907"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9444644533793132,
            "unit": "ns",
            "range": "± 0.0015868346703631479"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9437002607981365,
            "unit": "ns",
            "range": "± 0.0024683312482628988"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9411774400075276,
            "unit": "ns",
            "range": "± 0.0007529340998840511"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.243328183889389,
            "unit": "ns",
            "range": "± 0.07989681876655748"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 192.35455703735352,
            "unit": "ns",
            "range": "± 3.2802070002913637"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 931.6897207895914,
            "unit": "ns",
            "range": "± 27.24480052493848"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.76061902443568,
            "unit": "ns",
            "range": "± 0.005399596994464976"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.8293148179849,
            "unit": "ns",
            "range": "± 0.015913038039744514"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.28889008363088,
            "unit": "ns",
            "range": "± 0.005791372780516204"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 34.90771643320719,
            "unit": "ns",
            "range": "± 0.43810630696244396"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 197.9949063460032,
            "unit": "ns",
            "range": "± 0.05381537427134641"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9924321733415127,
            "unit": "ns",
            "range": "± 0.008494403904500888"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.1061266288161278,
            "unit": "ns",
            "range": "± 0.0015902111421463698"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0363529734313488,
            "unit": "ns",
            "range": "± 0.004485379769210845"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.54420980811119,
            "unit": "ns",
            "range": "± 0.21058371478231935"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.548730418086052,
            "unit": "ns",
            "range": "± 0.004162436738920127"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.429770683248838,
            "unit": "ns",
            "range": "± 0.000792752965916336"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 25.56095637877782,
            "unit": "ns",
            "range": "± 0.018046178840364593"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.961929346124331,
            "unit": "ns",
            "range": "± 0.01823727609075156"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 31.02854307492574,
            "unit": "ns",
            "range": "± 0.19852904541834362"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 613.2732566197714,
            "unit": "ns",
            "range": "± 1.7754572878145982"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 74.40758176644643,
            "unit": "ns",
            "range": "± 1.5624511700195645"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4796.174168904622,
            "unit": "ns",
            "range": "± 12.184806858257529"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 505.3783178329468,
            "unit": "ns",
            "range": "± 11.792374789720698"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 46408.36796061198,
            "unit": "ns",
            "range": "± 156.3652045750582"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 31.194257020950317,
            "unit": "ns",
            "range": "± 0.22249011660160004"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 77.98930943012238,
            "unit": "ns",
            "range": "± 0.3445007132905662"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 44.38925984501839,
            "unit": "ns",
            "range": "± 0.13870129063965236"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 170.46828134854636,
            "unit": "ns",
            "range": "± 1.2103873673079606"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 92.89566274483998,
            "unit": "ns",
            "range": "± 0.21518271543991965"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 109.56425034999847,
            "unit": "ns",
            "range": "± 1.6562601909759715"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 339.64717022577923,
            "unit": "ns",
            "range": "± 2.521870267855781"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 663.9977321624756,
            "unit": "ns",
            "range": "± 4.762068408347291"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 314.4472001393636,
            "unit": "ns",
            "range": "± 1.5246289383329013"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1454.0134741465251,
            "unit": "ns",
            "range": "± 15.320370499478932"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 869.7316939036051,
            "unit": "ns",
            "range": "± 4.376868313870743"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 976.1665331522623,
            "unit": "ns",
            "range": "± 22.41530549380322"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5199.265920003255,
            "unit": "ns",
            "range": "± 46.6941214577506"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8506.872767130533,
            "unit": "ns",
            "range": "± 94.26171328362373"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3793.4585240681968,
            "unit": "ns",
            "range": "± 61.12679972945179"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 19771.892832438152,
            "unit": "ns",
            "range": "± 123.6181896900483"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 13098.832041422525,
            "unit": "ns",
            "range": "± 48.382605658125776"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 8081.027608235677,
            "unit": "ns",
            "range": "± 90.08013025498202"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ea8c798cca9409405a338c0398215d9866539fc2",
          "message": "ci: use absolute test project path for .NET 11 RC1 dotnet test bug",
          "timestamp": "2026-10-31T13:37:14Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ea8c798cca9409405a338c0398215d9866539fc2"
        },
        "date": 1793529989307,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 53.39532574017843,
            "unit": "ns",
            "range": "± 0.8636063212484917"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 53.4691558877627,
            "unit": "ns",
            "range": "± 0.6000003754399502"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 54.65043660004934,
            "unit": "ns",
            "range": "± 0.6940850291606385"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 120.62811636924744,
            "unit": "ns",
            "range": "± 0.6555082556183763"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 65.25364402929942,
            "unit": "ns",
            "range": "± 0.43223584336689486"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 56.02248881260554,
            "unit": "ns",
            "range": "± 0.4855658976856968"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 38.1065908074379,
            "unit": "ns",
            "range": "± 0.4408143030631143"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 70.5884860754013,
            "unit": "ns",
            "range": "± 0.567775738271055"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 15.765376965204874,
            "unit": "ns",
            "range": "± 0.17998792075671136"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 89.00693047046661,
            "unit": "ns",
            "range": "± 2.472699595633912"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 49.48313906788826,
            "unit": "ns",
            "range": "± 0.9617955087664107"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 388.2526698112488,
            "unit": "ns",
            "range": "± 1.19541795973402"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 689.116099357605,
            "unit": "ns",
            "range": "± 5.144403437595128"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.908711155255636,
            "unit": "ns",
            "range": "± 0.06372405778407916"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 595.0168463389078,
            "unit": "ns",
            "range": "± 6.775887601248373"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 533.3913358052572,
            "unit": "ns",
            "range": "± 20.42683693000204"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5360.542182922363,
            "unit": "ns",
            "range": "± 27.319430546872656"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13875.726058959961,
            "unit": "ns",
            "range": "± 72.17462272616557"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.993783404429754,
            "unit": "ns",
            "range": "± 0.16485502262937296"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7563.724067687988,
            "unit": "ns",
            "range": "± 33.436905134402814"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7127.046708424886,
            "unit": "ns",
            "range": "± 24.945433301456955"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 1.074472786237796,
            "unit": "ns",
            "range": "± 0.010093270255311176"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6223613359034061,
            "unit": "ns",
            "range": "± 0.0021980672907169862"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0,
            "unit": "ns",
            "range": "± 0"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2209569637974103,
            "unit": "ns",
            "range": "± 0.0018709570048651221"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.741802260279655,
            "unit": "ns",
            "range": "± 0.03565290272787156"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 0.9413104467391967,
            "unit": "ns",
            "range": "± 0.000485029708478441"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 0.9423578271865845,
            "unit": "ns",
            "range": "± 0.003053451022596899"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.058821898142497,
            "unit": "ns",
            "range": "± 0.0002707635439737491"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9487660442988078,
            "unit": "ns",
            "range": "± 0.012909944405662326"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6366363910039267,
            "unit": "ns",
            "range": "± 0.013754671562434383"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8009896545410156,
            "unit": "ns",
            "range": "± 0.0017403975838417307"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 1.254148235321045,
            "unit": "ns",
            "range": "± 0.0006254860509341735"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.0576117553710938,
            "unit": "ns",
            "range": "± 0.001482566619889469"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 49.52294021844864,
            "unit": "ns",
            "range": "± 0.6770510097187461"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6304496218363445,
            "unit": "ns",
            "range": "± 0.0008541740185464495"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.33631400267283,
            "unit": "ns",
            "range": "± 0.12033786843064666"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9418200836181642,
            "unit": "ns",
            "range": "± 0.00030299260839911914"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9407941427230835,
            "unit": "ns",
            "range": "± 0.00045905017409376796"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 1.008095932006836,
            "unit": "ns",
            "range": "± 0.005954410910405449"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9441591812769573,
            "unit": "ns",
            "range": "± 0.0014058904905239974"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9435990416208903,
            "unit": "ns",
            "range": "± 0.00246623679700482"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.272509654362997,
            "unit": "ns",
            "range": "± 0.014663740557870134"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 194.5835858186086,
            "unit": "ns",
            "range": "± 0.9146572505235008"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 867.3200629552206,
            "unit": "ns",
            "range": "± 4.346086356690601"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.775067667166393,
            "unit": "ns",
            "range": "± 0.007998076804789256"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 13.008835226297379,
            "unit": "ns",
            "range": "± 0.2947872721654057"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.315903559327126,
            "unit": "ns",
            "range": "± 0.044780095127917655"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 36.41017452875773,
            "unit": "ns",
            "range": "± 0.08527475855239353"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 194.2266949812571,
            "unit": "ns",
            "range": "± 0.2179628711084587"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9044220969080925,
            "unit": "ns",
            "range": "± 0.0046531412676269965"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.1081134378910065,
            "unit": "ns",
            "range": "± 0.001756165949267814"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0369351717333,
            "unit": "ns",
            "range": "± 0.00455034336321433"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 11.86695040265719,
            "unit": "ns",
            "range": "± 0.16851166648653076"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.61582009245952,
            "unit": "ns",
            "range": "± 0.09306759752986453"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.450846624871095,
            "unit": "ns",
            "range": "± 0.02026269815883709"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 23.280318548281986,
            "unit": "ns",
            "range": "± 0.304576898252409"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9501007546981177,
            "unit": "ns",
            "range": "± 0.018428813962821505"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 28.062017659346264,
            "unit": "ns",
            "range": "± 0.08471973635661803"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 589.7030541102091,
            "unit": "ns",
            "range": "± 2.7200035714690562"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 73.75759987036388,
            "unit": "ns",
            "range": "± 0.718968503622391"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4829.7366282145185,
            "unit": "ns",
            "range": "± 2.470892026270004"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 481.04445870717365,
            "unit": "ns",
            "range": "± 18.762704843089892"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 46810.45371500651,
            "unit": "ns",
            "range": "± 880.8150956934455"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 39.494283537069954,
            "unit": "ns",
            "range": "± 0.8223675882223721"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 78.77284723520279,
            "unit": "ns",
            "range": "± 1.7223066239321339"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 43.03443465630213,
            "unit": "ns",
            "range": "± 0.16209119242689313"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 163.04675229390463,
            "unit": "ns",
            "range": "± 1.3921139156417774"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 104.11578464508057,
            "unit": "ns",
            "range": "± 1.931612859212035"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 110.11068079868953,
            "unit": "ns",
            "range": "± 5.2858658205870235"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 324.0589640935262,
            "unit": "ns",
            "range": "± 1.9068283882603478"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 634.2144638697306,
            "unit": "ns",
            "range": "± 4.975032225833461"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 303.71677017211914,
            "unit": "ns",
            "range": "± 0.7226642613574066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1395.4817212422688,
            "unit": "ns",
            "range": "± 3.868117530927861"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 832.734117825826,
            "unit": "ns",
            "range": "± 2.153973216656869"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 891.812923113505,
            "unit": "ns",
            "range": "± 20.566237306201273"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5040.732213338216,
            "unit": "ns",
            "range": "± 21.438143206469444"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 7125.200838724772,
            "unit": "ns",
            "range": "± 66.32164607023772"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3674.730037689209,
            "unit": "ns",
            "range": "± 27.7614196051795"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 18663.439717610676,
            "unit": "ns",
            "range": "± 206.83789814303736"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12422.43951924642,
            "unit": "ns",
            "range": "± 72.4336018612141"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7623.4619216918945,
            "unit": "ns",
            "range": "± 30.210009965414436"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ea8c798cca9409405a338c0398215d9866539fc2",
          "message": "ci: use absolute test project path for .NET 11 RC1 dotnet test bug",
          "timestamp": "2026-10-31T13:37:14Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ea8c798cca9409405a338c0398215d9866539fc2"
        },
        "date": 1793619085353,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 51.003199607133865,
            "unit": "ns",
            "range": "± 0.28481380802404044"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 50.820536375045776,
            "unit": "ns",
            "range": "± 0.23169653251663974"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 51.22982563575109,
            "unit": "ns",
            "range": "± 0.31170274903439066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 120.09133807818095,
            "unit": "ns",
            "range": "± 1.1338950652398792"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 60.132893900076546,
            "unit": "ns",
            "range": "± 0.05277472487986043"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 53.64909758170446,
            "unit": "ns",
            "range": "± 0.11646115130651616"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 36.874091486136116,
            "unit": "ns",
            "range": "± 0.06278562244830907"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 86.47111252943675,
            "unit": "ns",
            "range": "± 0.2073929358077643"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 15.560778051614761,
            "unit": "ns",
            "range": "± 0.013166611400341052"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 91.36470333735149,
            "unit": "ns",
            "range": "± 0.35074255241639457"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 48.420973151922226,
            "unit": "ns",
            "range": "± 0.12977325366714845"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 381.92256784439087,
            "unit": "ns",
            "range": "± 1.0083161023065035"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 675.951943397522,
            "unit": "ns",
            "range": "± 5.919181588859942"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.297484755516052,
            "unit": "ns",
            "range": "± 0.03214478538027449"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 586.0542624791464,
            "unit": "ns",
            "range": "± 1.2963761832159182"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 520.7141660054525,
            "unit": "ns",
            "range": "± 4.513821625099528"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5312.396344502767,
            "unit": "ns",
            "range": "± 13.466438793071646"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13886.28982035319,
            "unit": "ns",
            "range": "± 156.74277168860203"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 16.087874521811802,
            "unit": "ns",
            "range": "± 0.17212106332384997"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7550.159619649251,
            "unit": "ns",
            "range": "± 56.47183512987158"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7017.8683522542315,
            "unit": "ns",
            "range": "± 3.117679632840333"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.7444656267762184,
            "unit": "ns",
            "range": "± 0.0067721171037705905"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.5913888290524483,
            "unit": "ns",
            "range": "± 0.011438564810024896"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0,
            "unit": "ns",
            "range": "± 0"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.22214964342614,
            "unit": "ns",
            "range": "± 0.002140475239755478"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.473930378754934,
            "unit": "ns",
            "range": "± 0.04226363319733222"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 0.9463590459823609,
            "unit": "ns",
            "range": "± 0.00925808313947071"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 0.9420515209833781,
            "unit": "ns",
            "range": "± 0.0021617962457473484"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.126968746185303,
            "unit": "ns",
            "range": "± 0.05074395146405564"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9415242757797241,
            "unit": "ns",
            "range": "± 0.0025029440848645806"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6290657154719035,
            "unit": "ns",
            "range": "± 0.0005325662529541153"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8028180669148763,
            "unit": "ns",
            "range": "± 0.003059505165459038"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 1.251061679204305,
            "unit": "ns",
            "range": "± 0.00023819916298497266"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.0604800809224444,
            "unit": "ns",
            "range": "± 0.006778044385139473"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 47.75844669342041,
            "unit": "ns",
            "range": "± 0.3977999316455575"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.628793054262797,
            "unit": "ns",
            "range": "± 0.00017796172773572666"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 37.29714252551397,
            "unit": "ns",
            "range": "± 0.2218290438855713"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9407647596995036,
            "unit": "ns",
            "range": "± 0.00038118821330233914"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9435698877970378,
            "unit": "ns",
            "range": "± 0.0014254867297913726"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9527519257863363,
            "unit": "ns",
            "range": "± 0.0017964546763450618"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9426726977030436,
            "unit": "ns",
            "range": "± 0.00017965210166290602"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9410468537012736,
            "unit": "ns",
            "range": "± 0.00022187008385250755"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.181321233510971,
            "unit": "ns",
            "range": "± 0.020516963249052403"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 182.65744638442993,
            "unit": "ns",
            "range": "± 0.2202071595858343"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 981.3711738586426,
            "unit": "ns",
            "range": "± 1.178714870682404"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.78724017739296,
            "unit": "ns",
            "range": "± 0.004302194636066123"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.893292923768362,
            "unit": "ns",
            "range": "± 0.2210785805384287"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.31036508580049,
            "unit": "ns",
            "range": "± 0.05817858466439425"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 36.4081272482872,
            "unit": "ns",
            "range": "± 0.07083290564296235"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 195.69449162483215,
            "unit": "ns",
            "range": "± 0.3006498930609604"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.6255162215481203,
            "unit": "ns",
            "range": "± 0.0016365915781124938"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.107974983751774,
            "unit": "ns",
            "range": "± 0.001061263210661575"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0418785971899827,
            "unit": "ns",
            "range": "± 0.013469865547142953"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 11.66701877117157,
            "unit": "ns",
            "range": "± 0.04787283472799295"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.5486214756965637,
            "unit": "ns",
            "range": "± 0.0058914681672586405"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.4269718527793884,
            "unit": "ns",
            "range": "± 0.010680302620847947"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 21.960987736781437,
            "unit": "ns",
            "range": "± 0.010200749141165557"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.96039300908645,
            "unit": "ns",
            "range": "± 0.008965380321062538"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 27.119082033634186,
            "unit": "ns",
            "range": "± 0.07505591545945499"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 591.36115137736,
            "unit": "ns",
            "range": "± 0.8319695570368251"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 69.76054207483928,
            "unit": "ns",
            "range": "± 0.3974137811942158"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4722.031239827474,
            "unit": "ns",
            "range": "± 34.405132146567794"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 473.90103308359784,
            "unit": "ns",
            "range": "± 12.716063851348329"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 44055.889628092445,
            "unit": "ns",
            "range": "± 87.02846258931532"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 29.824613243341446,
            "unit": "ns",
            "range": "± 0.30900990644312254"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 75.53342101971309,
            "unit": "ns",
            "range": "± 0.0721014832916551"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 41.62956032156944,
            "unit": "ns",
            "range": "± 0.14086953895232265"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 163.77023768424988,
            "unit": "ns",
            "range": "± 1.1203056346636513"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 86.65617209672928,
            "unit": "ns",
            "range": "± 0.011446058144120958"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 104.96429872512817,
            "unit": "ns",
            "range": "± 0.40749356589055385"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 317.7134041786194,
            "unit": "ns",
            "range": "± 0.3184507891267708"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 636.6638609568278,
            "unit": "ns",
            "range": "± 4.043287033302462"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 304.9134985605876,
            "unit": "ns",
            "range": "± 1.8786808054543238"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1398.1017678578694,
            "unit": "ns",
            "range": "± 4.190711127420778"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 828.9444739023844,
            "unit": "ns",
            "range": "± 1.0486407682486352"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 931.2226524353027,
            "unit": "ns",
            "range": "± 12.771266722519467"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5017.176719665527,
            "unit": "ns",
            "range": "± 7.901326158347286"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 7169.6479415893555,
            "unit": "ns",
            "range": "± 138.44194200245744"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3652.540719350179,
            "unit": "ns",
            "range": "± 20.9560990318465"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 18915.656656901043,
            "unit": "ns",
            "range": "± 69.31771424034567"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12353.23324584961,
            "unit": "ns",
            "range": "± 70.91633066535196"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7705.35533396403,
            "unit": "ns",
            "range": "± 52.514600334196615"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1793704213780,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 62.81751571098963,
            "unit": "ns",
            "range": "± 4.5058164625397685"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1988.2287317911785,
            "unit": "ns",
            "range": "± 63.87469365299485"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 68.25880044698715,
            "unit": "ns",
            "range": "± 2.8469904113682083"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 1957.6856486002605,
            "unit": "ns",
            "range": "± 33.062296151910154"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 90.81351900100708,
            "unit": "ns",
            "range": "± 5.6826866372357605"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 59.33233012755712,
            "unit": "ns",
            "range": "± 2.3531485635149516"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 40.372894525527954,
            "unit": "ns",
            "range": "± 1.0841708083021377"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 78.28582654396693,
            "unit": "ns",
            "range": "± 2.4343407373525734"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 16.287677437067032,
            "unit": "ns",
            "range": "± 0.3494011701746476"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 89.540674050649,
            "unit": "ns",
            "range": "± 0.700697637026879"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 50.50972239176432,
            "unit": "ns",
            "range": "± 0.3278724461605761"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 393.92079973220825,
            "unit": "ns",
            "range": "± 1.9293494653523222"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 720.7843481699625,
            "unit": "ns",
            "range": "± 2.5805504146736307"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 17.647053748369217,
            "unit": "ns",
            "range": "± 0.26890020284693"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 665.5513734817505,
            "unit": "ns",
            "range": "± 7.246551444606682"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 556.7333354949951,
            "unit": "ns",
            "range": "± 1.6619687973479973"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5904.3081792195635,
            "unit": "ns",
            "range": "± 44.75794561187327"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 14356.430608113607,
            "unit": "ns",
            "range": "± 87.321285507318"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 16.863640407721203,
            "unit": "ns",
            "range": "± 0.13535889407744076"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 8084.2594807942705,
            "unit": "ns",
            "range": "± 200.77797534683387"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7575.777681986491,
            "unit": "ns",
            "range": "± 154.131685396444"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8963633415599664,
            "unit": "ns",
            "range": "± 0.0007620233947479404"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.7017545873920122,
            "unit": "ns",
            "range": "± 0.010192345595833981"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.3467904676993688,
            "unit": "ns",
            "range": "± 0.0011924690731654984"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2160028082629044,
            "unit": "ns",
            "range": "± 0.004721484676652664"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.906550973653793,
            "unit": "ns",
            "range": "± 0.08739652450637346"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6173891588846843,
            "unit": "ns",
            "range": "± 0.0007226079322387199"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.506166426340739,
            "unit": "ns",
            "range": "± 0.007645946300789364"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8561609204610185,
            "unit": "ns",
            "range": "± 0.0027598081670611938"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9454120229085287,
            "unit": "ns",
            "range": "± 0.008260763788145975"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6286954129536947,
            "unit": "ns",
            "range": "± 0.0005026256548910599"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.814818047205607,
            "unit": "ns",
            "range": "± 0.017895661051059778"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.311954002380371,
            "unit": "ns",
            "range": "± 0.0006174700889135028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.854267239888509,
            "unit": "ns",
            "range": "± 0.004741616602393429"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 54.34644595781962,
            "unit": "ns",
            "range": "± 0.17751467305595028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6296887874603272,
            "unit": "ns",
            "range": "± 0.0008338723396009883"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 40.1812489827474,
            "unit": "ns",
            "range": "± 0.3209400344675232"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9428783032099406,
            "unit": "ns",
            "range": "± 0.00031672861642790504"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9434626623789469,
            "unit": "ns",
            "range": "± 0.00033943659481081305"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 1.0056833826700846,
            "unit": "ns",
            "range": "± 0.0006265832496270073"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9515939203898113,
            "unit": "ns",
            "range": "± 0.011733830608873078"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9431758607228597,
            "unit": "ns",
            "range": "± 0.0005627183952136805"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.301535298426947,
            "unit": "ns",
            "range": "± 0.03170734557744453"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 199.67378131548563,
            "unit": "ns",
            "range": "± 2.0199613254257223"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 906.7972561518351,
            "unit": "ns",
            "range": "± 2.6339150659710144"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 23.01625946164131,
            "unit": "ns",
            "range": "± 0.007126497383316648"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.947035565972328,
            "unit": "ns",
            "range": "± 0.18682822897224383"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.289122278491655,
            "unit": "ns",
            "range": "± 0.018901934966328346"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 35.0614347755909,
            "unit": "ns",
            "range": "± 0.28444876911854966"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 194.48087541262308,
            "unit": "ns",
            "range": "± 0.26663018822783924"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9986423961818218,
            "unit": "ns",
            "range": "± 0.004288100601303332"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.1822900474071503,
            "unit": "ns",
            "range": "± 0.1355150727611423"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.073849589874347,
            "unit": "ns",
            "range": "± 0.002077943588126086"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 13.088717977205912,
            "unit": "ns",
            "range": "± 0.15704397993847025"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.551948015888532,
            "unit": "ns",
            "range": "± 0.004710930051976334"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 3.081108512977759,
            "unit": "ns",
            "range": "± 0.0029796347864168337"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 24.198814660310745,
            "unit": "ns",
            "range": "± 0.4467117689608327"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9524129604299865,
            "unit": "ns",
            "range": "± 0.005109937385323899"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 27.98376526435216,
            "unit": "ns",
            "range": "± 0.25347463375199897"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 630.3884798685709,
            "unit": "ns",
            "range": "± 2.006486897735274"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 82.28829034169515,
            "unit": "ns",
            "range": "± 5.0956621062872225"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 5026.992785135905,
            "unit": "ns",
            "range": "± 50.92413208835265"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 525.9193735122681,
            "unit": "ns",
            "range": "± 39.814080783925746"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 45237.464935302734,
            "unit": "ns",
            "range": "± 208.99266731886723"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 31.613499114910763,
            "unit": "ns",
            "range": "± 1.0048011959041985"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 80.39519898096721,
            "unit": "ns",
            "range": "± 1.772259388355626"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 43.08463376760483,
            "unit": "ns",
            "range": "± 0.05723134627143297"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 227.1952010790507,
            "unit": "ns",
            "range": "± 1.587043616695401"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 100.29483012358348,
            "unit": "ns",
            "range": "± 0.9173722431081808"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 111.64969861507416,
            "unit": "ns",
            "range": "± 2.390991867876732"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 333.03066698710126,
            "unit": "ns",
            "range": "± 3.48049169662835"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 663.3495979309082,
            "unit": "ns",
            "range": "± 35.881640905347176"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 310.94765027364093,
            "unit": "ns",
            "range": "± 4.596619759309746"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2278.394301096598,
            "unit": "ns",
            "range": "± 23.258530527259154"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 845.1697050730387,
            "unit": "ns",
            "range": "± 5.261337529164992"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 981.8976364135742,
            "unit": "ns",
            "range": "± 34.677929681905816"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5100.612002054851,
            "unit": "ns",
            "range": "± 13.81929153569246"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8430.524709065756,
            "unit": "ns",
            "range": "± 107.96493873567162"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3654.9826850891113,
            "unit": "ns",
            "range": "± 54.172817306783394"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 32430.077748616535,
            "unit": "ns",
            "range": "± 233.73806835480508"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12943.881790161133,
            "unit": "ns",
            "range": "± 88.55527014522148"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 8389.955378214518,
            "unit": "ns",
            "range": "± 148.96150157952812"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1793790891935,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 53.340413788954415,
            "unit": "ns",
            "range": "± 0.2770853425833415"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1507.3314107259114,
            "unit": "ns",
            "range": "± 8.408503545959135"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 54.32537862658501,
            "unit": "ns",
            "range": "± 0.264283838763659"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 1612.1131273905437,
            "unit": "ns",
            "range": "± 15.32686334975527"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 63.11577391624451,
            "unit": "ns",
            "range": "± 0.5968389100616371"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 46.21843893329302,
            "unit": "ns",
            "range": "± 2.6056632149421053"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 27.55961763858795,
            "unit": "ns",
            "range": "± 0.23306400291011792"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 55.833537608385086,
            "unit": "ns",
            "range": "± 0.21024005217782246"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 12.931230266888937,
            "unit": "ns",
            "range": "± 0.11095674241054966"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 68.86304314931233,
            "unit": "ns",
            "range": "± 0.22521625690823444"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 41.02922355135282,
            "unit": "ns",
            "range": "± 0.560568098054971"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 303.86168177922565,
            "unit": "ns",
            "range": "± 8.544344024491329"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 449.5176881154378,
            "unit": "ns",
            "range": "± 13.586896976246496"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 13.185236106316248,
            "unit": "ns",
            "range": "± 0.13058121542304554"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 571.3859004974365,
            "unit": "ns",
            "range": "± 1.189686221346711"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 460.2558798789978,
            "unit": "ns",
            "range": "± 2.0799445076530256"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 4579.144287109375,
            "unit": "ns",
            "range": "± 29.851435931209625"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 6033.736844380696,
            "unit": "ns",
            "range": "± 83.98648006456578"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 13.329877038796743,
            "unit": "ns",
            "range": "± 0.18296009293447335"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 6124.37494913737,
            "unit": "ns",
            "range": "± 180.51352577678807"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 5737.121686299642,
            "unit": "ns",
            "range": "± 72.1940686434101"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.4992275610566139,
            "unit": "ns",
            "range": "± 0.001989384455321404"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.3988211912413438,
            "unit": "ns",
            "range": "± 0.012288456521064441"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.41498103737831116,
            "unit": "ns",
            "range": "± 0.012060522399758423"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 0.9951528608798981,
            "unit": "ns",
            "range": "± 0.002609027276004444"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 8.983535880843798,
            "unit": "ns",
            "range": "± 0.09126697068804066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.23315647761027,
            "unit": "ns",
            "range": "± 0.022669364001284952"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 1.9178418324788413,
            "unit": "ns",
            "range": "± 0.0021554353106190993"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.5131204795837405,
            "unit": "ns",
            "range": "± 0.019970960382643975"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.7863832724889118,
            "unit": "ns",
            "range": "± 0.0006020253082957171"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.5505852044423422,
            "unit": "ns",
            "range": "± 0.00024407652694156577"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.5863537012736002,
            "unit": "ns",
            "range": "± 0.004356802383157857"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 2.360436383565267,
            "unit": "ns",
            "range": "± 0.0034468755583259127"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.463622191111247,
            "unit": "ns",
            "range": "± 0.0022103977779879755"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 41.033153623342514,
            "unit": "ns",
            "range": "± 0.19941260131194247"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.5504889446894328,
            "unit": "ns",
            "range": "± 0.000610232879442707"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 33.090738167365394,
            "unit": "ns",
            "range": "± 0.26343158799630784"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.7359536774953206,
            "unit": "ns",
            "range": "± 0.001158531577523047"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.7358309806187947,
            "unit": "ns",
            "range": "± 0.00020233160401479468"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.7739991057713826,
            "unit": "ns",
            "range": "± 0.00040234988645609514"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.8296838881174723,
            "unit": "ns",
            "range": "± 0.0022999259558984493"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.825575450261434,
            "unit": "ns",
            "range": "± 0.00019161121669902296"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 12.982086350520452,
            "unit": "ns",
            "range": "± 0.034592179650644546"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 155.47623459498087,
            "unit": "ns",
            "range": "± 2.395507413241404"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 796.9523185094198,
            "unit": "ns",
            "range": "± 1.801665956165346"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 18.538435121377308,
            "unit": "ns",
            "range": "± 0.007788400922348834"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 11.584159309665361,
            "unit": "ns",
            "range": "± 0.01218539164916609"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 8.180425514777502,
            "unit": "ns",
            "range": "± 0.04622039574840131"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 25.19770661989848,
            "unit": "ns",
            "range": "± 0.013660033292478644"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 121.68635574976604,
            "unit": "ns",
            "range": "± 0.134656836374574"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.4444560122986634,
            "unit": "ns",
            "range": "± 0.009406750767833275"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 1.612096435079972,
            "unit": "ns",
            "range": "± 0.009541531929120592"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 0.7444589026272297,
            "unit": "ns",
            "range": "± 0.003920466191597753"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 10.50364171465238,
            "unit": "ns",
            "range": "± 0.03078123606478025"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.08288266758124,
            "unit": "ns",
            "range": "± 0.002706638776024504"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.5186854153871536,
            "unit": "ns",
            "range": "± 0.017657965408253085"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 17.514614860216778,
            "unit": "ns",
            "range": "± 0.08960020557439219"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 1.5310523087779682,
            "unit": "ns",
            "range": "± 0.07900799822547123"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 19.57153332233429,
            "unit": "ns",
            "range": "± 0.08066782619146035"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 422.97038888931274,
            "unit": "ns",
            "range": "± 0.9066334629338075"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 53.053266694148384,
            "unit": "ns",
            "range": "± 0.8616907904580937"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 3458.757919947306,
            "unit": "ns",
            "range": "± 123.52693202135231"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 407.91245635350543,
            "unit": "ns",
            "range": "± 8.29127151844521"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 30644.253326416016,
            "unit": "ns",
            "range": "± 326.30367052067095"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 30.074920217196148,
            "unit": "ns",
            "range": "± 0.024782242527326075"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 63.45914556582769,
            "unit": "ns",
            "range": "± 0.165262624825141"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 33.314432402451835,
            "unit": "ns",
            "range": "± 0.03662457625714622"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 177.65829737981161,
            "unit": "ns",
            "range": "± 2.8723557124725216"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 63.15213495492935,
            "unit": "ns",
            "range": "± 0.21633441666240474"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 66.16204317410786,
            "unit": "ns",
            "range": "± 0.5240534056394288"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 276.2688051859538,
            "unit": "ns",
            "range": "± 0.9344227952990203"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 526.686645825704,
            "unit": "ns",
            "range": "± 7.13149657558139"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 249.72488371531168,
            "unit": "ns",
            "range": "± 3.058806965997969"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1950.855977376302,
            "unit": "ns",
            "range": "± 69.36665605656655"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 407.6506878534953,
            "unit": "ns",
            "range": "± 5.2966853045645585"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 420.76614570617676,
            "unit": "ns",
            "range": "± 3.9064670085734403"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 4391.558870951335,
            "unit": "ns",
            "range": "± 59.10933444618064"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 6062.658381144206,
            "unit": "ns",
            "range": "± 102.19429247701476"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3372.625082651774,
            "unit": "ns",
            "range": "± 150.74656921722206"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 29573.045288085938,
            "unit": "ns",
            "range": "± 90.87694717653594"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 5693.878196716309,
            "unit": "ns",
            "range": "± 110.47065340282623"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 6314.827105204265,
            "unit": "ns",
            "range": "± 199.22135502365103"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1793877280338,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 58.27194823821386,
            "unit": "ns",
            "range": "± 0.4464393096637548"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1908.375576019287,
            "unit": "ns",
            "range": "± 62.02904701409994"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 70.98914072910945,
            "unit": "ns",
            "range": "± 5.661712525046869"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2053.770907084147,
            "unit": "ns",
            "range": "± 24.721499987991475"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 91.63132667541504,
            "unit": "ns",
            "range": "± 2.534327314172263"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 58.2551828622818,
            "unit": "ns",
            "range": "± 0.7540102219328433"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 59.12492301066717,
            "unit": "ns",
            "range": "± 0.11092311118521804"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 76.32336201270421,
            "unit": "ns",
            "range": "± 2.6193598918847316"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 16.34808298945427,
            "unit": "ns",
            "range": "± 0.2468124094639308"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 91.74288020531337,
            "unit": "ns",
            "range": "± 0.7881127493096277"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 50.81613420446714,
            "unit": "ns",
            "range": "± 0.8013801827410049"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 398.36279884974164,
            "unit": "ns",
            "range": "± 4.797892767139856"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 698.7649685541788,
            "unit": "ns",
            "range": "± 2.2988725195545725"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 16.20976796746254,
            "unit": "ns",
            "range": "± 0.22595056619122675"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 623.0234562555949,
            "unit": "ns",
            "range": "± 16.582608348100727"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 542.7135155995687,
            "unit": "ns",
            "range": "± 5.206689512078181"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5519.565745035808,
            "unit": "ns",
            "range": "± 33.24411141128238"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 14196.459543863932,
            "unit": "ns",
            "range": "± 502.88658464776665"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 16.256294926007588,
            "unit": "ns",
            "range": "± 0.21779732123076956"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7883.22482808431,
            "unit": "ns",
            "range": "± 169.68944247424858"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7237.786331176758,
            "unit": "ns",
            "range": "± 48.94373321461787"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8935663575927416,
            "unit": "ns",
            "range": "± 0.002002193588469555"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.681989062577486,
            "unit": "ns",
            "range": "± 0.0031787382083824844"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.35627039273579914,
            "unit": "ns",
            "range": "± 0.011756811072304415"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2060150367518265,
            "unit": "ns",
            "range": "± 0.0019356031840720423"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.881617223223051,
            "unit": "ns",
            "range": "± 0.12677281735624285"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.632272221883138,
            "unit": "ns",
            "range": "± 0.03030732221842241"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.4971841278076172,
            "unit": "ns",
            "range": "± 0.0026437722729665144"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8463962338765456,
            "unit": "ns",
            "range": "± 0.012165758976322478"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9392362731297811,
            "unit": "ns",
            "range": "± 0.0005393030701309926"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6300431311925253,
            "unit": "ns",
            "range": "± 0.001928496278024604"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8031975218454999,
            "unit": "ns",
            "range": "± 0.003801335671151221"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.313417864481608,
            "unit": "ns",
            "range": "± 0.01330363947724142"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.8523005523681637,
            "unit": "ns",
            "range": "± 0.00443831558110189"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 55.3530001839002,
            "unit": "ns",
            "range": "± 0.35167164892658204"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6291062593460083,
            "unit": "ns",
            "range": "± 0.0010067219309984848"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.92878065506617,
            "unit": "ns",
            "range": "± 0.14075202010335283"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.941142465909322,
            "unit": "ns",
            "range": "± 0.0006753763763817394"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9416480404535928,
            "unit": "ns",
            "range": "± 0.0010441989687771062"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9981025339762369,
            "unit": "ns",
            "range": "± 0.0011258326079648827"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9417205794652302,
            "unit": "ns",
            "range": "± 0.0003982195347092977"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9533051932652791,
            "unit": "ns",
            "range": "± 0.020965211857148097"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.352232952912649,
            "unit": "ns",
            "range": "± 0.16155484068772866"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 216.07151397069296,
            "unit": "ns",
            "range": "± 2.165839996664091"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 857.2531429926554,
            "unit": "ns",
            "range": "± 5.104315073464594"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.848551829655964,
            "unit": "ns",
            "range": "± 0.04129784759312533"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 14.017092953125635,
            "unit": "ns",
            "range": "± 0.021560544987773864"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.303086012601852,
            "unit": "ns",
            "range": "± 0.044497503263473404"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 35.53629058599472,
            "unit": "ns",
            "range": "± 0.2987419608097401"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 197.41692399978638,
            "unit": "ns",
            "range": "± 0.5295326171755107"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.990307526042064,
            "unit": "ns",
            "range": "± 0.0031399484508875773"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.1048769851525626,
            "unit": "ns",
            "range": "± 0.005214051224003857"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.027057943244775,
            "unit": "ns",
            "range": "± 0.00635456033232043"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 13.093503127495447,
            "unit": "ns",
            "range": "± 0.12551042741264434"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.5160632133483887,
            "unit": "ns",
            "range": "± 0.0021828129432894653"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.7071011289954185,
            "unit": "ns",
            "range": "± 0.003881300722685794"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 23.26807050903638,
            "unit": "ns",
            "range": "± 0.20623820240955817"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9222482815384865,
            "unit": "ns",
            "range": "± 0.0047653646802912186"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 29.523806889851887,
            "unit": "ns",
            "range": "± 0.12904874958781865"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 626.2746098836263,
            "unit": "ns",
            "range": "± 2.857949535845231"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 83.28814373413722,
            "unit": "ns",
            "range": "± 2.6715431961340954"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4976.106424967448,
            "unit": "ns",
            "range": "± 67.88601902315118"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 500.33337148030597,
            "unit": "ns",
            "range": "± 21.3061088680795"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 45700.75767008463,
            "unit": "ns",
            "range": "± 133.95167570741674"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 31.783169329166412,
            "unit": "ns",
            "range": "± 0.42893128982699186"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 85.50101053714752,
            "unit": "ns",
            "range": "± 0.8880684413544497"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 45.02064083019892,
            "unit": "ns",
            "range": "± 0.58480263569322"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 224.53437074025473,
            "unit": "ns",
            "range": "± 2.605927233798825"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 103.66924734910329,
            "unit": "ns",
            "range": "± 2.8979126559532298"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 109.87926721572876,
            "unit": "ns",
            "range": "± 0.6028365382364061"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 352.8419311841329,
            "unit": "ns",
            "range": "± 2.7478960024453594"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 697.3969402313232,
            "unit": "ns",
            "range": "± 3.217893184289271"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 335.0785086949666,
            "unit": "ns",
            "range": "± 3.9549700253430045"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2386.224189758301,
            "unit": "ns",
            "range": "± 19.85327473372247"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 890.7578109105428,
            "unit": "ns",
            "range": "± 2.640073308542285"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 987.8156776428223,
            "unit": "ns",
            "range": "± 20.14862500678753"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5221.462788899739,
            "unit": "ns",
            "range": "± 35.67298282673743"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 7350.621322631836,
            "unit": "ns",
            "range": "± 89.02579467299906"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 4535.31416575114,
            "unit": "ns",
            "range": "± 95.20968718500468"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 33152.20955403646,
            "unit": "ns",
            "range": "± 185.09135530207467"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 13265.139063517252,
            "unit": "ns",
            "range": "± 124.37102625772329"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7857.995992024739,
            "unit": "ns",
            "range": "± 130.75620084020437"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1793963588148,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 70.09643300374348,
            "unit": "ns",
            "range": "± 1.29179297548828"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 2545.0110193888345,
            "unit": "ns",
            "range": "± 34.82354286891579"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 74.38743058840434,
            "unit": "ns",
            "range": "± 1.1428996762589463"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2819.435801188151,
            "unit": "ns",
            "range": "± 184.8632612301688"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 86.69005799293518,
            "unit": "ns",
            "range": "± 0.5481907775270628"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 59.53482242425283,
            "unit": "ns",
            "range": "± 0.6616666919608694"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 56.27388562758764,
            "unit": "ns",
            "range": "± 0.1980227111238758"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 85.29234592119853,
            "unit": "ns",
            "range": "± 0.6948615669058749"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 19.048892160256703,
            "unit": "ns",
            "range": "± 0.1134046317979577"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 103.2398828069369,
            "unit": "ns",
            "range": "± 0.2772136920821438"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 68.09946944316228,
            "unit": "ns",
            "range": "± 0.2999186835784588"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 597.029283841451,
            "unit": "ns",
            "range": "± 5.604458390457951"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 800.1374336878458,
            "unit": "ns",
            "range": "± 8.863669497460373"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 20.12028059363365,
            "unit": "ns",
            "range": "± 0.15432182623291366"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 897.371574083964,
            "unit": "ns",
            "range": "± 4.421144567766259"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 729.6741409301758,
            "unit": "ns",
            "range": "± 2.390721726757409"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 9161.407409667969,
            "unit": "ns",
            "range": "± 37.183203214788115"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 11784.963628133139,
            "unit": "ns",
            "range": "± 83.80783139912599"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 19.549829870462418,
            "unit": "ns",
            "range": "± 0.006242559005525904"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 13003.992904663086,
            "unit": "ns",
            "range": "± 65.48972158000548"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 11158.671712239584,
            "unit": "ns",
            "range": "± 52.17228411755582"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.2773262808720271,
            "unit": "ns",
            "range": "± 0.0030273445516465244"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.27323376139005023,
            "unit": "ns",
            "range": "± 0.00044159387988371916"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.282685703287522,
            "unit": "ns",
            "range": "± 0.013879762242943947"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 0.7695809155702591,
            "unit": "ns",
            "range": "± 0.004545811625615039"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 12.700873757402102,
            "unit": "ns",
            "range": "± 0.1212399566743745"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.2129935620625814,
            "unit": "ns",
            "range": "± 0.002522135835587424"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 1.8533590984344481,
            "unit": "ns",
            "range": "± 0.004018928992873401"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.4173541717529297,
            "unit": "ns",
            "range": "± 0.009186881268102567"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.8825828196207682,
            "unit": "ns",
            "range": "± 0.005315972085213975"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.5842690219879151,
            "unit": "ns",
            "range": "± 0.0005289505469610559"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.690994316736857,
            "unit": "ns",
            "range": "± 0.004985176892484854"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 2.2121870028177892,
            "unit": "ns",
            "range": "± 0.0007599820613132437"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.4107820370992026,
            "unit": "ns",
            "range": "± 0.0006333907914049503"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 57.58233970403671,
            "unit": "ns",
            "range": "± 0.23029087756516223"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.8630699033737184,
            "unit": "ns",
            "range": "± 0.00026430947537408017"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 36.033890545368195,
            "unit": "ns",
            "range": "± 0.5092380056704271"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.8750818678538005,
            "unit": "ns",
            "range": "± 0.00032322148537403946"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.8746973085403443,
            "unit": "ns",
            "range": "± 0.000982384842435859"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 1.1580387306213378,
            "unit": "ns",
            "range": "± 0.00938582754905949"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 1.0652073758443195,
            "unit": "ns",
            "range": "± 0.03839651943077488"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 1.4350529276529949,
            "unit": "ns",
            "range": "± 0.0015481449924750073"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 15.73019814491272,
            "unit": "ns",
            "range": "± 0.019199631472953716"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 213.7217893600464,
            "unit": "ns",
            "range": "± 2.819166912660388"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 1006.5555216471354,
            "unit": "ns",
            "range": "± 1.9628363341705906"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 22.571307162443798,
            "unit": "ns",
            "range": "± 0.09157185790591327"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 14.186182936032614,
            "unit": "ns",
            "range": "± 0.024253337775367978"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 8.15758535762628,
            "unit": "ns",
            "range": "± 0.0176958739856812"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 24.291036556164425,
            "unit": "ns",
            "range": "± 0.1057317031816021"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 165.94957002003989,
            "unit": "ns",
            "range": "± 0.8141510760704908"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 0.8812821010748545,
            "unit": "ns",
            "range": "± 0.0008175943350017212"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 1.0341627647479374,
            "unit": "ns",
            "range": "± 0.012621712662708789"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 0.27638719355066615,
            "unit": "ns",
            "range": "± 0.002171312283227134"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 14.203292300303778,
            "unit": "ns",
            "range": "± 0.032354871674465546"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 1.240773191054662,
            "unit": "ns",
            "range": "± 0.03175682963156124"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 1.081612702459097,
            "unit": "ns",
            "range": "± 0.0022300734061152325"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 23.36251665155093,
            "unit": "ns",
            "range": "± 0.16799542772714976"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.0218719889720282,
            "unit": "ns",
            "range": "± 0.0053319420148021535"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 35.339407642682396,
            "unit": "ns",
            "range": "± 0.1638513481719927"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 631.7360363006592,
            "unit": "ns",
            "range": "± 1.218028996123209"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 121.11298418045044,
            "unit": "ns",
            "range": "± 2.5622856351759453"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4763.613283793132,
            "unit": "ns",
            "range": "± 1.2788974945113443"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 940.937686920166,
            "unit": "ns",
            "range": "± 22.250808569454968"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 48749.19761149088,
            "unit": "ns",
            "range": "± 174.41700772328"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 54.619147618611656,
            "unit": "ns",
            "range": "± 0.1237307338346237"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 92.06581685940425,
            "unit": "ns",
            "range": "± 0.5086108964321745"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 51.58788619438807,
            "unit": "ns",
            "range": "± 0.07163503883891979"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 243.32184187571207,
            "unit": "ns",
            "range": "± 0.7494531101667302"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 100.23918330669403,
            "unit": "ns",
            "range": "± 0.5139441501571325"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 116.37352605660756,
            "unit": "ns",
            "range": "± 0.8161625691864998"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 585.7804094950358,
            "unit": "ns",
            "range": "± 2.4451255451589704"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 848.67564868927,
            "unit": "ns",
            "range": "± 3.139528358361703"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 453.22213705380756,
            "unit": "ns",
            "range": "± 2.398802237239633"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2463.363900502523,
            "unit": "ns",
            "range": "± 7.225628931355079"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 758.4389108022054,
            "unit": "ns",
            "range": "± 2.719839531327107"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 1034.111312866211,
            "unit": "ns",
            "range": "± 3.157811652190334"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 8942.000096638998,
            "unit": "ns",
            "range": "± 2.215242263461168"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 12493.539967854818,
            "unit": "ns",
            "range": "± 34.27788292021074"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 6351.447367350261,
            "unit": "ns",
            "range": "± 122.25315868007198"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 34604.98445638021,
            "unit": "ns",
            "range": "± 89.0657502092106"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 10911.49696858724,
            "unit": "ns",
            "range": "± 49.47128864979801"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 14811.169469197592,
            "unit": "ns",
            "range": "± 199.07113126911116"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794047577689,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 65.85403009255727,
            "unit": "ns",
            "range": "± 2.6786816005735306"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1854.8375778198242,
            "unit": "ns",
            "range": "± 12.251852684150988"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 71.46796532471974,
            "unit": "ns",
            "range": "± 0.7032538763712028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2125.449373881022,
            "unit": "ns",
            "range": "± 33.32578662339184"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 95.66591197252274,
            "unit": "ns",
            "range": "± 0.7260194549907153"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 59.53035640716553,
            "unit": "ns",
            "range": "± 0.8433790119028257"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 40.022201697031655,
            "unit": "ns",
            "range": "± 0.16325251197854374"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 77.91866779327393,
            "unit": "ns",
            "range": "± 0.4582050235305754"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 17.37597582737605,
            "unit": "ns",
            "range": "± 0.0977468625293034"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 91.7385271191597,
            "unit": "ns",
            "range": "± 0.09687389614131457"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 52.0607520143191,
            "unit": "ns",
            "range": "± 0.249520803454368"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 406.72924025853473,
            "unit": "ns",
            "range": "± 4.740404657474401"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 730.814287185669,
            "unit": "ns",
            "range": "± 8.130481524332028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 17.321842223405838,
            "unit": "ns",
            "range": "± 0.1530062936811627"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 668.8660580317179,
            "unit": "ns",
            "range": "± 3.7880220969873304"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 567.8706906636556,
            "unit": "ns",
            "range": "± 3.398275742174961"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5962.061576843262,
            "unit": "ns",
            "range": "± 54.356952343768285"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 14887.537150065104,
            "unit": "ns",
            "range": "± 183.3943726141634"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 19.975848843653996,
            "unit": "ns",
            "range": "± 3.676849213364798"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 8691.012557983398,
            "unit": "ns",
            "range": "± 33.54099228419755"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7901.297281901042,
            "unit": "ns",
            "range": "± 106.50469656744997"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8957351172963778,
            "unit": "ns",
            "range": "± 0.0008655576602240295"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6863824104269346,
            "unit": "ns",
            "range": "± 0.008004722209513808"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.3562958153585593,
            "unit": "ns",
            "range": "± 0.002023070548129638"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2113280817866325,
            "unit": "ns",
            "range": "± 0.005830685195416644"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 12.23913791278998,
            "unit": "ns",
            "range": "± 0.07472895127086007"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.615538101196289,
            "unit": "ns",
            "range": "± 0.0005119580515122155"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.497555372873942,
            "unit": "ns",
            "range": "± 0.0004240970741633028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8552694142659507,
            "unit": "ns",
            "range": "± 0.029078846002889472"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9391315129597982,
            "unit": "ns",
            "range": "± 0.0009407734141051855"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6276959985097249,
            "unit": "ns",
            "range": "± 0.0003319202683560736"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8005369517008463,
            "unit": "ns",
            "range": "± 0.00030967020432901227"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.3067074419657394,
            "unit": "ns",
            "range": "± 0.0003877796453703411"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.8419807319641115,
            "unit": "ns",
            "range": "± 0.0023975342503743313"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 61.414682586987816,
            "unit": "ns",
            "range": "± 0.438483245067262"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6296515544255574,
            "unit": "ns",
            "range": "± 0.0008594358535391458"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 39.945170839627586,
            "unit": "ns",
            "range": "± 0.1790700786472513"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9476382296880086,
            "unit": "ns",
            "range": "± 0.01263716540406139"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9410559161504111,
            "unit": "ns",
            "range": "± 0.0011830773470573317"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 1.005467560450236,
            "unit": "ns",
            "range": "± 0.00045928194798688563"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9433923606872558,
            "unit": "ns",
            "range": "± 0.000046203299971951736"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9420531454086304,
            "unit": "ns",
            "range": "± 0.00104510610964771"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.009692390759787,
            "unit": "ns",
            "range": "± 0.11269087576604032"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 217.77665400505066,
            "unit": "ns",
            "range": "± 1.1105145332465416"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 933.1630325317383,
            "unit": "ns",
            "range": "± 1.4787227678714814"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.79503591855367,
            "unit": "ns",
            "range": "± 0.017560478516187434"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.944464226563772,
            "unit": "ns",
            "range": "± 0.24268903688214685"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.277082731326422,
            "unit": "ns",
            "range": "± 0.020373179802930037"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 35.26514951388041,
            "unit": "ns",
            "range": "± 0.0893210477272667"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 196.59177549680075,
            "unit": "ns",
            "range": "± 0.04956152126185636"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.8993469191094239,
            "unit": "ns",
            "range": "± 0.0013027743102661808"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.119761437177658,
            "unit": "ns",
            "range": "± 0.02519081685849942"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0351377439995606,
            "unit": "ns",
            "range": "± 0.002459273583215779"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 14.655575027068457,
            "unit": "ns",
            "range": "± 0.060944541838399785"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.2528830679754415,
            "unit": "ns",
            "range": "± 0.0015959035031825912"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 3.07916342963775,
            "unit": "ns",
            "range": "± 0.00240586723636954"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 24.805696934461594,
            "unit": "ns",
            "range": "± 0.14473974699823508"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9036403596401215,
            "unit": "ns",
            "range": "± 0.0075333611495436385"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 31.393063684304554,
            "unit": "ns",
            "range": "± 0.25919297674510366"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 652.8547264734904,
            "unit": "ns",
            "range": "± 2.1205390964288973"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 92.20306044816971,
            "unit": "ns",
            "range": "± 1.6762497019317348"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 5126.014981587728,
            "unit": "ns",
            "range": "± 27.73075812575166"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 697.247740427653,
            "unit": "ns",
            "range": "± 20.389821211387044"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 48571.37316894531,
            "unit": "ns",
            "range": "± 119.89732471903413"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 35.484665463368096,
            "unit": "ns",
            "range": "± 0.12963934161710222"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 84.34049997727077,
            "unit": "ns",
            "range": "± 0.4156651917177141"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 47.383294343948364,
            "unit": "ns",
            "range": "± 0.1683840396746671"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 241.5484617551168,
            "unit": "ns",
            "range": "± 0.6239274557751977"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 107.93197911977768,
            "unit": "ns",
            "range": "± 0.5811123453516623"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 121.32912596066792,
            "unit": "ns",
            "range": "± 0.8839303057655649"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 369.68459129333496,
            "unit": "ns",
            "range": "± 1.2851843341819291"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 723.9767665863037,
            "unit": "ns",
            "range": "± 6.7784162459028465"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 348.691454410553,
            "unit": "ns",
            "range": "± 0.702671568359644"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2448.040915171305,
            "unit": "ns",
            "range": "± 8.402726738407182"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 917.3986539840698,
            "unit": "ns",
            "range": "± 1.3273105933566927"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 1129.313965479533,
            "unit": "ns",
            "range": "± 8.247235418707739"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5774.487047831218,
            "unit": "ns",
            "range": "± 34.93760687182209"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8580.97087097168,
            "unit": "ns",
            "range": "± 193.534948046744"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 4967.305740356445,
            "unit": "ns",
            "range": "± 36.7168089597034"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 35525.50351969401,
            "unit": "ns",
            "range": "± 133.49587299019385"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 13385.027043660482,
            "unit": "ns",
            "range": "± 14.931864466716554"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 9608.664672851562,
            "unit": "ns",
            "range": "± 172.24971439744456"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794134468613,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 57.67081731557846,
            "unit": "ns",
            "range": "± 0.25774817855166154"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1997.983814239502,
            "unit": "ns",
            "range": "± 78.73622688525617"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 62.18387180566788,
            "unit": "ns",
            "range": "± 0.32624252664651265"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2060.2017415364585,
            "unit": "ns",
            "range": "± 117.68949635670012"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 86.77863605817159,
            "unit": "ns",
            "range": "± 0.41245882100476866"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 55.1323055823644,
            "unit": "ns",
            "range": "± 0.23734872354190198"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 36.39037597179413,
            "unit": "ns",
            "range": "± 0.2849067661442337"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 71.14905526240666,
            "unit": "ns",
            "range": "± 0.1638158843339583"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 15.258985648552576,
            "unit": "ns",
            "range": "± 0.08238859567290302"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 87.05050327380498,
            "unit": "ns",
            "range": "± 0.37907426760582"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 48.71884521842003,
            "unit": "ns",
            "range": "± 0.08452839492834646"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 382.0625475247701,
            "unit": "ns",
            "range": "± 4.06847342189822"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 700.4143015543619,
            "unit": "ns",
            "range": "± 5.801571614855382"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.1006088356177,
            "unit": "ns",
            "range": "± 0.09400405521210828"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 581.2458451588949,
            "unit": "ns",
            "range": "± 4.380072513820802"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 516.3723243077596,
            "unit": "ns",
            "range": "± 1.093433834033678"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5290.892819722493,
            "unit": "ns",
            "range": "± 16.353227991793613"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13559.363805135092,
            "unit": "ns",
            "range": "± 86.05404747599125"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.29700775941213,
            "unit": "ns",
            "range": "± 0.06438724147367711"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7381.976219177246,
            "unit": "ns",
            "range": "± 25.260022208630314"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7095.366277058919,
            "unit": "ns",
            "range": "± 105.16433368648686"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8953383030990759,
            "unit": "ns",
            "range": "± 0.0013877986494751605"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6883042727907499,
            "unit": "ns",
            "range": "± 0.0007429953540017985"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.3496011321743329,
            "unit": "ns",
            "range": "± 0.0030523937781764985"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.213349599391222,
            "unit": "ns",
            "range": "± 0.005459154773566648"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.692408810059229,
            "unit": "ns",
            "range": "± 0.2172172797379001"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6142923304239907,
            "unit": "ns",
            "range": "± 0.0015602494334744639"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.4957020200093587,
            "unit": "ns",
            "range": "± 0.0025197704450814315"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8555360221862798,
            "unit": "ns",
            "range": "± 0.006124549181905791"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9419289528528849,
            "unit": "ns",
            "range": "± 0.0014632167331029063"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6280352595647176,
            "unit": "ns",
            "range": "± 0.0002219332104090395"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.801642122268677,
            "unit": "ns",
            "range": "± 0.004979013800244033"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.3046000785827636,
            "unit": "ns",
            "range": "± 0.0013443222379083339"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.877415626525879,
            "unit": "ns",
            "range": "± 0.04622544089080781"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 55.21176892518997,
            "unit": "ns",
            "range": "± 0.03539684284840486"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6281176722844442,
            "unit": "ns",
            "range": "± 0.0004724832560431588"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.35517064730326,
            "unit": "ns",
            "range": "± 0.2035048674616788"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9416461242039998,
            "unit": "ns",
            "range": "± 0.0013892685265323103"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9397105112075805,
            "unit": "ns",
            "range": "± 0.0003755105154482622"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9571807301839192,
            "unit": "ns",
            "range": "± 0.00519707008321937"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9433509683609009,
            "unit": "ns",
            "range": "± 0.0023638269891366184"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.939988478978475,
            "unit": "ns",
            "range": "± 0.00016653282864352294"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.18394140402476,
            "unit": "ns",
            "range": "± 0.03851640685444509"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 185.5351505279541,
            "unit": "ns",
            "range": "± 1.3647240065568582"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 870.1718807220459,
            "unit": "ns",
            "range": "± 2.3630855963102233"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.792408049106598,
            "unit": "ns",
            "range": "± 0.07879301647855969"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.753176291783651,
            "unit": "ns",
            "range": "± 0.036159864525618"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.29042741159598,
            "unit": "ns",
            "range": "± 0.017874010890756346"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 36.20760162671407,
            "unit": "ns",
            "range": "± 0.054369155143433594"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 196.9354957739512,
            "unit": "ns",
            "range": "± 0.15732480527003267"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9871217148999374,
            "unit": "ns",
            "range": "± 0.002962111368190903"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.104991147915522,
            "unit": "ns",
            "range": "± 0.00041222866443187155"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.021604726711909,
            "unit": "ns",
            "range": "± 0.00045763488908138187"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.195291096965471,
            "unit": "ns",
            "range": "± 0.0386544258442842"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.5454824194312096,
            "unit": "ns",
            "range": "± 0.0034320195433200414"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 3.0879559939106307,
            "unit": "ns",
            "range": "± 0.018189824803303608"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.17938130100568,
            "unit": "ns",
            "range": "± 0.04657786149827377"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9157678236564,
            "unit": "ns",
            "range": "± 0.005925722365954021"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 28.79339192310969,
            "unit": "ns",
            "range": "± 0.06317645374196078"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 599.0217078526815,
            "unit": "ns",
            "range": "± 2.4518988998016655"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 70.17655098438263,
            "unit": "ns",
            "range": "± 0.2959550202833403"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4643.314860026042,
            "unit": "ns",
            "range": "± 28.759314523194917"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 441.84787638982135,
            "unit": "ns",
            "range": "± 2.648624403969818"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 44464.93103027344,
            "unit": "ns",
            "range": "± 235.26377318925066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 30.328410764535267,
            "unit": "ns",
            "range": "± 0.1644781175971972"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 75.44856280088425,
            "unit": "ns",
            "range": "± 1.2821464540350485"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 42.17760273814201,
            "unit": "ns",
            "range": "± 0.07638539942296454"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 217.81332230567932,
            "unit": "ns",
            "range": "± 0.8181185829554521"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 97.50878643989563,
            "unit": "ns",
            "range": "± 0.35453921135630995"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 106.18141122659047,
            "unit": "ns",
            "range": "± 0.8847943024729147"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 316.632212638855,
            "unit": "ns",
            "range": "± 1.7346717501874647"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 712.452241897583,
            "unit": "ns",
            "range": "± 2.8137363161969233"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 302.5164572397868,
            "unit": "ns",
            "range": "± 1.2100854878222318"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2308.34783299764,
            "unit": "ns",
            "range": "± 40.364095029585776"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 822.9495709737142,
            "unit": "ns",
            "range": "± 0.40114741348020233"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 918.9942509333292,
            "unit": "ns",
            "range": "± 6.3262217976335515"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 4997.89539082845,
            "unit": "ns",
            "range": "± 16.85082531870857"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 6794.62649790446,
            "unit": "ns",
            "range": "± 50.146363021934704"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3586.2904739379883,
            "unit": "ns",
            "range": "± 14.008817952070247"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 31962.753814697266,
            "unit": "ns",
            "range": "± 90.32088274621476"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12558.90737915039,
            "unit": "ns",
            "range": "± 83.05229229543279"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7372.033126831055,
            "unit": "ns",
            "range": "± 6.485871933181581"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794225269778,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 60.70094714562098,
            "unit": "ns",
            "range": "± 0.24157245532017144"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 2011.9684575398762,
            "unit": "ns",
            "range": "± 48.67123727825653"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 62.360687692960106,
            "unit": "ns",
            "range": "± 0.4613012738901945"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2066.869916280111,
            "unit": "ns",
            "range": "± 35.68950339535792"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 85.99367998043697,
            "unit": "ns",
            "range": "± 0.9233326448340535"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 56.86919208367666,
            "unit": "ns",
            "range": "± 0.1915649430534954"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 38.36498914162318,
            "unit": "ns",
            "range": "± 0.1325811276797276"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 73.13952354590099,
            "unit": "ns",
            "range": "± 0.23504813714620146"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 16.498480796813965,
            "unit": "ns",
            "range": "± 0.34241803285914474"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 90.14546064535777,
            "unit": "ns",
            "range": "± 1.2341396634840023"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 55.06612447897593,
            "unit": "ns",
            "range": "± 0.07928424095741697"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 394.8823874791463,
            "unit": "ns",
            "range": "± 5.2541465081311"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 691.1688782374064,
            "unit": "ns",
            "range": "± 3.5532575188231763"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.82872360944748,
            "unit": "ns",
            "range": "± 0.22863729572628"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 611.4847780863444,
            "unit": "ns",
            "range": "± 5.461178281344425"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 533.6079683303833,
            "unit": "ns",
            "range": "± 5.001377131209918"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5417.65340423584,
            "unit": "ns",
            "range": "± 40.50609547432451"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13886.176920572916,
            "unit": "ns",
            "range": "± 100.84001426357534"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.844459414482117,
            "unit": "ns",
            "range": "± 0.05748814710723953"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7635.578903198242,
            "unit": "ns",
            "range": "± 24.109920290735996"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7487.211598714192,
            "unit": "ns",
            "range": "± 316.18351868392466"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8952156007289886,
            "unit": "ns",
            "range": "± 0.0017983065665407015"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.684242827196916,
            "unit": "ns",
            "range": "± 0.008224457929219454"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.3526218918462594,
            "unit": "ns",
            "range": "± 0.0031953129697928095"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.217125903815031,
            "unit": "ns",
            "range": "± 0.006009317177765605"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.976536010702452,
            "unit": "ns",
            "range": "± 0.15644505407741643"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6135574455261232,
            "unit": "ns",
            "range": "± 0.0016371878327132248"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.5269863408406574,
            "unit": "ns",
            "range": "± 0.033706184969530144"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.850437431335449,
            "unit": "ns",
            "range": "± 0.02144449658641278"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9409010410308838,
            "unit": "ns",
            "range": "± 0.0012098150806995988"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.636850728670756,
            "unit": "ns",
            "range": "± 0.006606599975927921"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8005502885182698,
            "unit": "ns",
            "range": "± 0.001566778658489834"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.3028478711446128,
            "unit": "ns",
            "range": "± 0.00017948455914715394"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.843361394246419,
            "unit": "ns",
            "range": "± 0.004845267667335621"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 53.966456631819405,
            "unit": "ns",
            "range": "± 0.20544269759032202"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6594801476796469,
            "unit": "ns",
            "range": "± 0.0004370354374691583"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 45.873237470785774,
            "unit": "ns",
            "range": "± 0.10952582538214617"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9394092919031779,
            "unit": "ns",
            "range": "± 0.00033342370883327865"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9419381914138794,
            "unit": "ns",
            "range": "± 0.0008985077648178287"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9524980198542276,
            "unit": "ns",
            "range": "± 0.016697177431622988"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.942735146522522,
            "unit": "ns",
            "range": "± 0.0016367758316614821"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.941119044303894,
            "unit": "ns",
            "range": "± 0.00015030340654097774"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.442203263441721,
            "unit": "ns",
            "range": "± 0.2886198815301022"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 189.48494561513266,
            "unit": "ns",
            "range": "± 2.408188372051083"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 850.0383806228638,
            "unit": "ns",
            "range": "± 3.0785596530921175"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.78839139143626,
            "unit": "ns",
            "range": "± 0.03529472480046652"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.923962498704592,
            "unit": "ns",
            "range": "± 0.34946079037872224"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.26448085407416,
            "unit": "ns",
            "range": "± 0.011458072059583501"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 35.33168637752533,
            "unit": "ns",
            "range": "± 0.14563291262801892"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 204.3200157483419,
            "unit": "ns",
            "range": "± 2.6239631330340827"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.933628401408593,
            "unit": "ns",
            "range": "± 0.0026040432082203237"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.106102243065834,
            "unit": "ns",
            "range": "± 0.0017111649389119972"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.025206475208203,
            "unit": "ns",
            "range": "± 0.002303979822080678"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.924694592754046,
            "unit": "ns",
            "range": "± 0.14576501901507946"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.525573785106341,
            "unit": "ns",
            "range": "± 0.004921000783059554"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.7200521379709244,
            "unit": "ns",
            "range": "± 0.002476707666232161"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.885858873526256,
            "unit": "ns",
            "range": "± 0.09718954483417977"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.903405333558718,
            "unit": "ns",
            "range": "± 0.0030721701648208193"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 29.924879133701324,
            "unit": "ns",
            "range": "± 0.27332471810807885"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 616.3467038472494,
            "unit": "ns",
            "range": "± 6.02069529842004"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 75.3170296351115,
            "unit": "ns",
            "range": "± 0.4429825360833133"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4837.242164611816,
            "unit": "ns",
            "range": "± 40.54497849742341"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 501.9726982116699,
            "unit": "ns",
            "range": "± 17.14245871808878"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 47694.657552083336,
            "unit": "ns",
            "range": "± 283.24285960371105"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 30.686652799447376,
            "unit": "ns",
            "range": "± 0.31116067176128714"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 76.0701683362325,
            "unit": "ns",
            "range": "± 0.41439282539346256"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 43.48032559951147,
            "unit": "ns",
            "range": "± 0.30753056547649105"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 228.57615860303244,
            "unit": "ns",
            "range": "± 2.4763368353970536"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 100.14358621835709,
            "unit": "ns",
            "range": "± 0.7510056826916285"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 109.36276853084564,
            "unit": "ns",
            "range": "± 1.8666466323302906"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 327.7060783704122,
            "unit": "ns",
            "range": "± 2.503649262810782"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 643.7719395955404,
            "unit": "ns",
            "range": "± 4.309593849850798"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 316.18606241544086,
            "unit": "ns",
            "range": "± 1.1301829260768304"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2257.3594398498535,
            "unit": "ns",
            "range": "± 13.628641985469946"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 867.3322534561157,
            "unit": "ns",
            "range": "± 2.211590522033836"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 937.3418731689453,
            "unit": "ns",
            "range": "± 8.737531441685844"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5254.1848704020185,
            "unit": "ns",
            "range": "± 156.69424894921215"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 7052.749314626058,
            "unit": "ns",
            "range": "± 57.422091209689384"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 3656.198397318522,
            "unit": "ns",
            "range": "± 32.50190780481358"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 32141.931630452473,
            "unit": "ns",
            "range": "± 242.71306387486163"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 13004.179733276367,
            "unit": "ns",
            "range": "± 29.983875934344287"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7751.108322143555,
            "unit": "ns",
            "range": "± 80.74409334913128"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794310290626,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 34.988296608130135,
            "unit": "ns",
            "range": "± 0.7491550860489081"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1338.254409790039,
            "unit": "ns",
            "range": "± 50.585651085103414"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 39.785301595926285,
            "unit": "ns",
            "range": "± 1.334379466100878"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 1306.2377471923828,
            "unit": "ns",
            "range": "± 15.240705443855182"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 42.17036245266596,
            "unit": "ns",
            "range": "± 1.7107936396533339"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 28.99668585260709,
            "unit": "ns",
            "range": "± 0.732613935397553"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 22.56536211570104,
            "unit": "ns",
            "range": "± 0.9202155793366974"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 39.5194738705953,
            "unit": "ns",
            "range": "± 0.16400587040637707"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 10.0663623213768,
            "unit": "ns",
            "range": "± 0.452716580112149"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 55.051334232091904,
            "unit": "ns",
            "range": "± 1.4674942515945135"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 35.1409193277359,
            "unit": "ns",
            "range": "± 1.802900243189328"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 240.78385066986084,
            "unit": "ns",
            "range": "± 3.29448687192028"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 338.9721196492513,
            "unit": "ns",
            "range": "± 28.701833844925858"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 10.221344808737436,
            "unit": "ns",
            "range": "± 0.1928751085381993"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 363.73766136169434,
            "unit": "ns",
            "range": "± 14.778682562276003"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 342.4432012240092,
            "unit": "ns",
            "range": "± 22.670845939081882"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 4797.064470926921,
            "unit": "ns",
            "range": "± 254.22584062848244"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 4363.948471069336,
            "unit": "ns",
            "range": "± 293.86189143044146"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 10.718309715390205,
            "unit": "ns",
            "range": "± 0.5402135328560349"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 5337.7498779296875,
            "unit": "ns",
            "range": "± 126.71834315763573"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 5444.249305725098,
            "unit": "ns",
            "range": "± 22.969544526027168"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.08932301588356495,
            "unit": "ns",
            "range": "± 0.012772004504257497"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.011072184269626936,
            "unit": "ns",
            "range": "± 0.010497240180234615"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.0040797752638657885,
            "unit": "ns",
            "range": "± 0.0034550129719149304"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 0.4366867591937383,
            "unit": "ns",
            "range": "± 0.01307184044715513"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 7.304131875435512,
            "unit": "ns",
            "range": "± 0.4020854928776469"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 1.4691663363774616,
            "unit": "ns",
            "range": "± 0.010105648955859777"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 1.4610326550801593,
            "unit": "ns",
            "range": "± 0.01600186426015053"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 1.5790607624053956,
            "unit": "ns",
            "range": "± 0.006498438558694547"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.5554295352300008,
            "unit": "ns",
            "range": "± 0.010007752326257233"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.3889838372866312,
            "unit": "ns",
            "range": "± 0.007405494999440653"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.0919916330973305,
            "unit": "ns",
            "range": "± 0.01493863386124381"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 1.4727053133646646,
            "unit": "ns",
            "range": "± 0.01623623734732529"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 1.5941172281901042,
            "unit": "ns",
            "range": "± 0.014393947109099222"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 28.2145949502786,
            "unit": "ns",
            "range": "± 0.26129727735813335"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.3578837626775106,
            "unit": "ns",
            "range": "± 0.0019339083181832794"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 20.49434272448222,
            "unit": "ns",
            "range": "± 0.4359810311519855"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.5282018477121989,
            "unit": "ns",
            "range": "± 0.005312143787318479"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.5415671971638997,
            "unit": "ns",
            "range": "± 0.0036276551277425605"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.48443086846669514,
            "unit": "ns",
            "range": "± 0.0021174533609036005"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.5982983395258586,
            "unit": "ns",
            "range": "± 0.01130399852595531"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.6535276981989543,
            "unit": "ns",
            "range": "± 0.0006086921590711093"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 8.705792342623075,
            "unit": "ns",
            "range": "± 0.0028559602623759108"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 104.33143254121144,
            "unit": "ns",
            "range": "± 2.585781817912057"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 700.4390700658163,
            "unit": "ns",
            "range": "± 4.318829791941356"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 15.233623618880907,
            "unit": "ns",
            "range": "± 0.06141299315110276"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 7.9183981865644455,
            "unit": "ns",
            "range": "± 0.054065669333042445"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 5.259791485965252,
            "unit": "ns",
            "range": "± 0.02656401289373001"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 20.084510013461113,
            "unit": "ns",
            "range": "± 0.17981706076510823"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 83.40677364667256,
            "unit": "ns",
            "range": "± 1.1390555687105748"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 0.7708437455197176,
            "unit": "ns",
            "range": "± 0.0050395743203660425"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 1.0016665297249954,
            "unit": "ns",
            "range": "± 0.09476046307043594"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 0.14933199932177862,
            "unit": "ns",
            "range": "± 0.003569114910510307"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 7.968444575866063,
            "unit": "ns",
            "range": "± 0.2005456724345515"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 0.9945474378764629,
            "unit": "ns",
            "range": "± 0.0036071800953279072"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 1.163977261632681,
            "unit": "ns",
            "range": "± 0.022000764783600663"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 13.720542013645172,
            "unit": "ns",
            "range": "± 0.3444921309475891"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 1.1406448408961296,
            "unit": "ns",
            "range": "± 0.007708403763543073"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 16.109002391497295,
            "unit": "ns",
            "range": "± 1.3218292344705989"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 287.595285097758,
            "unit": "ns",
            "range": "± 2.018958433827245"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 36.63307795921961,
            "unit": "ns",
            "range": "± 0.2119134815919897"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 2289.8524602254233,
            "unit": "ns",
            "range": "± 21.592958351312575"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 290.51000531514484,
            "unit": "ns",
            "range": "± 18.71601690077926"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 23433.278645833332,
            "unit": "ns",
            "range": "± 715.6343306938115"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 18.61845784385999,
            "unit": "ns",
            "range": "± 1.128684497817064"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 41.141411105791725,
            "unit": "ns",
            "range": "± 0.2900567103468417"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 24.08533177773158,
            "unit": "ns",
            "range": "± 0.44098886210946586"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 122.9972071647644,
            "unit": "ns",
            "range": "± 1.8357285235043945"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 41.3185310959816,
            "unit": "ns",
            "range": "± 0.745952653825363"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 50.47897152105967,
            "unit": "ns",
            "range": "± 3.9272936403972727"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 208.1081972916921,
            "unit": "ns",
            "range": "± 4.886483339435083"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 392.5244719187419,
            "unit": "ns",
            "range": "± 16.58574586571237"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 203.15413610140482,
            "unit": "ns",
            "range": "± 20.73010719943564"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1358.6352812449138,
            "unit": "ns",
            "range": "± 42.197572613400574"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 289.0910089810689,
            "unit": "ns",
            "range": "± 3.5494133606690808"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 308.973017056783,
            "unit": "ns",
            "range": "± 1.1942731209366955"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 3301.711741129557,
            "unit": "ns",
            "range": "± 94.93044592346823"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 3883.5624364217124,
            "unit": "ns",
            "range": "± 85.21953055292288"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 2676.8405125935874,
            "unit": "ns",
            "range": "± 102.2506818827239"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 19059.121744791668,
            "unit": "ns",
            "range": "± 445.1958413241087"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 4299.228636423747,
            "unit": "ns",
            "range": "± 136.94823898541534"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 3915.8162841796875,
            "unit": "ns",
            "range": "± 264.7892256100939"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794395910773,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 51.75930597384771,
            "unit": "ns",
            "range": "± 0.06357370754703588"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1572.8085530598958,
            "unit": "ns",
            "range": "± 108.22353148165331"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 51.90191829204559,
            "unit": "ns",
            "range": "± 0.04692285318162148"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 1620.021218617757,
            "unit": "ns",
            "range": "± 7.753453224759593"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 60.83840864896774,
            "unit": "ns",
            "range": "± 0.39043282433816384"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 46.11617467800776,
            "unit": "ns",
            "range": "± 0.14461826407412637"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 27.217550198237102,
            "unit": "ns",
            "range": "± 0.08616739259036989"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 53.24952537814776,
            "unit": "ns",
            "range": "± 0.19970032137658825"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 14.01525828242302,
            "unit": "ns",
            "range": "± 0.4112746405562942"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 66.49558401107788,
            "unit": "ns",
            "range": "± 0.36298392201279306"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 39.29688451687495,
            "unit": "ns",
            "range": "± 0.0713303818525009"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 278.3356312910716,
            "unit": "ns",
            "range": "± 0.7712321800643265"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 398.81000598271686,
            "unit": "ns",
            "range": "± 1.6732519465659372"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 14.200085918108622,
            "unit": "ns",
            "range": "± 0.1014582223551126"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 459.61440563201904,
            "unit": "ns",
            "range": "± 2.2196359722119094"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 433.76037549972534,
            "unit": "ns",
            "range": "± 1.0609062633525352"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 4425.934290568034,
            "unit": "ns",
            "range": "± 212.65810079787488"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 5410.213620503743,
            "unit": "ns",
            "range": "± 43.798309060098326"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 14.217220902442932,
            "unit": "ns",
            "range": "± 0.1455562198959775"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 6066.785092671712,
            "unit": "ns",
            "range": "± 45.35794467594952"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 5603.303049723308,
            "unit": "ns",
            "range": "± 15.025787115710243"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.5013158619403839,
            "unit": "ns",
            "range": "± 0.006060167578649417"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.40414364201327163,
            "unit": "ns",
            "range": "± 0.030709642128000234"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.4168022498488426,
            "unit": "ns",
            "range": "± 0.005440773277980214"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 0.9078498122592767,
            "unit": "ns",
            "range": "± 0.012652025649838448"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 8.867479786276817,
            "unit": "ns",
            "range": "± 0.032182915246798185"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.206612424214681,
            "unit": "ns",
            "range": "± 0.0016773457612259354"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 1.924085865020752,
            "unit": "ns",
            "range": "± 0.009050866458695302"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.4673501027425133,
            "unit": "ns",
            "range": "± 0.006711266210909066"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.7867211176554362,
            "unit": "ns",
            "range": "± 0.00029867466975808885"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.5542105795542399,
            "unit": "ns",
            "range": "± 0.005594174405464791"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.588601526260376,
            "unit": "ns",
            "range": "± 0.0062943526851439915"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 2.357307590484619,
            "unit": "ns",
            "range": "± 0.0013229681077193383"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.4783781878153484,
            "unit": "ns",
            "range": "± 0.01445925084600162"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 41.3189962208271,
            "unit": "ns",
            "range": "± 0.018751669592254692"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.5529363514582316,
            "unit": "ns",
            "range": "± 0.0033464097520165873"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 31.44574071963628,
            "unit": "ns",
            "range": "± 0.3715414835974105"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.7369191090265911,
            "unit": "ns",
            "range": "± 0.0012392963584965386"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.7364463100433349,
            "unit": "ns",
            "range": "± 0.0009846032234998431"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.7783182744979859,
            "unit": "ns",
            "range": "± 0.00718730979244543"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.829412329673767,
            "unit": "ns",
            "range": "± 0.0006618643963345782"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.6939337501525878,
            "unit": "ns",
            "range": "± 0.003274799834296603"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 13.035539408524832,
            "unit": "ns",
            "range": "± 0.03032282853835597"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 152.64582586288452,
            "unit": "ns",
            "range": "± 1.156587026679927"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 723.9105084737142,
            "unit": "ns",
            "range": "± 1.8448494734696372"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 18.62376720706622,
            "unit": "ns",
            "range": "± 0.07387846721667168"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 11.606435418128967,
            "unit": "ns",
            "range": "± 0.0473057571657487"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 8.13268598417441,
            "unit": "ns",
            "range": "± 0.004341793147996975"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 24.87741333246231,
            "unit": "ns",
            "range": "± 0.09276445692551852"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 124.96023511886597,
            "unit": "ns",
            "range": "± 0.09622708908007328"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.468909313281377,
            "unit": "ns",
            "range": "± 0.03153341507370315"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 1.6218987566729386,
            "unit": "ns",
            "range": "± 0.022968202805725892"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 0.7422140017151833,
            "unit": "ns",
            "range": "± 0.00024220968848339552"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 10.320924227436384,
            "unit": "ns",
            "range": "± 0.06500687253907271"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.0944707406063876,
            "unit": "ns",
            "range": "± 0.015134184495311412"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.4431980277101197,
            "unit": "ns",
            "range": "± 0.026323895593027113"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 17.19297311703364,
            "unit": "ns",
            "range": "± 0.4435856944878353"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 1.0016970882813137,
            "unit": "ns",
            "range": "± 0.09580234247691806"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 20.396798342466354,
            "unit": "ns",
            "range": "± 0.18718745804383544"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 416.74161863327026,
            "unit": "ns",
            "range": "± 2.0197401219482907"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 52.27140927314758,
            "unit": "ns",
            "range": "± 1.070963640559095"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 3332.700527191162,
            "unit": "ns",
            "range": "± 14.42576628262064"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 348.6064642270406,
            "unit": "ns",
            "range": "± 7.27737527459271"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 29808.863408406574,
            "unit": "ns",
            "range": "± 25.856918048256667"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 29.382103542486828,
            "unit": "ns",
            "range": "± 0.21396958278563724"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 60.44271902243296,
            "unit": "ns",
            "range": "± 0.09186677722337702"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 32.975686897834144,
            "unit": "ns",
            "range": "± 0.618341570465514"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 181.91740306218466,
            "unit": "ns",
            "range": "± 1.9405490604119824"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 62.34879938761393,
            "unit": "ns",
            "range": "± 0.07975341323035971"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 64.59494187434514,
            "unit": "ns",
            "range": "± 0.0024950464699414992"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 260.29502566655475,
            "unit": "ns",
            "range": "± 1.741870945760992"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 495.3188645044963,
            "unit": "ns",
            "range": "± 2.338060024553589"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 234.0790191491445,
            "unit": "ns",
            "range": "± 0.6661505512436151"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 1810.7561779022217,
            "unit": "ns",
            "range": "± 23.803189946730054"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 391.2912503878276,
            "unit": "ns",
            "range": "± 2.078344544234417"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 408.1107711791992,
            "unit": "ns",
            "range": "± 1.4929047258910737"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 4069.7873026529946,
            "unit": "ns",
            "range": "± 18.85143329877101"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 5862.011772155762,
            "unit": "ns",
            "range": "± 35.577950389416245"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 2962.0054105122886,
            "unit": "ns",
            "range": "± 20.542677713608956"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 27643.313298543293,
            "unit": "ns",
            "range": "± 1421.8637576708463"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 5605.16687520345,
            "unit": "ns",
            "range": "± 16.13166012828767"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 6237.708676656087,
            "unit": "ns",
            "range": "± 23.703620729412254"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794483392118,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 57.03986303011576,
            "unit": "ns",
            "range": "± 0.35548653781854517"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1902.9056193033855,
            "unit": "ns",
            "range": "± 74.56289494054282"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 60.45079851150513,
            "unit": "ns",
            "range": "± 0.6392511496784292"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2040.867509206136,
            "unit": "ns",
            "range": "± 28.54049871665024"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 86.30727597077687,
            "unit": "ns",
            "range": "± 0.7202911925104786"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 54.93857981761297,
            "unit": "ns",
            "range": "± 0.334937846334532"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 36.012598395347595,
            "unit": "ns",
            "range": "± 0.09302374433791283"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 70.48885587851207,
            "unit": "ns",
            "range": "± 0.5825189096718955"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 15.28791148463885,
            "unit": "ns",
            "range": "± 0.12601952782656797"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 87.4773308634758,
            "unit": "ns",
            "range": "± 0.4605625520757564"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 54.13993491729101,
            "unit": "ns",
            "range": "± 0.19039977593459537"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 379.0030900637309,
            "unit": "ns",
            "range": "± 0.8380340118799396"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 668.1505533854166,
            "unit": "ns",
            "range": "± 3.559121095582733"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.157968451579412,
            "unit": "ns",
            "range": "± 0.04394504121647083"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 581.1035114924113,
            "unit": "ns",
            "range": "± 3.430661879229238"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 520.0780045191447,
            "unit": "ns",
            "range": "± 4.969248205764517"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5297.014737447103,
            "unit": "ns",
            "range": "± 39.59327582057144"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13553.191223144531,
            "unit": "ns",
            "range": "± 77.07000284425627"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.761842250823975,
            "unit": "ns",
            "range": "± 0.24554865453564947"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7306.86592356364,
            "unit": "ns",
            "range": "± 9.419487934448476"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7080.015398661296,
            "unit": "ns",
            "range": "± 42.87420607000584"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8991411166886488,
            "unit": "ns",
            "range": "± 0.005773038193968922"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.682497094074885,
            "unit": "ns",
            "range": "± 0.0075491475200549244"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.3711984430750211,
            "unit": "ns",
            "range": "± 0.021194377806637705"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.233971121410529,
            "unit": "ns",
            "range": "± 0.025737507212746896"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.396600981553396,
            "unit": "ns",
            "range": "± 0.131421708342512"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6145262819925947,
            "unit": "ns",
            "range": "± 0.0019754047065401624"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.496938337961833,
            "unit": "ns",
            "range": "± 0.003142962341966784"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8613341051737464,
            "unit": "ns",
            "range": "± 0.023265901490093603"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9400276482899983,
            "unit": "ns",
            "range": "± 0.00045190058271245764"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6276159207026164,
            "unit": "ns",
            "range": "± 0.0003695611402927743"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8048549245198569,
            "unit": "ns",
            "range": "± 0.001322218765116415"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.3089935328165687,
            "unit": "ns",
            "range": "± 0.012294174643036355"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.8573912353515625,
            "unit": "ns",
            "range": "± 0.017527240404374433"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 51.594890693823494,
            "unit": "ns",
            "range": "± 0.07962682018482482"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6308951822916667,
            "unit": "ns",
            "range": "± 0.0035200109084431246"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 40.623864591121674,
            "unit": "ns",
            "range": "± 0.1417643436077121"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9408883272806804,
            "unit": "ns",
            "range": "± 0.000782534153774986"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9430425621668498,
            "unit": "ns",
            "range": "± 0.0038331196661101205"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 1.0113471202850342,
            "unit": "ns",
            "range": "± 0.008645390216937053"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9495628337860108,
            "unit": "ns",
            "range": "± 0.007380642608266286"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9498117138544718,
            "unit": "ns",
            "range": "± 0.013819534490247275"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.22304113705953,
            "unit": "ns",
            "range": "± 0.02284886696041981"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 183.36759114265442,
            "unit": "ns",
            "range": "± 1.8518929665398223"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 863.933136622111,
            "unit": "ns",
            "range": "± 3.0901071717842057"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.9989948173364,
            "unit": "ns",
            "range": "± 0.1789273403366831"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 12.799942150712013,
            "unit": "ns",
            "range": "± 0.10013157696968326"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.322648187478384,
            "unit": "ns",
            "range": "± 0.08809149884491894"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 34.815994799137115,
            "unit": "ns",
            "range": "± 0.19804181053591022"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 194.7112488746643,
            "unit": "ns",
            "range": "± 0.036447199227226036"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9737262465059757,
            "unit": "ns",
            "range": "± 0.0029618194507034846"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.120352347691854,
            "unit": "ns",
            "range": "± 0.030712783146281362"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0424759301046531,
            "unit": "ns",
            "range": "± 0.019635750249433162"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.043559874097506,
            "unit": "ns",
            "range": "± 0.047179406499240434"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.562001700202624,
            "unit": "ns",
            "range": "± 0.036897046784520944"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 3.089621009926001,
            "unit": "ns",
            "range": "± 0.018236744596345562"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.15024843811989,
            "unit": "ns",
            "range": "± 0.04138496510336006"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.919308915734291,
            "unit": "ns",
            "range": "± 0.0026729483703203955"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 26.824946800867718,
            "unit": "ns",
            "range": "± 0.2398385399695009"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 574.4199345906576,
            "unit": "ns",
            "range": "± 1.0218806884797453"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 66.17611543337505,
            "unit": "ns",
            "range": "± 0.5964320498362367"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4829.872281392415,
            "unit": "ns",
            "range": "± 46.99605158704351"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 444.03373368581134,
            "unit": "ns",
            "range": "± 7.558252482690929"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 45765.664611816406,
            "unit": "ns",
            "range": "± 298.6239445233078"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 30.183841695388157,
            "unit": "ns",
            "range": "± 0.05325325081099768"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 74.83408772945404,
            "unit": "ns",
            "range": "± 0.20395418380683025"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 42.249364932378136,
            "unit": "ns",
            "range": "± 0.8464836222928263"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 215.0324048201243,
            "unit": "ns",
            "range": "± 0.3578561155642132"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 97.08837314446767,
            "unit": "ns",
            "range": "± 0.5946074555588327"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 102.68897630771,
            "unit": "ns",
            "range": "± 0.7898457240163465"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 319.07851298650104,
            "unit": "ns",
            "range": "± 1.998450337993135"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 625.5412238438925,
            "unit": "ns",
            "range": "± 0.6265860338906226"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 301.9016760190328,
            "unit": "ns",
            "range": "± 2.1446678935788257"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2200.3447291056314,
            "unit": "ns",
            "range": "± 10.216407283374291"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 852.5310039520264,
            "unit": "ns",
            "range": "± 7.445105315022502"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 930.6503356297811,
            "unit": "ns",
            "range": "± 7.447042165547957"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 4971.278048197429,
            "unit": "ns",
            "range": "± 48.50476179172482"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 6818.771926879883,
            "unit": "ns",
            "range": "± 139.700202902039"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 4145.59391784668,
            "unit": "ns",
            "range": "± 11.980388143619711"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 31953.78076171875,
            "unit": "ns",
            "range": "± 231.8103619720385"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12096.65668741862,
            "unit": "ns",
            "range": "± 192.78780701298544"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7256.415400187175,
            "unit": "ns",
            "range": "± 39.952110691738575"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794568798023,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 58.13295189539591,
            "unit": "ns",
            "range": "± 0.2964423396255812"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 1906.2202173868816,
            "unit": "ns",
            "range": "± 56.359110755541096"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 61.49937395254771,
            "unit": "ns",
            "range": "± 0.6217944286755512"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 1943.6210657755535,
            "unit": "ns",
            "range": "± 77.73286139134152"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 83.8994928797086,
            "unit": "ns",
            "range": "± 1.1607973420824629"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 55.878589510917664,
            "unit": "ns",
            "range": "± 0.6711060507313393"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 37.68560083707174,
            "unit": "ns",
            "range": "± 0.4068041806102809"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 71.12771791219711,
            "unit": "ns",
            "range": "± 0.565342455730083"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 15.889074484507242,
            "unit": "ns",
            "range": "± 0.19964373008934122"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 86.70858774582545,
            "unit": "ns",
            "range": "± 1.1496139002657655"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 49.24666374921799,
            "unit": "ns",
            "range": "± 0.22558009582683522"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 384.6739155451457,
            "unit": "ns",
            "range": "± 4.327011074260665"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 678.0521087646484,
            "unit": "ns",
            "range": "± 1.3591877673580164"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 15.707763393719992,
            "unit": "ns",
            "range": "± 0.1074118290239836"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 591.2123769124349,
            "unit": "ns",
            "range": "± 2.426466530199759"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 640.4592342376709,
            "unit": "ns",
            "range": "± 1.4956882656582264"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5376.607231140137,
            "unit": "ns",
            "range": "± 47.86648111728319"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13610.178756713867,
            "unit": "ns",
            "range": "± 70.63894189248845"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.521246552467346,
            "unit": "ns",
            "range": "± 0.13171106658122386"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7430.646947224935,
            "unit": "ns",
            "range": "± 28.315891123380574"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7107.073267618815,
            "unit": "ns",
            "range": "± 54.908486984381575"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.8924422326187292,
            "unit": "ns",
            "range": "± 0.0031809333048612638"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 0.6796671319752932,
            "unit": "ns",
            "range": "± 0.00579595188996588"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.35855506857236225,
            "unit": "ns",
            "range": "± 0.017520663484002563"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2251889208952587,
            "unit": "ns",
            "range": "± 0.012066015065637566"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.87251728773117,
            "unit": "ns",
            "range": "± 0.16901919929074039"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6237623812357587,
            "unit": "ns",
            "range": "± 0.010740953009616394"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.5016579818725586,
            "unit": "ns",
            "range": "± 0.010151552614115527"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8404003156026207,
            "unit": "ns",
            "range": "± 0.0036664045834927583"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9406938953399658,
            "unit": "ns",
            "range": "± 0.0011794368706475095"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6317917575836182,
            "unit": "ns",
            "range": "± 0.004204528429939273"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.7989487794240315,
            "unit": "ns",
            "range": "± 0.0007786256357727598"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.3524808209737142,
            "unit": "ns",
            "range": "± 0.07300168742781411"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.8700787404378256,
            "unit": "ns",
            "range": "± 0.017111614939418304"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 53.02257704734802,
            "unit": "ns",
            "range": "± 0.4030611010349513"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 0.6338446140289307,
            "unit": "ns",
            "range": "± 0.005240513780845414"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.5766084988912,
            "unit": "ns",
            "range": "± 0.27929660077072954"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9401810992558796,
            "unit": "ns",
            "range": "± 0.0017868387162004254"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9409741621017457,
            "unit": "ns",
            "range": "± 0.0004986541275098592"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.95637473487854,
            "unit": "ns",
            "range": "± 0.016701093608311252"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.945657875061035,
            "unit": "ns",
            "range": "± 0.004059700587764228"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9436242300669352,
            "unit": "ns",
            "range": "± 0.0024306072056433444"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.414850324392319,
            "unit": "ns",
            "range": "± 0.06735062309968994"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 183.6056138674418,
            "unit": "ns",
            "range": "± 0.252943256346984"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 875.7002499898275,
            "unit": "ns",
            "range": "± 7.964382845839101"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 22.348934958378475,
            "unit": "ns",
            "range": "± 0.10078862110834085"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 13.415839900573095,
            "unit": "ns",
            "range": "± 0.14857836400955177"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.33712163567543,
            "unit": "ns",
            "range": "± 0.05534052002564359"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 36.03134300311407,
            "unit": "ns",
            "range": "± 0.03243314867338792"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 195.310187180837,
            "unit": "ns",
            "range": "± 0.6182935677680629"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9309546152750652,
            "unit": "ns",
            "range": "± 0.0016376213247839945"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.10253386447827,
            "unit": "ns",
            "range": "± 0.0032455280810701997"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.032237968718012,
            "unit": "ns",
            "range": "± 0.016858585792992563"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.350716019670168,
            "unit": "ns",
            "range": "± 0.08445448833635032"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.547081877787908,
            "unit": "ns",
            "range": "± 0.0032319304204063383"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.7575342481335006,
            "unit": "ns",
            "range": "± 0.0013275382076189102"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.763577967882156,
            "unit": "ns",
            "range": "± 0.49500220301013687"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9678316016991935,
            "unit": "ns",
            "range": "± 0.03464829318781612"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 27.07862577835719,
            "unit": "ns",
            "range": "± 0.057033752628290996"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 589.9419072469076,
            "unit": "ns",
            "range": "± 1.9214889613048929"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 68.74268078804016,
            "unit": "ns",
            "range": "± 0.31156879001136367"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4930.247138977051,
            "unit": "ns",
            "range": "± 26.50961912331567"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 453.3171130816142,
            "unit": "ns",
            "range": "± 4.3166367800234715"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 47387.30112711588,
            "unit": "ns",
            "range": "± 565.6196626910138"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 31.67826247215271,
            "unit": "ns",
            "range": "± 0.34077662986990315"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 75.85535097122192,
            "unit": "ns",
            "range": "± 0.34209093765213466"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 42.15794778863589,
            "unit": "ns",
            "range": "± 0.06163375973438983"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 219.2635393937429,
            "unit": "ns",
            "range": "± 1.8271430145769554"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 98.38809682925542,
            "unit": "ns",
            "range": "± 0.519928658048659"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 103.79190925757091,
            "unit": "ns",
            "range": "± 0.5409145788397627"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 321.22313928604126,
            "unit": "ns",
            "range": "± 1.6592767586464454"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 636.7711362838745,
            "unit": "ns",
            "range": "± 5.28339049303583"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 304.96038722991943,
            "unit": "ns",
            "range": "± 2.473880446836344"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2227.2958501180015,
            "unit": "ns",
            "range": "± 7.837451003054735"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 854.5333490371704,
            "unit": "ns",
            "range": "± 4.142566117531646"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 877.2631953557333,
            "unit": "ns",
            "range": "± 3.402746831869008"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5005.023544311523,
            "unit": "ns",
            "range": "± 31.30564804515188"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8128.015553792317,
            "unit": "ns",
            "range": "± 89.43163197912038"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 4184.44596862793,
            "unit": "ns",
            "range": "± 18.247684553461234"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 32236.239908854168,
            "unit": "ns",
            "range": "± 266.3587495870416"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12543.565404256186,
            "unit": "ns",
            "range": "± 42.863447919196055"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7538.107096354167,
            "unit": "ns",
            "range": "± 167.06601428551787"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "committer": {
            "name": "Kai Orion",
            "username": "kai-orion",
            "email": "bre97.web@gmail.com"
          },
          "id": "ab7a655339db332a92bcacf1da8cc3cee2cd4f50",
          "message": "docs: point PackageReleaseNotes at CHANGELOG without a version\n\nCo-authored-by: Cursor <cursoragent@cursor.com>",
          "timestamp": "2026-11-03T06:18:11Z",
          "url": "https://github.com/sandlada/RopSharp/commit/ab7a655339db332a92bcacf1da8cc3cee2cd4f50"
        },
        "date": 1794653181718,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Completed",
            "value": 59.198558727900185,
            "unit": "ns",
            "range": "± 0.8368289931626797"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_Pending",
            "value": 2048.703987121582,
            "unit": "ns",
            "range": "± 77.58869065316205"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Completed",
            "value": 62.72670531272888,
            "unit": "ns",
            "range": "± 0.6613182191362917"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.BindOkAsync_Pending",
            "value": 2147.752389272054,
            "unit": "ns",
            "range": "± 81.08209374908353"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapErrAsync_OnErr",
            "value": 81.71916751066844,
            "unit": "ns",
            "range": "± 0.7153988997642442"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.AsyncBenchmarks.MapOkAsync_OnErr",
            "value": 57.90873038768768,
            "unit": "ns",
            "range": "± 0.4064914386636133"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 16)",
            "value": 37.78848272562027,
            "unit": "ns",
            "range": "± 1.6915016396787312"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 16)",
            "value": 71.99006245533626,
            "unit": "ns",
            "range": "± 0.34015851668274016"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 16)",
            "value": 16.307733565568924,
            "unit": "ns",
            "range": "± 0.0764295532045608"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 16)",
            "value": 88.82960492372513,
            "unit": "ns",
            "range": "± 0.14315734460850427"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 16)",
            "value": 49.819329579671226,
            "unit": "ns",
            "range": "± 0.5518239192011484"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 256)",
            "value": 385.62277094523114,
            "unit": "ns",
            "range": "± 2.8663806882989613"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 256)",
            "value": 701.5053189595541,
            "unit": "ns",
            "range": "± 6.681403935823336"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 256)",
            "value": 16.220424125591915,
            "unit": "ns",
            "range": "± 0.19189054617683068"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 256)",
            "value": 600.9363301595052,
            "unit": "ns",
            "range": "± 2.6936328994183003"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 256)",
            "value": 518.7688976923624,
            "unit": "ns",
            "range": "± 0.7100989848106777"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Values_AllSome(N: 4096)",
            "value": 5384.418357849121,
            "unit": "ns",
            "range": "± 41.91021419760156"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_AllSome(N: 4096)",
            "value": 13456.756159464518,
            "unit": "ns",
            "range": "± 484.4618443766128"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Sequence_HalfNone(N: 4096)",
            "value": 15.54428963859876,
            "unit": "ns",
            "range": "± 0.19608188717767847"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.Traverse_AllSome(N: 4096)",
            "value": 7468.736048380534,
            "unit": "ns",
            "range": "± 91.1914057437468"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.CollectionBenchmarks.ToSingleError_FromValidation(N: 4096)",
            "value": 7188.585110982259,
            "unit": "ns",
            "range": "± 82.34187295786934"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnRight",
            "value": 0.9174820159872373,
            "unit": "ns",
            "range": "± 0.02506509812714604"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.MapRight_OnLeft",
            "value": 1.4163109933336575,
            "unit": "ns",
            "range": "± 0.011903046305496915"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Swap_OnRight",
            "value": 0.34944435954093933,
            "unit": "ns",
            "range": "± 0.001676607889961766"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.Match_OnRight",
            "value": 1.2191323538621266,
            "unit": "ns",
            "range": "± 0.011817483009064515"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EitherBenchmarks.ToResult_OnLeft",
            "value": 9.735737293958664,
            "unit": "ns",
            "range": "± 0.11568306406768682"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_SameValue",
            "value": 2.6221849377950033,
            "unit": "ns",
            "range": "± 0.008985836574090163"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_Equals_Different",
            "value": 2.496091037750244,
            "unit": "ns",
            "range": "± 0.0020354424332496004"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_GetHashCode",
            "value": 2.8585586547851562,
            "unit": "ns",
            "range": "± 0.01902120881055436"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_SameValue",
            "value": 0.9464409720102945,
            "unit": "ns",
            "range": "± 0.0022189303067404104"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_Equals_None",
            "value": 0.6285228344599406,
            "unit": "ns",
            "range": "± 0.0003708774130183129"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_GetHashCode",
            "value": 1.8075245234171549,
            "unit": "ns",
            "range": "± 0.008369491952544916"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_Equals_SameValue",
            "value": 3.307356836954753,
            "unit": "ns",
            "range": "± 0.0014337111146401425"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_GetHashCode",
            "value": 2.8637377713521324,
            "unit": "ns",
            "range": "± 0.021875805512104073"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Result_ToString",
            "value": 52.41895288228989,
            "unit": "ns",
            "range": "± 0.09219330055773954"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Option_ToString",
            "value": 8.105293818155923,
            "unit": "ns",
            "range": "± 0.028331163891990547"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.EqualityBenchmarks.Either_ToString",
            "value": 38.577531933784485,
            "unit": "ns",
            "range": "± 0.416013203386928"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnSome",
            "value": 0.9437097082138061,
            "unit": "ns",
            "range": "± 0.00272726835003839"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.BindSome_OnSome",
            "value": 0.9411957467397055,
            "unit": "ns",
            "range": "± 0.001514180535982671"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.MapSome_OnNone",
            "value": 0.9561039263407389,
            "unit": "ns",
            "range": "± 0.010775844404684333"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Filter_OnSome",
            "value": 0.9464319826761881,
            "unit": "ns",
            "range": "± 0.006288040154650529"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.OptionBenchmarks.Match_OnSome",
            "value": 0.9429568497339885,
            "unit": "ns",
            "range": "± 0.001483656127108057"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Success",
            "value": 14.241687923669815,
            "unit": "ns",
            "range": "± 0.10629746062234932"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_Failure",
            "value": 185.8153703212738,
            "unit": "ns",
            "range": "± 1.4544351330197447"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Int_LongFailure",
            "value": 878.5332384109497,
            "unit": "ns",
            "range": "± 5.31512028463676"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Long_Success",
            "value": 21.98307650287946,
            "unit": "ns",
            "range": "± 0.023007436149618046"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Success",
            "value": 13.114864349365234,
            "unit": "ns",
            "range": "± 0.2965403207487561"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.TryParse_Int_Failure",
            "value": 9.312878608703613,
            "unit": "ns",
            "range": "± 0.06617429088628808"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_Guid_Success",
            "value": 35.356196681658425,
            "unit": "ns",
            "range": "± 0.18007037500098363"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ParsingBenchmarks.Parse_DateTime_Success",
            "value": 194.62667846679688,
            "unit": "ns",
            "range": "± 0.5245070550519513"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnOk",
            "value": 1.9412369305888812,
            "unit": "ns",
            "range": "± 0.016278196424092433"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.BindOk_OnOk",
            "value": 2.110393665730953,
            "unit": "ns",
            "range": "± 0.008494925272559998"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapOk_OnErr",
            "value": 1.0246325843036175,
            "unit": "ns",
            "range": "± 0.0020486266254985965"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.MapErr_OnErr",
            "value": 12.457391073306402,
            "unit": "ns",
            "range": "± 0.18631993769524507"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Match_OnOk",
            "value": 2.513263506193956,
            "unit": "ns",
            "range": "± 0.00455185049629112"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.ResultBenchmarks.Validate_Two",
            "value": 2.7198888485630355,
            "unit": "ns",
            "range": "± 0.010229021111512302"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1)",
            "value": 22.848965028921764,
            "unit": "ns",
            "range": "± 0.16169476395165416"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1)",
            "value": 2.9205252379179,
            "unit": "ns",
            "range": "± 0.0062593981671501975"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 10)",
            "value": 27.88066091140111,
            "unit": "ns",
            "range": "± 0.5382226998939506"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 10)",
            "value": 590.8785371780396,
            "unit": "ns",
            "range": "± 6.209630946267845"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 100)",
            "value": 70.42213918765385,
            "unit": "ns",
            "range": "± 0.9813438916169964"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 100)",
            "value": 4631.143277486165,
            "unit": "ns",
            "range": "± 40.952375444332446"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine_CopyOnly_LowerBound(N: 1000)",
            "value": 459.72691774368286,
            "unit": "ns",
            "range": "± 7.990584051254019"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.RopErrorBenchmarks.Combine(N: 1000)",
            "value": 46105.67409261068,
            "unit": "ns",
            "range": "± 142.77545936207426"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 16)",
            "value": 30.84759486714999,
            "unit": "ns",
            "range": "± 0.08325766599529426"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 16)",
            "value": 76.13023720184962,
            "unit": "ns",
            "range": "± 0.13254838730749938"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 16)",
            "value": 42.44095387061437,
            "unit": "ns",
            "range": "± 0.20335685726322394"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 16)",
            "value": 217.55472898483276,
            "unit": "ns",
            "range": "± 1.8514599306095176"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 16)",
            "value": 97.19926915566127,
            "unit": "ns",
            "range": "± 0.5448328453367974"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 16)",
            "value": 105.1008455355962,
            "unit": "ns",
            "range": "± 0.7651644734161736"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 256)",
            "value": 334.0953516960144,
            "unit": "ns",
            "range": "± 4.99512006195249"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 256)",
            "value": 732.4573669433594,
            "unit": "ns",
            "range": "± 6.175447564221639"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 256)",
            "value": 309.8982605934143,
            "unit": "ns",
            "range": "± 1.8226092433053371"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 256)",
            "value": 2231.123737335205,
            "unit": "ns",
            "range": "± 34.20421934459179"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 256)",
            "value": 836.5163536071777,
            "unit": "ns",
            "range": "± 3.2715220353192973"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 256)",
            "value": 957.9902041753134,
            "unit": "ns",
            "range": "± 10.750245266207296"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_Manual_AllOk(N: 4096)",
            "value": 5073.573280334473,
            "unit": "ns",
            "range": "± 6.378582628861267"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_AllOk(N: 4096)",
            "value": 8227.798919677734,
            "unit": "ns",
            "range": "± 89.4632303599112"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Sequence_ErrorAtMiddle(N: 4096)",
            "value": 4280.581886291504,
            "unit": "ns",
            "range": "± 46.03774523615365"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_AllErr(N: 4096)",
            "value": 32347.988423665363,
            "unit": "ns",
            "range": "± 290.5609918378581"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.SequenceValidation_HalfErr(N: 4096)",
            "value": 12810.44086710612,
            "unit": "ns",
            "range": "± 86.38897920902251"
          },
          {
            "name": "Sandlada.RopSharp.Benchmarks.SequenceBenchmarks.Partition_Half(N: 4096)",
            "value": 7601.793251037598,
            "unit": "ns",
            "range": "± 205.2657157373557"
          }
        ]
      }
    ]
  }
}