window.BENCHMARK_DATA = {
  "lastUpdate": 1793260139003,
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
      }
    ]
  }
}