' ==========================================
' FileFormatInfo.vb
' CSV/TSVなどのファイル形式情報
' 役割: 入力ファイルがCSVかTSVかを判定した結果を保持
' 使い所: ファイル読み込み時とOKデータ出力時
' ==========================================
Public Class FileFormatInfo
    ''' <summary>拡張子（例: ".csv", ".tsv"）</summary>
    Public Property Extension As String
    
    ''' <summary>デリミタ（例: "," or vbTab）</summary>
    Public Property Delimiter As String
    
    ''' <summary>形式名（例: "CSV", "TSV"）</summary>
    Public Property Name As String
    
    Public Sub New()
    End Sub
End Class