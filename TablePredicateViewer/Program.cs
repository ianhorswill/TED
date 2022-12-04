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

            // Variables for rules
            var person = (Var<string>)"person";
            var sex = (Var<string>)"sex";
            var age = (Var<int>)"age";
            var woman = (Var<string>)"woman";
            var man = (Var<string>)"man";

            // ReSharper disable InconsistentNaming

            // Predicates loaded from disk
            var Person = TablePredicate<string, string, int>.FromCsv("../../../Population.csv");
            var MaleName = TablePredicate<string>.FromCsv("../../../male_name.csv");
            var FemaleName = TablePredicate<string>.FromCsv("../../../female_name.csv");

            // Death
            var Dead = new TablePredicate<string>("Dead", person);
            var Died = new TablePredicate<string>("Died", person).If(Person[person, sex, age], Prob[0.01f], Not[Dead[person]]);
            Dead.Accumulates(Died);

            // Birth
            var Man = new TablePredicate<string>("Man", person).If(Person[person, "m", age], Not[Dead[person]], age > 18);
            var Woman = new TablePredicate<string>("Woman", person).If(Person[person, "f", age], Not[Dead[person]], age > 18);
            var BirthTo = new TablePredicate<string, string, string>("BirthTo", woman, man, sex)
                    .If(Woman[woman], Prob[0.1f], RandomElement(Man, man), PickRandomly(sex, "f", "m"));

            // Naming of newborns
            var NewBorn = new TablePredicate<string, string, int>("NewBorn", person, sex, age);
            NewBorn[person, "f", 0].If(BirthTo[man, woman, "f"], RandomElement(FemaleName, person));
            NewBorn[person, "m", 0].If(BirthTo[man, woman, "m"], RandomElement(MaleName, person));

            // Add births to the population
            Person.Accumulates(NewBorn);
            // ReSharper restore InconsistentNaming

            var timer = new System.Windows.Forms.Timer();
            timer.Tick += (_, _) => { UpdateCycle(Person); };
            timer.Interval = 100;
            timer.Start();
            PredicateViewer.ShowPredicates(Person, Dead, BirthTo, NewBorn, Woman);
            Application.Run(PredicateViewer.Of(Person));
        }

        private static void UpdateCycle(TablePredicate<string, string, int> person)
        {
            person.UpdateRows((ref (string name, string sex, int age) row) => row.age++);
            TablePredicate.RecomputeAll();
            TablePredicate.AppendAllInputs();
            PredicateViewer.UpdateAll();
        }
    }
}