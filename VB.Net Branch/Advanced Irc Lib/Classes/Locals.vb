' Locals.vb
' Advanced IRC Library Project
' See LICENSE file for Copyrights
' Website "http://code.google.com/p/airclib/"

Public MustInherit Class Locals

    Public Structure IrcServer
        Public Server As String
        Public Port As Integer
    End Structure
    Public Structure ChannelMessageData
        Public Server As String
        Public Command As String
        Public Channel As String
        Public Message As String
    End Structure
End Class
