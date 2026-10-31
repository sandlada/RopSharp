window.BENCHMARK_DATA = {
  "lastUpdate": 1793440584371,
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
      }
    ]
  }
}