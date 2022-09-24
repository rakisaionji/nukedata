using NukeDataTool.Taskbar;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable CS0618 // Type or member is obsolete

namespace NukeDataTool
{
    public partial class FrmMain : Form
    {
        private TaskbarManager taskbarManager = null;
        private readonly byte[] key = new byte[16];
        private readonly byte[] iv = new byte[16];
        private string src, dst, dsm, dsk;
        private Thread thread;

        public FrmMain()
        {
            InitializeComponent();
            if (TaskbarManager.IsPlatformSupported) taskbarManager = TaskbarManager.Instance;
        }

        internal DialogResult MsgBox(string text, int icon = 0)
        {
            if (Program.IsNoGui)
            {
                if (icon == 32)
                {
                    if (Program.IsSilent || Program.IsAuto) return DialogResult.Yes;

                    ConsoleKey response;
                    do
                    {
                        Console.Write(text + " [yes/no] ");
                        response = Console.ReadKey().Key;
                        if (response != ConsoleKey.Enter) Console.WriteLine();
                    } while (response != ConsoleKey.Y && response != ConsoleKey.N);

                    if (response == ConsoleKey.Y) return DialogResult.Yes;
                    else return DialogResult.No;
                }
                else
                {
                    if (!Program.IsSilent) Console.WriteLine(text);
                    return DialogResult.OK;
                }
            }
            else
            {
                var owner = this;
                if (this.InvokeRequired) owner = null;
                if (icon == 32)
                {
                    if (Program.IsSilent) return DialogResult.Yes;
                    return MessageBox.Show(owner, text, this.Text, MessageBoxButtons.YesNo, (MessageBoxIcon)icon);
                }
                else
                {
                    if (Program.IsAuto || Program.IsSilent) return DialogResult.OK;
                    return MessageBox.Show(owner, text, this.Text, MessageBoxButtons.OK, (MessageBoxIcon)icon);
                }
            }
        }

