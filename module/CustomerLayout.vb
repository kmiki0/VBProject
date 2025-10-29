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
                New ColumnDefinition(0, "顧客コード"),
                New ColumnDefinition(1, "支店コード")
            },
            .UniqueColumns = {0, 1},
            .RequiredColumns = {
                New ColumnDefinition(0, "顧客コード"),
                New ColumnDefinition(1, "支店コード"),
                New ColumnDefinition(2, "顧客名")
            },
            .LengthRules = {
                New LengthRule(0, "顧客コード", 10),
                New LengthRule(1, "支店コード", 3),
                New LengthRule(2, "顧客名", 50, 1),
                New LengthRule(3, "住所", 100),
                New LengthRule(4, "電話番号", 13)
            },
            .DateColumns = {
                New ColumnDefinition(5, "登録日"),
                New ColumnDefinition(6, "更新日")
            }
        }
    End Function
    
End Class