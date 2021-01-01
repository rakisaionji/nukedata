using NukeDataTool;
using NukeDataTool.Taskbar;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable CS0618 // Type or member is obsolete

namespace NukePackTool
{
    public partial class FrmMain : Form
    {
        private const int btidSize = 0x2800;
        private const int offset = 0x200000;
        private const int segSize = 0x40000;
        private TaskbarManager taskbarManager;
        private readonly byte[] key = new byte[16];
        private readonly byte[] iv = new byte[16];
        private string src, dst, met;
        private byte[] noiseTable;
        private Thread thread;

        public FrmMain()
        {
            InitializeComponent();
            if (TaskbarManager.IsPlatformSupported) taskbarManager = TaskbarManager.Instance;
            else taskbarManager = null;
        }

        internal DialogResult MsgBox(string text, int icon = 0)
        {
            var owner = this;
            if (this.InvokeRequired) owner = null;
            if (icon == 32)
            {
                return MessageBox.Show(owner, text, this.Text, MessageBoxButtons.YesNo, (MessageBoxIcon)icon);
            }
            else
            {
                return MessageBox.Show(owner, text, this.Text, MessageBoxButtons.OK, (MessageBoxIcon)icon);
            }
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            var of = txtDst.Text;
            var od = new OpenFileDialog();
            od.Title = "Input File";
            od.Filter = "Disk Image File|*.img|All Files|*.*";
            od.Multiselect = false;
            od.CheckFileExists = true;
            if (!String.IsNullOrEmpty(txtSrc.Text))
            {
                var fi = new FileInfo(txtSrc.Text);
                od.FileName = fi.Name;
                od.InitialDirectory = fi.DirectoryName;
            }
            if (od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var file = od.FileName;
                    txtSrc.Text = file;
                    if (String.IsNullOrEmpty(of) || !of.Equals(file))
                    {
                        var dsf = Path.GetFileNameWithoutExtension(file);
                        var dsd = Path.GetDirectoryName(file);
                        txtDst.Text = String.Format("{0}\\{1}.app", dsd, dsf);
                    }
                }
                catch (Exception ex)
                {
                    MsgBox($"Error: {ex.Message}", 16);
                }
            }
        }

