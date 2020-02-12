Imports System.IO
Imports System.Reflection
Imports CommandLine
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports ProcessBuilder.Serializable
Imports ObjectTree = ProcessBuilder.Node(Of ProcessBuilder.Serializable.Type, ProcessBuilder.Serializable.JsonValue)

Class Form
    Public Sub New(pair As Pair)
        
    End Sub
End Class

Class Pair
    Public Sub New(left As String, right As String)
        
    End Sub
End Class

Module ProcessBuilder
    
    <CommandLine.Verb("create-target")>
    Class CreateOptions
        <CommandLine.Value(0)>
        Public Property Name As String
    End Class
    
    <CommandLine.Verb("read-target")>
    Class ReadOptions
        <CommandLine.Value(0)>
        Public Property Name As String
    End Class
    
    <CommandLine.Verb("delete-target")>
    Class DeleteOptions
        <CommandLine.Value(0)>
        Public Property Name As String
    End Class
    
    Sub Main(args As String())
        CommandLine.Parser.Default.ParseArguments(Of CreateOptions, ReadOptions, DeleteOptions)(args) _
            .MapResult(
                Function (options As CreateOptions) Create(options),
                Function (options As ReadOptions) Read(options),
                Function (options As DeleteOptions) Delete(options),
                Function (errors) 1)
    End Sub
End Module

Partial Module ProcessBuilder
    Sub New()
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssembly
        
        Targets("sample") = File.ReadAllText("sample-target.json")
    End Sub
    
    Private Function ResolveAssembly(sender As Object, args As ResolveEventArgs) As Reflection.Assembly
        Return AppDomain _
            .CurrentDomain _
            .GetAssemblies _
            .FirstOrDefault(Function (assembly) assembly.FullName = args.Name)
    End Function
End Module

Partial Module ProcessBuilder
    Private ReadOnly Targets As New Dictionary(Of String, String)
    
    Function Create(options As CreateOptions) As Integer
        Using jsonReader As New JsonTextReader(Console.In)
            If jsonReader.Read() Then
                Targets(options.Name) = JObject.ReadFrom(jsonReader).ToString
            Else
                Throw New Exception
            End If
        End Using
        
        Return 0
    End Function
    
    Function Read(options As ReadOptions) As Integer
        Console.WriteLine(Targets(options.Name))
        
        Return 0
    End Function
    
    Function Delete(options As DeleteOptions) As Integer
        Targets.Remove(options.Name)
        Return 0
    End Function
    
    Private Function Indentation(level As Integer) As String
        Return String.Concat(Enumerable.Repeat("    ", level))
    End Function
    
    Private Sub PrettyPrintJToken(level As Integer, token As JToken)
        Select Case token.Type
            Case JTokenType.Object
                Dim jObject = CType(token, JObject)
                Console.WriteLine("{")
                For Each [property] In jObject.Properties()
                    PrettyPrintJToken(level + 1, [property])
                Next
                Console.WriteLine("}")
            Case JTokenType.Property
                Dim jProperty = CType(token, JProperty)
                WithForegroundColor(ConsoleColor.DarkBlue, Sub() Console.Write(jProperty.Name))
                Console.Write(": ")
                PrettyPrintJToken(level + 1, jProperty.Value)
                Console.Write($"{Indentation(level)},")
            Case JTokenType.String
                WithForegroundColor(ConsoleColor.DarkMagenta, Sub() Console.WriteLine(token.ToString))
            Case JTokenType.Array
                Dim jArray = CType(token, JArray)
                Console.WriteLine("[")
                For Each elementToken In jArray
                    PrettyPrintJToken(level + 1, elementToken)
                    Console.WriteLine($"{Indentation(level)},")
                Next
                Console.WriteLine($"{Indentation(level)}]")
        End Select
    End Sub
End Module

Module Dependencies
    Function LeafCase(value As JsonValue) As List(Of String)
        Return New List(Of String) From {value.type.Assembly.Location}
    End Function
    
    Function BranchCase(children As IEnumerable(Of List(Of String)), type As Type) As List(Of String)
        Dim dependencies As New List(Of String) From {type.Assembly.Location}
        dependencies.AddRange(children.SelectMany(AddressOf Identity))
        Return dependencies
    End Function
End Module

Module Instantiate
    Function LeafCase(value As JsonValue) As Object
        Dim valueType As System.Type = System.Type.GetType(value.Type.FullName)
                    
        Dim deserializeObject As Func(Of String, Object) = AddressOf JsonConvert.DeserializeObject(Of Object)
        Dim deserializeGenericMethodDefinition As MethodInfo = deserializeObject.Method.GetGenericMethodDefinition()
        Dim deserialize As MethodInfo = deserializeGenericMethodDefinition.MakeGenericMethod(valueType)
        
        Return deserialize.Invoke(Nothing, New Object(){value.Literal})
    End Function
    
    Function BranchCase(children As IEnumerable(Of Object), type As Serializable.Type) As Object
        Return Activator.CreateInstance(System.Type.GetType(type.FullName), children.ToArray)
    End Function
End Module

Module Print
    Function LeafCase(value As JsonValue) As Action
        Return AddressOf value.Print
    End Function
    
    Function BranchCase(children As IEnumerable(Of Action), type As Serializable.Type) As Action
        Return Sub ()
            WithForegroundColor(ConsoleColor.DarkBlue, Sub() Console.Write("New "))
            WithForegroundColor(ConsoleColor.DarkMagenta, Sub() Console.Write(type.DisplayName))
            Console.Write("(")
            If children.Any Then
                For i = 0 To children.Count - 2
                    children(i).Invoke()
                    Console.Write(", ")
                Next i
                children.Last.Invoke
            End If
            Console.Write(")")
        End Sub
    End Function
End Module