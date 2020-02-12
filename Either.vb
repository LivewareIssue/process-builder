Imports System.Runtime.CompilerServices

Interface IEither(Of TLeft, TRight)
    Function Either(Of TResult)(selectLeft As Func(Of TLeft, TResult), selectRight As Func(Of TRight, TResult)) As TResult
    
    NotInheritable Class Left
        Implements IEither(Of TLeft, TRight)
        
        Private ReadOnly _value As TLeft
        
        Sub New(value As TLeft)
            _value = value
        End Sub

        Public Function Either (Of TResult)(selectLeft As Func(Of TLeft,TResult), selectRight As Func(Of TRight,TResult)) As TResult Implements IEither(Of TLeft,TRight).Either
            Return selectLeft(_value)
        End Function
    End Class
    
    NotInheritable Class Right
        Implements IEither(Of TLeft, TRight)
        
        Private ReadOnly _value As TRight
        
        Sub New(value As TRight)
            _value = value
        End Sub

        Public Function Either (Of TResult)(selectLeft As Func(Of TLeft,TResult), selectRight As Func(Of TRight,TResult)) As TResult Implements IEither(Of TLeft,TRight).Either
            Return selectRight(_value)
        End Function
    End Class
End Interface

Module Either
    Public Function [Of](Of TLeft, TRight)(value As TLeft)
        Return New IEither(Of TLeft, TRight).Left(value)
    End Function
    
    Public Function [Of](Of TLeft, TRight)(value As TRight)
        Return New IEither(Of TLeft, TRight).Right(value)
    End Function
End Module