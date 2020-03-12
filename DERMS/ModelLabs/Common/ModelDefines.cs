using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Common
{
    [DataContract]
    public enum DMSType : short
    {
        [System.Runtime.Serialization.EnumMemberAttribute()]
        MASK_TYPE = unchecked((short)0xFFFF),

        [System.Runtime.Serialization.EnumMemberAttribute()]
        GEOGRAPHICALREGION = 0x0001,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        SUBGEOGRAPHICALREGION = 0x000b,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ENEGRYSOURCE = 0x0009,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        SUBSTATION = 0x000a,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        BREAKER = 0x0002,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        CONNECTIVITYNODE = 0x0003,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ENERGYCONSUMER = 0x0004,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        GENERATOR = 0x0005,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TERMINAL = 0x0006,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ANALOG = 0x0007,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        DISCRETE = 0x0008,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ACLINESEGMENT = 0x000c,
        [System.Runtime.Serialization.EnumMemberAttribute()]
        POINT = 0x000d,
    }

    [Flags]
    public enum ModelCode : long
    {
        IDOBJ = 0x1000000000000000,
        IDOBJ_GID = 0x1000000000000104,
        IDOBJ_DESCRIPTION = 0x1000000000000207,
        IDOBJ_MRID = 0x1000000000000307,
        IDOBJ_NAME = 0x1000000000000407,

        GEOGRAPHICALREGION = 0x1100000000010000,
        GEOGRAPHICALREGION_SUBGEOREGS = 0x1100000000010119,
        GEOGRAPHICALREGION_LONGITUDE = 0x1100000000010205,
        GEOGRAPHICALREGION_LATITUDE = 0x1100000000010305,

        SUBGEOGRAPHICALREGION = 0x16000000000b0000,
        SUBGEOGRAPHICALREGION_SUBSTATIONS = 0x16000000000b0119,
        SUBGEOGRAPHICALREGION_GEOREG = 0x16000000000b0209,
        SUBGEOGRAPHICALREGION_LONGITUDE = 0x16000000000b0305,
        SUBGEOGRAPHICALREGION_LATITUDE = 0x16000000000b0405,

        CONNECTIVITYNODE = 0x1200000000030000,
        CONNECTIVITYNODE_TERMINALS = 0x1200000000030119,
        CONNECTIVITYNODE_CONTAINER = 0x1200000000030209,

        CONNECTIVITYNODECONTAINER = 0x1420000000000000,
        CONNECTIVITYNODECONTAINER_CON_NODES = 0x1420000000000119,

        EQUIPMENTCONTAINER = 0x1421000000000000,
        EQUIPMENTCONTAINER_EQUIPMENTS = 0x1421000000000119,
        EQUIPMENTCONTAINER_LONGITUDE = 0x1421000000000205,
        EQUIPMENTCONTAINER_LATITUDE = 0x1421000000000305,

        //FEEDEROBJECT = 0x1421100000090000,

        SUBSTATION = 0x14212000000a0000,
        SUBSTATION_SUBGEOREG = 0x14212000000a0109,

        MEASUREMENT = 0x1300000000000000,
        MEASUREMENT_MEAS_TYPE = 0x130000000000010a,
        MEASUREMENT_PSR = 0x1300000000000209,
        MEASUREMENT_LATITUDE = 0x1300000000000305,
        MEASUREMENT_LONGITUDE = 0x1300000000000405,

        ANALOG = 0x1310000000070000,
        ANALOG_MAX_VALUE = 0x1310000000070205,
        ANALOG_MIN_VALUE = 0x1310000000070305,
        ANALOG_NORMAL_VALUE = 0x1310000000070105,

        DISCRETE = 0x1320000000080000,
        DISCRETE_MAX_VALUE = 0x1320000000070203,
        DISCRETE_MIN_VALUE = 0x1320000000070303,
        DISCRETE_NORMAL_VALUE = 0x1320000000070103,

        PSR = 0x1400000000000000,
        PSR_MEASUREMENTS = 0x1400000000000119,

        EQUIPMENT = 0x1410000000000000,
        EQUIPMENT_CONTAINER = 0x1410000000000109,

        CONDEQ = 0x1411000000000000,
        CONDEQ_TERMINALS = 0x1411000000000119,
        CONDEQ_LONGITUDE = 0x1411000000000205,
        CONDEQ_LATITUDE = 0x1411000000000305,

        ENERGYCONSUMER = 0x1411100000040000,
        ENERGYCONSUMER_PFIXED = 0x1411100000040205,
        ENERGYCONSUMER_QFIXED = 0x1411100000040105,

        REGULATINGCONDEQ = 0x1411200000000000,

        GENERATOR = 0x1411210000050000,
        GENERATOR_MAXQ = 0x1411210000050205,
        GENERATOR_MINQ = 0x1411210000050305,
        GENERATOR_CONSIDERP = 0x1411210000050405,
        GENERATOR_GENERATORTYPE = 0x141121000005050a,
        GENERATOR_FLEXIBILITY = 0x1411210000050601,
        GENERATOR_MAXFLEX = 0x1411210000050705,
        GENERATOR_MINFLEX = 0x1411210000050805,

        SWITCH = 0x1411300000000000,
        SWITCH_NORMAL_OPEN = 0x1411300000000101,
        SWITCH_FEEDER_ID1 = 0x1411300000000207,
        SWITCH_FEEDER_ID2 = 0x1411300000000307,

        PROTECTEDSWITCH = 0x1411310000000000,

        BREAKER = 0x1411311000020000,

        ENERGYSOURCE = 0x1411400000090000,
        ENERGYSOURCE_ACTIVEPOWER = 0x1411400000090105,
        ENERGYSOURCE_VOLTAGE = 0x1411400000090205,
        ENERGYSOURCE_MAGNITUDE = 0x1411400000090305,
        ENERGYSOURCE_TYPE = 0x141140000009040a,

        CONDUCTOR = 0x1411500000000000,
        CONDUCTOR_TYPE = 0x141150000000010a,

        ACLINESEGMENT = 0x14115100000c0000,
        ACLINESEGMENT_FEEDERCABLE = 0x14115100000c0101,
        ACLINESEGMENT_CURRENTFLOW = 0x14115100000c0205,
        ACLINESEGMENT_POINTS = 0x14115100000c0319,

        POINT = 0x14115110000d0000,
        POINT_LONGITUDE = 0x14115110000d0105,
        POINT_LATITUDE = 0x14115110000d0205,
        POINT_LINE = 0x14115110000d0309,

        TERMINAL = 0x1500000000060000,
        TERMINAL_CONNECTIVITY_NODE = 0x1500000000060109,
        TERMINAL_COND_EQ = 0x1500000000060209,

    }

    [Flags]
    public enum ModelCodeMask : long
    {
        MASK_TYPE = 0x00000000ffff0000,
        MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
        MASK_ATTRIBUTE_TYPE = 0x00000000000000ff,

        MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
        MASK_FIRSTNBL = unchecked((long)0xf000000000000000),
        MASK_DELFROMNBL8 = unchecked((long)0xfffffff000000000),
    }
}


