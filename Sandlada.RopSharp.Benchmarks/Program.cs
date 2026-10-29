using System.Reflection;
using BenchmarkDotNet.Running;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

EnvironmentProvenance.Print();

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
