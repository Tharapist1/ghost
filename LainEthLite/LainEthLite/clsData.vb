﻿Imports Db4objects


#Region "FLAGS"
<Flags()> _
Public Enum adminFlags As Integer
    NORMAL = 0
    SAY = 1
    HOST = 2            'host, hostby, pub, priv
    UNHOST = 4
    VERSION = 8
    GETGAMES = 16
    MAP = 32
    RECONNECT = 64
    CHANNEL = 128
    ADDADMIN = 256
    REMOVEADMIN = 512
    START = 1024        'start, end, abort
    OPEN = 2048         'open, close, comp
    SWAP = 4096
    KICK = 8192
    PING = 16384
    FROM = 32768
    HOLD = 65536
    ADMIN = 131072
    REFRESH = 262144
    SPOOF = 524288
    LOCK = 1048576      'lock and unlock
    SP = 2097152
    LATENCY = 4194304

End Enum
#End Region

Public Class clsData
    Public userList As clsUserList
    Public adminList As clsAdminList
    Public botSettings As clsBotSettings

    Public database As Db4o.IObjectContainer

    Public Sub New()
        userList = New clsUserList
        adminList = New clsAdminList
        botSettings = New clsBotSettings

    End Sub

#Region "LOAD/SAVE BOT SETTINGS"
    Public Sub save_botSettings()
        Dim results As Db4o.IObjectSet
        Dim found As clsBotSettings = New clsBotSettings
        Try
            'open database
            database = Db4o.Db4oFactory.OpenFile("ghost.db")

            'check if database already contains this data
            results = database.Get(GetType(clsBotSettings))
            If results.Count > 0 Then
                found = CType(results.Next, clsBotSettings)
                database.Delete(found)
                'MsgBox("Updating bot settings in database.")
                database.Set(frmLainEthLite.data.botSettings)
            Else
                'MsgBox("Writting bot settings to database.")
                database.Set(botSettings)
            End If

        Finally
            'close database / commit transaction
            database.Close()

        End Try
    End Sub
    Public Function load_botSettings() As Boolean
        Dim results As Db4o.IObjectSet
        Dim retVal As Boolean = True

        Try
            'open database
            database = Db4o.Db4oFactory.OpenFile("ghost.db")

            'get data
            results = database.Get(GetType(clsBotSettings))

            'work with data
            If results.Count > 0 Then
                'MsgBox(String.Format("number of returned results: {0}", results.Count))
                frmLainEthLite.data.botSettings = CType(results.Next, clsBotSettings)
            Else
                retVal = False
                'MsgBox("No results found in the database.")
            End If

        Finally
            'close database / commit transaction
            database.Close()
        End Try
        Return retVal
    End Function
#End Region
#Region "LOAD/SAVE ADMIN LIST"
    Public Sub save_adminData()
        Dim results As Db4o.IObjectSet
        Dim found As clsAdminList = New clsAdminList
        Try
            'open database
            database = Db4o.Db4oFactory.OpenFile("ghost.db")

            'check if database already contains this data
            results = database.Get(GetType(clsAdminList))
            If results.Count > 0 Then
                found = CType(results.Next, clsAdminList)
                database.Delete(found)
                'MsgBox("Updating bot settings in database.")
                database.Set(frmLainEthLite.data.adminList)
            Else
                'MsgBox("Writting bot settings to database.")
                database.Set(adminList)
            End If

        Finally
            'close database / commit transaction
            database.Close()
        End Try
    End Sub
    Public Function load_adminData() As Boolean
        Dim results As Db4o.IObjectSet
        Dim retVal As Boolean = True

        Try
            'open database
            database = Db4o.Db4oFactory.OpenFile("ghost.db")

            'get data
            results = database.Get(GetType(clsAdminList))

            'work with data
            If results.Count > 0 Then
                'MsgBox(String.Format("number of returned results: {0}", results.Count))
                frmLainEthLite.data.adminList = CType(results.Next, clsAdminList)
            Else
                retVal = False
                'MsgBox("No results found in the database.")
            End If

        Finally
            'close database / commit transaction
            database.Close()
        End Try
        Return retVal
    End Function
#End Region
End Class

