namespace Tester
{
    using Library;
    using System;

    public class EntryPoint
    {
        public static void Main()
        {
            var instanceOfThisClassWithDependency = AssemblyPool.GetInstance<SomeClassWithDependency>();
            Console.WriteLine(instanceOfThisClassWithDependency);
            var instanceOfThisInterface = AssemblyPool.GetInstance<ISomeInterface>();
            Console.WriteLine(instanceOfThisInterface);
            Console.WriteLine();
        }
    }

    public interface ISomeInterface
    {
        int ClassSomeInt { get; }

        string ClassSomeString { get; }

        DateTime ClassSomeDate { get; }
    }

    public class SomeClassWithoutDependencies : ISomeInterface
    {
        public SomeClassWithoutDependencies()
        {
            this.ClassSomeInt = 2355678;
            this.ClassSomeString = "Some String";
            this.ClassSomeDate = DateTime.Now;
        }

        public int ClassSomeInt { get; set; }

        public string ClassSomeString { get; set; }

        public DateTime ClassSomeDate { get; set; }
    }

    public class SomeClassWithDependency
    {
        private SomeClassWithoutDependencies withoutDependencies;

        public SomeClassWithDependency(SomeClassWithoutDependencies withoutDependencies)
        {
            this.withoutDependencies = withoutDependencies;
        }
    }
}