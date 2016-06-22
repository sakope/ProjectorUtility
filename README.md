ProjectorUtility
====


## Description
複数台のプロジェクターのブレンドとマスク、斜めマスク、UVシフトの設定を行うツールです。  
設定はchromeの様なタブの切り替えで、各プロジェクター毎に行います。  
主にキッズ用に作成しており、キッズ系のコンポーネント群と連携した作りにしておりますが、
キッズ系コンポーネントは内包していますので、こちらをDLして戴ければそのまま使えます。
キッズに追加する際は、Pluginsディレクトリを抜いて使用してください。
複数台のプロジェクターへの対応と、OSC等からのビューポート座標補正用APIの用意、uGUIビュー、ブレンドを中央から広げる事等を目的としています。

## Usage
ProjectorUtility/Sampleの中に、サンプルシーンが二つ入っていますので、こちらの流れでコンポーネントを設置してください。  
ControllerUISeparateは、ViewとControllerを分けて、特定のカメラにImageEffectの要領でブレンディングを行います。  
ControllerOnUISampleは、ViewとControllerをひとまとめにアタッチして、ブレンディング用のカメラを個別に用意した物です。  

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
  
  
```CS
Vector2 GetAdjustedPosition(Vector2 position)
```
ViewPort座標を引数に持たせると、ブレンド喪失分とuvシフト分を補正した値を返します。  
  
  
```CS
float BlendingWidth()
```
全ブレンド分のスクリーン座標での横幅の合算値を返します  
  
  
```CS
float BlendingHeight()
```
全ブレンド分のスクリーン座標での高さの合算値を返します  
  
  
```CS
float NormalizedBlendWidth()
```
全ブレンド分のビューポート座標での横幅の合算値を返します  
  
  
```CS
float NormalizedBlendHeight()
```
全ブレンド分のビューポート座標での高さの合算値を返します  


## RequireComponent
#####Sketchシリーズコンポーネント群  
[XMLStrage](http://github.team-lab.local/SketchSeries/XmlStorage)  
UIComponent  

#####外部ソース  
[UniRX](https://github.com/neuecc/UniRx)  
  
*※全て内包しています*  
