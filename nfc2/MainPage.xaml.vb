' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

Imports Windows.Networking.Proximity
Imports NdefLibrary.Ndef
Imports System.Text
Imports System.Net.Http
Imports Newtonsoft.Json
Imports System.Net.Http.Headers
Imports Windows.UI.Core
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    Dim nfcDevice As ProximityDevice
    Private Sub MainPage_Loading(sender As FrameworkElement, args As Object) Handles Me.Loading

        nfcDevice = ProximityDevice.GetDefault()
        If nfcDevice IsNot Nothing Then

            AddHandler nfcDevice.DeviceArrived, AddressOf NFCDevideArrived
            AddHandler nfcDevice.DeviceDeparted, AddressOf NFCDeviceDeparted
            nfcDevice.SubscribeForMessage("NDEF", AddressOf messagedReceived)
        End If
    End Sub

    Private Function messagedReceived(sender As ProximityDevice, message As ProximityMessage) As Task

        Dim rawMsg = message.Data.ToArray

        Dim ndefmsg = NdefMessage.FromByteArray(rawMsg)
        For Each record In ndefmsg
            Dim typ = Encoding.UTF8.GetString(record.Type, 0, record.Type.Length)
            '' If record.CheckSpecializedType(False) = GetType(NdefSpRecord) Then

            Dim txt = New NdefTextRecord(record)
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Sub()
                                                                   text1.Text = txt.Text
                                                               End Sub)

            Dim msg As New ServiceMeldungen
            msg.Datum = Date.Now
            msg.Meldung = txt.Text
            msg.Id = 0
            Task.Run(Function() CreateMessageAsync(msg))
        Next



    End Function

    Private Sub NFCDeviceDeparted(sender As ProximityDevice)

    End Sub

    Private Sub NFCDevideArrived(sender As ProximityDevice)
    End Sub
    Async Function CreateMessageAsync(msg As ServiceMeldungen) As Task

        Dim client = New HttpClient()
        Dim json = JsonConvert.SerializeObject(msg)
        Dim Content = New StringContent(json, Encoding.ASCII, "application/json")
        Content.Headers.ContentType = New MediaTypeHeaderValue("application/json")
        Dim response = Await client.PostAsync(
           "https://iotservice2018.azurewebsites.net/api/servicemeldungens", Content)
        response.EnsureSuccessStatusCode()

    End Function

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Application.Current.Exit()
    End Sub
End Class
