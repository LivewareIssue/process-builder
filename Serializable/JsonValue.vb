Imports Newtonsoft.Json

Namespace Serializable
    Public Class JsonValue
        Public Property Type As Type
        Public Property Literal As String
        
        Public Shared Function [Of](Of T)(value As T)
            Return New JsonValue With {
                .Type = New Type(GetType(T)),
                .Literal = JsonConvert.SerializeObject(value)
            }
        End Function
        
        Public Sub Print()
            Console.Write(Literal)
            WithForegroundColor(ConsoleColor.DarkBlue, Sub() Console.Write(" As "))
            WithForegroundColor(ConsoleColor.DarkMagenta, Sub() Console.Write(Type.DisplayName))
        End Sub
    End Class
End NameSpace