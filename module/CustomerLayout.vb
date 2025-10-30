' ==========================================
' 顧客マスタファイルのレイアウト定義
' ==========================================
Public Enum FileLayout
    顧客コード = 0
    支店コード = 1
    顧客名 = 2
    住所 = 3
    電話番号 = 4
    登録日 = 5
    更新日 = 6
    削除フラグ = 7
End Enum

Public Class CustomerLayout
    
    ''' <summary>
    ''' 顧客マスタのファイル定義を取得
    ''' </summary>
    Public Shared Function GetDefinition() As FileDefinition
        Return New FileDefinition With {
            .Name = "顧客マスタ",
            .OutputFileName = "customer",
            .Delimiter = vbTab,
            .ExpectedColumnCount = 8,  
            .KeyColumns = {
                New ColumnDefinition(FileLayout.顧客コード),
                New ColumnDefinition(FileLayout.支店コード)
            },
            .UniqueColumns = {
                FileLayout.顧客コード,
                FileLayout.支店コード
            },
            .RequiredColumns = {
                New ColumnDefinition(FileLayout.顧客コード),
                New ColumnDefinition(FileLayout.支店コード),
                New ColumnDefinition(FileLayout.顧客名)
            },
            .LengthRules = {
                New LengthRule(FileLayout.顧客コード, 10),
                New LengthRule(FileLayout.支店コード, 3),
                New LengthRule(FileLayout.顧客名, 50, 1),
                New LengthRule(FileLayout.住所, 100),
                New LengthRule(FileLayout.電話番号, 13)
            }
        }
    End Function
    
End Class