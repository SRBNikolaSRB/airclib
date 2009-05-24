' IrcClient.vb
' Advanced IRC Library Project
' See LICENSE file for Copyrights
' Website "http://code.google.com/p/airclib/"

Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports Advanced_Irc_Lib.Locals

Public Class IrcClient
    Inherits Locals

    Public WithEvents irc As New TcpClient
    Public Stream As System.Net.Sockets.NetworkStream

    'Events
    Public Event OnConnect()
    Public Event OnDataSent(ByVal Data As String)
    Public Event OnReciveData(ByVal Data As String)

    'Class vars
    Private isConnected As Boolean
    Private Nick As String
    Private ChannelCount As Integer

    '''<summary>  
    '''Connection event, connects to server IP/Address and port. 2 Overloads.
    '''</summary>
    Public Overloads Sub Connect(ByVal Server As String, ByVal Port As Integer)
        Try
            irc.Connect(Server, Port)
            Stream = irc.GetStream()
            isConnected = True
            RaiseEvent OnConnect()
        Catch e As Exception
            Return
        End Try
    End Sub
    Public Overloads Sub Connect(ByVal IrcServer As IrcServer)
        Try
            irc.Connect(IrcServer.Server, IrcServer.Port)
            Stream = irc.GetStream()
            isConnected = True
            RaiseEvent OnConnect()
        Catch e As Exception
            Return
        End Try
    End Sub

    '''<summary>  
    '''Return isConnected, boolean.
    '''</summary>
    Public Function Connected() As Boolean
        If isConnected = True Then
            Return True
        Else
            Return False
        End If
    End Function
    '''<summary>  
    '''Check's if client is in channel.
    '''</summary>
    Public Function InChannel() As Boolean
        If (isConnected = False Or ChannelCount = 0) Then
            Return False
        Else
            Return True
        End If

    End Function
    '''<summary>  
    '''Returns current connection Nick Name
    '''</summary>
    Public Function GetNick() As String
        If Nick = Nothing Then
            Return Nothing
        Else
            Return Nick
        End If
    End Function

    '''<summary>  
    '''Sends data to currently connected irc server.
    '''</summary>
    Public Sub SendData(ByVal Data As String)
        Try
            Dim Writer As New StreamWriter(Stream)
            Writer.WriteLine(Data, Stream)
            Writer.Flush()
            RaiseEvent OnDataSent(Data)
        Catch e As Exception
            Return
        End Try
    End Sub

    '''<summary>  
    '''Returns irc client current stream as System.Net.Sockets.NetworkStream.
    '''</summary>
    Public Function GetStream() As System.Net.Sockets.NetworkStream
        Return irc.GetStream()
    End Function

    '''<summary>  
    '''Current connection reads all incoming data, if not null it will start OnReciveData event.
    '''</summary>
    Public Sub Listen(ByVal Listen As Boolean)
        If Listen = False Then
            Return
        End If

        If isConnected = False Then
            Return
        End If

        Dim Reader As New StreamReader(Stream)
        Dim Data As String = Reader.ReadLine()
        While (isConnected = True And Data <> "")
            RaiseEvent OnReciveData(Data)
            Me.Listen(True)
        End While

        If (InStr(Data, "PING")) Then
            SendData(Replace(Data, "PING", "PONG"))
        End If

        Reader.Dispose()
        Data = Nothing
        Me.Listen(True)
    End Sub
    '''<summary> 
    '''Fast, unsafe disconnect.
    '''</summary>
    Public Sub Disconnect()
        irc.Close()
    End Sub
    '''<summary> 
    '''Quits from server, Reason needed. 
    '''</summary>
    Public Sub Quit(ByVal Reason As String)
        SendData("QUIT #" & Reason)
        irc.Close()
    End Sub
    Public Sub JoinChannel(ByVal Channel As String)
        SendData("JOIN #" & Channel)
        ChannelCount += 1
        GetTopic(Channel)
        GetNames(Channel)
    End Sub
    Public Sub LeaveChannel(ByVal Channel As String, ByVal Reason As String)
        SendData("PART #" & Channel)
        ChannelCount -= 1
    End Sub

    '''<summary>  
    '''Sets current connection Nick Name
    '''</summary>
    Public Sub SetNick(ByVal Nick As String)
        Try
            SendData("NICK " & Nick)
            Me.Nick = Nick
        Catch e As Exception
            Return
        End Try
    End Sub

    '''<summary>  
    '''Reads data in channel format.
    '''</summary>
    Public Function ReadChannelData(ByVal Data As String) As IrcClient.ChannelMessageData
        If ChannelCount = 0 Then
            Return Nothing
        End If

        Try
            Dim msg As New ChannelMessageData
            Dim MessageArray() As String = Split(Data, " ", 4)

            msg.Server = MessageArray(0)
            msg.Command = MessageArray(1)
            msg.Channel = MessageArray(2)
            msg.Message = MessageArray(3)

            Return msg
        Catch e As Exception
            Return Nothing
        End Try

    End Function
    '''<summary>  
    '''Reads message from channel data.
    '''</summary>
    Public Function ReadMessageFromChannel(ByVal Data As String) As String
        If ChannelCount = 0 Then
            Return Nothing
        End If

        Dim msg As ChannelMessageData = ReadChannelData(Data)

        If (msg.Command <> "PRIVMSG") Then
            Return Data
        End If

        Return msg.Message
    End Function

    Public Function SendMessageToChannel(ByVal Channel As String, ByVal Message As String)
        If isConnected = False Or ChannelCount = 0 Then
            Return Nothing
        End If

        SendData("PRIVMSG " + Channel + " :" + Message)
        Return Nothing
    End Function
    Public Function SendMessageToUser(ByVal User As String, ByVal Message As String)
        If isConnected = False Then
            Return Nothing
        End If

        SendData("PRIVMSG " + User + " :" + Message)
        Return Nothing
    End Function
    Public Function GetTopic(ByVal Channel As String) As String
        If isConnected = False Or ChannelCount = 0 Then
            Return Nothing
        End If

        SendData("TOPIC " & "#" & Channel)
        Return Nothing
    End Function
    Public Function GetNames(ByVal Channel As String) As String
        If isConnected = False Or ChannelCount = 0 Then
            Return Nothing
        End If

        SendData("NAMES #" & Channel)
        Return Nothing
    End Function
    Public Function SetTopic(ByVal Channel As String, ByVal Topic As String)
        If isConnected = False Or ChannelCount = 0 Then
            Return Nothing
        End If

        Dim Data As String = String.Format("TOPIC #{0} :{1}", Channel, Topic)
        SendData(Data)
        Return Data
    End Function
    Public Function WhoIs(ByVal Nick As String)
        If isConnected = False Then
            Return Nothing
        End If

        SendData("WHOIS " & Nick)
        Return Nothing
    End Function
    Public Function DoAction(ByVal Channel As String, ByVal Action As String)
        If isConnected = False Or ChannelCount = 0 Then
            Return Nothing
        End If

        SendData("PRIVMSG #" & Channel & " :ACTION " & Action & "")
        Return Nothing
    End Function
End Class