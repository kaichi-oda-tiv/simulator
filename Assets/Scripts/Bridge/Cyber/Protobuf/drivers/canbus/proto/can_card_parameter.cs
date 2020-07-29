// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: can_card_parameter.proto

#pragma warning disable 0612, 1591, 3021
namespace apollo.drivers.canbus
{

    [global::ProtoBuf.ProtoContract()]
    public partial class CANCardParameter : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);
        }
        public CANCardParameter()
        {
            OnConstructor();
        }

        partial void OnConstructor();

        [global::ProtoBuf.ProtoMember(1)]
        [global::System.ComponentModel.DefaultValue(CANCardBrand.FakeCan)]
        public CANCardBrand brand
        {
            get { return __pbn__brand ?? CANCardBrand.FakeCan; }
            set { __pbn__brand = value; }
        }
        public bool ShouldSerializebrand()
        {
            return __pbn__brand != null;
        }
        public void Resetbrand()
        {
            __pbn__brand = null;
        }
        private CANCardBrand? __pbn__brand;

        [global::ProtoBuf.ProtoMember(2)]
        [global::System.ComponentModel.DefaultValue(CANCardType.PciCard)]
        public CANCardType type
        {
            get { return __pbn__type ?? CANCardType.PciCard; }
            set { __pbn__type = value; }
        }
        public bool ShouldSerializetype()
        {
            return __pbn__type != null;
        }
        public void Resettype()
        {
            __pbn__type = null;
        }
        private CANCardType? __pbn__type;

        [global::ProtoBuf.ProtoMember(3)]
        [global::System.ComponentModel.DefaultValue(CANChannelId.ChannelIdZero)]
        public CANChannelId channel_id
        {
            get { return __pbn__channel_id ?? CANChannelId.ChannelIdZero; }
            set { __pbn__channel_id = value; }
        }
        public bool ShouldSerializechannel_id()
        {
            return __pbn__channel_id != null;
        }
        public void Resetchannel_id()
        {
            __pbn__channel_id = null;
        }
        private CANChannelId? __pbn__channel_id;

        [global::ProtoBuf.ProtoContract()]
        public enum CANCardBrand
        {
            [global::ProtoBuf.ProtoEnum(Name = @"FAKE_CAN")]
            FakeCan = 0,
            [global::ProtoBuf.ProtoEnum(Name = @"ESD_CAN")]
            EsdCan = 1,
            [global::ProtoBuf.ProtoEnum(Name = @"SOCKET_CAN_RAW")]
            SocketCanRaw = 2,
            [global::ProtoBuf.ProtoEnum(Name = @"HERMES_CAN")]
            HermesCan = 3,
        }

        [global::ProtoBuf.ProtoContract()]
        public enum CANCardType
        {
            [global::ProtoBuf.ProtoEnum(Name = @"PCI_CARD")]
            PciCard = 0,
            [global::ProtoBuf.ProtoEnum(Name = @"USB_CARD")]
            UsbCard = 1,
        }

        [global::ProtoBuf.ProtoContract()]
        public enum CANChannelId
        {
            [global::ProtoBuf.ProtoEnum(Name = @"CHANNEL_ID_ZERO")]
            ChannelIdZero = 0,
            [global::ProtoBuf.ProtoEnum(Name = @"CHANNEL_ID_ONE")]
            ChannelIdOne = 1,
            [global::ProtoBuf.ProtoEnum(Name = @"CHANNEL_ID_TWO")]
            ChannelIdTwo = 2,
            [global::ProtoBuf.ProtoEnum(Name = @"CHANNEL_ID_THREE")]
            ChannelIdThree = 3,
        }

    }

}

#pragma warning restore 0612, 1591, 3021
