Namespace Serializable
    Class Type
        Public Property FullName As String
        Public Property Assembly As Assembly
        
        Public Sub New()
            
        End Sub
        
        Public Sub New(type As System.Type)
            FullName = type.FullName
            Assembly = New Assembly(type.Assembly)
        End Sub
    End Class
End Namespace