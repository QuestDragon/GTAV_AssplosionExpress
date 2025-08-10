# Assplosion Express - Created By QuestDragon
Version: 1.0.0
## 作成した経緯
Trainer Modにはテレポートという機能があります。パッとすぐ移動できて便利ですが、味気ないですよね。

そこで、インパクトのある移動手段として、お尻を爆発させながら移動できるスクリプトを作ってみました。

ちなみに、モチーフにしているのはとりすーぷ氏のInferno MOD内、パルプンテ機能のうちの1つ、[KetsuWarp.cs](https://github.com/TORISOUP/GTAV_InfernoScripts/blob/master/Inferno/InfernoScripts/Parupunte/Scripts/KetsuWarp.cs)となります。

## 機能
至ってシンプルで、行き先マーカーに向かって爆発しながらぶっ飛ぶスクリプトです。

車に乗っている場合は車ごとくるくる回りながら爆発して目的地までぶっ飛びます。

徒歩で使うと滑空姿勢になり、お尻あたりを爆発させながら目的地までぶっ飛びます。

## 機能追加、フィードバックについて
制作者は初心者なので何かと至らないところがあると思います。

不具合等を発見しましたら、QuestDragonまでご連絡ください。

また、「こんな機能がほしい！」「ここはこうしてほしい！」という要望がありましたらご相談ください。

こちらもスクリプトModについて勉強したいので、ご意見や要望はいつでもお待ちしております。

## 開発環境
C#を使用しています。

ScriptHookV DotNetを使用しており、バージョンは3.6.0のNightly ビルド 57で開発しています。

> [!IMPORTANT]
> 現時点では3.7.0のNightlyビルドが公開されています。本スクリプトを使用する際は3.7.0のNightlyビルドを導入の上ご使用ください。

## インストール
以下から各種ファイルをダウンロードし、スクリプトMod本体はScriptsフォルダに、前提条件のファイルはGTA5.exeと同じフォルダにコピーしてください。

| [Assplosion Express](https://github.com/QuestDragon/GTAV_AssplosionExpress/releases/latest/download/AssplosionExpress.zip) | [ScriptHookV](http://dev-c.com/gtav/scripthookv/) | [ScriptHookV DotNet 3.7.0 Nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases/latest) |
| ------------- | ------------- | ------------- | 

## インストール時のSCRIPT HOOK V ERRORについて
ScriptHookV DotNet Nightlyビルドを導入してGTA5を起動すると、「SCRIPT HOOK V ERROR」が表示され、scriptsフォルダに導入されている.NETスクリプトのすべてが読み込まれない現象になることがあります。

これは、ScriptHookV DotNetの前提条件を満たしていないことが原因です。**Releaseビルドでは動いていても、Nightlyビルドでは動かないことがあります。**

そのため、今一度次のコンポーネントがインストールされているかご確認ください。

| [.NET Framework 4.8 （ランタイム、開発者パックの"両方"が必要です。）](https://dotnet.microsoft.com/download/dotnet-framework/net48) | [Visual C++ Redistributable for Visual Studio 2019 x64](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) |
| ------------- | ------------- |

## 各種設定
設定はiniファイルから行います。

### Keys
**Start**：本スクリプトを動作させるキーです。押すと目的地までぶっ飛びます。

**End**：本スクリプトの動作を停止させるキーです。押すとスクリプトの動作が止まります。

指定する文字列は[こちらのサイト](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?redirectedfrom=MSDN&view=windowsdesktop-7.0)をご確認ください。指定が正しくない場合、キー操作は無効になります。

### Display
Assplosion Expressが表示する文字列を編集できます。

**WaypointText**：目的地マーカーがセットされていない場合に表示されます。

**WaypointTip**：目的地マーカーがセットされていない場合に、画面左上のヘルプテキストとして表示されます。

**NearestTip**：動作時に、目的地マーカーが現在位置と近い場合に画面左上のヘルプテキストとして表示されます。

**StartText**：動作時に表示されます。

**EmergencyUpText**：動作中、前方にぶつかりそうになると表示されます。

**ArrivingSoonText**：徒歩状態で動作中、目的地に近づいてパラシュートが展開される際に表示されます。

**ArrivingSoonTip**：徒歩状態で動作中、目的地に近づいてパラシュートが展開される際に画面左上のヘルプテキストとして表示されます。

**NotArriveText**：動作終了時、行き先マーカーがあった場所付近に着地していない場合に表示されます。

**ArrivedText**：動作終了時、行き先マーカーがあった場所付近に着地している場合に表示されます。

## 使い方
行き先マーカーをマップやクイックアクションメニュー等からセットしたら、iniファイルで指定したStartキーを押すだけで目的地までぶっ飛びます。

動作を止めたい場合は、iniファイルで指定したEndのキーを押すと爆発が止まります。

動作を止めると、スクリプトが一切制御しなくなるので、無敵が解除され（Trainer Modやチートで有効にしていない場合）、慣性に従って落下します。
必要に応じて着地を行ってください。

## 余談
ChatGPTにも協力してもらいました。さすがGPT。
そしてとりすーぷさん、ありがとうございます。

あ、あとスクリプト名の由来ですが、お尻のAss、爆発のExplosionを組み合わせてAssplosion、そこに高速移動のExpressを合わせて「Assplosion Express」と名付けました。

## 免責事項
本スクリプトModを使用したことにより生じた被害に関して、私QuestDragonは一切の責任を負いかねます。自己責任でご使用ください。

ファイルに一切変更を加えない状態での2次配布は禁止です。

予告なく配布を停止することがあります。予めご了承ください。

改造はご自由にしていただいて構いませんが、配布の際はクレジット表記していただけると助かります。

「一から自作した」というのではなく、「QuestDragonのスクリプト(Assplosion Express)の〇〇を△△にした」のように表記していただければと思います。

## 制作者
QuestDragon

## 参考
[TORISOUP氏](https://github.com/TORISOUP)

