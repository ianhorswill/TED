using TED;
using static TED.Primitives;

namespace TablePredicateViewer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var Person = TablePredicate<string, string, int>.FromCsv("../../../Population.csv");
            var MaleName = TablePredicate<string>.FromCsv("../../../male_name.csv");
            PredicateViewer.Of(MaleName).SuppressUpdates = true;
            var FemaleName = TablePredicate<string>.FromCsv("../../../female_name.csv");
            PredicateViewer.Of(FemaleName).SuppressUpdates = true;

            var Man = new TablePredicate<string>("man", "Name");
            var Woman = new TablePredicate<string>("woman", "Name");
            var p = (Var<string>)"p";
            var sex = (Var<string>)"sex";
            var a = (Var<int>)"a";
            var Dead = new TablePredicate<string>("dead", "Name");
            var Died = new TablePredicate<string>("died", "Name");
            Died[p].If(Person[p, sex, a], Prob[0.01f], Not[Dead[p]]);

            Man[p].If(Person[p, "m", a], Not[Dead[p]], a > 18);
            Woman[p].If(Person[p, "f", a], Not[Dead[p]], a > 18);
            
            var Birth = new TablePredicate<string, string, string>("birth", "Mom", "Dad", "sex");
            var w = (Var<string>)"w";
            var m = (Var<string>)"m";
            Birth[w, m, sex].If(Woman[w], Prob[0.1f], RandomElement(Man, m), PickRandomly(sex, "f", "m"));
            var NewBorn = new TablePredicate<string, string, int>("newBorn", "Name", "Sex", "age");
            NewBorn[p, "f", 0].If(Birth[m, w, "f"], RandomElement(FemaleName, p));
            NewBorn[p, "m", 0].If(Birth[m, w, "m"], RandomElement(MaleName, p));

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            var timer = new System.Windows.Forms.Timer();
            timer.Tick += (sender, args) => { UpdateCycle(Person, Dead, Died, NewBorn); };
            timer.Interval = 100;
            timer.Start();
            foreach (var pred in TablePredicate.AllTablePredicates)
                PredicateViewer.Of(pred);
            Application.Run(PredicateViewer.Of(Person));
        }

        private static void UpdateCycle(TablePredicate<string, string, int> person, TablePredicate<string> dead, TablePredicate<string> died, TablePredicate<string, string, int> newBorn)
        {
            person.UpdateRows((ref (string name, string sex, int age) row) => row.age++);
            TablePredicate.RecomputeAll();
            dead.Append(died);
            person.Append(newBorn);
            PredicateViewer.UpdateAll();
        }
    }
}