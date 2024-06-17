using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TED.Interpreter;
using TED.Tables;

namespace TED.Compiler
{
    public class Compiler : IDisposable, IAsyncDisposable
    {
        public readonly Program Program;

        public readonly string NamespaceName;

        public readonly string ClassName;

        public readonly Stack<TextWriter> OutputStack = new Stack<TextWriter>();

        public TextWriter Output => OutputStack.Peek();

        private static string DefaultClassName(string programName) => $"{programName}__Compiled";

        public Compiler(Program program, string namespaceName, string dirPath)
            : this(program, namespaceName, DefaultClassName(program.Name), new StreamWriter(Path.Combine(dirPath, $"{DefaultClassName(program.Name)}.cs")))
        { }

        public Compiler(Program program, string namespaceName, string className, TextWriter output)
        {
            Program = program;
            ClassName = className;
            OutputStack.Push(output);
            NamespaceName = namespaceName;
        }

        #region Top level
        public void GenerateSource()
        {
            try
            {
                Output.WriteLine("// ReSharper disable InconsistentNaming");
                Output.WriteLine("// ReSharper disable JoinDeclarationAndInitializer");
                Output.WriteLine("// ReSharper disable RedundantUsingDirective");

                Output.WriteLine("using TED;");
                Output.WriteLine("using TED.Interpreter;");
                Output.WriteLine("using TED.Compiler;");
                Output.WriteLine("using TED.Tables;");
                
                NewLine();
                Output.WriteLine("// ReSharper disable once CheckNamespace");
                Output.WriteLine($"namespace {NamespaceName};");
                NewLine();
                Output.WriteLine("#pragma warning disable 0164,8618,8600,8620");
                NewLine();
                
                Output.WriteLine($"[CompiledHelpersFor(\"{Program.Name}\")]");
                Output.WriteLine($"public class {ClassName} : TED.Compiler.CompiledTEDProgram");
                CurlyBraceBlock(CompileProgram);
                Output.WriteLine();

                Output.WriteLine("#pragma warning restore 0164,8618,8600,8620");
            }
            finally
            {
                Output.Close();
            }
        }

        private void CompileProgram()
        {
            foreach (var predicate in Program.Tables)
            {
                var table = predicate.TableUntyped;
                //Indented($"[LinkToTable(\"{predicate.Name}\")]");
                //Indented($"public static {FormatType(table.GetType())} {table.Name};");

                var tableType = table.GetType();
                Field(predicate.Name,
                    tableType,
                    $"({FormatType(tableType)})program[\"{predicate.Name}\"].TableUntyped");

                foreach (var index in table.Indices)
                {
                    var indexType = index.GetType();
                    Field(VariableNameForIndex(index), indexType,
                        $"({FormatType(indexType)}){predicate.Name}.IndexFor({string.Join(", ", index.ColumnNumbers.Select(n => n.ToString()))})");
                }
                //    Indented($"public static {FormatType(index.GetType())} {VariableNameForIndex(index)};");
            }
            Output.WriteLine();

            foreach (var table in Program.Tables)
            {
                if (table.UpdateMode == UpdateMode.Rules)
                {
                    CompileTable(table);
                }
            }

            Output.WriteLine();
            Indented("public override void Link(TED.Program program)");
            CurlyBraceBlock(CompileInitializers);
            NewLine();

            foreach (var field in Fields)
                Indented($"public static {FormatType(field.Type)} {field.Name};");
        }