#Region "ADMIN DATA"
Public Class clsAdminList
    Private adminList As ArrayList

    Public Sub New()
        adminList = New ArrayList
    End Sub

    Public Function getList() As ArrayList
        Return adminList
    End Function
    Public Function getUser(ByVal name As String) As clsAdmin
        For Each user As clsAdmin In adminList
            Debug.WriteLine(String.Format("getUser :: comparing [{0}] to [{1}]", user.name.ToLower, name.ToLower))
            If user.name.ToLower = name.ToLower Then
                Return user
            End If
        Next

        Debug.WriteLine(String.Format("Adding {0} as a new user.", name))
        adminList.Add(New clsAdmin(name))
        For Each user As clsAdmin In adminList
            Debug.WriteLine(String.Format("getUser :: comparing [{0}] to [{1}]", user.name.ToLower, name.ToLower))
            If user.name.ToLower = name.ToLower Then
                Return user
            End If
        Next

        Return New clsAdmin
    End Function

    Public Sub addUser(ByVal name As String)
        Dim maxAdminFlag As adminFlags = adminFlags.NORMAL

        For Each value As adminFlags In [Enum].GetValues(GetType(adminFlags))
            maxAdminFlag = maxAdminFlag Or value
        Next

        addUser(name, maxAdminFlag)
    End Sub
    Public Sub addUser(ByVal name As String, ByVal flag As adminFlags)
        Dim arrayPosition As Integer = 0
        Try
            arrayPosition = adminList.Add(New clsAdmin(name))
            CType(adminList.Item(arrayPosition), clsAdmin).addAccess(flag)
        Catch ex As Exception
            'error handling
        End Try
    End Sub
    Public Sub removeUser(ByVal name As String)
        adminList.Remove(getUser(name))
    End Sub

End Class
Public Class clsAdmin

    Public name As String       'Battle.net name
    Public flags As adminFlags

    Public Sub New()
        Me.name = ""
        Me.flags = adminFlags.NORMAL
    End Sub

    Public Sub New(ByVal name As String)
        Me.name = name
        Me.flags = adminFlags.NORMAL
    End Sub

    Public Sub addAccess(ByVal flag As adminFlags)
        'todo
        'Client c = new Client();
        'c.ClientState = (ClientStates.HasDiscount|ClientStates.IsSupplier|ClientStates.IsOverdra
        flags = flags Or flag
    End Sub

    Public Sub removeAccess(ByVal flag As adminFlags)
        'todo
        'Client c = new Client();
        'c.ClientState = (ClientStates.HasDiscount|ClientStates.IsSupplier|ClientStates.IsOverdra
        flags = CType(flags - flag, adminFlags)
    End Sub
End Class
#End Region

#Region "USER DATA"
Public Class clsUser

    'user details
    Public name As String          'Battle.net name
    Public realm As Byte           'Server realm (useast, uswest, eurobattle.net, etc)
    Public firstDate As System.DateTime       'Date player was first detected
    Public recentGame As System.DateTime      'Date player was last detected
    Public score As Long           'Will be used to calculate ranking
    'Public userLevel As Byte       '0 = none/banned, 10 = rank checking, 20 = VIP, 40 = game hosting, 100 = full admin
    Public ban As String           'Ban description
    Public vip As Boolean           'VIP

    'network details
    Public internalIP As Byte()    'Last known internal IP address
    Public externalIP As Byte()    'Last known external IP address
    Public MAC As String           'Last known MAC address

    'game stats
    Public win As Integer          'number of games won
    Public lost As Integer         'number of games lost
    Public drop As Integer         'number of games dropped
    Public host As Integer         'number of games created on bot

    'time stats
    Public played As Integer       'total time played
    Public afk As Integer          'total time spent afk afk

    'ingame stats
    Public heroKill As Integer     'number of kills
    Public heroDeath As Integer    'number of deaths
    Public heroAssist As Integer   'number of assists.
    Public creepKill As Integer    'number of creep kills
    Public creepDeny As Integer    'number of creep denies

    Public Sub New()

        'user details
        Me.name = ""                'Battle.net name
        Me.realm = 0                'Server realm (useast, uswest, eurobattle.net, etc)
        Me.firstDate = Now            'Date player was first detected
        Me.recentGame = Now           'Date player was last detected
        Me.score = 0                'Will be used to calculate ranking
        'Me.userLevel = 0            '0 = none/banned, 10 = rank checking, 20 = game hosting, 100 = full admin
        Me.ban = ""                 'Ban description
        Me.vip = False              'VIP

        'network details
        Me.internalIP = New Byte() {}         'Last known internal IP address
        Me.externalIP = New Byte() {}          'Last known external IP address
        Me.MAC = ""                 'Last known MAC address

        'game stats
        Me.win = 0                  'number of games won
        Me.lost = 0                 'number of games lost
        Me.drop = 0                 'number of games dropped
        Me.host = 0                 'number of games created on bot

        'time stats
        Me.played = 0               'total time played
        Me.afk = 0                  'total time spent afk afk

        'ingame stats
        Me.heroKill = 0             'number of kills
        Me.heroDeath = 0            'number of deaths
        Me.heroAssist = 0           'number of assists.
        Me.creepKill = 0            'number of creep kills
        Me.creepDeny = 0            'number of creep denies

    End Sub
    Public Sub New(ByVal name As String, ByRef intIP As Byte(), ByVal extIP As Byte())

        'user details
        Me.name = name              'Battle.net name
        Me.realm = 0                'Server realm (useast, uswest, eurobattle.net, etc)
        Me.firstDate = Now          'Date player was first detected
        Me.recentGame = Now         'Date player was last detected
        Me.score = 0                'Will be used to calculate ranking
        'Me.userLevel = 10 'accessLevel  '0 = none/banned, 10 = rank checking, 20 = game hosting, 100 = full admin
        Me.ban = ""                 'Ban description
        Me.vip = False              'VIP

        'network details
        Me.internalIP = intIP       'Last known internal IP address
        Me.externalIP = extIP       'Last known external IP address
        Me.MAC = ""                 'Last known MAC address

        'game stats
        Me.win = 0                  'number of games won
        Me.lost = 0                 'number of games lost
        Me.drop = 0                 'number of games dropped
        Me.host = 0                 'number of games created on bot

        'time stats
        Me.played = 0               'total time played
        Me.afk = 0                  'total time spent afk afk

        'ingame stats
        Me.heroKill = 0             'number of kills
        Me.heroDeath = 0            'number of deaths
        Me.heroAssist = 0           'number of assists.
        Me.creepKill = 0            'number of creep kills
        Me.creepDeny = 0            'number of creep denies

    End Sub

