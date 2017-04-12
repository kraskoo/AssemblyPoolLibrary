# AssemblyPoolLibrary
Contains three classes with dependency between them, ProjectNameContainer is a public non-static
class with a single constant field, for specify a project name, which will be using by AssemblyCommon
class, which is public static class and support a maintainable logic within a properties and methods and
helper methods for extendability a System.Type and System.TypeInfo, but it use with only one purpose to help
providing an AssemblyPool main class, which is again a public static class with just two public methods for getting
an existence project type instances or a adding a new instance specify them by type of this instance.
All the instances are persist from a concurrent dictionary.