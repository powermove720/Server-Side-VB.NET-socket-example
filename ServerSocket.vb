Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Module SocketServer
    Sub Main()
        Dim listener As TcpListener = New TcpListener(IPAddress.Any, 12345)
        
        Try
            listener.Start()
            Console.WriteLine("Socket server listening on port 12345...")
            
            Dim stopSignal As ManualResetEventSlim = New ManualResetEventSlim(False)
            Dim threads As New List(Of Thread)()
            
            Dim logger As ILogger = New ConsoleLogger()
            Dim errorHandler As ErrorHandler = New ErrorHandler(logger)
            
            While True
                Dim client As TcpClient = listener.AcceptTcpClient()
                Dim stream As NetworkStream = client.GetStream()
                
                Try
                    Dim bytesRead As Integer = stream.Read(bytes, 0, bytes.Length)
                    
                    If bytesRead > 0 Then
                        Dim message As String = Encoding.ASCII.GetString(bytes, 0, bytesRead)
                        logger.LogMessage("Received message: " & message)
                        
                        Dim response As String = "Hello from the server!"
                        Byte[] responseBytes = Encoding.ASCII.GetBytes(response)
                        stream.Write(responseBytes, 0, responseBytes.Length)
                    End If
                Catch ex As SocketException
                    logger.LogError("Socket error: " & ex.Message)
                    errorHandler.HandleError(ex)
                Catch ex As IOException

                    logger.LogError("IO exception: " & ex.Message)
                    errorHandler.HandleError(ex)
                Finally
                    client.Close()
                End Try
                
                Dim thread As Thread = New Thread(ProcessClient)
                thread.Start(stream, logger)
                threads.Add(thread)
            End While
            
            stopSignal.Set()
            
            For Each thread In threads
                thread.Join()
            Next
        Catch ex As Exception
            logger.LogError("Socket server error: " & ex.Message)
            errorHandler.HandleError(ex)
        Finally
            listener.Stop()
        End Try
        
        Console.ReadLine()
    End Sub
    
    Private Sub ProcessClient(ByVal parameter As Object, ByVal logger As ILogger)
        Dim stream As NetworkStream = CType(parameter, NetworkStream)
        
        Try
            While True
                Dim bytesRead As Integer = stream.Read(bytes, 0, bytes.Length)
                
                If bytesRead > 0 Then
                    Dim message As String = Encoding.ASCII.GetString(bytes, 0, bytesRead)
                    logger.LogMessage("Received message: " & message)
                    
                    Dim response As String = "Hello from the server!"
                    Byte[] responseBytes = Encoding.ASCII.GetBytes(response)
                    stream.Write(responseBytes, 0, responseBytes.Length)
                End If
            End While
        Catch ex As SocketException
            logger.LogError("Socket error: " & ex.Message)
            errorHandler.HandleError(ex)
        Catch ex As IOException

            logger.LogError("IO exception: " & ex.Message)
            errorHandler.HandleError(ex)
        Finally
            stream.Close()
        End Try
        
        stopSignal.WaitOne(Timeout.Infinite)
    End Sub
    
    Private Class ErrorHandler
        Private logger As ILogger
        
        Public Sub New(ByVal logger As ILogger)
            Me.logger = logger
        End Sub
        
        Public Sub HandleError(ByVal ex As Exception)
            logger.LogError("Socket server error: " & ex.Message)
        End Sub
    End Class
    
    Private Interface ILogger
        Sub LogMessage(ByVal message As String)
        Sub LogError(ByVal errorMessage As String)
    End Interface
    
    Private Class ConsoleLogger : Implements ILogger
        Public Sub LogMessage(ByVal message As String)
            Console.WriteLine(message)
        End Sub
        
        Public Sub LogError(ByVal errorMessage As String)
            Console.WriteLine("Error: " & errorMessage)
        End Sub
    End Class
End Module
