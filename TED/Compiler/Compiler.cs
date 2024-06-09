using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using TED.Interpreter;
using TED.Tables;

namespace TED.Compiler
{
    public class Compiler
    {
        public readonly Program Program;

        public readonly string NamespaceName;

        public readonly string ClassName;

        public readonly TextWriter Output;

        public Compiler(Program program, string namespaceName, string dirPath)
            : this(program, namespaceName, $"{program.Name}Helpers", new StreamWriter(Path.Combine(dirPath, $"{program.Name}Helpers.cs")))
        { }

        public Compiler(Program program, string namespaceName, string className, TextWriter output)
        {
            Program = program;
            ClassName = className;
            Output = output;
            NamespaceName = namespaceName;
        }

        public void GenerateSource()
        {
            Output.WriteLine("using TED;");
            Output.WriteLine("using TED.Interpreter;");
            Output.WriteLine("using TED.Compiler;");
            Output.WriteLine("using TED.Tables;");
            NewLine();
            Output.WriteLine($"namespace {NamespaceName};");
            NewLine();
            Output.WriteLine("#pragma warning disable 0164,8618");
            NewLine();
            Output.WriteLine($"[CompiledHelpersFor(\"{Program.Name}\")]");
            Output.WriteLine($"public static class {ClassName}");
            Output.WriteLine("{");
            CompileProgram();
            Output.WriteLine("}");
            Output.WriteLine("#pragma warning restore 0164,8618");
            Output.Close();
        }

        private void CompileProgram()
        {
            foreach (var predicate in Program.Tables)
            {
                var table = predicate.TableUntyped;
                Output.WriteLine($"    [LinkToTable(\"{predicate.Name}\")]");
                Output.WriteLine($"    public static {FormatType(table.GetType())} {table.Name};");
                foreach (var index in table.Indices)
                {
                    Output.WriteLine($"    public static {FormatType(index.GetType())} {VariableNameForIndex(index)};");
                }
            }
            Output.WriteLine();

            foreach (var table in Program.Tables)
            {
                if (table.UpdateMode == UpdateMode.Rules)
                {
                    CompileTable(table);
                }
            }
        }

        public static string VariableNameForIndex(TableIndex index)
        {
            var b = new StringBuilder();
            b.Append(index.Predicate.Name);
            b.Append("_");
            foreach (var c in index.ColumnNumbers)
            {
                b.Append('_');
                b.Append(c);
            }
            return b.ToString();
        }