        private void CompileInitializers()
        {
            foreach (var u in tableUpdaters)
                Indented($"program[\"{u.TableName}\"].CompiledRules = (Action){u.Updater};");
            foreach (var f in Fields)
                Indented($"{f.Name} = {f.Initializer};");
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
            if (t.IsArray)
                return $"{FormatType(t.GetElementType()!)}[]";
            if (!t.IsGenericType)
            {
                if (t == typeof(int))
                    return "int";
                if (t == typeof(uint))
                    return "uint";
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

        private List<(string TableName, string Updater)> tableUpdaters = new List<(string TableName, string Updater)>(); 

        private void CompileTable(TablePredicate table)
        {
            var tableName = table.Name;
            var updaterName = $"{tableName}__CompiledUpdate";
            tableUpdaters.Add((tableName, updaterName));

            Indented($"public static void {updaterName}()");
            var nextRule = new Continuation(table.Rules!.Count > 1 ? "rule2" : EndLabel);
            CurlyBraceBlock(() =>
            {
                for (var ruleNumber = 1; ruleNumber <= table.Rules!.Count; ruleNumber++)
                {
                    CompileRule(table.Rules![ruleNumber-1], nextRule);
                    Label(nextRule); 
                    if (nextRule.Label != EndLabel)
                        Output.WriteLine(";");
                    nextRule = new Continuation(ruleNumber+2 > table.Rules!.Count ? EndLabel:$"rule{ruleNumber+2}");
                }
                Output.Write(';');
            });
        }

        private void CompileRule(Rule rule, Continuation end)
        {
            var predicate = rule.Predicate;
            var ruleNumber = predicate.Rules!.IndexOf(rule)+1;
            
            Indented($"// {rule}");

            CurlyBraceBlock(() =>
            {
                // Declare locals
                foreach (var c in rule.ValueCells)
                    Indented($"{FormatType(c.Type)} {c.Name};");

                var fail = end;

                // Compile subgoals
                fail = CompileBody(rule.Body, fail, $"_");

                Output.WriteLine();
                Indented($"// Write {rule.Head}");
                var argsToStore = GenerateWriteMode(rule.Head.Arguments);
                Indented($"{rule.Predicate.Name}.Add({argsToStore});");

                Indented(fail.Invoke + ";");
            });
            Output.WriteLine();
        }
        #endregion

        /// <summary>
        /// Compile the code compiled by generator, but move any variables it declares using LocalVariable to the beginning.
        /// This is needed because some parts of the state machine generated by the compiler have data dependencies that the
        /// C# compiler's control flow analyzer can't follow and so a direct translation of the output generates bogus CS0165
        /// errors complaining that local variables are used before they're initialized.  Wrapping code in this forces the
        /// variables to be initialized early.  You only need to use this if you use CompileJumpTable to create jumps into the
        /// middle of the block generated by this.  As of this writing, the only use case for this is the Or[] primitive.
        /// </summary>
        public void WithLiftedScope(Action generator)
        {
            if (liftedVariables != null)
                // Already lifting
                generator();
            else
            {
                liftedVariables = new List<(string VariableName, Type Type)>();
                try
                {
                    var compiledText = CompileToString(generator);

                    foreach (var v in liftedVariables)
                    {
                        var type = FormatType(v.Type);
                        Indented($"{type} {v.VariableName} = default({type});");
                    }

                    NewLine();
                    Output.Write(compiledText);
                }
                finally
                {
                    liftedVariables = null;
                }
            }
        }

        #region Declaration handling
        private List<(string VariableName, Type Type)>? liftedVariables;

        /// <summary>
        /// Generates the code necessary to have a local variable here with the specified name, type, and initializer.
        /// This is usually just a var name = initializer statement, but if we have lifted scope, the declaration will
        /// be moved earlier and the line generated here will just assign the value.
        /// </summary>
        /// <param name="name">Name to give to the variable</param>
        /// <param name="type">type to give to the variable</param>
        /// <param name="initializer">text of expression for its initial value</param>
        /// <param name="liftIfNecessary">If false, never lift.</param>
        /// <returns>name</returns>
        public string LocalVariable(string name, Type type, string initializer, bool liftIfNecessary = true)
        {
            if (liftedVariables != null)
            {
                liftedVariables.Add((name, type));
                Indented($"{name} = {initializer};");
            }
            else
                Indented($"var {name} = {initializer};");

            return name;
        }

        private List<(string Name, Type Type, string Initializer)> Fields = new List<(string fieldName, Type Type, string initializer)>();

        /// <summary>
        /// Add the specified field with the specified type an initializer to the class containing the compiled code.
        /// This can be used to link to outside data (e.g. tables) or to retain state from update to update (e.g. RNGs).
        /// </summary>
        /// <param name="name">Name to give to the field</param>
        /// <param name="type">date type for the field</param>
        /// <param name="initializer">Expression for an initial value.  This is assigned during linking.</param>
        /// <returns>The name</returns>
        public string Field(string name, Type type, string initializer)
        {
            Fields.Add((name, type, initializer));
            return name;
        }

        private Dictionary<string, int> fieldCounters = new Dictionary<string, int>();

        public string FieldUniqueName(string name, Type type, string initializer)
        {
            int uid;
            if (fieldCounters.TryGetValue(name, out var counter))
            {
                fieldCounters[name] = uid = counter + 1;
            }
            else
                uid = 0;

            return Field($"{name}__{uid}", type, initializer);
        }

        private int RngCounter;
        public string MakeRng() => Field($"_Rng{RngCounter++}", typeof(Random), "MakeRng()");
        #endregion

        #region Utilities for compiling common expression types
        /// <summary>
        /// Compile a series of goals (basically a conjunction)
        /// </summary>
        /// <param name="body">Goals to compile</param>
        /// <param name="fail">Where to fail to if the body fails</param>
        /// <param name="identifierSuffix">String that if placed after an identifier will guarantee the identifier is unique</param>
        /// <returns>Continuation that will restart this body</returns>
        public Continuation CompileBody(IEnumerable<Call> body, Continuation fail, string identifierSuffix)
        {
            var currentFail = fail;
            int goalNumber = 0;
            foreach (var call in body)
            {
                Output.WriteLine();
                currentFail = CompileGoal(call, currentFail, $"{identifierSuffix}_{goalNumber++}");
            }

            return currentFail;
        }
        
        /// <summary>
        /// Compile a single goal
        /// </summary>
        /// <param name="call">Goal to compile</param>
        /// <param name="fail">Where this goal should fail to if it fials</param>
        /// <param name="identifierSuffix">String that if placed after an identifier will guarantee the identifier is unique</param>
        /// <returns>Continuation that will restart this goal</returns>
        public Continuation CompileGoal(Call call, Continuation fail, string identifierSuffix)
        {
            Indented($"// {call}");
            return call.Compile(this, fail, identifierSuffix);
        }

        /// <summary>
        /// Returns a string that, when executed will give you the value of the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to get the value of </param>
        /// <returns>Expression that computes the vlaue of the pattern (usually a tuple)</returns>
        /// <exception cref="InvalidOperationException">If pattern is not fully instantiated</exception>
        public string PatternValueExpression(IPattern pattern)
            => pattern.Arguments.Length switch
            {
                1 => ArgumentExpression(pattern.Arguments[0]),
                _ => $"({ArgumentExpressionsWithCommas(pattern)})"
            };

        public string ArgumentExpression(IMatchOperation arg) => arg.Opcode switch
        {
            Opcode.Constant => ToSourceLiteral(arg.Cell.BoxedValue),
            Opcode.Read => arg.Cell.Name,
            _ => throw new InvalidOperationException(
                $"Can't compile {arg} to a value expression because it's not read or constant mode")
        };

        public IEnumerable<string> ArgumentExpressions(IPattern p) => ArgumentExpressions(p.Arguments); 

        public IEnumerable<string> ArgumentExpressions(IEnumerable<IMatchOperation> args) =>
            args.Select(ArgumentExpression);

        public string ArgumentExpressionsWithCommas(IPattern p) => string.Join(", ", ArgumentExpressions(p));

        /// <summary>
        /// Emit code to match tupleVar to the specified pattern.
        /// This will update variables or compare them to tuple values, depending on the read/write modes of the pattern's arguments.
        /// </summary>
        /// <param name="tupleVar">Name of the variable containing the tuple</param>
        /// <param name="p">Pattern to match the tuple to</param>
        /// <param name="fail">Where to branch to if the match fails (falls through for success)</param>
        public void CompilePatternMatch(string tupleVar, IPattern p, Continuation fail)
        {
            var patternArgs = p.Arguments;
            var singleColumn = patternArgs.Length == 1;
            for (var i = 0; i < patternArgs.Length; i++) 
                CompileMatch(singleColumn?tupleVar:$"{tupleVar}.Item{i+1}", patternArgs[i], fail);
        }

        /// <summary>
        /// Emit code to match item to the specified pattern argument (variable+read/write mode or constant).
        /// This will update variables or compare them to tuple values, depending on the read/write modes of the pattern's arguments.
        /// </summary>
        /// <param name="item">Code for expression containing the item</param>
        /// <param name="patternArg">Match operation</param>
        /// <param name="fail">Target to branch to upon failure (falls through for success)</param>
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

        /// <summary>
        /// Emits code to branch to the controlExpression'th label in targets.
        /// </summary>
        /// <param name="controlExpression">Integer variable containing the number of the target to branch to</param>
        /// <param name="targets">The targets, in order</param>
        public void CompileJumpTable(string controlExpression, IEnumerable<Continuation> targets)
        {
            Indented($"switch ({controlExpression})");
            CurlyBraceBlock(() =>
            {
                var i = 0;
                foreach (var target in targets)
                    Indented($"case {i++}: {target.Invoke};");
            });
        }
        #endregion

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

                case int[] intArray:
                {
                    var b = new StringBuilder();
                    b.Append("new [] { ");
                    foreach (var e in intArray)
                    {
                        b.Append(e);
                        b.Append(", ");
                    }

                    b.Append("}");
                    return b.ToString();
                }
 
                default:
                    return o.ToString();
            }
        }

