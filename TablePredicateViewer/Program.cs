using TED;
using static TED.Language;

namespace TablePredicateViewer
{
    internal static class Program
    {
        public static Simulation Simulation = null!;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread] 
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Simulation = new Simulation("Test");
            
            // Variables for rules
            var person = (Var<string>)"person";
            var name = (Var<string>)"name";
            var sex = (Var<string>)"sex";
            var age = (Var<int>)"age";
            var woman = (Var<string>)"woman";
            var man = (Var<string>)"man";

            // ReSharper disable InconsistentNaming

            // Predicates loaded from disk
            var Person = TablePredicate<string, string, int>.FromCsv("../../../Population.csv", person, sex, age);
            Person.IndexBy(1);
            var MaleName = TablePredicate<string>.FromCsv("../../../male_name.csv", name);
            var FemaleName = TablePredicate<string>.FromCsv("../../../female_name.csv", name);

            // Death
            var Dead = Predicate("Dead", person);
            var Alive = Definition("Alive", person).Is(Not[Dead[person]]);

            var Died = Predicate("Died", person).If(Person, Prob[0.01f], Alive[person]);
            Dead.Accumulates(Died);

            // Birth
            var Man = Predicate("Man", person).If(Person[person, "m", age], Alive[person], age > 18);
            var Woman = Predicate("Woman", person).If(Person[person, "f", age], Alive[person], age > 18);
            var BirthTo = Predicate("BirthTo", woman, man, sex)
                    .If(Woman[woman], Prob[0.1f], RandomElement(Man, man), PickRandomly(sex, "f", "m"));

            // Naming of newborns
            var NewBorn = Predicate("NewBorn", person, sex, age);
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
            Simulation.Update();
            PredicateViewer.UpdateAll();
        }
    }
}