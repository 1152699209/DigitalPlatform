using DigitalPlatform.Entities;

namespace Entity
{
    public class DeviceEntity
    {
        public string DeviceNum { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string W { get; set; }
        public string H { get; set; }
        public string DeviceTypeName { get; set; }
        public string Header { get; set; }

        public string FlowDirection { get; set; } = "0";
        public string Rotate { get; set; } = "0";

        public List<DevicePropItemEntity> Props { get; set; }
        public List<VariableEntity> Vars { get; set; }
        public List<ManualEntity> ManualControls { get; set; }
    }
}
