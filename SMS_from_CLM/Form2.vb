Imports System.IO.Ports
Imports System.Xml
Public Class Form2
    Dim TargetPhone As String
    Dim TargetPort As String
    Dim Autoport As String = ""
    Dim Message
    Dim separators As String = " "
    Dim commands As String = Command()
    Dim args() As String = commands.Split(separators.ToCharArray)

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        KillThemAll()
        ReadSettings()

        'AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

        If args.Count >= 1 Then
            If args(0) = "Setting" And args.Count = 1 Then 'открыть настройки
                Me.Show()       '
                TextBox1.Text = TargetPort
                'добавить порты
                PortsAdd()

                If Autoport = "true" Then
                        CheckBox1.Checked = True
                    End If

                    TextBox1.ForeColor = Color.DarkGreen
                    TextBox1.Text = TargetPort
                    TextBox2.Text = TargetPhone


                'проверка на пустое поле ввода команды
                If TextBox3.Text = "" Then
                    Button3.Enabled = False
                End If
                'горячие кнопки
                AcceptButton = Button1  'enter
                CancelButton = Button5  'esc
                'загрузка чёрного списка
                Try
                    ListBox1.Items.AddRange(IO.File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list"))
                Catch ex As Exception
                End Try
            Else 'если первый парамеир не сеттинг то шлём смс
                'загоняем параметры в переменную Messаge
                For i = 0 To args.Count - 1
                    Message = Message & " " & args(i)
                Next
                Try
                    If Autoport = "true" Then
                        GetPort()
                    End If
                    sms()    'я эсэсмэско!
                    LogMrg() 'удаление пустых строк из лога   
                    End
                Catch ex As Exception
                    Dim D As Date = Now
                    My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " СМС с текстом: '" & Text & " не была послана на: " & TargetPhone & vbNewLine & ex.Message & vbNewLine, True)
                    End
                End Try
                LogMrg() 'удаление пустых строк из лога   
                End
            End If
        ElseIf args.Count = 0 Then
            End
        End If

    End Sub

    'считать настройки
    Private Sub ReadSettings()
        If IO.File.Exists((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")) = True Then
            Try
                Dim Xml As New System.Xml.XmlDocument()
                Dim XmlNodes As XmlNodeList, xNode As XmlNode
                Xml.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")
                xNode = Xml.DocumentElement
                xNode = Xml.DocumentElement.SelectSingleNode("Phone")
                TargetPhone = xNode.FirstChild.Value
                xNode = Xml.DocumentElement.SelectSingleNode("COM")
                TargetPort = xNode.FirstChild.Value
                xNode = Xml.DocumentElement.SelectSingleNode("Autoport")
                Autoport = xNode.FirstChild.Value
                'xNode = Xml.DocumentElement.SelectSingleNode("Speed")
                'Speed = xNode.FirstChild.Value
                Xml = Nothing
                XmlNodes = Nothing
                xNode = Nothing
            Catch ex As Exception
                Dim D As Date = Now
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & ": ReadSettings_ERROR: " & ex.Message & vbNewLine, True)
            End Try
        Else
            Reset()
        End If
    End Sub

    Public Sub KillThemAll()
        Try 'получаем список процессоф всяких конектманагеров и убиваем их нахрен
            Dim killlist() As String = IO.File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list")
            For i = 0 To killlist.Count - 1
                ' shell("cmd.exe -c " & killlist(i))
                Process.GetProcessesByName(killlist(i))(0).Kill()
            Next
        Catch ex As Exception
        End Try
    End Sub

    Public Sub LogMrg()
        'удаление пустых строк из лога
        Try
            Dim textS As String = IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt")
            Dim textE = textS.Replace(New Char() {vbLf, vbCr}, "").TrimEnd(New Char() {vbLf, vbCr})
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", textE & vbNewLine, False)
        Catch ex As Exception
        End Try
    End Sub

    'sms PDU mode
    Public Sub sms()
        Dim portcheck As String
        Dim telnum As String
        Try
            portcheck = TargetPort
            telnum = TargetPhone

            If Autoport = "true" Then
                GetPort()
            End If

            'проверка корректности вводных дданных
            If telnum = "" And portcheck <> "" Then
                Dim D As Date = Now
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " Укажите номер для отправки!" & vbNewLine, True)
                Exit Sub
            ElseIf portcheck = "" And telnum <> "" Then
                Dim D As Date = Now
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " Укажите порт используемый вашим модемом!" & vbNewLine, True)
                Exit Sub
            ElseIf portcheck = "" And telnum = "" Then
                Dim D As Date = Now
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " Укажите номер телефона и порт для отправки!" & vbNewLine, True)
                Exit Sub
            End If
        Catch ex As Exception
            '       End
            TextBox4.Text = "Задайте настройки!" 'ex.Message
            Exit Sub
        End Try

        'преобразование телефона к PDU
        Dim tel, tel1, tel2, tel3 As String
        Dim tellong As Integer
        tel = telnum
        tel1 = tel.Replace("+", "") 'откидывает + заменой на ""
        tellong = Len(tel1)
        If tellong Mod 2 Then 'проверка на чётность с добавлением F
            tel2 = tel1 & "F"
        Else
            tel2 = tel1
        End If
        'намешиваем символы в номере  
        Dim rez As String
        rez = ""
        Dim i As Byte
        For i = 1 To Len(tel2) Step 2
            rez = rez & Mid(tel2, i + 1, 1) & Mid(tel2, i, 1)
        Next i
        tel3 = rez

        '######################преобразование текста в UCS-2######################
        Dim text0() As Byte = System.Text.Encoding.BigEndianUnicode.GetBytes(Message) '(text)
        Dim text1 As String = BitConverter.ToString(text0).Replace("-", "")
        Dim textlong As String = Len(text1) 'определяем длину фразы
        Dim textlongHEX As String = Hex(textlong) 'перегоняем длину сообщения в HEX
        Dim l As String = 26 + textlong 'два 0 спереди уже откинуто
        Dim l1 = l / 2
        Dim MSG As String = "0001000B91" & tel3 & "0008" & textlongHEX & text1
        Try
            Dim comport As String = TargetPort
            Dim SP As New SerialPort()
            SP.PortName = comport
            SP.BaudRate = 115200
            SP.Parity = Parity.None
            SP.StopBits = StopBits.One
            SP.DataBits = 8
            SP.Handshake = Handshake.RequestToSend
            SP.DtrEnable = True
            SP.RtsEnable = True
            SP.Open()
            SP.WriteLine("AT" & Chr(13) & vbCrLf)
            Threading.Thread.Sleep(1000)
            SP.WriteLine("AT+CMGF=0" & Chr(13) & vbCrLf)
            Threading.Thread.Sleep(1000)
            SP.WriteLine("AT+CMGS=" & l1 & vbCrLf)
            Threading.Thread.Sleep(1000)
            SP.WriteLine(MSG & Chr(26) & vbCrLf)
            Threading.Thread.Sleep(1000)
            Dim sp_result As String = SP.ReadExisting()
            If Message = "тестовое сообщение" Then
                TextBox4.Text = sp_result
                TextBox4.Text = TextBox4.Text.Replace(New Char() {vbLf, vbCr}, "").TrimEnd(New Char() {vbLf, vbCr})
            End If
            SP.Close()
            'логгирование
            Dim D As Date = Now
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " Начата отправка СМС c текстом:" & Message & " на номер: " & tel & vbNewLine & sp_result & vbNewLine, True)
        Catch ex As Exception
            Dim D As Date = Now
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " СМС c текстом:" & Message & " не была послана на: " & tel & vbNewLine & ex.Message & vbNewLine, True)
        End Try
    End Sub

    Public Sub GetPort()
        'автоопределения порта модема
        Dim ports() As String = SerialPort.GetPortNames()
        If ports.Count = 0 Then
            TargetPort = "ERR"
            Exit Sub
        End If
        For i = 0 To ports.Count - 1
            Try
                Dim SP As New SerialPort()
                SP.PortName = ports(i) ' comport
                SP.BaudRate = 9600
                SP.Parity = Parity.None
                SP.StopBits = StopBits.One
                SP.DataBits = 8
                SP.Handshake = Handshake.RequestToSend
                SP.DtrEnable = True
                SP.RtsEnable = True
                SP.Open()
                SP.WriteLine("AT" & Chr(13) & vbCrLf)
                Threading.Thread.Sleep(500)
                Dim at As String = SP.ReadExisting
                If at <> "" Then
                    TargetPort = ports(i)
                    TextBox1.Text = TargetPort
                    ' Exit For
                End If
                ' Thread.Sleep(1000)
                SP.Close()
            Catch ex As Exception
            End Try
        Next
    End Sub

    'save 
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Try
            If CheckBox1.Checked = True Then
                Autoport = "true"
            ElseIf CheckBox1.Checked = False Then
                Autoport = "false"
            End If
            Dim Wr As New XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml", System.Text.Encoding.GetEncoding("UTF-8"))
            Dim Xml As New System.Xml.XmlDocument
            If TextBox2.Text <> "" Then
                If TextBox1.Text = "" Then
                    Autoport = "true"
                    TextBox1.Text = "Auto"
                End If
                With Wr
                    .WriteStartDocument()
                    .WriteStartElement("Config")
                    .WriteStartElement("Phone")
                    .WriteValue(TextBox2.Text)
                    .WriteEndElement() '/Phone
                    .WriteStartElement("COM")
                    .WriteValue(TextBox1.Text)
                    .WriteEndElement() '/COM
                    .WriteStartElement("Autoport")
                    .WriteValue(Autoport)
                    .WriteEndElement() '/Autoport
                    .WriteEndElement() '/config
                    .WriteEndDocument() '/
                    .Flush()
                    .Close()
                End With
            End If
            Xml.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")
            Xml.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")
            Wr = Nothing
            Xml = Nothing
            MsgBox("Настройки сохранены!")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    'reset all
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim msg As String
        Dim title As String
        Dim style As MsgBoxStyle
        Dim response As MsgBoxResult
        msg = "Точно хотите сбросить настройки?" '"Do you want to continue?"   ' Define message.
        style = MsgBoxStyle.DefaultButton2 Or
        MsgBoxStyle.Critical Or MsgBoxStyle.YesNo
        title = "erSMS settings"   ' Define title.
        ' Display message.
        response = MsgBox(msg, style, title)
        If response = MsgBoxResult.Yes Then   ' User chose Yes.
            Reset()
            ' Perform some action.
        Else
            ' Perform some other action.
        End If
    End Sub

    'reset
    Private Sub Reset()
        Dim cp As String
        TextBox2.Clear()
        TextBox1.Clear()
        CheckBox1.Checked = False
        TextBox2.Clear()
        TextBox1.Clear()
        PortsAdd()
        If ComboBox1.Items.Count > 0 Then
            cp = ComboBox1.Items.Item(0)
        Else
            cp = "N/A"
        End If
        Try
            Dim Wr As New XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml", System.Text.Encoding.GetEncoding("UTF-8"))
            Dim Xml As New System.Xml.XmlDocument
            With Wr
                .WriteStartDocument()
                .WriteStartElement("Config")
                .WriteStartElement("Phone")
                .WriteValue("N/A")
                .WriteEndElement() '/Phone
                .WriteStartElement("COM")
                .WriteValue(cp)
                .WriteEndElement() '/COM
                .WriteStartElement("Autoport")
                .WriteValue("true")
                .WriteEndElement() '/Autoport
                .WriteStartElement("Speed")
                .WriteValue(9600)
                .WriteEndElement() '/speed
                .WriteEndElement() '/config
                .WriteEndDocument() '/
                .Flush()
                .Close()
            End With
            Xml.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")
            Xml.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\Settings.xml")
            Wr = Nothing
            Xml = Nothing
            MsgBox("Настройки сброшены!")
            Dim D As Date = Now
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & ": RESET_TO_DEFAULT!" & vbNewLine, True)
        Catch ex As Exception
            MsgBox(ex.Message)
            Dim D As Date = Now
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & ": RESET_ERROR: " & ex.Message & vbNewLine, True)
        End Try
        ReadSettings()
    End Sub

    'терминал
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            If TextBox1.Text <> "" Then
                TextBox4.Clear()
                Dim SP As New SerialPort()
                SP.PortName = TextBox1.Text
                SP.BaudRate = 9600
                SP.Parity = Parity.None
                SP.StopBits = StopBits.One
                SP.DataBits = 8
                SP.Handshake = Handshake.RequestToSend
                SP.DtrEnable = True
                SP.RtsEnable = True
                SP.Open()
                SP.WriteLine(TextBox3.Text & vbCrLf)
                Threading.Thread.Sleep(1000)
                TextBox4.Text = SP.ReadExisting()
                TextBox4.Text = TextBox4.Text.Replace(New Char() {vbLf, vbCr}, "").TrimEnd(New Char() {vbLf, vbCr})
                SP.Close()
                Dim D As Date = Now
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt", D & " Послана команда: " & TextBox3.Text & vbNewLine & TextBox4.Text & vbNewLine, True)
            Else
                MsgBox("Выберете порт!")
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        LogMrg()
    End Sub

    'close
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        LogMrg()
        End
    End Sub

    'port selection
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TextBox1.Text = ComboBox1.SelectedItem
    End Sub

    'проверка на пустое поле ввода команды 
    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If TextBox3.Text <> "" Then
            Button3.Enabled = True
        ElseIf TextBox3.TextLength < 1 Then
            Button3.Enabled = False
        End If
    End Sub

    'ввод только цифр и плюсиков в поле ввода телефона
    Private Sub TextBox2_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox2.KeyPress
        'If Not Char.IsDigit(e.KeyChar) Then e.Handled = True
        If Not Char.IsDigit(e.KeyChar) And e.KeyChar <> "+" And e.KeyChar <> vbBack Then e.Handled = True
    End Sub

    'показать лог
    Private Sub ПросмотрЛогаToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ПросмотрЛогаToolStripMenuItem.Click
        LogMrg()
        Try
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\erSMS_log.txt")
        Catch ex As Exception
        End Try
    End Sub

    'выход
    Private Sub ВыходToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ВыходToolStripMenuItem.Click
        LogMrg()
        End
    End Sub

    'тест мопеда
    Private Sub ТестМодемаToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ТестМодемаToolStripMenuItem.Click
        Try
            If TextBox1.Text = "" And CheckBox1.Checked = True Then
                GetPort()
            ElseIf TextBox1.Text <> "" Then
                TargetPort = TextBox1.Text
            End If
            TabControl1.SelectedTab = TabPage2
            TextBox4.Clear()
            Dim SP As New SerialPort()
            SP.PortName = TargetPort
            SP.BaudRate = 9600
            SP.Parity = Parity.None
            SP.StopBits = StopBits.One
            SP.DataBits = 8
            SP.Handshake = Handshake.RequestToSend
            SP.DtrEnable = True
            SP.RtsEnable = True
            SP.Open()
            SP.WriteLine("ATI" & vbCrLf)
            Threading.Thread.Sleep(1000)
            TextBox4.Text = SP.ReadExisting()
            TextBox4.Text = TextBox4.Text.Replace(New Char() {vbLf, vbCr}, "").TrimEnd(New Char() {vbLf, vbCr})
            SP.Close()
        Catch ex As Exception
            TextBox4.Text = ex.Message
            Exit Sub
        End Try
    End Sub

    'обновить список портов
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        PortsAdd()
    End Sub

    'тестовое сообщение
    Private Sub ТестовоеСообщениеToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ТестовоеСообщениеToolStripMenuItem.Click
        TabControl1.SelectedTab = TabPage2
        ReadSettings()
        If CheckBox1.Checked = True Then
            GetPort()
        End If
        Message = "тестовое сообщение"
        sms()
        LogMrg()
    End Sub

    'сканирование портов
    Public Function PortsAdd()
        Try
            ComboBox1.Items.Clear()
            Dim ports() As String = SerialPort.GetPortNames()
            Dim port As String
            For Each port In ports
                ComboBox1.Items.Add(port)
            Next port
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return 0
    End Function

    'автопоиск да/нет
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Try
            If CheckBox1.Checked = True Then
                Autoport = "true"
            ElseIf CheckBox1.Checked = False Then
                Autoport = "false"
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    'найти модем(кнопка)
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        GetPort()
        TextBox1.Text = TargetPort
    End Sub

    'добавить процесс в список завершения
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If TextBox5.Text <> "" Then
            ListBox1.Items.Add(TextBox5.Text)
            TextBox5.Clear()
            Try
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list", "", False)
                For i = 0 To ListBox1.Items.Count - 1
                    My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list", ListBox1.Items.Item(i) & vbNewLine, True)
                Next
            Catch ex As Exception
            End Try
        Else
            MsgBox("Заполните поле!")
        End If
    End Sub

    'удаление по одному
    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        ListBox1.Items.Remove(ListBox1.SelectedItem)
        Try
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list", "", False)
            For i = 0 To ListBox1.Items.Count - 1
                My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list", ListBox1.Items.Item(i) & vbNewLine, True)
            Next
            ListBox1.Items.Clear()
            ListBox1.Items.AddRange(IO.File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list"))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    'clear killlist
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        ListBox1.Items.Clear()
        Try
            My.Computer.FileSystem.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\CLM\Plugins\erSMS_Resources\kill.list", "", False)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
End Class