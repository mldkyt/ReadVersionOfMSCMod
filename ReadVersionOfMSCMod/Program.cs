using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MSCLoader;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace ReadVersionOfMSCMod
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ReadVersionUsingMSCLoaderMethod(args[0]);
        }

        private static void ReadVersionUsingMSCLoaderMethod(string path)
        {
            var moduleDefinition = ModuleDefinition.ReadModule(path);
            Console.WriteLine($"{moduleDefinition.Name} {moduleDefinition.RuntimeVersion}");
            var modClass = DetermineMainModClass(moduleDefinition);
            Console.WriteLine($"Main Mod Class {modClass.FullName}");
            var version = ReadModVersion(modClass);
            Console.WriteLine($"Mod Version: {version}");
        }

        private static TypeDefinition DetermineMainModClass(ModuleDefinition assembly)
        {
            foreach (var type in assembly.Types.Where(type => type.BaseType?.FullName == "MSCLoader.Mod"))
            {
                return type;
            }

            throw new Exception("Mod class not found");
        }

        private static string ReadModVersion(TypeDefinition mainModClass)
        {
            var versionProperty = mainModClass.Properties.FirstOrDefault(x => x.Name == "Version");
            if (versionProperty == null)
            {
                Console.WriteLine("Mod class doesn't contain Version property");
                return "";
            }

            Console.WriteLine(mainModClass.FullName);
            
            var getVersionInstructions = versionProperty.GetMethod.Body.Instructions;
            
            if (getVersionInstructions.Count > 2 && getVersionInstructions[1].OpCode == OpCodes.Ldstr)
            {
                return getVersionInstructions[1].Operand.ToString();
            }

            return "0.0";
        }
    }
}