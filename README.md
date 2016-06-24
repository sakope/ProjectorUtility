:black_large_square::black_medium_small_square::black_large_square:  
:black_medium_small_square::black_large_square::black_medium_small_square:  
:black_large_square::black_medium_small_square::black_large_square:  
ProjectorUtility
====


## Description
複数台のプロジェクターのブレンドとマスク、斜めマスク、UVシフトの設定を行うツールです  
設定はchromeの様なタブの切り替えで、各プロジェクター毎に行います。  
主にキッズ用に作成しており、キッズ系のコンポーネント群と連携した作りにしておりますが、
キッズ系コンポーネントは内包していますので、こちらをDLして戴ければそのまま使えます。  
Pluginsディレクトリ内のコンポーネントがすでに入っているプロジェクトの場合は、該当ディレクトリを抜いて使用してください。
複数台のプロジェクターへの対応と、OSC等からのビューポート座標補正用APIの用意、uGUIビュー、ブレンドを中央から広げる事等を目的としています。

## Usage
ProjectorUtility/Sampleの中に、サンプルシーンが二つ入っていますので、こちらの流れでコンポーネントを設置してください。  
ControllerUISeparateは、ViewとControllerを分けて、特定のカメラにImageEffectの要領でブレンディングう場合のセットアップです。  
ControllerOnUISampleは、ViewとControllerをひとまとめにアタッチして、ブレンディング用のカメラを専用に用意した物です。  

1. サンプルシーンでは、Eキーを押すと設定画面が開きます。
1. Commonタブの"Number of projectors"の中に、楯列と横列のプロジェクター数を入力して頂くと、
タブがプロジェクターの数だけ生成されます。
1. 各タブで個別にひとつずつブレンディングやマスキング、UVシフトを行います。
1. CommonタブのApply screen 1 to allにチェックを入れて頂くとスクリーン1の設定と同じものが全スクリーンに反映されます。
1. CommonタブのBlacknessでブレンディングの濃さ、Curveでグラデーションの緩やかさを変更できます。
1. Saveボタンを押すと設定が保存されます。
1. Discardボタンを押すと、設定をすべて以前のSave時の物に戻します。
1. Eキーを押すと閉じます。閉じたときに保存も行いますので、保存したくない場合は、Discardボタンを押して、設定を元に戻してから閉じて下さい。 


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
float NormalizedMinUpperBlendHeight
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
float BlendingWidth()
```
全ブレンド分の横幅の合算値を返します（スクリーン座標）.  
  
  
```CS
float BlendingHeight()
```
全ブレンド分の高さの合算値を返します（スクリーン座標）.  
  
  
```CS
float NormalizedBlendWidth()
```
全ブレンド分の横幅の合算値を返します（ビューポート座標）。  
  
  
```CS
float NormalizedBlendHeight()
```
全ブレンド分の高さの合算値を返します（ビューポート座標）。  
  
  
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
  
  
## RequireComponent
#####Sketchシリーズコンポーネント群  
[XMLStrage](http://github.team-lab.local/SketchSeries/XmlStorage)  
UIComponent  

#####外部ソース  
[UniRX](https://github.com/neuecc/UniRx)  
  
*※全て内包しています*  
