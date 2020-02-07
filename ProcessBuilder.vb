Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports ProcessBuilder.Serializable

Module ProcessBuilder
    Public Sub Main(args As String())
        Dim serializer As New JsonSerializer With { .Formatting = Formatting.Indented }
        
        Dim rootNode As Constructor = Constructor.Constructor(Of TestForm)
        Dim childNode As Constructor = Constructor.Constructor(Of Test)
        
        rootNode.AddArgument("test", ChildNode)
        childNode.AddArgument(Of String)("s", """hello""")
        childNode.AddArgument(Of Integer)("n", "1")
        
        'rootNode.Instantiate()
        Dim foo As ObjectTreeNode = Nothing
        Using stream As New MemoryStream()
            Using writer As New StreamWriter(stream, Encoding.UTF8, 1024, True)
                rootNode.Serialize(writer, serializer)
            End Using
            
            stream.Seek(0, SeekOrigin.Begin)
            
            Using reader As New StreamReader(stream)
                Dim jsonReader As JsonReader = New JsonTextReader(reader)
                Dim root As JObject = JObject.Load(jsonReader)
                foo = root.Unfold(Of ObjectTreeNode)(AddressOf AddChild, AddressOf Deserialize)
            End Using
        End Using
        
        Using stream As New MemoryStream()
            Using writer As New StreamWriter(stream, Encoding.UTF8, 1024, True)
                foo.Serialize(writer, serializer)
            End Using
            
            Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray))
        End Using
    End Sub
End Module

Partial Module ProcessBuilder
    Sub New()
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssembly
    End Sub
    
    Private Function ResolveAssembly(sender As Object, args As ResolveEventArgs) As Reflection.Assembly
        Return AppDomain _
            .CurrentDomain _
            .GetAssemblies _
            .FirstOrDefault(Function (assembly) assembly.FullName = args.Name)
    End Function
    
    Public Sub WithForegroundColor(color As ConsoleColor, action As Action)
        Dim currentColor As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = color
        action()
        Console.ForegroundColor = currentColor
    End Sub
End Module

