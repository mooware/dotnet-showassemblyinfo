using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

if (args.Length < 1)
{
    Console.Error.WriteLine("No assembly file specified");
    return 1;
}

string assemblyPath = args[0];
if (!File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"File '{assemblyPath}' not found");
    return 1;
}

Console.WriteLine($"Assembly metadata of file '{assemblyPath}'");
Console.WriteLine();

var paths = new List<string>() { assemblyPath };

// these seem to be required, see example from
// https://docs.microsoft.com/en-us/dotnet/standard/assembly/inspect-contents-using-metadataloadcontext
paths.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));

var pathResolver = new PathAssemblyResolver(paths);
var context = new MetadataLoadContext(pathResolver);
var assembly = context.LoadFromAssemblyPath(assemblyPath);

var attributes = assembly.GetCustomAttributesData();
foreach (var attr in attributes)
{
    // attribute class names end with Attribute, but it's usually not displayed
    var name = attr.AttributeType.FullName ?? attr.AttributeType.Name;
    if (name.EndsWith("Attribute"))
        name = name.Substring(0, name.Length - 9);

    Console.Write(name);

    if (attr.ConstructorArguments.Count > 0)
    {
        var ctorArgs =
            attr.ConstructorArguments
            .Where(x => x.Value is object)
            .Select(x => x.Value!.ToString());

        Console.Write(": ");
        Console.Write(string.Join(", ", ctorArgs));
    }

    Console.WriteLine();

    if (attr.NamedArguments.Count > 0)
    {
        foreach (var arg in attr.NamedArguments)
            Console.WriteLine($"  {arg.MemberName}: {arg.TypedValue.Value}");
    }
}

Console.WriteLine();
Console.WriteLine("Referenced assemblies");
Console.WriteLine();

foreach (var refAssembly in assembly.GetReferencedAssemblies())
{
    Console.WriteLine(refAssembly);
}

return 0;