        public void Label(Continuation k, bool includeEmptyStatement = false)
        {
            Indented($"{k.Label}:");
            if (includeEmptyStatement)
                Output.Write(" ;");
        }

        public void NewLine() => Output.WriteLine();

        private int indentLevel;
        private const string EndLabel = "end";

        public void NewLineIndented()
        {
            Output.Write(Output.NewLine);
            for (int i = 0; i < indentLevel*4;i++) Output.Write(' ');
        }

        public void Indented(string text)
        {
            NewLineIndented();
            Output.Write(text);
        }

        public void FurtherIndented(Action code)
        {
            indentLevel++;
            try
            {
                code();
            }
            finally
            {
                indentLevel--;
            }
        }

        public void CurlyBraceBlock(Action code)
        {
            Indented("{");
            FurtherIndented(code);
            Indented("}");
        }

        private string CompileToString(Action generator)
        {
            OutputStack.Push(new StringWriter());
            string results;
            try
            {
                generator();
            }
            finally
            {
                results = OutputStack.Pop().ToString();
            }

            return results;
        }
        #endregion

        #region Linking
        public static void Link(Program p, bool failOnTypeMissing = false, Type? helper = null)
        {
            if (helper == null)
            {
                var assembly = new StackTrace().GetFrames()[1].GetMethod().DeclaringType.Assembly;
                var helpName = DefaultClassName(p.Name);
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
            
            ((CompiledTEDProgram)helper.InvokeMember(null, BindingFlags.CreateInstance, null, null,
                    Array.Empty<object>())).Link(p);
        }
        #endregion

        public void Dispose()
        {
            Output.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Output.DisposeAsync();
        }
    }
}