        private void btnMetatada_Click(object sender, EventArgs e)
        {
            var od = new OpenFileDialog();
            od.Title = "Input File";
            od.Filter = "Supported Files|*.pack;*.app;*.opt;*.dat|All Files|*.*";
            od.Multiselect = false;
            od.CheckFileExists = true;
            if (!String.IsNullOrEmpty(txtMet.Text))
            {
                var fi = new FileInfo(txtMet.Text);
                od.FileName = fi.Name;
                od.InitialDirectory = fi.DirectoryName;
            }
            if (od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var file = od.FileName;
                    txtMet.Text = file;
                }
                catch (Exception ex)
                {
                    MsgBox($"Error: {ex.Message}", 16);
                }
            }
        }

        private void btnDest_Click(object sender, EventArgs e)
        {
            var mf = txtMet.Text;
            if (String.IsNullOrEmpty(mf)) return;

            var sd = new SaveFileDialog();
            sd.Title = "Output File";
            sd.Filter = "SEGA System Packages (*.pack)|*.pack|SEGA Application Packages (*.app)|*.app|SEGA Option Packages (*.opt)|*.opt|All Files|*.*";
            if (!String.IsNullOrEmpty(txtDst.Text))
            {
                var fi = new FileInfo(txtDst.Text);
                sd.FileName = fi.Name;
                sd.InitialDirectory = fi.DirectoryName;
            }

            int index;
            var fm = new FileInfo(mf);
            switch (fm.Extension.ToLower())
            {
                case ".pack":
                    index = 1;
                    break;
                case ".app":
                    index = 2;
                    break;
                case ".opt":
                    index = 3;
                    break;
                default:
                    index = 4;
                    break;
            }
            sd.FilterIndex = index;

            if (sd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    txtDst.Text = sd.FileName;
                }
                catch (Exception ex)
                {
                    MsgBox($"Error: {ex.Message}", 16);
                }
            }
        }

        private void btnKey_Click(object sender, EventArgs e)
        {
            var od = new OpenFileDialog();
            od.Title = "Key File";
            od.Filter = "Dumped Key File|*.bin|All Files|*.*";
            od.Multiselect = false;
            od.CheckFileExists = true;
            if (!String.IsNullOrEmpty(txtKey.Text))
            {
                var fi = new FileInfo(txtKey.Text);
                od.FileName = fi.Name;
                od.InitialDirectory = fi.DirectoryName;
            }
            if (od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var file = od.FileName;
                    var finf = new FileInfo(file);
                    if (finf.Length != 32)
                    {
                        throw new InvalidDataException("Keyfile must be 32 bytes.");
                    }
                    txtKey.Text = file;
                }
                catch (Exception ex)
                {
                    MsgBox($"Error: {ex.Message}", 16);
                }
            }
        }

        private void SetProgressText(string text)
        {
            lblStatus.InvokeIfRequired(l =>
            {
                l.Text = text;
            });
        }

        private void SetProgressValue(string message, long val, long max)
        {
            var txt = String.Format("{0}: {1} / {2} ({3:P2})", message,
                    FormatFactory.FormatSize(val), FormatFactory.FormatSize(max), (double)val / max);

            lblStatus.InvokeIfRequired(l =>
            {
                l.Text = txt;
            });
            barProgress.InvokeIfRequired(l =>
            {
                l.Value = Convert.ToInt32(((double)val / max) * int.MaxValue);
            });
            if (taskbarManager != null)
            {
                taskbarManager.SetProgressValue(barProgress.Value, barProgress.Maximum);
            }
        }

        private void FscryptEncrypt_OnStop()
        {
            btnEncrypt.Text = "&Encrypt";
            barProgress.Value = 0;
            if (taskbarManager != null)
            {
                taskbarManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            tblParams.Enabled = true;
            lblStatus.Text = "Ready";
            this.Cursor = Cursors.Default;
        }

        private void FscryptEncrypt_Stop()
        {
            MethodInvoker method = FscryptEncrypt_OnStop;
            if (this.InvokeRequired) this.Invoke(method);
            else method();
        }

        internal void FscryptEncrypt_Cancel()
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Suspended) thread.Resume();
                thread.Abort();
            }
            FscryptEncrypt_Stop();
        }

        static uint FastCrc32(uint crc, byte[] buf, int len)
        {
            crc = ~crc;
            var i = 0;
            while (len-- > 0)
            {
                crc ^= buf[i++];
                for (int k = 0; k < 8; k++)
                    crc = ((crc & 1) > 0) ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
            }
            return ~crc;
        }

        bool CheckBootIdForImage(FileStream fsBootIdFile, string imageFileName)
        {
            // ========================================
            //   Read Boot Sector
            // ========================================

            var arrBuffer = new byte[btidSize];
            fsBootIdFile.Seek(0, SeekOrigin.Begin);
            var bytesRead = fsBootIdFile.Read(arrBuffer, 0, btidSize);

            if (bytesRead != btidSize)
            {
                MsgBox("Error: ReadFile() read boot sector error.");
                return false;
            }

            // ========================================
            //   Verify Boot Sector
            // ========================================

            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var ict = aes.CreateDecryptor(Secret.MasterKey, Secret.MasterIV))
            using (var cms = new CryptoStream(new MemoryStream(arrBuffer), ict, CryptoStreamMode.Read))
            {
                cms.Read(arrBuffer, 0, btidSize);

                var arrBootCk = arrBuffer.Skip(4).ToArray();
                var crcBootId = FastCrc32(0, arrBootCk, arrBootCk.Length);

                using (var rd = new BinaryReader(new MemoryStream(arrBuffer)))
                {
                    // Check CRC32
                    var crc1 = rd.ReadUInt32();
                    if (crc1 != crcBootId)
                    {
                        MsgBox("Error: Boot sector checksum error.");
                        return false;
                    }

                    // Check Header
                    var pi = rd.ReadUInt32();
                    if (pi != 0x2800U)
                    {
                        MsgBox("Error: Boot sector header size error.");
                        return false;
                    }

                    // Check Magic
                    pi = rd.ReadUInt32();
                    if (pi != 0x44495442U)
                    {
                        MsgBox("Error: Boot sector identification error.");
                        return false;
                    }

                    // Skip some data
                    rd.ReadBytes(0x14);

                    // Get segment info
                    var segCount = rd.ReadUInt64();   // Total Count
                    var segSize = rd.ReadUInt64();    // Segment Size
                    var segHeader = rd.ReadUInt64();  // Header Count

                    // Show segment info
                    var imageSize = Convert.ToInt64(segCount * segSize);
                    var dataOffset = Convert.ToInt64(segHeader * segSize);

                    // Check segment info
                    if (segHeader != 8)
                    {
                        MsgBox("Error: Property 'header_count' is invalid.");
                        return false;
                    }
                    if (segCount < 9)
                    {
                        MsgBox("Error: Property 'segment_count' is invalid.");
                        return false;
                    }
                    if (segSize != 0x40000)
                    {
                        MsgBox("Error: Property 'segment_size' is invalid.");
                        return false;
                    }

                    // Check image file info
                    var fileSize = new FileInfo(imageFileName).Length + dataOffset;
                    if (fileSize != imageSize)
                    {
                        MsgBox("Error: Property 'image_size' is invalid.");
                        return false;
                    }

                    // Boot Sector is valid
                    return true;
                }
            }
        }

        private void FscryptEncrypt()
        {
            uint value;

            var file = new FileInfo(src);
            var allLength = file.Length;

            if (!file.Exists)
            {
                MsgBox("Input file not exists.", 48);
                FscryptEncrypt_Stop(); return;
            }

            // Read boot id from source file
            var header = new byte[btidSize];
            using (var mfs = new FileStream(met, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var headLength = mfs.Read(header, 0, btidSize);
                if (headLength < btidSize)
                {
                    MsgBox("Invalid source file length.", 48);
                    FscryptEncrypt_Stop(); return;
                }
                // Verify boot sector
                if (!CheckBootIdForImage(mfs, src))
                {
                    FscryptEncrypt_Stop(); return;
                }
            }

            // Initialize checksum table
            var segCount = (allLength / segSize) + 8;
            var crcTable = new uint[segCount];

            // Delete existing destination file
            if (File.Exists(dst)) File.Delete(dst);

            int blockSize = 0x1000;
            var msg = "Encrypting data";
            var buffer = new byte[blockSize];
            using (var ifs = new FileStream(src, FileMode.Open, FileAccess.Read))
            using (var ofs = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var ict = aes.CreateEncryptor(key, iv))
            {
                // Write boot id
                ofs.Write(header, 0, btidSize);

                // Write the rest of the data
                long leftover = noiseTable.Length - btidSize;
                ofs.Write(noiseTable, btidSize, (int)leftover);

                // Complete writing header segments
                blockSize = noiseTable.Length;
                while (ofs.Position < offset)
                {
                    leftover = offset - ofs.Position;

                    if (noiseTable.Length > leftover)
                        blockSize = Convert.ToInt32(leftover);

                    ofs.Write(noiseTable, 0, blockSize);
                }

                // Check if file cursor drifted away
                if (!ofs.CanSeek || ofs.Position > offset)
                {
                    MsgBox("Image file writing cursor error.", 48);
                    FscryptEncrypt_Stop(); return;
                }

                // Encrypt and write image data
                blockSize = 0x1000;
                ofs.Seek(offset, SeekOrigin.Begin);
                while (ifs.Position < allLength)
                {
                    SetProgressValue(msg, ifs.Position, allLength);

                    var location = ifs.Position;

                    blockSize = ifs.Read(buffer, 0, blockSize);

                    var value1 = BitConverter.ToInt64(buffer, 0) ^ location;
                    var value2 = BitConverter.ToInt64(buffer, 8) ^ location;

                    Array.Copy(BitConverter.GetBytes(value1), 0, buffer, 0, 8);
                    Array.Copy(BitConverter.GetBytes(value2), 0, buffer, 8, 8);

                    using (var oms = new MemoryStream())
                    using (var cms = new CryptoStream(oms, ict, CryptoStreamMode.Write))
                    {
                        cms.Write(buffer, 0, blockSize);
                        var array = oms.ToArray();
                        ofs.Write(array, 0, blockSize);

                        var segNo = 8 + (location / segSize);
                        crcTable[segNo] = FastCrc32(crcTable[segNo], array, blockSize);
                    }
                }

                // Write checksum table
                ofs.Seek(0x2A00, SeekOrigin.Begin);
                for (int i = 0; i < segCount; i++)
                {
                    value = crcTable[i];
                    buffer[0] = (byte)value;
                    buffer[1] = (byte)(value >> 8);
                    buffer[2] = (byte)(value >> 16);
                    buffer[3] = (byte)(value >> 24);
                    ofs.Write(buffer, 0, 4);
                }

                // Reflect changes to file
                ofs.Flush();
            }

            // Write correct checksum table and signature
            using (var ofs = new FileStream(dst, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                // Initialize first sector crc value
                crcTable[0] = FastCrc32(0, header, btidSize);

                // Skip to segment #1 and calculate crc
                blockSize = 0x2000;
                ofs.Seek(segSize, SeekOrigin.Begin);
                for (int i = 1; i < 8; i++)
                {
                    for (int j = 0; j < 0x20; j++)
                    {
                        ofs.Read(header, 0, blockSize);
                        crcTable[i] = FastCrc32(crcTable[i], header, blockSize);
                    }
                }

                // Update checksum table in file
                ofs.Seek(0x2A00, SeekOrigin.Begin);
                for (int i = 0; i < 8; i++)
                {
                    value = crcTable[i];
                    buffer[0] = (byte)value;
                    buffer[1] = (byte)(value >> 8);
                    buffer[2] = (byte)(value >> 16);
                    buffer[3] = (byte)(value >> 24);
                    ofs.Write(buffer, 0, 4);
                }

                // Reflect changes to file
                ofs.Flush();

                // Skip to checksum table and read
                ofs.Seek(0x2A04, SeekOrigin.Begin);

                // Read the rest of the data
                while (ofs.Position < segSize)
                {
                    var leftover = segSize - ofs.Position;
                    if (blockSize > leftover) blockSize = Convert.ToInt32(leftover);
                    ofs.Read(header, 0, blockSize);
                    crcTable[0] = FastCrc32(crcTable[0], header, blockSize);
                }

                // Update checksum in file
                ofs.Seek(0x2A00, SeekOrigin.Begin);
                value = crcTable[0];
                buffer[0] = (byte)value;
                buffer[1] = (byte)(value >> 8);
                buffer[2] = (byte)(value >> 16);
                buffer[3] = (byte)(value >> 24);
                ofs.Write(buffer, 0, 4);

                // Reflect changes to file
                ofs.Flush();

                // Calculate and write signature
                ofs.Seek(0x2A00, SeekOrigin.Begin);
                using (var hmac = new HMACSHA1(Secret.HmacSecret))
                {
                    // Calculate signature
                    var bytesRead = blockSize = 0x1000;
                    var bytesLeft = 0x200000 - 0x2A00;
                    while (true)
                    {
                        if (bytesLeft < 0x1000) blockSize = bytesLeft;
                        bytesRead = ofs.Read(buffer, 0, blockSize);
                        if (bytesLeft - bytesRead == 0)
                        {
                            hmac.TransformFinalBlock(buffer, 0, bytesRead);
                            break;
                        }
                        else
                        {
                            hmac.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                            bytesLeft -= bytesRead;
                        }
                    }
                    // Write down to file
                    var calcSign = hmac.Hash;
                    ofs.Seek(0x2800, SeekOrigin.Begin);
                    ofs.Write(calcSign, 0, calcSign.Length);
                }

                // Reflect changes to file
                ofs.Flush();
            }

            // Finish
            lblStatus.InvokeIfRequired(l =>
            {
                l.Text = "Success";
            });
            FscryptEncrypt_Stop();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            noiseTable = Secret.GetNoiseTable(0x4000, 0x102);

        }

        private void CancelTask()
        {
            if (thread != null) thread.Suspend();
            if (taskbarManager != null)
            {
                taskbarManager.SetProgressState(TaskbarProgressBarState.Paused);
            }
            var dlg = MsgBox("Abort task?", 32);
            if (dlg == DialogResult.Yes) { FscryptEncrypt_Cancel(); }
            else
            {
                if (taskbarManager != null)
                {
                    taskbarManager.SetProgressState(TaskbarProgressBarState.Normal);
                }
                if (thread != null) thread.Resume();
            }
        }

        private void lblStatus_TextChanged(object sender, EventArgs e)
        {
            if (lblStatus.Text.Equals("Success"))
            {
                MsgBox("Data encrypt success.", 64);
                lblStatus.Text = "Ready";
            }
        }

        internal void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (btnEncrypt.Text.Contains("Cancel"))
            {
                CancelTask();
                return;
            }

            var kf = txtKey.Text.Trim();
            src = txtSrc.Text.Trim();
            met = txtMet.Text.Trim();
            dst = txtDst.Text.Trim();

            if (String.IsNullOrEmpty(kf))
            {
                MsgBox("Please specify key file.", 48); return;
            }
            if (String.IsNullOrEmpty(src))
            {
                MsgBox("Please specify input file.", 48); return;
            }
            if (String.IsNullOrEmpty(met))
            {
                MsgBox("Please specify source file.", 48); return;
            }
            if (String.IsNullOrEmpty(dst))
            {
                MsgBox("Please specify output file.", 48); return;
            }
            if (!File.Exists(kf))
            {
                MsgBox("Key file not exists.", 48); return;
            }
            if (!File.Exists(src))
            {
                MsgBox("Input file not exists.", 48); return;
            }
            if (!File.Exists(met))
            {
                MsgBox("Source file not exists.", 48); return;
            }
            if (dst.Equals(met, StringComparison.CurrentCultureIgnoreCase))
            {
                MsgBox("Source file and output file cannot be the same.", 48); return;
            }
            if (File.Exists(dst))
            {
                var msg = MsgBox("Output file exists. Overwrite?", 32);
                if (msg == DialogResult.No) return;
            }
            this.Cursor = Cursors.WaitCursor;
            tblParams.Enabled = false;
            SetProgressText("Reading key file...");
            barProgress.Value = 1;
            btnEncrypt.Text = "&Cancel";

            var kd = File.ReadAllBytes(kf);
            if (kd.Length != 32)
            {
                MsgBox("Keyfile must be 32 bytes.", 48);
                FscryptEncrypt_Stop(); return;
            }
            Array.Copy(kd, 0, key, 0, 16);
            Array.Copy(kd, 16, iv, 0, 16);

            var fi = new FileInfo(met);
            if (fi.Length < offset)
            {
                MsgBox(String.Format("Source file must be {0} or more.", FormatFactory.FormatSize(offset)), 48);
                FscryptEncrypt_Stop(); return;
            }

            SetProgressText("Start encrypt task...");
            thread = new Thread(FscryptEncrypt);
            thread.Start();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnEncrypt.Text.Contains("Cancel"))
            {
                e.Cancel = true;
                CancelTask();
            }
        }
    }
}