        public string FormatType(Type t)
        {
            if (!t.IsGenericType)
            {
                if (t == typeof(int))
                    return "int";
                if (t == typeof(float))
                    return "float";
                if (t == typeof(bool))
                    return "bool";
                if (t == typeof(string))
                    return "string";
                return t.Name;
            }
            return
                $"{t.Name.Substring(0, t.Name.IndexOf("`"))}<{string.Join(",", t.GetGenericArguments().Select(FormatType))}>";
        }

        private void CompileTable(TablePredicate table)
        {
            Output.WriteLine($"    [CompiledRulesFor(\"{table.Name}\")]");
            Output.WriteLine($"    public static void {table.Name}__CompiledUpdate()");
            Output.WriteLine("    {");
            foreach (var rule in table.Rules!)
                CompileRule(rule);
            Output.WriteLine("    }");
        }

        private void CompileRule(Rule rule)
        {
            var predicate = rule.Predicate;
            var ruleNumber = predicate.Rules!.IndexOf(rule)+1;
            var start = new Continuation($"start{ruleNumber}");
            var end = new Continuation($"end{ruleNumber}");
            Output.Write("        {");
            Label(start);
            Indented($"// {rule}");


            // Declare locals
            foreach (var c in rule.ValueCells) 
                Indented($"{FormatType(c.Type)} {c.Name};");

            var fail = end;

            var i = 0;
            // Compile subgoals
            foreach (var call in rule.Body)
                fail = CompileGoal(call, fail, i++);
            var argsToStore = GenerateWriteMode(rule.Head.Arguments);
            Indented($"{rule.Predicate.Name}.Add({argsToStore});");

            Indented(fail.Invoke+";");
            Label(end);
            Output.WriteLine(" ;");  // Can't have a label at the end without an empty statement.
            Output.Write("        }");
            Output.WriteLine();
        }
        
        private string GenerateWriteMode(IMatchOperation[] headArguments)
        {
            string ArgText(IMatchOperation op)
            {
                switch (op.Opcode)
                {
                    case Opcode.Constant:
                        return ToSourceLiteral(op.Cell.BoxedValue);

                    default:
                        return op.Cell.Name;
                }
            }
            var text = string.Join(",", headArguments.Select(ArgText));
            return headArguments.Length == 1 ? text : $"({text})";
        }

        private Continuation CompileGoal(Call call, Continuation fail, int index)
        {
            Indented($"// {call}");
            return call.Compile(this, fail, $"__{index}");
        }

        public string PatternValueExpression(IPattern pattern)
        {
            string Expression(IMatchOperation arg) => arg.Opcode switch
            {
                Opcode.Constant => ToSourceLiteral(arg.Cell.BoxedValue),
                Opcode.Read => arg.Cell.Name,
                _ => throw new InvalidOperationException(
                    $"Can't compile {arg} to a value expression because it's not read or constant mode")
            };
            var args = pattern.Arguments;
            if (args.Length == 1)
                return Expression(args[0]);
            return $"({string.Join(",", args.Select(Expression))})";
        }

        public void CompilePatternMatch(string tupleVar, IPattern p, Continuation fail)
        {
            var patternArgs = p.Arguments;
            var singleColumn = patternArgs.Length == 1;
            for (var i = 0; i < patternArgs.Length; i++) 
                CompileMatch(singleColumn?tupleVar:$"{tupleVar}.Item{i+1}", patternArgs[i], fail);
        }

        public void CompileMatch(string item, IMatchOperation patternArg, Continuation fail)
        {
            switch (patternArg.Opcode)
            {
                case Opcode.Constant:
                    Indented($"if ({item} != {ToSourceLiteral(patternArg.Cell.BoxedValue)}) {fail.Invoke};");
                    break;

                case Opcode.Read:
                    Indented($"if ({item} != {patternArg.Cell.Name}) {fail.Invoke};");
                    break;

                case Opcode.Write:
                    Indented($"{patternArg.Cell.Name} = {item};");
                    break;

                case Opcode.Ignore:
                    break;
            }
        }

        #region Low-level output
        public static string ToSourceLiteral(object? o)
        {
            switch (o)
            {
                case null:
                    return "null";

                case float f:
                    return $"{f}f";

                case Enum e:
                    return $"{e.GetType().Name}.{e}";

                case string s:
                    return $"\"{s}\"";

                default:
                    return o.ToString();
            }
        }

        public void Label(Continuation k) => Indented($"{k.Label}:");

        public void NewLine() => Output.WriteLine();

        public void NewLineIndented() => Output.Write("\n            ");

        public void Indented(string text)
        {
            NewLineIndented();
            Output.Write(text);
        }
        #endregion

        #region Linking

        public static void Link(Program p, bool failOnTypeMissing = false, Type? helper = null)
        {
            if (helper == null)
            {
                var assembly = new StackTrace().GetFrames()[1].GetMethod().DeclaringType.Assembly;
                var helpName = $"{p.Name}Helpers";
                helper = assembly.GetTypes().FirstOrDefault(t =>
                {
                    var a = t.GetCustomAttributes(typeof(CompiledHelpersForAttribute), false);
                    return a.Length > 0 && ((CompiledHelpersForAttribute)a[0]).ProgramName == p.Name;
                });
                if (helper == null)
                {
                    if (failOnTypeMissing)
                        throw new InvalidOperationException(
                            $"The compiled form of the TED program {p.Name} should be in the class {helpName}, but no such class was found.");
                    return;
                }
            }

            // Fill in the static fields of helper with the Table objects of the predicates it references
            foreach (var field in helper.GetFields()
                         .Where(f => f.GetCustomAttributes(typeof(LinkToTableAttribute), false).Length > 0))
            {
                var tableName =
                    ((LinkToTableAttribute)field.GetCustomAttributes(typeof(LinkToTableAttribute), false)[0])
                    .TableName;
                var table = p[tableName].TableUntyped;
                field.SetValue(null, table);
                foreach (var i in table.Indices) 
                    helper.GetField(VariableNameForIndex(i)).SetValue(null, i);
            }

            foreach (var method in helper.GetMethods()
                         .Where(m => m.GetCustomAttributes(typeof(CompiledRulesForAttribute), false).Length > 0))
            {
                var tableName =
                    ((CompiledRulesForAttribute)method.GetCustomAttributes(typeof(CompiledRulesForAttribute), false)[0])
                    .TableName;
                var table = p[tableName];
                table.CompiledRules = (Action)Delegate.CreateDelegate(typeof(Action), method);
            }
        }
        #endregion
    }
}
