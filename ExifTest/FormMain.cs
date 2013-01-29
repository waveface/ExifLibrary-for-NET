using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.ComponentModel;

namespace ExifLibrary
{
    public partial class FormMain : Form
    {
        private ExifFile data;

        public FormMain()
        {
            InitializeComponent();

            lblByteOrder.Text = "";
            lblThumbnail.Text = "";

            lvExif.ListViewItemSorter = new ListViewColumnSorter();

            string lastfile = Settings.Default.Lastfile;
            if (File.Exists(lastfile))
            {
                ReadFile(lastfile);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (fdOpen.ShowDialog() == DialogResult.OK)
            {
                ReadFile(fdOpen.FileName);
            }
        }

        private void ReadFile(string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                //data = ExifFile.Read(filename);
                data = ExifFile.Read(file);
            }

            Settings.Default.Lastfile = filename;
            Settings.Default.Save();
            lvExif.Items.Clear();
            foreach (ExifProperty item in data.Properties.Values)
            {
                ListViewItem lvitem = new ListViewItem(item.Name);
                lvitem.SubItems.Add(item.ToString());
                lvitem.SubItems.Add(Enum.GetName(typeof(IFD), ExifTagFactory.GetTagIFD(item.Tag)));
                lvitem.Tag = item;
                lvExif.Items.Add(lvitem);
            }
            if (data.Thumbnail == null)
                pbThumb.Image = null;
            else
                pbThumb.Image = new Bitmap(data.Thumbnail);
#if DEBUG
            binaryMapViewer1.Map = data.Map;
#endif

            this.Text = Path.GetFileName(filename) + " - Exif Test";
            lblStatus.Text = Path.GetFileName(filename);
            lblByteOrder.Text = "Byte Order: " + (data.ByteOrder == BitConverterEx.ByteOrder.LittleEndian ? "Little-Endian" : "Big-Endian");
            lblThumbnail.Text = "Thumbnail: " + (data.Thumbnail == null ? "None" : pbThumb.Image.Width.ToString() + "x" + pbThumb.Image.Height.ToString());

            lvExif.Sort();
        }

        private void lvExif_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvExif.SelectedItems.Count == 0)
                tbField.Text = "";
            else
            {
                ExifProperty item = (ExifProperty)lvExif.SelectedItems[0].Tag;

                StringBuilder s = new StringBuilder();
                s.AppendFormat("Tag: {0}{1}", item.Tag, Environment.NewLine);
                string val = item.ToString();
                if (val.Length > 50) val = val.Substring(0, 50) + " ...";
                s.AppendFormat("Value: {0}{1}", val, Environment.NewLine);
                s.AppendFormat("IFD: {0}{1}", item.IFD, Environment.NewLine);
                s.AppendFormat("Interop. TagID: {0} (0x{0:X2}){1}", item.Interoperability.TagID, Environment.NewLine);
                s.AppendFormat("Interop. Type: {0} (0x{0:X2}){1}", item.Interoperability.TypeID, Environment.NewLine);
                s.AppendFormat("Interop. Count: {0} (0x{0:X4}){1}", item.Interoperability.Count, Environment.NewLine);
                s.AppendFormat("Interop. Data Length: {0}{1}", item.Interoperability.Data.Length, Environment.NewLine);
                s.AppendFormat("Interop. Data: {0}", ByteArrayToString(item.Interoperability.Data));
                tbField.Text = s.ToString();
            }
        }

        private string ByteArrayToString(byte[] data)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte b in data)
                s.AppendFormat("0x{0:X2} ", b);
            return s.ToString();
        }

#if DEBUG
        private void binaryMapViewer1_BinSelect(object sender, Bin bin)
        {
            if (bin.Tag == null)
                lvExif.SelectedItems.Clear();

            foreach (ListViewItem item in lvExif.Items)
            {
                if (item.Tag == bin.Tag)
                {
                    item.Selected = true;
                    lvExif.EnsureVisible(lvExif.Items.IndexOf(item));
                    return;
                }
            }
        }
#endif

        private void lvExif_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewColumnSorter sorter = (ListViewColumnSorter)lvExif.ListViewItemSorter;

            if (e.Column == sorter.SortColumn)
                sorter.ReverseSortOrder();
            else
            {
                sorter.SortColumn = e.Column;
                sorter.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            }

            lvExif.Sort();
        }
    }
}

