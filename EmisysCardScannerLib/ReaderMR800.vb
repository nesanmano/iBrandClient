Option Strict Off
Option Explicit On
Imports System.Text
' ReSharper disable once InconsistentNaming
Public Class ReaderMR800

    Public Structure SCARD_IO_REQUEST
        Dim dwProtocol As Integer
        Dim cbPciLength As Integer
    End Structure

    Public Enum LED_COLOR
        RED = &H10S
        GREEN = &H20S
        BLUE = &H40S
        YELLOW = &H80S
    End Enum
    Public Enum BUZZER_BEEP
        BEEP_ERROR = 1
        BEEP_READ = 2
        BEEP_SUCCESS = 4
    End Enum

    Public Const SCARD_S_SUCCESS As Short = 0
    Public Const SCARD_ATR_LENGTH As Short = 50

    Public Const SCARD_SCOPE_USER As Short = 0
    Public Const SCARD_SCOPE_TERMINAL As Short = 1
    Public Const SCARD_SCOPE_SYSTEM As Short = 2

    Public Const SCARD_STATE_UNAWARE As Short = &H0S
    Public Const SCARD_STATE_IGNORE As Short = &H1S
    Public Const SCARD_STATE_CHANGED As Short = &H2S
    Public Const SCARD_STATE_UNKNOWN As Short = &H4S
    Public Const SCARD_STATE_UNAVAILABLE As Short = &H8S
    Public Const SCARD_STATE_EMPTY As Short = &H10S
    Public Const SCARD_STATE_PRESENT As Short = &H20S
    Public Const SCARD_STATE_ATRMATCH As Short = &H40S
    Public Const SCARD_STATE_EXCLUSIVE As Short = &H80S
    Public Const SCARD_STATE_INUSE As Short = &H100S
    Public Const SCARD_STATE_MUTE As Short = &H200S
    Public Const SCARD_STATE_UNPOWERED As Short = &H400S

    Public Const SCARD_SHARE_EXCLUSIVE As Short = 1
    Public Const SCARD_SHARE_SHARED As Short = 2
    Public Const SCARD_SHARE_DIRECT As Short = 3

    Public Const SCARD_LEAVE_CARD As Short = 0
    Public Const SCARD_RESET_CARD As Short = 1
    Public Const SCARD_UNPOWER_CARD As Short = 2
    Public Const SCARD_EJECT_CARD As Short = 3

    'Error Codes
    Public Const SCARD_F_INTERNAL_ERROR As Integer = &H80100001
    Public Const SCARD_E_CANCELLED As Integer = &H80100002
    Public Const SCARD_E_INVALID_HANDLE As Integer = &H80100003
    Public Const SCARD_E_INVALID_PARAMETER As Integer = &H80100004
    Public Const SCARD_E_INVALID_TARGET As Integer = &H80100005
    Public Const SCARD_E_NO_MEMORY As Integer = &H80100006
    Public Const SCARD_F_WAITED_TOO_LONG As Integer = &H80100007
    Public Const SCARD_E_INSUFFICIENT_BUFFER As Integer = &H80100008
    Public Const SCARD_E_UNKNOWN_READER As Integer = &H80100009
    Public Const SCARD_E_TIMEOUT As Integer = &H8010000A
    Public Const SCARD_E_SHARING_VIOLATION As Integer = &H8010000B
    Public Const SCARD_E_NO_SMARTCARD As Integer = &H8010000C
    Public Const SCARD_E_UNKNOWN_CARD As Integer = &H8010000D
    Public Const SCARD_E_CANT_DISPOSE As Integer = &H8010000E
    Public Const SCARD_E_PROTO_MISMATCH As Integer = &H8010000F
    Public Const SCARD_E_NOT_READY As Integer = &H80100010
    Public Const SCARD_E_INVALID_VALUE As Integer = &H80100011
    Public Const SCARD_E_SYSTEM_CANCELLED As Integer = &H80100012
    Public Const SCARD_F_COMM_ERROR As Integer = &H80100013
    Public Const SCARD_F_UNKNOWN_ERROR As Integer = &H80100014
    Public Const SCARD_E_INVALID_ATR As Integer = &H80100015
    Public Const SCARD_E_NOT_TRANSACTED As Integer = &H80100016
    Public Const SCARD_E_READER_UNAVAILABLE As Integer = &H80100017
    Public Const SCARD_P_SHUTDOWN As Integer = &H80100018
    Public Const SCARD_E_PCI_TOO_SMALL As Integer = &H80100019
    Public Const SCARD_E_READER_UNSUPPORTED As Integer = &H8010001A
    Public Const SCARD_E_DUPLICATE_READER As Integer = &H8010001B
    Public Const SCARD_E_CARD_UNSUPPORTED As Integer = &H8010001C
    Public Const SCARD_E_NO_SERVICE As Integer = &H8010001D
    Public Const SCARD_E_SERVICE_STOPPED As Integer = &H8010001E
    Public Const SCARD_W_UNSUPPORTED_CARD As Integer = &H80100065
    Public Const SCARD_W_UNRESPONSIVE_CARD As Integer = &H80100066
    Public Const SCARD_W_UNPOWERED_CARD As Integer = &H80100067
    Public Const SCARD_W_RESET_CARD As Integer = &H80100068
    Public Const SCARD_W_REMOVED_CARD As Integer = &H80100069

    ' Protocol
    Public Const SCARD_PROTOCOL_UNDEFINED As Short = &H0S
    Public Const SCARD_PROTOCOL_T0 As Short = &H1S
    Public Const SCARD_PROTOCOL_T1 As Short = &H2S
    Public Const SCARD_PROTOCOL_RAW As Integer = &H10000
    Public Const SCARD_PROTOCOL_DEFAULT As Integer = &H80000000


    ' Reader State
    Public Const SCARD_UNKNOWN As Short = 0
    Public Const SCARD_ABSENT As Short = 1
    Public Const SCARD_PRESENT As Short = 2
    Public Const SCARD_SWALLOWED As Short = 3
    Public Const SCARD_POWERED As Short = 4
    Public Const SCARD_NEGOTIABLE As Short = 5
    Public Const SCARD_SPECIFIC As Short = 6

    'Buffer size
    Public Const NUMBER_OF_READERS As Short = 4
    Public Const NAME_LENGTH As Short = 100
    Public Const MAX_RESPONSE As Short = 600
    Public Const MAX_APDU_BUFFER As Short = 1024

    'Error
    Public Const ERROR_GENERAL As Short = &HFFFES
    Public Const SUCCESS As Integer = 36864 '&H9000

    'Get Data type
    Public Const GET_MIFAREONTE As Short = &H0S
    Public Const GET_TYPEA_SMART_CARD As Short = &H100S
    Public Const GET_ULTRALIGHT As Short = &H200S
    Public Const GET_MIFAREPLUS_L0 As Short = &H300S
    Public Const GET_MIFAREPLUS_L1 As Short = &H301S
    Public Const GET_MIFAREPLUS_L2 As Short = &H302S
    Public Const GET_MIFAREPLUS_L3 As Short = &H303S
    Public Const GET_MIFAREPLUS_L0S As Short = &H304S
    Public Const GET_TYPEB_SMART_CARD As Short = &H2000S
    Public Const GET_SR176 As Short = &H2100S
    Public Const GET_SRIX4K As Short = &H2200S
    Public Const GET_AT88F020 As Short = &H2300S
    Public Const GET_SRI512 As Short = &H2200S
    Public Const GET_ISO15693 As Short = &H4000S
    Public Const GET_SAM_1 As Short = &H6000S
    Public Const GET_SAM_2 As Short = &H6001S
    Public Const GET_SAM_3 As Short = &H6002S
    Public Const GET_SAM_4 As Short = &H6003S

    'Protocol
    '  SCARD_PCI_T0       : SCARD_IO_REQUEST = (dwProtocol:1; dbPciLength:8)
    '  SCARD_PCI_T1       : SCARD_IO_REQUEST = (dwProtocol:2; dbPciLength:8)


    Public Declare Function GetOpenCardNamea Lib "Winscard.dll" (ByVal pDlgStruc As Integer) As Integer

    Public Declare Function SCardEstablishContext Lib "Winscard.dll" (ByVal dwScope As Integer, ByVal pvReserved1 As Integer, ByVal pvReserved2 As Integer, ByRef phContext As Integer) As Integer

    Public Declare Function SCardReleaseContext Lib "Winscard.dll" (ByVal hContext As Integer) As Integer

    Public Declare Function SCardConnect Lib "Winscard.dll" Alias "SCardConnectA" (ByVal hContext As Integer, ByVal szReaderName As String, ByVal dwShareMode As Integer, ByVal dwPrefProtocol As Integer, ByRef hCard As Integer, ByRef ActiveProtocol As Integer) As Integer

    Public Declare Function SCardDisconnect Lib "Winscard.dll" (ByVal hCard As Integer, ByVal Disposistion As Integer) As Integer

    Public Declare Function SCardBeginTransaction Lib "Winscard.dll" (ByVal hCard As Integer) As Integer

    Public Declare Function SCardEndTransaction Lib "Winscard.dll" (ByVal hCard As Integer, ByVal Disposition As Integer) As Integer

    Public Declare Function SCardState Lib "Winscard.dll" (ByVal hCard As Integer, ByRef State As Integer, ByRef Protocol As Integer, ByRef ATR As Byte, ByRef ATRLen As Integer) As Integer

    Public Declare Function SCardStatus Lib "Winscard.dll" Alias "SCardStatusA" (ByVal hCard As Integer, ByVal szReaderName As String, ByRef pcchReaderLen As Integer, ByRef State As Integer, ByRef Protocol As Integer, ByRef ATR As Byte, ByRef ATRLen As Integer) As Integer

    'UPGRADE_WARNING: ½á¹¹ SCARD_IO_REQUEST ¿ÉÄÜÒªÇó·âËÍ´¦ÀíÊôÐÔ×÷Îª´Ë Declare Óï¾äÖÐµÄ²ÎÊý´«µÝ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"¡±
    'UPGRADE_WARNING: ½á¹¹ SCARD_IO_REQUEST ¿ÉÄÜÒªÇó·âËÍ´¦ÀíÊôÐÔ×÷Îª´Ë Declare Óï¾äÖÐµÄ²ÎÊý´«µÝ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"¡±
    Public Declare Function SCardTransmit Lib "Winscard.dll" (ByVal hCard As Integer, ByRef pioSendRequest As SCARD_IO_REQUEST, ByRef SendBuff As Byte, ByVal SendBuffLen As Integer, ByRef pioRecvRequest As SCARD_IO_REQUEST, ByRef RecvBuff As Byte, ByRef RecvBuffLen As Integer) As Integer

    Public Declare Function SCardListReaders Lib "Winscard.dll" Alias "SCardListReadersA" (ByVal hContext As Integer, ByVal mzGroup As String, ByVal ReaderList As String, ByRef pcchReaders As Integer) As Integer

    'UPGRADE_ISSUE: ²»Ö§³Ö½«²ÎÊýÉùÃ÷Îª¡°As Any¡±¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"¡±
    'UPGRADE_ISSUE: ²»Ö§³Ö½«²ÎÊýÉùÃ÷Îª¡°As Any¡±¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"¡±
    Public Declare Sub CopyMemory Lib "kernel32 " Alias "RtlMoveMemory " (ByRef Destination As String, ByRef Source As String, ByVal Length As Integer)


    Public SendRequest As SCARD_IO_REQUEST
    Public ReceRequest As SCARD_IO_REQUEST

    Public SW1, SW2 As Integer
    Public ProtocolType As Integer

    '±äÁ¿¶¨Òå
    Dim m_bProtocolType As Short 'PC/SC Reader µ±Ç°Ð­Òé
    Dim DeviceContext As Integer 'PC/SC Reader Éè±¸ÃèÊö·û
    Public m_bReaderCounter As Short 'Reader ¸öÊý
    Public m_bActiveNO As Short 'µ±Ç°¼¤»îµÄReaderÊÇÄÄÒ»¸ö
    Public m_bActiveFlag As Boolean 'ÊÇ·ñÓÐ¼¤»îReader
    Public ReaderName(NUMBER_OF_READERS) As String 'ËùÓÐReaderÃû×Ö
    Dim SendAPDU(MAX_APDU_BUFFER) As Byte '·¢ËÍAPDU Buffer
    Dim ReceiveAPDU(MAX_APDU_BUFFER) As Byte '½ÓÊÕAPDU Buffer
    Dim hScardHandle As Integer 'Reader ¾ä±ú   THandle= Cardinal

    'Updata Reader number
    'Return Reader number
    Public Function UpdateReadersStatus() As Short

        Dim lReturn, ReaderListSize As Object
        Dim ReaderList As String
        ReaderList = New System.String(vbNullChar, 256)
        Dim sReaderGroup As String

        'Get PC/SC devicecontext
        m_bActiveFlag = False
        m_bActiveNO = 0
        m_bReaderCounter = 0

        'Is connected
        If (m_bActiveFlag) Then
            'Connected
            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            lReturn = SCardDisconnect(hScardHandle, SCARD_EJECT_CARD)
            SCardReleaseContext(DeviceContext)
        End If

        'Establish deveice context
        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        lReturn = SCardEstablishContext(SCARD_SCOPE_USER, 0, 0, DeviceContext)
        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        Dim TempStr As String
        Dim IndexNum As Short
        If (SCARD_S_SUCCESS = lReturn) Then
            'success£ºList all device context
            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó ReaderListSize µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            lReturn = SCardListReaders(DeviceContext, sReaderGroup, CStr(0), ReaderListSize)

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            If lReturn = SCARD_S_SUCCESS Then
                '   Dim ReaderList As String * ReaderListSize
                'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó ReaderListSize µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
                'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
                lReturn = SCardListReaders(DeviceContext, sReaderGroup, ReaderList, ReaderListSize)


                IndexNum = 1
                TempStr = ""

                ReaderName = Split(ReaderList, vbNullChar)
                m_bReaderCounter = 0
                While (m_bReaderCounter < ReaderName.Length - 1 And ReaderName(m_bReaderCounter) <> "")
                    m_bReaderCounter = m_bReaderCounter + 1
                End While

                UpdateReadersStatus = m_bReaderCounter
                Exit Function
            End If
        End If
        UpdateReadersStatus = 0

    End Function

    'Connect Reader
    Public Function ConnectReader(ByVal ReaderNo As Short) As Boolean

        Dim lReturn As Short
        Dim ActiveProtocol As Integer

        'Reader count
        If (m_bReaderCounter < (ReaderNo + 1)) Then
            ConnectReader = False
        End If

        If (m_bActiveFlag = False) Then
            'Connect Reader
            lReturn = SCardDisconnect(hScardHandle, SCARD_EJECT_CARD)
            m_bProtocolType = SCARD_PROTOCOL_T0 Or SCARD_PROTOCOL_T1
            lReturn = SCardConnect(DeviceContext, ReaderName(ReaderNo), SCARD_SHARE_EXCLUSIVE, m_bProtocolType, hScardHandle, ActiveProtocol)
            If (SCARD_S_SUCCESS = lReturn) Then
                m_bProtocolType = ActiveProtocol
                m_bActiveNO = ReaderNo
                m_bActiveFlag = True

                ConnectReader = True
                Exit Function
            End If
        End If
        ConnectReader = False
    End Function

    'Disconnect reader
    Public Function DisconnectReader() As Boolean

        Dim lReturn As Short

        If (m_bActiveFlag) Then
            lReturn = SCardDisconnect(hScardHandle, SCARD_EJECT_CARD)
            If (SCARD_S_SUCCESS <> lReturn) Then
                DisconnectReader = False
                Exit Function
            End If
        End If
        m_bActiveFlag = False
        DisconnectReader = True
    End Function



    'Send APDU
    Public Function APDUCommand(ByRef APDU() As Byte, ByVal iLen As Integer, ByRef cReceiveData() As Byte, ByRef iReceiveLen As Short) As Integer

        Dim cReceive(MAX_RESPONSE) As Byte
        Dim lReturn As Object
        Dim i As Short
        Dim SW1 As Object
        Dim SW2 As Short
        Dim ReceiveLen As Integer

        SendRequest.dwProtocol = m_bProtocolType
        SendRequest.cbPciLength = Len(SendRequest)
        ReceRequest.dwProtocol = SCARD_PROTOCOL_T0
        ReceRequest.cbPciLength = Len(ReceRequest)

        ReceiveLen = MAX_RESPONSE + 1

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        lReturn = SCardTransmit(hScardHandle, SendRequest, APDU(0), iLen, ReceRequest, cReceive(0), ReceiveLen)

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó lReturn µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        If (lReturn <> SCARD_S_SUCCESS) Then
            iReceiveLen = 0
            APDUCommand = &HFFFFS
            Exit Function
        End If

        For i = 0 To ReceiveLen - 1
            cReceiveData(i) = cReceive(i)
        Next i

        iReceiveLen = ReceiveLen - 2
        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó SW1 µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        SW1 = cReceive(ReceiveLen - 2)
        SW2 = cReceive(ReceiveLen - 1)
        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó SW1 µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        APDUCommand = SW1 * 256 + SW2
    End Function

    'Get DATA APDU
    Public Function GetData(ByVal iCardType As Short, ByRef cReceiveData() As Byte, ByRef iReceiveLen As Short) As Integer

        Dim iSW As Integer

        iSW = ERROR_GENERAL

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HCAS
            SendAPDU(2) = (iCardType \ 2 ^ 8) And &HFFS
            SendAPDU(3) = iCardType And &HFFS
            SendAPDU(4) = &H0S

            iSW = APDUCommand(SendAPDU, 5, cReceiveData, iReceiveLen)
        End If
        GetData = iSW
    End Function

    'Authentication APDU
    Public Function Authentication(ByVal iBlockAddr As Short, ByVal cKeyType As Byte, ByVal cKeyCofig As Byte, ByRef cpKeyData() As Byte) As Integer

        Dim iSendLen As Object
        Dim i As Short
        Dim iSW As Integer
        Dim iReceiveLen As Short

        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &H88S
            SendAPDU(2) = (iBlockAddr \ 2 ^ 8) And &HFFS
            SendAPDU(3) = iBlockAddr And &HFFS
            SendAPDU(4) = cKeyType
            SendAPDU(5) = cKeyCofig

            If ((cKeyCofig And &H80S) <> 0) Then
                'Authentication use saved key in flash
                SendAPDU(6) = cpKeyData(0)
                'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSendLen µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
                iSendLen = 7
            Else
                'Authentication use current inputing key
                'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSendLen µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
                iSendLen = 6 + cKeyCofig
                For i = 0 To cKeyCofig - 1
                    SendAPDU(6 + i) = cpKeyData(i)
                Next i
            End If

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSendLen µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            iSW = APDUCommand(SendAPDU, iSendLen, ReceiveAPDU, iReceiveLen)
        End If

        Authentication = iSW
    End Function

    'Read card block
    Public Function ReadBinaryBlock(ByVal iBlockAddr As Short, ByRef cReceiveData() As Byte, ByRef iReceiveLen As Short) As Integer

        Dim iSW As Object
        Dim i As Integer

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        iSW = ERROR_GENERAL

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HB0S
            SendAPDU(2) = (iBlockAddr \ 2 ^ 8) And &HFFS
            SendAPDU(3) = iBlockAddr And &HFFS
            SendAPDU(4) = iReceiveLen And &HFFS

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            iSW = APDUCommand(SendAPDU, 5, ReceiveAPDU, iReceiveLen)
            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            If (SUCCESS = iSW) Then
                For i = 0 To iReceiveLen - 1
                    cReceiveData(i) = ReceiveAPDU(i)
                Next i
            End If
        End If

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        ReadBinaryBlock = iSW
    End Function

    'Write card block
    Public Function UpdataBinaryBlock(ByVal iBlockAddr As Short, ByRef cSendData() As Byte, ByVal iDataLen As Short) As Integer

        Dim iSendLen As Object
        Dim i As Short
        Dim iSW As Integer
        Dim iReceiveLen As Short

        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HD6S
            SendAPDU(2) = (iBlockAddr \ 2 ^ 8) And &HFFS
            SendAPDU(3) = iBlockAddr And &HFFS
            SendAPDU(4) = iDataLen And &HFFS
            For i = 0 To iDataLen - 1
                SendAPDU(5 + i) = cSendData(i)
            Next i

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSendLen µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            iSendLen = 5 + iDataLen

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSendLen µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            iSW = APDUCommand(SendAPDU, iSendLen, ReceiveAPDU, iReceiveLen)
        End If

        UpdataBinaryBlock = iSW
    End Function

    'Value operation: Initialize purse(00), increase purse(01), decrease purse(02)
    Public Function ValueBlockOperation(ByVal iBlockAddr As Short, ByVal cOperateType As Byte, ByRef iValue As Integer) As Integer

        Dim iReceiveLen As Short
        Dim iSW As Integer

        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HD7S
            SendAPDU(2) = (iBlockAddr / 256) And &HFFS
            SendAPDU(3) = iBlockAddr And &HFFS
            SendAPDU(4) = &H5S
            SendAPDU(5) = cOperateType
            SendAPDU(9) = (iValue / 2 ^ 24) And &HFFS
            SendAPDU(8) = (iValue / 2 ^ 16) And &HFFS
            SendAPDU(7) = (iValue / 2 ^ 8) And &HFFS
            SendAPDU(6) = iValue And &HFFS

            iSW = APDUCommand(SendAPDU, 10, ReceiveAPDU, iReceiveLen)
        End If

        ValueBlockOperation = iSW
    End Function

    'read purse
    Public Function ReadValueBlock(ByVal iBlockAddr As Short, ByRef iValue As Integer) As Integer

        '		Dim i As Short
        Dim iReceiveLen As Short
        Dim iSW As Integer

        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HB1S
            SendAPDU(2) = (iBlockAddr \ 8) And &HFFS
            SendAPDU(3) = iBlockAddr And &HFFS
            SendAPDU(4) = &H4S

            iSW = APDUCommand(SendAPDU, 5, ReceiveAPDU, iReceiveLen)
            If (SUCCESS = iSW) Then
                iValue = ReceiveAPDU(0) * 2 ^ 24 + ReceiveAPDU(1) * 2 ^ 16 + ReceiveAPDU(2) * 2 ^ 8 + ReceiveAPDU(0)
            End If
        End If

        ReadValueBlock = iSW
    End Function

    'backup purse
    Public Function RestoreValueBlock(ByVal iSrcAddr As Short, ByVal iTargetAddr As Short) As Integer

        Dim iReceiveLen As Short
        Dim iSW As Integer
        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &HD7S
            SendAPDU(2) = (iSrcAddr \ 256) And &HFFS
            SendAPDU(3) = iSrcAddr And &HFFS
            SendAPDU(4) = &H3S
            SendAPDU(5) = &H3S
            SendAPDU(6) = (iTargetAddr \ 256) And &HFFS
            SendAPDU(7) = iTargetAddr And &HFFS

            iSW = APDUCommand(SendAPDU, 8, ReceiveAPDU, iReceiveLen)
        End If

        RestoreValueBlock = iSW
    End Function

    'TypeA card Halt
    Public Function TypeA_Halt() As Integer
        Dim iSW As Object
        Dim iReceiveLen As Short

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        iSW = ERROR_GENERAL
        iReceiveLen = 0

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS
            SendAPDU(1) = &H0S
            SendAPDU(2) = &H0S
            SendAPDU(3) = &H1S
            SendAPDU(4) = &H0S

            'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
            iSW = APDUCommand(SendAPDU, 5, ReceiveAPDU, iReceiveLen)
        End If

        'UPGRADE_WARNING: Î´ÄÜ½âÎö¶ÔÏó iSW µÄÄ¬ÈÏÊôÐÔ¡£ µ¥»÷ÒÔ»ñµÃ¸ü¶àÐÅÏ¢:¡°ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"¡±
        TypeA_Halt = iSW
    End Function

    Public Sub setLed(ByVal colors As Integer, Optional number As Integer = 1, Optional flash As Boolean = False)
        Dim iSW As Integer
        Dim CardInfo(20) As Byte
        Dim ReceiveLen As Short
        Dim colorMask As Integer = &HFFS
        Dim time_out As Integer = 0
        Dim time_in As Integer = &HFS
        If (colors = LED_COLOR.RED) Then
            colors = &H14S
        ElseIf (colors = LED_COLOR.GREEN) Then
            colors = &H24S
            colorMask = &H8FS
        ElseIf (colors = LED_COLOR.BLUE) Then
            colors = &H44S
        ElseIf (colors = LED_COLOR.YELLOW) Then
            colors = &H88S
            colorMask = &H88S
            If flash Then
                time_in = &H1S
                time_out = &H1S
            End If
        End If
        iSW = ERROR_GENERAL

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS  '' CLA
            SendAPDU(1) = &H0S  '' INS
            SendAPDU(2) = &HFFS  '' P1
            SendAPDU(3) = &H2S  '' P2
            SendAPDU(4) = &H5S  '' Le
            SendAPDU(5) = colors '' colors
            SendAPDU(6) = colorMask ''  colorMask
            SendAPDU(7) = time_in '' 
            SendAPDU(8) = time_out
            SendAPDU(9) = number '' Le

            iSW = APDUCommand(SendAPDU, 10, CardInfo, ReceiveLen)
            iSW = 0
        End If
    End Sub

    Public Sub setBuzzer(ByVal beep As Integer)
        Dim iSW As Integer
        Dim CardInfo(20) As Byte
        Dim ReceiveLen As Short
        iSW = ERROR_GENERAL

        Dim beep_time_in As Integer = 1
        Dim beep_time_out As Integer = 1
        Dim beep_nbr As Integer = 1
        If beep = BUZZER_BEEP.BEEP_SUCCESS Then
            beep_nbr = 3
        ElseIf beep = BUZZER_BEEP.BEEP_ERROR Then
            beep_time_out = 0
            beep_nbr = 8
        End If

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS  '' CLA
            SendAPDU(1) = &H0S  '' INS
            SendAPDU(2) = &HFFS  '' P1
            SendAPDU(3) = &H3S  '' P2
            SendAPDU(4) = &H5S  '' Le
            SendAPDU(5) = &H8S '' stat
            SendAPDU(6) = &H1S '' 
            SendAPDU(7) = beep_time_out '' 
            SendAPDU(8) = beep_time_in '' 
            SendAPDU(9) = beep_nbr '' Le

            iSW = APDUCommand(SendAPDU, 10, CardInfo, ReceiveLen)
        End If
    End Sub

    Public Sub setText(ByVal str As String, Optional clear As Boolean = True, Optional row As Integer = 0, Optional column As Integer = 0)
        Dim iSW As Integer
        Dim CardInfo(20) As Byte
        Dim ReceiveLen As Short

        Dim enc As Encoding = New UTF8Encoding(True, True)
        Dim textBytes() As Byte = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(str)
        iSW = ERROR_GENERAL

        Dim clearState As Integer = 10
        If clear Then
            clearState = 12
        End If

        If (m_bActiveFlag) Then
            SendAPDU(0) = &HFFS  '' CLA
            SendAPDU(1) = &H0S  '' INS
            SendAPDU(2) = &HFCS  '' P1
            SendAPDU(3) = &H2S  '' P2
            SendAPDU(4) = textBytes.Length + 3 '' Le
            SendAPDU(5) = clearState '' Le
            SendAPDU(6) = row * 2 '' Le
            SendAPDU(7) = column * 2 '' Le
            Array.Resize(SendAPDU, SendAPDU.Length + textBytes.Length)
            textBytes.CopyTo(SendAPDU, 8)

            iSW = APDUCommand(SendAPDU, textBytes.Length + 3 + 5, CardInfo, ReceiveLen)
        End If
    End Sub

    'Test
    Public Sub Test(ByVal beep_time_in As Integer, ByVal beep_time_out As Integer)

    End Sub


End Class
