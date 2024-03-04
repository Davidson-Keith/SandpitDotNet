using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SandpitDotNet;

public class CodeAnalysisCompiler {
  private const string firstClass =
    @"
    namespace A
    {
        public class Foo
        {
            public int Bar() => 21;
        }
    }";

  private const string secondClass =
    @"
    using A;

    namespace B
    {
        public class Test
        {
            public int GetValue() => new Foo().Bar();
        }
    }";

  private const string thirdClass =
    @" 
    void calc_func() {
        var invoiceDate = getInvoiceDate();
        var currentDate = DateTime.Now;
        if ((currentDate - invoiceDate).TotalDays >= 90) {
            switchWorkflow2(""review"");
            completeTask();
        }
    }

    public DateTime getInvoiceDate() {
    }

    public void switchWorkflow2() {
    }

    public void completeTask() {
    }";

  public CodeAnalysisCompiler() {
    Run();
  }

  public void Run() {
    var firstAssemblyFileName = Path.Combine(Path.GetTempPath(), "A.dll");
    var secondAssemblyFileName = Path.Combine(Path.GetTempPath(), "B.dll");
    var thirdAssemblyFileName = Path.Combine(Path.GetTempPath(), "C.dll");

    var compilation = CreateCompilation(CSharpSyntaxTree.ParseText(firstClass), "A");
    // var secondCompilation = CreateCompilation(CSharpSyntaxTree.ParseText(secondClass), "B")
      // .AddReferences(compilation.ToMetadataReference());
    var secondCompilation = CreateCompilation(CSharpSyntaxTree.ParseText(secondClass), "B");
    var thirdCompilation = CreateCompilation(CSharpSyntaxTree.ParseText(thirdClass), "C");

    EmitResult emitResult1 = compilation.Emit(firstAssemblyFileName);
    EmitResult emitResult2 = secondCompilation.Emit(secondAssemblyFileName);
    EmitResult emitResult3 = thirdCompilation.Emit(thirdAssemblyFileName);

    WriteResult(emitResult1);
    WriteResult(emitResult2);
    WriteResult(emitResult3);
    
    // dynamic testType = Activator.CreateInstanceFrom(secondAssemblyFileName, "B.Test").Unwrap();
    // var value = testType.GetValue();
    // Console.WriteLine($"\ntestType.GetValue() = {value}");
  }

  private void WriteResult(EmitResult result) {
    Console.WriteLine();
    Console.WriteLine($"Compile success = {result.Success}");
    Console.WriteLine($"Error diagnostics = {result.Diagnostics.Length}");
    foreach (Diagnostic diagnostic in result.Diagnostics) {
      Console.WriteLine($"{diagnostic.Severity}: {diagnostic.Id}: " +
                        $"{diagnostic.Location.Kind}{diagnostic.Location.GetLineSpan()}: " +
                        $"{diagnostic.GetMessage()}");
      // Console.WriteLine($"{diagnostic.Descriptor.HelpLinkUri}");
    }
  }

  private static CSharpCompilation CreateCompilation(SyntaxTree tree, string name) {
    return CSharpCompilation
      .Create(name, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
      .AddSyntaxTrees(tree);
  }
}