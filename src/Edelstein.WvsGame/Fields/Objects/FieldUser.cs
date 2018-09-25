using System;
using System.Linq;
using System.Threading.Tasks;
using Edelstein.Common.Packets;
using Edelstein.Database.Entities;
using Edelstein.Network.Packets;
using Edelstein.WvsGame.Fields.Movements;
using Edelstein.WvsGame.Packets;
using Edelstein.WvsGame.Sockets;

namespace Edelstein.WvsGame.Fields.Objects
{
    public class FieldUser : FieldObject
    {
        public GameClientSocket Socket { get; set; }
        public Character Character { get; set; }

        public FieldUser(GameClientSocket socket, Character character)
        {
            Socket = socket;
            Character = character;
        }

        public bool OnPacket(GameRecvOperations operation, InPacket packet)
        {
            switch (operation)
            {
                case GameRecvOperations.UserTransferFieldRequest:
                    this.OnUserTransferFieldRequest(packet);
                    break;
                case GameRecvOperations.UserMove:
                    this.OnUserMove(packet);
                    break;
                case GameRecvOperations.UserChat:
                    this.OnUserChat(packet);
                    break;
                case GameRecvOperations.UserEmotion:
                    this.OnUserEmotion(packet);
                    break;
                case GameRecvOperations.UserCharacterInfoRequest:
                    this.OnUserCharacterInfoRequest(packet);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private void OnUserTransferFieldRequest(InPacket packet)
        {
            packet.Decode<byte>();
            packet.Decode<int>();

            var portalName = packet.Decode<string>();
            var portal = Field.Template.Portals.Values.Single(p => p.Name.Equals(portalName));
            var targetField = Socket.WvsGame.FieldFactory.Get(portal.ToMap);
            var targetPortal = targetField.Template.Portals.Values.Single(p => p.Name.Equals(portal.ToName));

            Character.FieldPortal = (byte) targetPortal.ID;
            targetField.Enter(this);
        }

        private void OnUserMove(InPacket packet)
        {
            packet.Decode<long>();
            packet.Decode<byte>();
            packet.Decode<long>();
            packet.Decode<int>();
            packet.Decode<int>();
            packet.Decode<int>();

            var movementPath = new MovementPath();

            movementPath.Decode(packet);

            using (var p = new OutPacket(GameSendOperations.UserMove))
            {
                p.Encode(ID);
                movementPath.Encode(p);

                X = movementPath.X;
                Y = movementPath.Y;
                MoveAction = movementPath.MoveActionLast;
                Foothold = movementPath.FHLast;
                Field.BroadcastPacket(this, p);
            }
        }

        private void OnUserChat(InPacket packet)
        {
            packet.Decode<int>();

            var message = packet.Decode<string>();
            var onlyBalloon = packet.Decode<bool>();

            using (var p = new OutPacket(GameSendOperations.UserChat))
            {
                p.Encode<int>(ID);
                p.Encode<bool>(false);
                p.Encode<string>(message);
                p.Encode<bool>(onlyBalloon);
                Field.BroadcastPacket(p);
            }
        }

        private void OnUserEmotion(InPacket packet)
        {
            var emotion = packet.Decode<int>();
            var duration = packet.Decode<int>();
            var byItemOption = packet.Decode<bool>();

            using (var p = new OutPacket(GameSendOperations.UserEmotion))
            {
                p.Encode<int>(ID);
                p.Encode<int>(emotion);
                p.Encode<int>(duration);
                p.Encode<bool>(byItemOption);
                Field.BroadcastPacket(this, p);
            }
        }

        public OutPacket GetSetFieldPacket()
        {
            using (var p = new OutPacket(GameSendOperations.SetField))
            {
                p.Encode<short>(0); // ClientOpt

                p.Encode<int>(0);
                p.Encode<int>(0);

                p.Encode<bool>(true); // sNotifierMessage._m_pStr
                p.Encode<bool>(!Socket.IsInstantiated);
                p.Encode<short>(0); // nNotifierCheck, loops

                if (!Socket.IsInstantiated)
                {
                    p.Encode<int>((int) Socket.Random.Seed1);
                    p.Encode<int>((int) Socket.Random.Seed2);
                    p.Encode<int>((int) Socket.Random.Seed3);

                    Character.EncodeData(p);

                    p.Encode<int>(0);
                    for (var i = 0; i < 3; i++) p.Encode<int>(0);
                }
                else
                {
                    p.Encode<byte>(0);
                    p.Encode<int>(Character.FieldID);
                    p.Encode<byte>(Character.FieldPortal);
                    p.Encode<int>(Character.HP);
                    p.Encode<bool>(false);
                }

                p.Encode<long>(0);
                return p;
            }
        }

        private void OnUserCharacterInfoRequest(InPacket packet)
        {
            packet.Decode<int>();
            var user = Field.GetUser(packet.Decode<int>());
            if (user == null) return;

            using (var p = new OutPacket(GameSendOperations.CharacterInfo))
            {
                var c = user.Character;

                p.Encode<int>(ID);
                p.Encode<byte>(c.Level);
                p.Encode<short>(c.Job);
                p.Encode<short>(c.POP);

                p.Encode<byte>(0);

                p.Encode<string>(""); // sCommunity
                p.Encode<string>(""); // sAlliance

                p.Encode<byte>(0);
                p.Encode<byte>(0);
                p.Encode<byte>(0); // TamingMobInfo
                p.Encode<byte>(0); // WishItemInfo

                p.Encode<int>(0); // MedalAchievementInfo
                p.Encode<short>(0);

                p.Encode<int>(0); // ChairItemInfo
                SendPacket(p);
            }
        }

        public override OutPacket GetEnterFieldPacket()
        {
            using (var p = new OutPacket(GameSendOperations.UserEnterField))
            {
                p.Encode<int>(ID);

                p.Encode<byte>(Character.Level);
                p.Encode<string>(Character.Name);

                // Guild
                p.Encode<string>("");
                p.Encode<short>(0);
                p.Encode<byte>(0);
                p.Encode<short>(0);
                p.Encode<byte>(0);

                p.Encode<long>(0);
                p.Encode<long>(0);
                p.Encode<byte>(0);
                p.Encode<byte>(0);

                p.Encode<short>(Character.Job);
                Character.EncodeLook(p);

                p.Encode<int>(0);
                p.Encode<int>(0);
                p.Encode<int>(0);
                p.Encode<int>(0);
                p.Encode<int>(0);
                p.Encode<int>(0);

                p.Encode<short>(X);
                p.Encode<short>(Y);
                p.Encode<byte>(MoveAction);
                p.Encode<short>(Foothold);
                p.Encode<byte>(0);

                p.Encode<byte>(0);

                p.Encode<int>(0);
                p.Encode<int>(0);
                p.Encode<int>(0);

                p.Encode<byte>(0);

                p.Encode<byte>(0);
                p.Encode<byte>(0);
                p.Encode<byte>(0);
                p.Encode<byte>(0);

                p.Encode<byte>(0);

                p.Encode<byte>(0);
                p.Encode<int>(0);
                return p;
            }
        }

        public override OutPacket GetLeaveFieldPacket()
        {
            using (var p = new OutPacket(GameSendOperations.UserLeaveField))
            {
                p.Encode<int>(ID);
                return p;
            }
        }

        public Task SendPacket(OutPacket packet) => this.Socket.SendPacket(packet);
    }
}