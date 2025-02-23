using Constant.Server;
using MainGame.Network.Receive;
using MainGame.Network.Send;
using MainGame.UnityView.Player;
using UnityEngine;
using VContainer.Unity;

namespace MainGame.Presenter.Player
{
    public class PlayerPositionSender : ITickable
    {
        private readonly IPlayerObjectController _playerObjectController;
        private readonly SendPlayerPositionProtocolProtocol _protocol;


        private bool _startPositionSend;

        private float _timer;

        public PlayerPositionSender(SendPlayerPositionProtocolProtocol protocol, IPlayerObjectController playerObjectController, ReceiveInitialHandshakeProtocol receiveInitialHandshakeProtocol)
        {
            _protocol = protocol;
            _playerObjectController = playerObjectController;

            //_startPositionSendがないとプレイヤーの座標が0,0,0の時にプレイヤー座標が送信されるため、
            //不要なチャンクデータの受信など不都合が発生する可能性がある（チャンクのデータはプレイヤーの周りの情報が帰ってくる）
            //そのため、ハンドシェイクが終わってからプレイヤー座標の送信を始める
            receiveInitialHandshakeProtocol.OnFinishHandshake += _ => _startPositionSend = true;
        }

        /// <summary>
        ///     Updateと同じタイミングで呼ばれる
        /// </summary>
        public void Tick()
        {
            if (!_startPositionSend) return;


            _timer += Time.deltaTime;
            if (_timer < NetworkConst.UpdateIntervalSeconds) return;
            _timer = 0;
            _protocol.Send(_playerObjectController.Position2d);
        }
    }
}