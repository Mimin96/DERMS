namespace Common
{
    public interface IConfigItem
    {
        PointType RegistryType { get; }
        ushort NumberOfRegisters { get; }
        ushort StartAddress { get; }
        ushort DecimalSeparatorPlace { get; }
        int MinValue { get; }
        int MaxValue { get; }
        int DefaultValue { get; }
        string ProcessingType { get; }
        string Description { get; }
        int AcquisitionInterval { get; }
        double ScaleFactor { get; }
        double Deviation { get; }
        double EGU_Min { get; }
        double EGU_Max { get; }
        ushort AbnormalValue { get; }
        double HighAlarm { get; }
        double LowAlarm { get; }
        long Gid { get; }
        long GidGeneratora { get; }
    }
}
