:black_large_square::black_medium_small_square::black_large_square:  
:black_medium_small_square::black_large_square::black_medium_small_square:  
:black_large_square::black_medium_small_square::black_large_square:  
ProjectorUtility
====


## Description
複数台のプロジェクターのブレンドとマスク・レクトマスク・斜めマスク・UVシフトの設定を行うツールです  
設定はchromeの様なタブの切り替えで、各プロジェクター毎に行います。  

## Usage
ProjectorUtility/Sampleの中に、サンプルシーンが二つ入っていますので、こちらの流れでコンポーネントを設置してください。  
サンプルシーンでは、Pキーを押すと詳細設定画面・Mキーでマスク・Rキーでレクトマスク・Sキーでシンプルブレンディングが開きます。以下はPキーのブレンディング画面の説明になります。　　

1. Commonタブの"Number of projectors"の中に、楯列と横列のプロジェクター数を入力して頂くと、
タブがプロジェクターの数だけ生成されます。
1. BlacknessとBright Adjustは1にしてください。
1. Curveにはプロジェクターのガンマ値を入力してください。（プロジェクターの設定は、なるべくフラット特性なモードにして、全部のガンマ値は同じにしてください）
1. 各タブで個別にひとつずつブレンディングやマスキング、UVシフトを行います。
1. CommonタブのSymmetryにチェックを入れて頂くとスクリーン1の設定と同じものが全スクリーンに反映されます。
1. Saveボタンを押すと設定が保存され閉じます。
1. Discardボタンを押すと、設定をすべて以前のSave時の物に戻して閉じます。


## API
apiは
`ProjectorUtilityController` 
に集約しています。  
シングルトンにしていますので、
ProjectorUtilityController.Instance.API名
といった形で取得してください。  
  
***  
```CS
Vector2 GetAdjustedPosition(Vector2 position)
```
ViewPort座標を引数に持たせると、ブレンド喪失分とuvシフト分を補正した値を返します。  
  
  
```CS
float MinUpperBlendHeight()
```
一番上段のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（スクリーン座標）.  
  
  
```CS
float MinLowerBlendHeight()
```
一番下段のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（スクリーン座標）。  
  
  
```CS
float MinLeftBlendWidth()
```
一番左列のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（スクリーン座標）.  
  
  
```CS
float MinRightBlendWidth()
```
一番右列のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（スクリーン座標）.  
  
  
```CS
float NormalizedMinUpperBlendHeight()
```
一番上段のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（ビューポート座標）.  
  
  
```CS
float NormalizedMinLowerBlendHeight
```
一番下段のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（ビューポート座標）。  
  
  
```CS
float NormalizedMinLeftBlendWidth
```
一番左列のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（ビューポート座標）.  
  
  
```CS
float NormalizedMinRightBlendWidth
```
一番右列のプロジェクターのブレンド幅の中で、最も値が小さい物を返します（ビューポート座標）.  
  
  
```CS
float BlendingWidth(int row)
```
特定列の全ブレンド分の横幅の合算値を返します（スクリーン座標）.  
  
  
```CS
float BlendingHeight(int col)
```
特定列の全ブレンド分の高さの合算値を返します（スクリーン座標）.  
  
  
```CS
float NormalizedBlendWidth(int row)
```
特定行の全ブレンド分の横幅の合算値を返します（ビューポート座標）。  
  
  
```CS
float NormalizedBlendHeight(int col)
```
特定列の全ブレンド分の高さの合算値を返します（ビューポート座標）。  
  
  
```CS
int NumOfScreen
```
プロジェクターの数を取得できます（リードオンリー）.  
  
  
```CS
CommonSettingEntity GetCommonSettingEntity()
```
このEntityからプロジェクターの横の数・縦の数、濃度、power等の一般設定プロパティを参照できます（リードオンリー）.  
  
  
```CS
List<ScreenSettingEntity> GetScreenSettingEntities()
```
このEntityリストから、各プロジェクターのブレンド幅やマスク、UVシフト系の様々なプロパティを取得できます（リードオンリー）.  

  
## Respect and thanks
Kohei Aoyama氏のXMLStorageを保存機能に使用しています。

#####外部ソース  
[UniRX](https://github.com/neuecc/UniRx)  
