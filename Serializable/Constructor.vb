Imports Newtonsoft.Json

Namespace Serializable
    Class Constructor
        Inherits ObjectTreeNode
        Public Property DeclaringType As Type
        Public ReadOnly Property Arguments As New List(Of Parameter)
        
        Public Sub New()
            
        End Sub
        
        Public Shared Function Constructor(Of T) As Constructor
            Return New Constructor With {
                .DeclaringType = New Type(GetType(T))
            }
        End Function
        
        Public Overrides ReadOnly Property Children As IEnumerable(Of ObjectTreeNode)
            Get
                Return Arguments.Select(Function(parameter) parameter.Argument)
            End Get
        End Property
        
        Public Sub AddArgument(Of T)(parameterName As String, literal As String)
            Arguments.Add(New Parameter With {
                 .Name = parameterName,
                 .Member = Me,
                 .Argument = New Value(Me) With {
                     .Literal = literal,
                     .Type = New Type(GetType(T))
                 }
            })
        End Sub
        
        Public Sub AddArgument(parameterName As String, constructor As Constructor)
            Arguments.Add(New Parameter With {
                .Name = parameterName,
                .Member = Me,
                .Argument = constructor
            })
        End Sub
    End Class
End Namespace