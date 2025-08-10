using GTA;
using GTA.Math;
using GTA.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssplosionExpress
{
    internal class Class1 : Script
    {
        private const string ScriptName = "AssplosionExpress";

        private readonly Keys StartKey;
        private readonly Keys EndKey;

        private readonly string WaypointText = "どこに行きたい！？";
        private readonly string WaypointTip = "行き先マーカーをセットしよう。";
        private readonly string NearTip = "近すぎるぞ！もっと遠くがいい！";
        private readonly string StartText = "安心しろ、お前のケツがどこへでも連れて行ってくれる…。";
        private readonly string EmergencyUpText = "あっ、危ない！！";
        private readonly string ArrivingSoonText = "もうすぐ着くぞ！";
        private readonly string ArrivingSoonTip = "ここから先は自分で操作してくれ。";
        private readonly string TerminateText = "やっぱりやめた";
        private readonly string EndText = "ヘイお待ち！";

        private Entity target;
        private bool BoomReady = false;
        private bool BoomLock = false;
        private bool FirstBoomOperated = false;
        private bool WaypointSoon = false;
        private bool Finalizing = false;
        private DateTime EmgTextOKTime;
        private DateTime nextExplosionTime;

        public Class1()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            Aborted += OnAbort;

            BoomReady = false;
            BoomLock = false;

            ScriptSettings ini = ScriptSettings.Load($@"scripts\{ScriptName}.ini"); //INI File
            StartKey = ini.GetValue("Keys", "Start", Keys.None);
            EndKey = ini.GetValue("Keys", "End", Keys.None);
            if (StartKey == Keys.None)
            {
                Notification.Show(NotificationIcon.Blocked, ScriptName, "Warning", "The Start key is set to None.\nCheck your ini if this is not what you intended.");
            }

            WaypointText = ini.GetValue("Display", "WaypointText", "");
            WaypointTip = ini.GetValue("Display", "WaypointTip", "");
            NearTip = ini.GetValue("Display", "NearestTip", "");
            StartText = ini.GetValue("Display", "StartText", "");
            EmergencyUpText = ini.GetValue("Display", "EmergencyUpText", "");
            ArrivingSoonText = ini.GetValue("Display", "ArrivingSoonText", "");
            ArrivingSoonTip = ini.GetValue("Display", "ArrivingSoonTip", "");
            TerminateText = ini.GetValue("Display", "NotArriveText", "");
            EndText = ini.GetValue("Display", "ArrivedText", "");

#if DEBUG
            Notification.Show($"{ScriptName} is READY.");
#endif
        }

        private void OnAbort(object sender, EventArgs e)
        {
            // NotImplemented
        }

        /// <summary>
        /// 状態を初期状態へ戻します。
        /// </summary>
        private void ResetState()
        {
            BoomReady = false;
            BoomLock = false;
            FirstBoomOperated = false;
            WaypointSoon = false;
            Finalizing = false;

            return;
        }

        // 開始
        private void StartBoomFly()
        {
            if (BoomLock) return;

            // 最大10秒間、ウェイポイントが設定されるのを待つ
            int waitCounter = 0;
            bool BoomGo = false;
            BoomLock = true;
            while (BoomLock && waitCounter < 10)
            {
                waitCounter++;

                var blip = GTA.World.WaypointBlip;
                if (blip != null && blip.Exists())
                {
                    BoomGo = true;
                    break;
                }

                GTA.UI.Screen.ShowSubtitle(WaypointText);
                if (waitCounter == 1) GTA.UI.Screen.ShowHelpText(WaypointTip, 10000);
                Wait(1000); // 1秒待機
            }

            // マーカーが用意されなかった場合
            if (!BoomGo)
            {
                ResetState();
                return;
            }

            GTA.UI.Screen.ClearHelpText();

            Vehicle PlayerVehicle = Game.Player.Character.CurrentVehicle;
            if (PlayerVehicle != null) //車両に乗っている場合
            {
                target = PlayerVehicle;
            }
            else
            {
                target = Game.Player.Character;
            }

            // Waypoint地点付近にいる場合
            float CalculatedDist = target.Position.Length() - GTA.World.WaypointBlip.Position.Length();
            if (Math.Abs(CalculatedDist) < 25f)
            {
                GTA.UI.Screen.ShowHelpText(NearTip);
                GTA.UI.Screen.ShowSubtitle(TerminateText);
                ResetState();
                return; //処理終了
            }

            target.IsInvincible = true;
            BoomReady = true;
            FirstBoomOperated = false;
            nextExplosionTime = DateTime.Now;
            EmgTextOKTime = DateTime.Now.AddMilliseconds(2500);
            GTA.UI.Screen.ShowSubtitle(StartText);
        }

        // 毎フレーム
        private async void OnTick(object sender, EventArgs e)
        {
            // GTA.UI.Screen.ShowSubtitle("IsFly: " + Game.Player.Character.IsInParachuteFreeFall);
            if (!BoomReady || Finalizing) return;

            // 動作対象は車両だが、プレイヤーがその車両に乗っていない場合
            if (target.Model.IsVehicle && Game.Player.Character.CurrentVehicle == null)
            {
                if (target.Exists()) //まだ車両が残っている場合
                {
                    target.IsInvincible = false; //無敵解除
                }

                // 対象をプレイヤーに変更
                target = Game.Player.Character;
                target.IsInvincible = true;
                Game.Player.Character.Task.ClearAllImmediately(); //タスク終了
                Game.Player.Character.Task.Skydive(); //舞空術モーション

                await Task.Delay(2); //待機
            }

            if (target == null || !target.Exists())
            {
                StopBoomFly();
                GTA.UI.Screen.ShowSubtitle(TerminateText);
                return;
            }

            // 行き先マーカーが消えたら
            var blip = World.WaypointBlip;
            if (blip == null || !blip.Exists())
            {
                StopBoomFly();
                GTA.UI.Screen.ShowSubtitle(TerminateText);
                return;
            }

            if (!FirstBoomOperated) //初回
            {
                // 推進力付与
                target.ApplyForce(Vector3.WorldUp * 300.0f);
                if (target is Ped ped) // 車両に乗ってなければスカイフォールチートのモーション
                {
                    ped.Task.ClearAllImmediately();
                    // 少し待つ（物理挙動の初期化を待つ）
                    Wait(100);
                    // スカイダイブモーション開始
                    ped.Task.Skydive();
                    Wait(500); //FreeFall判定のためちょっとまち
                }
                FirstBoomOperated = true;
            }

            Vector3 goal = blip.Position;
            Vector3 current = target.Position;
            // 目的地方向
            Vector3 dir = goal - current;
            Vector3 dirN = (goal - current).Normalized;

            // マーカーは消えていない状態で、対象はプレイヤー（徒歩）の場合
            if (target is Ped p2)
            {
                // XY平面上での角度（Heading）は Atan2 で求める
                float heading = (float)(Math.Atan2(dirN.Y, dirN.X) * (180.0 / Math.PI));
                // GTAのHeadingは北を0度、時計回りなので変換
                heading = (heading + 270f) % 360f;
                // プレイヤーの向きを変更
                target.Heading = heading;

                // 舞空術モーションが終わってしまい、かつ目的地付近でない場合
                if (!p2.IsInParachuteFreeFall && !WaypointSoon)  // ※Pedならp2変数を作ってそこにキャストして入れる 
                {
                    Notification.Show("Break!");
                    StopBoomFly(p2);
                    GTA.UI.Screen.ShowSubtitle(TerminateText);
                    return;
                }
            }

            #region Vector3's
            var WaypointDistance = new Vector3(dir.X, dir.Y, 0).Length(); //目的地までの距離（高さは見ていない。Normalizedは使用不可）

            // 現在の高度を求める（地面からの高さ） 引数：対象位置、取得結果の代入先（今回はgghを作成してそこに入れる）、取得方法
            World.GetGroundHeight(target.Position, out float ggh, GetGroundHeightMode.Normal);
            float Altitude = target.Position.Z - ggh; //高度計算結果

            // 前方の障害物を検知
            float rayDistance = 50.0f; //検知距離
            Vector3 rayEndPos = current + dirN * rayDistance; // レイの終点
            RaycastResult raycastResult = World.Raycast(current, rayEndPos, IntersectFlags.Map); // レイキャストで前方に障害物があるか調べる

            //後方爆発Randomエフェクト
            Random r = new Random();
            Vector3 forward = target.ForwardVector; // 対象の向き（正面ベクトル）を取得
            Vector3 right = target.RightVector; // 対象の向き（右側ベクトル）を取得
            Vector3 up = Vector3.WorldUp; // 上方向

            float backOffset = 1.5f; // トランクの後ろ位置（基準）
            Vector3 basePos = current - forward * backOffset;

            // Y軸（上下）とZ軸（右-左方向）にランダムオフセットを作成
            float yOffset = (r.Next(-100, 100) / 100);   // -1 〜 +1mの範囲で上下
            float zOffset = (r.Next(-100, 100) / 100);    // -1 〜 +1mの範囲で左右
            Vector3 explosionPos = basePos + (up * yOffset) + (right * zOffset);
            Vector3 RotateV3;

            if (target is Ped)
            {
                RotateV3 = default;
            }
            else //車両ならくるくる回す
            {
                RotateV3 = new Vector3(
                    (float)(r.NextDouble() * 20 - 10),  // X軸回転トルク
                    (float)(r.NextDouble() * 20 - 10),  // Y軸回転トルク
                    (float)(r.NextDouble() * 20 - 10)   // Z軸回転トルク
                );
            }

            #endregion

            if (DateTime.Now >= nextExplosionTime)
            {
                if (WaypointDistance < 100 && target is Vehicle) //現在地が目的地から指定距離未満で車なら
                {
                    Finalizing = true;
                    if (target.Exists()) //対象が存在
                    {
                        target.ApplyForce(dirN * 25.0f + new Vector3(0, 0, -Altitude)); //進行方向へ25+高度分たたきつけ
                        GTA.World.AddExplosion(explosionPos, GTA.ExplosionType.Grenade, 3.5f, 0.2f);
                        Wait(250); 
                        StopBoomFly();
                        GTA.UI.Screen.ShowSubtitle(EndText);
                        return; //処理終了
                    }
                }
                else if (WaypointDistance < 200) //現在地が目的地から指定距離未満なら着地準備
                {
                    Vector3 WaypointPos = blip.Position;
                    if (target.Exists()) //対象が存在
                    {
                        if (target is Ped ped1) //プレイヤーの場合
                        {
                            Finalizing = true;

                            GTA.UI.Screen.ShowSubtitle(ArrivingSoonText);
                            GTA.UI.Screen.ShowHelpText(ArrivingSoonTip);
                            WaypointSoon = true;
                            // 目的地に向けてパラシュート降下モーション
                            ped1.Task.ParachuteTo(WaypointPos);

                            // 着地待ちループ（例：ポーリング）
                            while (ped1.IsInParachuteFreeFall || ped1.IsInAir)
                            {
                                Wait(500); // 0.5秒ごとにチェック
                            }

                            // Waypoint地点付近に着地していなければ失敗。
                            if ((target.Position.Length() - WaypointPos.Length()) > 25f)
                            {
                                StopBoomFly();
                                GTA.UI.Screen.ShowSubtitle(TerminateText);
                                return; //処理終了
                            }

                            StopBoomFly();
                            GTA.UI.Screen.ShowSubtitle(EndText);
                            return; //処理終了
                        }

                        // 車の場合は無視
                    }
                }

                // 障害物検知用
                float hitDistance = Vector3.Distance(current, raycastResult.HitPosition);

                if (WaypointDistance < 300) //距離未満なら速度をさっきより弱めて現在高度をもとに降下開始
                {
                    WaypointSoon = true;
                    if (Altitude > 20f && target is Ped)
                    {
                        // 降下していたら前方に障害物がある
                        if (hitDistance < rayDistance)
                        {
                            GTA.UI.Screen.ShowSubtitle(EmergencyUpText);
                            target.ApplyForce(Vector3.WorldUp * 50.0f); // 真上へ50の力で移動（高速上昇）
                        }
                        else
                        {
                            target.ApplyForce(dirN * 50.0f + new Vector3(0, 0, -(Altitude * 0.5f))); //進行方向へ50の強さ+ -Z方向(地面）へ高度*0.5xの強さをかける
                        }

                    }
                    else
                    {
                        target.ApplyForce(dirN * 30.0f); //進行方向へ30の強さ
                    }

                }
                else //まだまだ飛行
                {
                    // 前方に障害物がある
                    if (hitDistance < rayDistance)
                    {
                        // 「いってらっしゃい」が出ている最中に「あっ、危ない！」を出さない
                        if (DateTime.Now >= EmgTextOKTime) GTA.UI.Screen.ShowSubtitle(EmergencyUpText);
                        target.ApplyForce(Vector3.WorldUp * 500.0f, RotateV3); // 真上へ500の力で移動（高速上昇）
                    }
                    else if (Altitude < 100f) target.ApplyForce((dirN + Vector3.WorldUp * 0.5f) * 250.0f, RotateV3); // 進行方向+上= 斜め上(25度ぐらい)へ250の力で移動（上昇）
                    else target.ApplyForce((dirN + Vector3.WorldUp * 0.15f) * 250.0f, RotateV3); // ※参考：WorldUp * 1.0f で大体45度。

                    GTA.World.AddExplosion(
                        explosionPos, //位置
                        GTA.ExplosionType.Grenade, // 種類
                        3.5f, //強さ
                        0.1f); //カメラの揺れ
                }

                nextExplosionTime = DateTime.Now.AddMilliseconds(500);
            }

            /*
            // ゴール判定
            if (distance < 10f)
            {
                StopBoomFly();
                return;
            }

            if (isFirstBoom)
            {
                // 推進力付与
                target.ApplyForce(Vector3.WorldUp * 300.0f);
                isFirstBoom = true;
            }
            else
            {
                // 推進力付与
                target.ApplyForce((dir + Vector3.WorldUp * 0.5f) * 250.0f);
            }


            // 一定間隔で爆発
            if (DateTime.Now >= nextExplosionTime)
            {
                if (target.Exists())
                {
                    World.AddExplosion(
                        target.Position,
                        ExplosionType.Grenade,
                        3.5f,
                        0.1f
                    );
                }
                nextExplosionTime = DateTime.Now.AddMilliseconds(500);
            }
            */
        }

        /// <summary>
        /// スクリプト動作停止
        /// </summary>
        /// <param name="e">無敵解除対象</param>
        private void StopBoomFly(Entity e = null)
        {
            if (e == null)
            {
                e = target;
            }
            if (e != null && e.Exists())
            {
                e.IsInvincible = false;
            }
            ResetState();
        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == StartKey) StartBoomFly();
            if (e.KeyCode == EndKey)
            {
                if (BoomReady)
                {
                    BoomReady = false; //Tickの処理停止
                    BoomLock = false; //StartBoomFlyの処理停止
                    StopBoomFly();
                    GTA.UI.Screen.ShowSubtitle(TerminateText);
                }

            }
        }
    }
}
