ProjectorUtility
====


## Description
複数台のプロジェクターのブレンドとマスク、斜めマスク、UVシフトの設定を行うツールです。  
設定はchromeの様な、タブの切り替えで各種設定を行います。

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


## RequireComponent
#####Sketchシリーズコンポーネント群  
[XMLStrage](http://github.team-lab.local/SketchSeries/XmlStorage)  
UIComponent  

#####外部ソース  
[UniRX](https://github.com/neuecc/UniRx)  
  
*※全て内包しています*  
