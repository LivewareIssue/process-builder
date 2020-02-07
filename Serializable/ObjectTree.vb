Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.EntityFrameworkCore.Internal
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Serializable
    Public MustInherit Class ObjectTreeNode
        Private _parent As Constructor = Nothing
        
        <JsonIgnore>
        Public Overridable Property Parent As ObjectTreeNode
            Get
                Return _parent
            End Get
            Protected Set
                _parent = Value
            End Set
        End Property
    
        <JsonIgnore>
        Public MustOverride ReadOnly Property Children As IEnumerable(Of ObjectTreeNode)
    End Class
    
    Module ObjectTree
        <Extension>
        Public Sub Serialize(root As ObjectTreeNode, textWriter As TextWriter, Optional jsonSerializer As JsonSerializer = Nothing)
            If IsNothing(jsonSerializer)
                jsonSerializer = New JsonSerializer()
            End If
            Dim jsonWriter As New JsonTextWriter(textWriter) With { .Formatting = jsonSerializer.Formatting }
            
            WithForegroundColor(ConsoleColor.Blue, Sub() Console.Write($"Serializing object-tree: "))
            WithForegroundColor(ConsoleColor.Magenta, Sub() Console.WriteLine(GetNodeType(root).FullName))
            root.Fold(Of Action(Of JsonWriter))(AddressOf GetChildren, Serialize(New JsonSerializer))(jsonWriter)
            textWriter.Flush()
            WithForegroundColor(ConsoleColor.Green, Sub() Console.WriteLine("Done"))
        End Sub
    
        <Extension>
        Public Function Instantiate(root As ObjectTreeNode) As Object
            For Each dependency As Assembly In root _
                .Flatten(Function(node) node.Children) _
                .SelectMany(AddressOf GetDependencies) _
                .Distinct
                WithForegroundColor(ConsoleColor.Blue, Sub() Console.Write("Loading assembly: "))
                Console.WriteLine(dependency.FullName)
                Reflection.Assembly.LoadFile(dependency.Location)
            Next

            Dim instance = root.Fold(Of Object)(AddressOf GetChildren, AddressOf Instantiate)
            
            WithForegroundColor(ConsoleColor.Green, Sub() Console.WriteLine("Done"))
            Return instance
        End Function

        Public Function Deserialize(jObject As JObject) As Tuple(Of ObjectTreeNode, IEnumerable(Of JObject))
            If jObject.Properties.Any(Function ([property]) [property].Name = "DeclaringType") Then
                Dim declaringTypeProperty As JProperty = jObject.Property("DeclaringType")
                Dim declaringType = JsonConvert.DeserializeObject(Of Type)(declaringTypeProperty.Value.ToString)
                
                Dim argumentsProperty As JProperty = jObject.Property("Arguments")
                Dim arguments As JArray = CType(argumentsProperty.Value, JArray)
                
                Return New Tuple(Of ObjectTreeNode,IEnumerable(Of JObject))(New Constructor With { .DeclaringType = declaringType}, arguments.Select(Function (token) CType(token, JObject)))
            Else If jObject.Properties.Any(Function ([property]) [property].Name = "Type") Then
                Dim typeProperty As JProperty = jObject.Property("Type")
                Dim type = JsonConvert.DeserializeObject(Of Type)(typeProperty.Value.ToString)
                
                Dim literalProperty As JProperty = jObject.Property("Literal")
                Dim literal = JsonConvert.DeserializeObject(Of String)(literalProperty.Value.ToString)
                
                If type.FullName = "System.String"
                    literal = $"""{literal}"""
                End If
                
                Return New Tuple(Of ObjectTreeNode,IEnumerable(Of JObject))(New Value(Nothing) With { .Literal = literal, .Type = type }, New JObject(){})
            End If
                
            Throw New InvalidOperationException
        End Function
        
        Private Function Serialize(jsonSerializer As JsonSerializer) As Func(Of IEnumerable(Of Action(Of JsonWriter)), ObjectTreeNode, Action(Of JsonWriter))
            Return Function(childrenSerializers, node)
                Select Case node.GetType()
                    Case GetType(Constructor)
                        Dim declaringType = CType(node, Constructor).DeclaringType
                        Return Sub(jsonWriter)
                            jsonWriter.WriteStartObject
                            jsonWriter.WritePropertyName("DeclaringType")
                            jsonSerializer.Serialize(jsonWriter, declaringType)
                            jsonWriter.WritePropertyName("Arguments")
                            jsonWriter.WriteStartArray()
                            For Each serializer In childrenSerializers
                                serializer(jsonWriter)
                            Next
                            jsonWriter.WriteEndArray()
                            jsonWriter.WriteEndObject
                        End Sub
                    Case GetType(Value)
                        Dim value = CType(node, Value)
                        Return Sub(jsonWriter) jsonSerializer.Serialize(jsonWriter, value)
                    Case Else
                        Return Sub(ignored)
                        End Sub
                End Select
            End Function
        End Function
        
        Private Function Instantiate(arguments As IEnumerable(Of Object), node As ObjectTreeNode) As Object
            Select Case node.GetType()
                Case GetType(Constructor)
                    Dim declaringType = CType(node, Constructor).DeclaringType
                    Dim resolvedType = System.Type.GetType(declaringType.FullName)
                    
                    WithForegroundColor(ConsoleColor.Blue, Sub() Console.Write("Creating instance: "))
                    WithForegroundColor(ConsoleColor.Magenta, Sub() Console.WriteLine(resolvedType.FullName))
                    Return Activator.CreateInstance(resolvedType, arguments.ToArray)
                Case GetType(Value)
                    Dim value = CType(node, Value)
                    Dim valueType As System.Type = System.Type.GetType(value.Type.FullName)
                    
                    Dim deserializeObject As Func(Of String, Object) = AddressOf JsonConvert.DeserializeObject(Of Object)
                    Dim deserializeGenericMethodDefinition As MethodInfo = deserializeObject.Method.GetGenericMethodDefinition()
                    Dim deserialize As MethodInfo = deserializeGenericMethodDefinition.MakeGenericMethod(valueType)
                    
                    WithForegroundColor(ConsoleColor.Blue, Sub() Console.Write("Deserializing: "))
                    Console.Write(value.Literal)
                    WithForegroundColor(ConsoleColor.Blue, Sub() Console.Write(" as "))
                    WithForegroundColor(ConsoleColor.Magenta, Sub() Console.WriteLine(valueType.FullName))
                    Return deserialize.Invoke(Nothing, New Object(){value.Literal})
                Case Else
                    Return Nothing
            End Select
        End Function
        
        Private Function GetDependencies(node As ObjectTreeNode) As IEnumerable(Of Assembly)
            Dim dependencies As New List(Of Assembly)
            Select Case node.GetType()
                Case GetType(Constructor)
                    Dim declaringType = CType(node, Constructor).DeclaringType
                    If LoadRequired(declaringType) Then
                        dependencies.Add(declaringType.Assembly)
                    End If
                Case GetType(Value)
                    Dim value = CType(node, Value)
                    If LoadRequired(value.Type) Then
                        dependencies.Add(value.Type.Assembly)
                    End If
            End Select
            
            Return dependencies
        End Function
        
        Private Function LoadRequired(type As Type) As Boolean
            Try
                Dim runtimeType = System.Type.GetType(type.FullName)
                Return False
            Catch e As TypeLoadException
                Return True
            End Try
        End Function
        
        Private Function GetNodeType(node As ObjectTreeNode) As Type
            Select Case node.GetType()
                Case GetType(Constructor)
                    Dim declaringType = CType(node, Constructor).DeclaringType
                    Return declaringType
                Case GetType(Value)
                    Dim value = CType(node, Value)
                    Return value.Type
                Case Else
                    Return Nothing
            End Select
        End Function
        
        Public Function GetChildren(node As ObjectTreeNode) As IEnumerable(Of ObjectTreeNode)
            Return node.Children
        End Function
        
        Public Sub AddChild(parent As ObjectTreeNode, child As ObjectTreeNode)
            Dim constructor As Constructor = TryCast(parent, Constructor)
            If (constructor IsNot Nothing)
                'Todo: remove 'parameters'
                constructor.Arguments.Add(New Parameter With {
                     .Member= constructor,
                     .Argument = child
                })
            Else
                Throw New InvalidOperationException
            End If
        End Sub
    End Module
End Namespace