End Class
Public Class clsUserList
    Private userArrayList As ArrayList

    Public Sub New()
        userArrayList = New ArrayList
    End Sub

    Public Function getUser(ByVal name As String) As clsUser
        For Each user As clsUser In userArrayList
            If user.name.ToLower() = name.ToLower Then
                Return user
            End If
        Next
        Return New clsUser
    End Function

    Public Sub addUser(ByVal name As String, ByVal internalIP As Byte(), ByVal externalIP As Byte())
        userArrayList.Add(New clsUser(name, internalIP, externalIP))

    End Sub

    Public Function fixName(ByVal name As String) As String
        For Each user As clsUser In userArrayList
            If user.name.ToLower = name.ToLower Then
                Return user.name
            End If
        Next
        Return ""
    End Function

End Class
#End Region

#Region "BOT SETTINGS"
Public Class clsBotSettings
    'configuration
    Public wc3Path As String       'Path to war3.exe
    Public realm As String         'Server Realm
    Public rocKey As String        'CD-KEY for Reign of Chaos
    Public tftKey As String        'CD-KEY for The Frozen Throne
    Public username As String      'battle.net username
    Public password As String      'battle.net password
    Public homeChannel As String   'default channel used by the bot
    Public port As String         'port used by the bot for hosting games
    Public commandTrigger As String  'character to trigger a bot command

    'toggles
    Public enable_reconnect As Boolean         'reconnect after a disconnect?
    Public enable_refresh As Boolean           'auto-refresh public games?
    Public enable_refreshDisplay As Boolean    'display output when refreshing a game?
    Public enable_autoPingKick As Boolean      'auto-kick players above the max threshhold?
    Public enable_LCPings As Boolean           'List Checker uses pings that are half the standard value
    Public enable_experimentalPings As Boolean 'Experimental pings uses standard deviations to calculate with.

    'preferences
    Public countdown As Integer    '0 to disable, 1+ to enable
    Public maxPing As Integer      'Maximum ping before a player gets kicked
    Public maxGames As Byte         'Maximum games allowed to be hosted at a time

    'temporary
    Public RootAdmin As String      'used for the old uberadmin system

    Public Sub New()
        'configuration
        Me.wc3Path = "C:\Program Files\Warcraft III\"       'Path to war3.exe
        Me.realm = "useast.battle.net"                      'Server Realm
        Me.rocKey = "XXXXXXXXXXXXXXXXXXXXXXXXXX"            'CD-KEY for Reign of Chaos
        Me.tftKey = "XXXXXXXXXXXXXXXXXXXXXXXXXX"            'CD-KEY for The Frozen Throne
        Me.username = "changeme"                            'battle.net username
        Me.password = "changeme"                            'battle.net password
        Me.homeChannel = "Ghost"                            'default channel used by the bot
        Me.port = "6113"                                      'port used by the bot for hosting games
        Me.commandTrigger = "."                      'character to trigger a bot command

        'toggles
        Me.enable_reconnect = False                         'reconnect after a disconnect?
        Me.enable_refresh = True                            'auto-refresh public games?
        Me.enable_refreshDisplay = False                    'display output when refreshing a game?
        Me.enable_autoPingKick = True                       'auto-kick players above the max threshhold?
        Me.enable_LCPings = False                           '0 = normal, 255 = LC
        Me.enable_experimentalPings = False                 'Experimental pings uses standard deviations to calculate with.

        'preferences
        Me.countdown = 0                                    '0 to disable, 1+ to enable
        Me.maxPing = 150                                    'Maximum ping before a player gets kicked
        Me.maxGames = 255                                   'Maximum games allowed to be hosted at a

        Me.RootAdmin = "UberAdmin"                          'old uber admin system
    End Sub

End Class
#End Region

Public Class clsRank

    Private playerID As Long        'Unique ID used to identify players
    Private rank As Long            'Current rank

End Class