        private void btnSource_Click(object sender, EventArgs e)
        {
            var of = txtDst.Text;
            var od = new OpenFileDialog();
            od.Title = "Input File";
            od.Filter = "Supported Files|*.pack;*.app;*.opt|All Files|*.*";
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
                        txtDst.Text = String.Format("{0}\\{1}.img", dsd, dsf);
                    }
                }
                catch (Exception ex)
                {
                    MsgBox($"Error: {ex.Message}", 16);
                }
            }
        }

        private void btnDest_Click(object sender, EventArgs e)
        {
            var sd = new SaveFileDialog();
            sd.Title = "Output File";
            sd.Filter = "Disk Image File|*.img|All Files|*.*";
            if (!String.IsNullOrEmpty(txtKey.Text))
            {
                var fi = new FileInfo(txtDst.Text);
                sd.FileName = fi.Name;
                sd.InitialDirectory = fi.DirectoryName;
            }
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
            if (Program.IsNoGui && !Program.IsSilent)
            {
                Console.WriteLine(text);
                return;
            }
            lblStatus.InvokeIfRequired(l =>
            {
                l.Text = text;
            });
        }

        private void SetProgressValue(long val, long max)
        {
            var txt = String.Format("Decrypting data: {0} / {1} ({2:P2})",
                    FormatFactory.FormatSize(val), FormatFactory.FormatSize(max), (double)val / max);
            if (Program.IsNoGui && !Program.IsSilent)
            {
                Console.WriteLine(txt.PadRight(Console.BufferWidth - 2));
                Console.CursorTop--; return;
            }
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

        private void FscryptDecrypt_OnStop()
        {
            btnDecrypt.Text = "&Decrypt";
            barProgress.Value = 0;
            if (taskbarManager != null)
            {
                taskbarManager.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            tblParams.Enabled = true;
            lblStatus.Text = "Ready";
            this.Cursor = Cursors.Default;
        }

        private void FscryptDecrypt_Stop()
        {
            MethodInvoker method = FscryptDecrypt_OnStop;
            if (this.InvokeRequired) this.Invoke(method);
            else method();
        }

        internal void FscryptDecrypt_Cancel()
        {
            if (thread != null)
            {
                if (thread.ThreadState == ThreadState.Suspended) thread.Resume();
                thread.Abort();
            }
            FscryptDecrypt_Stop();
            if (Program.IsAuto) this.Close();
        }

        private void FscryptDecrypt()
        {
            int offset = 0x200000;

            var file = new FileInfo(src);
            if (!file.Exists)
            {
                MsgBox("Input file not exists.", 48);
                FscryptDecrypt_Stop(); return;
            }

            var allLength = file.Length;
            if (allLength < offset)
            {
                MsgBox("Invalid file length.", 48);
                FscryptDecrypt_Stop(); return;
            }

            if (File.Exists(dst)) File.Delete(dst);

            int blockSize = 0x2800;
            var data = new byte[blockSize];
            using (var ifs = new FileStream(src, FileMode.Open, FileAccess.Read))
            using (var ofs = new FileStream(dst, FileMode.Create, FileAccess.Write))
            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var ict = aes.CreateDecryptor(key, iv))
            {
                // Extract boot sector

                ifs.Seek(0, SeekOrigin.Begin);
                var bytesRead = ifs.Read(data, 0, blockSize);

                if (bytesRead != blockSize)
                {
                    MsgBox("Error: ReadFile() read boot sector error.", 48);
                    FscryptDecrypt_Stop(); return;
                }

                using (var bfs = new FileStream(dsm, FileMode.Create, FileAccess.Write))
                using (var bct = aes.CreateDecryptor(Secret.MasterKey, Secret.MasterIV))
                using (var cms = new CryptoStream(new MemoryStream(data), bct, CryptoStreamMode.Read))
                {
                    cms.Read(data, 0, blockSize);
                    bfs.Write(data, 0, blockSize);
                }

                // Extract game image

                blockSize = 0x1000;
                ifs.Seek(offset, SeekOrigin.Begin);

                while (ifs.Position < allLength)
                {
                    SetProgressValue(ifs.Position, allLength);

                    var location = ifs.Position - offset;

                    blockSize = ifs.Read(data, 0, blockSize);

                    using (var cms = new CryptoStream(new MemoryStream(data), ict, CryptoStreamMode.Read))
                    {
                        cms.Read(data, 0, blockSize);

                        var value1 = BitConverter.ToInt64(data, 0) ^ location;
                        var value2 = BitConverter.ToInt64(data, 8) ^ location;

                        Array.Copy(BitConverter.GetBytes(value1), 0, data, 0, 8);
                        Array.Copy(BitConverter.GetBytes(value2), 0, data, 8, 8);

                        ofs.Write(data, 0, blockSize);
                    }
                }
            }

            if (Program.IsNoGui && !Program.IsSilent) Console.WriteLine();
            lblStatus.InvokeIfRequired(l =>
            {
                l.Text = "Success";
            });
            FscryptDecrypt_Stop();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (Program.IsSplash) MsgBox("NukeDataTool", 64);
            if (!Program.IsNoGui && Program.IsAuto) btnDecrypt.PerformClick();
        }

        private void CancelTask()
        {
            if (thread != null) thread.Suspend();
            if (taskbarManager != null)
            {
                taskbarManager.SetProgressState(TaskbarProgressBarState.Paused);
            }
            var dlg = MsgBox("Abort task?", 32);
            if (dlg == DialogResult.Yes) { FscryptDecrypt_Cancel(); }
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
                MsgBox("Data decrypt success.", 64);
                if (Program.IsAuto) this.Close();
                lblStatus.Text = "Ready";
            }
        }

        internal void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (Program.IsNoGui) taskbarManager = null;

            if (btnDecrypt.Text.Contains("Cancel"))
            {
                CancelTask();
                return;
            }

            var kf = txtKey.Text.Trim();
            src = txtSrc.Text.Trim();
            dst = txtDst.Text.Trim();

            if (String.IsNullOrEmpty(src))
            {
                MsgBox("Please specify input file.", 48); return;
            }
            if (String.IsNullOrEmpty(dst))
            {
                MsgBox("Please specify output file.", 48); return;
            }
            if (String.IsNullOrEmpty(kf))
            {
                if (!src.EndsWith(".opt", StringComparison.InvariantCultureIgnoreCase))
                {
                    MsgBox("Please specify key file.", 48); return;
                }
                var msg = MsgBox("Key file is not set. Continue?", 32);
                if (msg == DialogResult.No)
                {
                    return;
                }
            }
            else if (!File.Exists(kf))
            {
                MsgBox("Key file not exists.", 48); return;
            }
            if (!File.Exists(src))
            {
                MsgBox("Input file not exists.", 48); return;
            }

            var dsf = Path.GetFileNameWithoutExtension(dst);
            var dsd = Path.GetDirectoryName(dst);
            dsm = String.Format("{0}\\{1}.dat", dsd, dsf);

            if (File.Exists(dst) || File.Exists(dsm))
            {
                var msg = MsgBox("Output data file exists. Overwrite?", 32);
                if (msg == DialogResult.No)
                {
                    if (Program.IsAuto) this.Close();
                    return;
                }
            }

            if (String.IsNullOrEmpty(kf))
            {
                dsk = String.Format("{0}\\{1}.bin", dsd, dsf);
                if (File.Exists(dsk))
                {
                    var msg = MsgBox("Output key file exists. Overwrite?", 32);
                    if (msg == DialogResult.No)
                    {
                        if (Program.IsAuto) this.Close();
                        return;
                    }
                }
                SetProgressText("Generating keyfile...");
                MakeKeyfile();
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                tblParams.Enabled = false;
                SetProgressText("Reading key file...");
                barProgress.Value = 1;
                btnDecrypt.Text = "&Cancel";

                var kd = File.ReadAllBytes(kf);
                if (kd.Length != 32)
                {
                    MsgBox("Keyfile must be 32 bytes.", 48);
                    FscryptDecrypt_Stop(); return;
                }
                Array.Copy(kd, 0, key, 0, 16);
                Array.Copy(kd, 16, iv, 0, 16);
            }

            SetProgressText("Start decrypt task...");
            thread = new Thread(FscryptDecrypt);
            thread.Start();
        }

        private void MakeKeyfile()
        {
            int offset = 0x200000;

            var file = new FileInfo(src);
            if (!file.Exists)
            {
                MsgBox("Input file not exists.", 48);
                FscryptDecrypt_Stop(); return;
            }

            var allLength = file.Length;
            if (allLength < offset)
            {
                MsgBox("Invalid file length.", 48);
                FscryptDecrypt_Stop(); return;
            }

            if (File.Exists(dsk)) File.Delete(dsk);

            int blockSize = 0x1000;
            var data = new byte[blockSize];
            var ikey = Secret.OptionKey;

            using (var ifs = new FileStream(src, FileMode.Open, FileAccess.Read))
            using (var ofs = new FileStream(dsk, FileMode.Create, FileAccess.Write))
            using (var aes = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None })
            using (var ict = aes.CreateDecryptor(ikey, new byte[16]))
            {
                ifs.Seek(offset + blockSize, SeekOrigin.Begin);
                var location = ifs.Position - offset;
                var bytesRead = ifs.Read(data, 0, blockSize);

                if (bytesRead != blockSize)
                {
                    MsgBox("Error: ReadFile() read second block error.", 48);
                    FscryptDecrypt_Stop(); return;
                }

                using (var cms = new CryptoStream(new MemoryStream(data), ict, CryptoStreamMode.Read))
                {
                    cms.Read(data, 0, blockSize);

                    var value1 = BitConverter.ToInt64(data, 0) ^ location;
                    var value2 = BitConverter.ToInt64(data, 8) ^ location;

                    Array.Copy(BitConverter.GetBytes(value1), 0, data, 0, 8);
                    Array.Copy(BitConverter.GetBytes(value2), 0, data, 8, 8);

                    ofs.Write(ikey, 0, 16);
                    ofs.Write(data, 0, 16);
                }
            }

            Array.Copy(ikey, 0, key, 0, 16);
            Array.Copy(data, 0, iv, 0, 16);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnDecrypt.Text.Contains("Cancel"))
            {
                e.Cancel = true;
                CancelTask();
            }
        }
    }
}
