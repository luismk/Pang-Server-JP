using System.Runtime.InteropServices;
using System.Windows.Forms;     
namespace PangLib.IFF.JP.Models.General
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
    public class IFFStats
    {
        public ushort Power { get; set; }
        public ushort Control { get; set; }
        public ushort Impact { get; set; }
        public ushort Spin { get; set; }
        public ushort Curve { get; set; }
        public byte[] getSlot => new byte[] { (byte) Power, (byte)Control, (byte) Impact, (byte)Spin, (byte) Curve};

    public void Log()
        {
            if (Power >0)
            {
               MessageBox.Show("test1: "+ Power.ToString());
            }

            if (Control > 0)
            {
               MessageBox.Show("test2: " + Control);
            }

            if (Impact > 0)
            {
               MessageBox.Show("test3: " + Impact);
            }

            if (Spin > 0)
            {
               MessageBox.Show("test4: " + Spin);
            }

            if (Curve > 0)
            {
               MessageBox.Show("test5: " + Curve);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
    public class IFFSlotStats
    {
        public ushort PowerSlot { get; set; }
        public ushort ControlSlot { get; set; }
        public ushort ImpactSlot { get; set; }
        public ushort SpinSlot { get; set; }
        public ushort CurveSlot { get; set; }

        public byte[] getSlot => new byte[] { (byte)PowerSlot, (byte)ControlSlot, (byte)ImpactSlot, (byte)SpinSlot, (byte)CurveSlot };
    }
}
