using TED;
using TED.Utilities;
using static TED.Language;

namespace TablePredicateViewer
{
    internal static class Program
    {
        public static Simulation Simulation = null!;

        enum Status
        {
            Alive,
            Dead
        };

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread] 
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Simulation = new Simulation("Test");
            Simulation.BeginPredicates();
            
            // Variables for rules
            var person = (Var<string>)"person";
            var name = (Var<string>)"name";
            var sex = (Var<string>)"sex";
            var age = (Var<int>)"age";
            var older = (Var<int>)"older";
            var woman = (Var<string>)"woman";
            var man = (Var<string>)"man";
            var mother = (Var<string>)"mother";
            var father = (Var<string>)"father";
            var status = (Var<Status>)"status";

            // ReSharper disable InconsistentNaming

            // Predicates loaded from disk
            var PrimordialPerson = TablePredicate<string, string, int>
                .FromCsv("../../../PrimordialPerson.csv", person, sex, age);
            var Person = Predicate("Person", person.Key, sex.Indexed, age, mother, father, status.Indexed);
            Person.Initially[person, sex, age, "God", "God", Status.Alive].Where(PrimordialPerson[person, sex, age]);
            var MaleName = TablePredicate<string>.FromCsv("../../../MaleName.csv", name);
            var FemaleName = TablePredicate<string>.FromCsv("../../../FemaleName.csv", name);

            // Death
            var Dead = Definition("Dead", person).Is(Person[person, __, __, __, __, Status.Dead]);
            var Alive = Definition("Alive", person).Is(Person[person, __, __, __, __, Status.Alive]);

            //var Died = Predicate("Died", person).If(Person, Prob[0.01f], Alive[person]);
            Person.Set(person, age, age+1).If(Person[person, __, age, __, __, Status.Alive]);
            Person.Set(person, status, Status.Dead).If(Alive[person], Prob[0.01f]);

            // Birth
            var Man = Predicate("Man", person).If(Person[person, "m", age, __, __, Status.Alive], age > 18);
            var Woman = Predicate("Woman", person).If(Person[person, "f", age, __, __, Status.Alive], age > 18);
            var BirthTo = Predicate("BirthTo", woman, man, sex)
                    .If(Woman[woman], Prob[0.1f], RandomElement(Man, man), PickRandomly(sex, "f", "m"));

            Person.Add[person, "f", 0, woman, man, Status.Alive]
                .If(BirthTo[man, woman, "f"], RandomElement(FemaleName, person));
            Person.Add[person, "m", 0, woman, man, Status.Alive]
                .If(BirthTo[man, woman, "m"], RandomElement(MaleName, person));

            // Add births to the population

            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();

            void MakeGraph()
            {
                var g = new TED.Utilities.GraphViz<string>();
                g.GlobalNodeAttributes["style"] = "filled";
                g.AddNode("God");
                foreach (var (who, gender, age, mother, father, status) in Person)
                {
                    g.AddNode(who);
                    g.NodeAttributes[who]["fillcolor"] =
                        (status == Status.Dead) ? "gray" : (gender == "f") ? "pink" : "lightblue";
                    g.AddEdge((who, mother));
                    g.AddEdge((who, father));
                }
                g.WriteGraph("People.dot");
            }

            var timer = new System.Windows.Forms.Timer();
            bool madeGraph = false;
            timer.Tick += (_, _) =>
            {
                UpdateCycle(Person);
                if (Person.Length > 100 && !madeGraph)
                {
                    MakeGraph();
                    madeGraph = true;
                }
            };
            timer.Interval = 100;
            timer.Start();

            PredicateViewer.ShowPredicates(Person, BirthTo, Woman);
            TED.Utilities.DataflowVisualizer.MakeGraph(Simulation, "Dataflow.dot");
            Application.Run(PredicateViewer.Of(Person));
        }

        private static void UpdateCycle(TablePredicate<string, string, int, string, string, Status> person)
        {
            Simulation.Update();
            PredicateViewer.UpdateAll();
        }
    }
}