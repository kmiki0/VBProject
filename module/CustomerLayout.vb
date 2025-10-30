Public Enum CustomerColumns
    顧客コード = 0
    支店コード = 1
    顧客名 = 2
    住所 = 3
    電話番号 = 4
    登録日 = 5
    更新日 = 6
    削除フラグ = 7
End Enum

' ==========================================
' CustomerLayout.vb
' 顧客マスタファイルのレイアウト定義
    ' 列インデックスは0始まり
    ' キー項目: 顧客コード + 支店コード（エラーログに表示される）
    ' 一意性制約: 顧客コード + 支店コードの組み合わせで重複チェック
    ' 必須項目: 顧客コード、支店コード、顧客名
    ' 桁数: 顧客コード10桁、顧客名は1〜50桁など
    ' 日付: 登録日、更新日
' ==========================================
Public Class CustomerLayout
    
    ''' <summary>
    ''' 顧客マスタのファイル定義を取得
    ''' </summary>
    Public Shared Function GetDefinition() As FileDefinition
        Return New FileDefinition With {
            .Name = "顧客マスタ",
            .OutputFileName = "customer",
            .KeyColumns = {
                New ColumnDefinition(CustomerColumns.顧客コード),
                New ColumnDefinition(CustomerColumns.支店コード)
            },
            .UniqueColumns = {
                CustomerColumns.顧客コード,
                CustomerColumns.支店コード
            },
            .RequiredColumns = {
                New ColumnDefinition(CustomerColumns.顧客コード),
                New ColumnDefinition(CustomerColumns.支店コード),
                New ColumnDefinition(CustomerColumns.顧客名)
            },
            .LengthRules = {
                New LengthRule(CustomerColumns.顧客コード, 10),
                New LengthRule(CustomerColumns.支店コード, 3),
                New LengthRule(CustomerColumns.顧客名, 50, 1),
                New LengthRule(CustomerColumns.住所, 100),
                New LengthRule(CustomerColumns.電話番号, 13)
            },
            .DateColumns = {
                New ColumnDefinition(CustomerColumns.登録日),
                New ColumnDefinition(CustomerColumns.更新日)
            }
        }
    End Function
    
End Class