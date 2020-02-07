Namespace Serializable
    Class Assembly
        Implements IEquatable(Of Assembly)
        Public Property FullName As String
        Public Property Location As String
        
        Public Sub New()
            
        End Sub
        
        Public Sub New(assembly As Reflection.Assembly)
            FullName = assembly.FullName
            Location = assembly.Location
        End Sub

        Public Overloads Function Equals(other As Assembly) As Boolean Implements IEquatable(Of Assembly).Equals
            If ReferenceEquals(Nothing, other) Then Return False
            If ReferenceEquals(Me, other) Then Return True
            Return String.Equals(FullName, other.FullName)
        End Function

        Public Overloads Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True
            If obj.GetType IsNot Me.GetType Then Return False
            Return Equals(DirectCast(obj, Assembly))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return FullName.GetHashCode
        End Function

        Public Shared Operator =(left as Assembly, right as Assembly) as Boolean
            Return Equals(left, right)
        End Operator

        Public Shared Operator <>(left as Assembly, right as Assembly) as Boolean
            Return Not Equals(left, right)
        End Operator
    End Class
End Namespace