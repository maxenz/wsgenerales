Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data.Odbc
Imports ShamanClases
Imports System.Xml
Imports System.Data.SqlClient
Imports RestSharp
Imports System.Net
Imports System.IO
Imports Newtonsoft.Json.Linq



' Para permitir que se llame a este servicio Web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la siguiente línea.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Service

    Inherits System.Web.Services.WebService

    Private myConn As SqlConnection
    Private myConn2 As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private myReaderMod As SqlDataReader
    Private myCmdMod As SqlCommand
    Private results As String

    <WebMethod()> _
    Public Function GetDistanciaTiempo(ByVal latMov As String, ByVal lngMov As String, _
                                 ByVal latDst As String, ByVal lngDst As String) As String



        GetDistanciaTiempo = Nothing
        Dim tiempo As String = ""
        Dim distancia As String = ""
        Dim url As String = "http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" & latMov & "," & lngMov & "&destinations=" & latDst & "," & lngDst & "&mode=driving&language=fr-FR&sensor=false"
        Dim status As String = ""
        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText


            Next

            If status = "OK" Then


                m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/duration/text")

                For Each m_node In m_nodelist

                    tiempo = m_node.ChildNodes.Item(0).InnerText

                Next

                m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/distance/text")

                For Each m_node In m_nodelist

                    distancia = m_node.ChildNodes.Item(0).InnerText

                Next

                GetDistanciaTiempo = distancia & "/" & tiempo

            Else

                GetDistanciaTiempo = "0/0"
            End If

        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try

    End Function

    <WebMethod()> _
    Public Function GetDireccion(ByVal lat As String, ByVal lng As String) As String

        GetDireccion = Nothing
        Dim strResultados As String = ""
        Dim url As String = "http://maps.googleapis.com/maps/api/geocode/xml?address=" & lat & "," & lng & "&sensor=false"
        Dim status As String = ""
        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText
                MsgBox(status)

            Next

            If status = "OK" Then




                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/formatted_address")

                For Each m_node In m_nodelist

                    Dim dire = m_node.ChildNodes.Item(0).InnerText

                    GetDireccion = dire

                Next

            Else
                GetDireccion = "0"

            End If
        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try


    End Function

    <WebMethod()> _
    Public Function GetLatLong(ByVal direccion As String) As String

        GetLatLong = Nothing
        Dim strResultados As String = ""
        Dim url As String = "http://maps.googleapis.com/maps/api/geocode/xml?address=" & direccion & "&sensor=false"
        Dim reader As XmlTextReader = New XmlTextReader(url)
        Dim status As String = ""

        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText

            Next

            If status = "OK" Then

                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/geometry/location")

                For Each m_node In m_nodelist

                    Dim lat = m_node.ChildNodes.Item(0).InnerText

                    Dim lng = m_node.ChildNodes.Item(1).InnerText

                    GetLatLong = lat & "/" & lng

                Next

            Else
                GetLatLong = "0/0"

            End If

        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try

    End Function

    <WebMethod()> _
    Public Function GetPuntosEnPoligono(ByVal pLat As Single, ByVal pLon As Single, ByVal pTip As String) As String
        GetPuntosEnPoligono = ""

        Try
            Dim shmSession As New PanelC.Conexion
            Dim objZonas As New CompuMapC.Zonificaciones

            If shmSession.Iniciar("192.168.0.249", 1972, "SHAMAN", "EMERGENCIAS", "JOB", 1, True) Then


                Dim vDev As String = objZonas.GetPoligonosInPoint(pLat, pLon, pTip, True)

                GetPuntosEnPoligono = vDev

                shmSession.Cerrar(shmSession.PID, True)

            Else

                GetPuntosEnPoligono = "Sin conexión"

            End If

        Catch ex As Exception

            GetPuntosEnPoligono = ex.Message

        End Try

    End Function

    <WebMethod()> _
    Public Function getIncidente(ByVal cod As String, ByVal fec As String) As String
        Dim result As String = ""
        Dim connectionString As String
        Dim cnn As OdbcConnection
        connectionString = "DSN=phpODBC;UID=_SYSTEM;Pwd=sys"
        cnn = New OdbcConnection(connectionString)
        Try
            cnn.Open()
            Dim Reader As OdbcDataReader
            Dim cmdString = "SELECT TOP 1 inc.HorInicial, inc.HorFinal,inc.sintoma," & _
            "incdom.Domicilio FROM Emergency.Incidentes inc INNER JOIN " & _
            " Emergency.IncidentesDomicilios incdom ON (inc.ID = incdom.IncidenteId)" & _
            " WHERE inc.FecIncidente = '" & fec & "' AND inc.NroIncidente = '" & cod & "'"
            Dim Cmd As New OdbcCommand(cmdString, cnn)
            Reader = Cmd.ExecuteReader()
            If (Reader.Read()) Then
                Dim dom As String = Reader("Domicilio")
                Dim sint As String = Reader("Sintoma")
                Dim horInicio As String = Reader("HorInicial")
                Dim horFinal As String = Reader("HorFinal")
                result = dom & "$" & sint & "$" & horInicio & "$" & horFinal
            End If
            cnn.Close()
        Catch ex As Exception

            result = "Error"
        End Try

        Return result
    End Function

    '<WebMethod()> _
    'Public Function getSerialSetLog(ByVal serialNumber As String) As String
    '    Dim result As String = "0"
    '    Dim LicenciaId As Integer = 0
    '    Dim ClienteIp As String = Context.Request.ServerVariables("remote_addr")
    '    Dim connectionString As String
    '    Dim SQL As String = ""
    '    Dim cnnDataSource As String
    '    Dim cnnCatalog As String
    '    Dim cnnUser As String
    '    Dim cnnPassword As String
    '    serialNumber = serialNumber.Replace("/", "")
    '    connectionString = "DSN=mysql_local;UID=root;Pwd=bac35714"

    '    Try

    '        Dim cnn As OdbcConnection
    '        cnn = New OdbcConnection(connectionString)
    '        cnn.Open()
    '        Dim Reader As OdbcDataReader
    '        SQL = "SELECT id,serial FROM licencias WHERE Serial = '" & serialNumber & "'"
    '        Dim Cmd As New OdbcCommand(SQL, cnn)
    '        Reader = Cmd.ExecuteReader()
    '        If (Reader.Read()) Then

    '            LicenciaId = Reader("ID")

    '            SQL = "SELECT id,cnn_data_source,cnn_catalog,cnn_user,cnn_pass FROM clientes_licencias WHERE licencia_id = " & LicenciaId
    '            Dim CmdConnection As New OdbcCommand(SQL, cnn)
    '            Reader = CmdConnection.ExecuteReader
    '            If (Reader.Read()) Then

    '                Dim CliLicId As Integer = Reader("id")
    '                cnnCatalog = Reader("cnn_catalog")
    '                cnnDataSource = Reader("cnn_data_source")
    '                cnnUser = Reader("cnn_user")
    '                cnnPassword = Reader("cnn_pass")
    '                SQL = "SELECT clilicprod.id as ID, prod.nro_producto as NROPROD FROM clientes_licencias_productos clilicprod "
    '                SQL = SQL & "INNER JOIN productos prod ON (prod.id = clilicprod.producto_id) "
    '                SQL = SQL & "WHERE clientes_licencia_id = " & CliLicId
    '                Dim cmdProd As New OdbcCommand(SQL, cnn)
    '                Reader = cmdProd.ExecuteReader
    '                Dim vMod As New Collection
    '                Dim vProd As New Collection

    '                While (Reader.Read())

    '                    Dim prod As String = Reader("NROPROD")
    '                    vProd.Add(prod)
    '                    Dim CliLicProdId As Integer = Reader("ID")
    '                    SQL = "SELECT pmod.abreviatura_id as MODULOEXC FROM clientes_licencias_productos_productos_modulos cpmod "
    '                    SQL = SQL & "INNER JOIN productos_modulos pmod ON (pmod.id = cpmod.productos_modulo_id) "
    '                    SQL = SQL & "WHERE cpmod.clientes_licencias_producto_id = " & CliLicProdId
    '                    Dim cmdMod As New OdbcCommand(SQL, cnn)
    '                    Dim ReaderMod As OdbcDataReader
    '                    ReaderMod = cmdMod.ExecuteReader
    '                    While (ReaderMod.Read())
    '                        Dim modulo As String = ReaderMod("MODULOEXC")
    '                        prod = formatProd(prod)
    '                        Dim prodMod As String = prod & modulo
    '                        vMod.Add(prodMod)
    '                    End While

    '                End While

    '                Dim prods As String = ""
    '                For Each prod As String In vProd
    '                    If prods = "" Then
    '                        prods = prod
    '                    Else
    '                        prods = prods & "/" & prod
    '                    End If
    '                Next

    '                prods = prods & "#"


    '                Dim prodModulos As String = ""

    '                For Each pmod As String In vMod
    '                    If prodModulos = "" Then
    '                        prodModulos = pmod
    '                    Else
    '                        prodModulos = prodModulos & "/" & pmod
    '                    End If
    '                Next

    '                result = cnnDataSource & "^" & cnnCatalog & "^" & cnnUser & "^" & cnnPassword & "^" & prods & prodModulos

    '            End If

    '            'SQL = "SELECT prod.nro_producto as nProd FROM productos prod "
    '            'SQL = SQL & "INNER JOIN licencias_productos lic ON (lic.producto_id = prod.id) "
    '            'SQL = SQL & "WHERE lic.licencia_id = " & LicenciaId
    '            'Dim CmdConection As New OdbcCommand(SQL, cnn)
    '            'Reader = CmdConection.ExecuteReader()
    '            'Dim prods As String = ""
    '            'While (Reader.Read())
    '            '    If (prods = "") Then

    '            '        prods = Reader("nProd")

    '            '    Else

    '            '        prods = prods & "/" & Reader("nProd")

    '            '    End If
    '            'End While

    '            'cnn.Close()
    '            'cnn.Open()

    '            'SQL = "SELECT cnn_data_source,cnn_catalog,cnn_user,cnn_pass FROM clientes_licencias "
    '            'SQL = SQL & "WHERE licencia_id = " & LicenciaId
    '            'CmdConection.CommandText = SQL
    '            'CmdConection.Connection = cnn
    '            'Reader = CmdConection.ExecuteReader()

    '            'If (Reader.Read()) Then

    '            '    Dim cnnDataSource As String = Reader("cnn_data_source")
    '            '    Dim cnnCatalog As String = Reader("cnn_catalog")
    '            '    Dim cnnUser As String = Reader("cnn_user")
    '            '    Dim cnnPassword As String = Reader("cnn_pass")

    '            '    result = cnnDataSource & "^" & cnnCatalog & "^" & cnnUser & "^" & cnnPassword & "^" & prods

    '            'End If
    '        End If

    '        cnn.Close()
    '        cnn.Open()
    '        Dim time As DateTime = DateTime.Now
    '        Dim format As String = "yyyy/MM/d HH:mm:ss"
    '        Dim sTime As String = time.ToString(format)
    '        SQL = "INSERT INTO licencias_logs (licencia_id,solicitud_id,ip,referencias,created_at,updated_at) "
    '        SQL = SQL & "VALUES (" & LicenciaId & ",0,'" & ClienteIp & "','" & serialNumber & "','" & sTime & "','" & sTime & "')"
    '        Dim cmdIns As New OdbcCommand(SQL, cnn)
    '        cmdIns.ExecuteNonQuery()
    '        cnn.Close()

    '    Catch ex As Exception

    '    End Try

    '    Return result
    'End Function

    Public Function formatProd(prod As String) As String

        prod = CType(prod, Integer)
        If prod < 10 Then
            prod = CType(prod, String)
            Return "00" & prod
        ElseIf prod < 100 Then
            prod = CType(prod, String)
            Return "0" & prod
        Else
            prod = CType(prod, String)
            Return prod
        End If

    End Function

    '-------------> PRUEBA DE WEBSERVICE CONTRA SQL SERVER

    <WebMethod()> _
    Public Function getSerialSetLog(ByVal serialNumber As String) As String
        Return getSerialSetLogLast(serialNumber, 0)
    End Function
    <WebMethod()> _
    Public Function getSerialSetLogLast(ByVal serialNumber As String, ByVal pRemote As Integer) As String
        Dim result As String = "0"
        Dim LicenciaId As Integer = 0
        Dim ClienteIp As String = Context.Request.ServerVariables("remote_addr")
        Dim connectionString As String
        Dim SQL As String = ""
        Dim cnnDataSource As String
        Dim cnnCatalog As String
        Dim cnnUser As String
        Dim cnnPassword As String
        Dim conexionServidor As String
        Dim fechaDeVencimiento As DateTime
        serialNumber = serialNumber.Replace("/", "")
        connectionString = "Data Source=LOG,9898\SQLEXPRESS;Initial Catalog=Gestion;Integrated Security=SSPI;User Id = dbaadmin; Password = yeike;"

        Try

            myConn = New SqlConnection(connectionString)
            myConn2 = New SqlConnection(connectionString)
            myCmd = myConn.CreateCommand
            myCmd.CommandText = "SELECT id,serial FROM licencias WHERE Serial = '" & serialNumber & "'"
            myConn.Open()
            myConn2.Open()
            myReader = myCmd.ExecuteReader()

            If myReader.Read() Then

                LicenciaId = myReader("ID")
                myReader.Close()

                myCmd.CommandText = "SELECT ID,CnnDataSource,CnnCatalog,CnnUser," & _
                                    "CnnPassword,ISNULL(ConexionServidor,'') AS ConexionServidor,FechaDeVencimiento FROM ClientesLicencias WHERE LicenciaID = " & _
                                    LicenciaId

                myReader = myCmd.ExecuteReader()
                If (myReader.Read()) Then

                    Dim CliLicId As Integer = myReader("ID")
                    cnnCatalog = myReader("CnnCatalog")
                    cnnDataSource = myReader("CnnDataSource")
                    cnnUser = myReader("CnnUser")
                    cnnPassword = myReader("CnnPassword")
                    conexionServidor = myReader("ConexionServidor")
                    fechaDeVencimiento = myReader("FechaDeVencimiento")
                    If (Date.Today > fechaDeVencimiento) Then
                        Return 0
                    End If
                    SQL = "SELECT clilicprod.ID as ID, prod.Numero as NROPROD FROM ClientesLicenciasProductos clilicprod "
                    SQL = SQL & "INNER JOIN Productos prod ON (prod.id = clilicprod.ProductoID) "
                    SQL = SQL & "WHERE ClientesLicenciaID = " & CliLicId
                    myCmd.CommandText = SQL
                    myReader.Close()
                    myReader = myCmd.ExecuteReader()
                    Dim vMod As New Collection
                    Dim vProd As New Collection

                    While (myReader.Read())

                        Dim prod As String = myReader("NROPROD")
                        vProd.Add(prod)
                        Dim CliLicProdId As Integer = myReader("ID")
                        SQL = "SELECT pmod.codigo as MODULOEXC FROM ClientesLicenciasProductosModulos cpmod "
                        SQL = SQL & "INNER JOIN ProductosModulos pmod ON (pmod.id = cpmod.ProductosModuloID) "
                        SQL = SQL & "WHERE cpmod.ClientesLicenciasProductoID = " & CliLicProdId
                        myCmdMod = myConn2.CreateCommand
                        myCmdMod.CommandText = SQL
                        myReaderMod = myCmdMod.ExecuteReader
                        While (myReaderMod.Read())
                            Dim modulo As String = myReaderMod("MODULOEXC")
                            prod = formatProd(prod)
                            Dim prodMod As String = prod & modulo
                            vMod.Add(prodMod)
                        End While
                        myReaderMod.Close()
                    End While

                    Dim prods As String = ""
                    For Each prod As String In vProd
                        If prods = "" Then
                            prods = prod
                        Else
                            prods = prods & "/" & prod
                        End If
                    Next

                    prods = prods & "#"

                    Dim prodModulos As String = ""

                    For Each pmod As String In vMod
                        If prodModulos = "" Then
                            prodModulos = pmod
                        Else
                            prodModulos = prodModulos & "/" & pmod
                        End If
                    Next

                    If (pRemote = 1) Then

                        Dim vInstance As String() = cnnDataSource.Split("\")
                        Dim instance As String = vInstance(1)
                        'cnnDataSource = conexionServidor & "\" & instance
                        cnnDataSource = conexionServidor
                    End If

                    result = cnnDataSource & "^" & cnnCatalog & "^" & cnnUser & "^" & cnnPassword & "^" & prods & prodModulos & "^" & fechaDeVencimiento

                End If

            End If

            myConn.Close()
            myConn.Open()

            Dim time As DateTime = DateTime.Now
            Dim format As String = "yyyy/MM/d HH:mm:ss"
            Dim sTime As String = time.ToString(format)
            SQL = "INSERT INTO LicenciasLogs (LicenciaID,SolicitudID,IP,Referencias,CreatedAt) "
            SQL = SQL & "VALUES (" & LicenciaId & ",0,'" & ClienteIp & "','" & serialNumber & "','" & sTime & "')"
            myCmd.CommandText = SQL
            myCmd.ExecuteNonQuery()
            myConn.Close()

        Catch ex As Exception

            Return ex.Message.ToString

        End Try

        Return result

    End Function

    <WebMethod()> _
    Public Function isInGestion(ByVal user As String, ByVal pass As String, ByVal llave As String) As String
        Dim client = New RestClient()
        client.BaseUrl = "http://localhost:57771/"

        Dim request = New RestRequest()
        request.Resource = "ExternalLogin/IsInGestion"
        request.AddParameter("user", user)
        request.AddParameter("pass", pass)
        request.AddParameter("llave", llave)

        Dim response As IRestResponse = client.Execute(request)

        Return response.Content

    End Function

    <WebMethod()> _
    Public Function setPushNotification(licencia As String, nroMovil As String, mensaje As String, appID As String, restApiKey As String) As Boolean
        'application id example Yu5MsVhQi7ih2ltKlNrQcrpFfvRlexZnGiecJZHd
        'rest api key example KRAjomjaryxsslXvwnuwHkZk7CRJpSwpGFkfY0aP

        Dim channel As String = Convert.ToString("m") & nroMovil & "_" & licencia

        Dim isPushMessageSend As Boolean = False

        Dim postString As String = ""
        Dim urlpath As String = "https://api.parse.com/1/push"
        Dim httpWebRequest = DirectCast(WebRequest.Create(urlpath), HttpWebRequest)
        postString = (Convert.ToString((Convert.ToString("{ ""channels"": [ """) & channel) + """  ], " + """data"" : {""alert"":""") & mensaje) + """}" + "}"
        httpWebRequest.ContentType = "application/json"
        httpWebRequest.ContentLength = postString.Length
        httpWebRequest.Headers.Add("X-Parse-Application-Id", appID)
        httpWebRequest.Headers.Add("X-Parse-REST-API-KEY", restApiKey)
        httpWebRequest.Method = "POST"
        Dim requestWriter As New StreamWriter(httpWebRequest.GetRequestStream())
        requestWriter.Write(postString)
        requestWriter.Close()
        Dim httpResponse = DirectCast(httpWebRequest.GetResponse(), HttpWebResponse)
        Using streamReader = New StreamReader(httpResponse.GetResponseStream())
            Dim responseText = streamReader.ReadToEnd()
            Dim jObjRes As JObject = JObject.Parse(responseText)
            If Convert.ToString(jObjRes).IndexOf("true") <> -1 Then
                isPushMessageSend = True
            End If
        End Using

        Return isPushMessageSend
    End Function

End Class
