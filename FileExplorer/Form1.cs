using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FileExplorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> _kopyalanacaklarListesi;
        bool _kesmi;

        private void Form1_Load(object sender, EventArgs e)
        {
            //bilgisayardaki mantıksal sürücüleri cmbSuruculer' e ekledik
            cmbSuruculer.Items.AddRange(DriveInfo.GetDrives());
        }

        private void cmbSuruculer_SelectedIndexChanged(object sender, EventArgs e)
        {
            DriveInfo seciliSurucu = (cmbSuruculer.SelectedItem as DriveInfo);
            //DriveInfo seciliSurucu = (DriveInfo)cmbSuruculer.SelectedItem;
            if (seciliSurucu != null && seciliSurucu.IsReady)
            {
                KlasorleriDoldurBySurucu(seciliSurucu.Name);
            }
            else
            {
                MessageBox.Show(@"Sürücü hazır değil");
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode tn in e.Node.Nodes)
            {
                AltKlasorDoldur(tn);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                DosyalariDoldur(e.Node.Tag.ToString());
            }
            catch
            {
                // ignored
            }
        }

        private void btnFiltrele_Click(object sender, EventArgs e)
        {
            if (cmbFiltre.SelectedIndex < 0) return;
            string seciliUzanti = cmbFiltre.SelectedItem.ToString().Substring(1);
            //cmbFiltre' nin 0' nci indexindeki eleman *.*
            // *.* haricinde bir eleman seçilirse IF bloğundaki kodlar çalışacak
            if (cmbFiltre.SelectedIndex > 0)
            {
                foreach (ListViewItem itm in listView1.Items)
                {
                    if (Path.GetExtension(itm.Text.ToLower()) != seciliUzanti)
                        itm.Remove();
                }
            }
            else
            {//*.* seçilirse buraya düşecek
                DosyalariDoldur(treeView1.SelectedNode.Tag.ToString());
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            DosyaCalistir(listView1.FocusedItem.Tag.ToString());
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _kesmi = false;
            KopyalaKes();
        }

        private void kesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _kesmi = true;
            KopyalaKes();
        }

        private void yapistirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string yapistirilacakYer = treeView1.SelectedNode.Tag.ToString();
            foreach (string kopyalanacakPath in _kopyalanacaklarListesi)
            {
                string dosyaAdi = Path.GetFileName(kopyalanacakPath);
                try
                {
                    if (dosyaAdi != null)
                    {
                        string yapistirilacakPath = Path.Combine(yapistirilacakYer, dosyaAdi);
                        if (!File.Exists(yapistirilacakPath))
                        {
                            //if (kopyalanacakPath == yapistirilacakPath)
                            //{
                            //    File.Copy(kopyalanacakPath, Path.Combine(yapistirilacakYer, "(Copy)" + dosyaAdi));
                            //}
                            //else
                            if (_kesmi)
                            {
                                File.Move(kopyalanacakPath, yapistirilacakPath);
                            }
                            else
                            {
                                File.Copy(kopyalanacakPath, yapistirilacakPath);
                                //ilk parametredeki kaynaktakini, ikinci parametredeki yere kopyalar.
                            }
                        }
                        else
                        {
                            if (MessageBox.Show(@"Dosya Zaten Var, yine de kopyalansınmı?", @"VAR", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_kesmi)
                                    File.Move(kopyalanacakPath, yapistirilacakPath);
                                else
                                    File.Copy(kopyalanacakPath, Path.Combine(yapistirilacakYer, "(Copy)" + dosyaAdi));
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            //dosyalar yapışıtırıldıktan sona, listView' e dosyaları tekrar doldurduk
            DosyalariDoldur(treeView1.SelectedNode.Tag.ToString());

        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Silmek istediğinizden emin seniz EVET' e, emin değilseniz HAYIR' a basınız", @"Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            foreach (ListViewItem silinecekItem in listView1.SelectedItems)
            {
                File.Delete(silinecekItem.Tag.ToString());
            }

            DosyalariDoldur(treeView1.SelectedNode.Tag.ToString());
        }


        void KlasorleriDoldurBySurucu(string surucuAdi)
        {
            treeView1.Nodes.Clear();
            //Seçili sürücüyü treeView' a ilk eleman olarak ekledik
            TreeNode surucuNode = new TreeNode();
            surucuNode.Text = surucuAdi;
            treeView1.Nodes.Add(surucuNode);

            DirectoryInfo dizin = new DirectoryInfo(surucuAdi);
            DirectoryInfo[] klasorler = dizin.GetDirectories();

            foreach (DirectoryInfo diKlasor in klasorler)
            {//cmbSurucu' den seçilen sürücü içindeki klasörleri treeView' e doldurduk
                TreeNode klasorNode = new TreeNode();
                klasorNode.Text = diKlasor.Name;
                klasorNode.Tag = diKlasor.FullName;
                surucuNode.Nodes.Add(klasorNode);
                //altKlasorDoldur(klasorNode);
            }
            surucuNode.Expand();
        }

        void AltKlasorDoldur(TreeNode nd)
        {
            try
            {
                nd.Nodes.Clear();
                DirectoryInfo altDizin = new DirectoryInfo(nd.Tag.ToString());
                DirectoryInfo[] altKlasorler = altDizin.GetDirectories();
                foreach (var diAltKlasor in altKlasorler)
                {
                    TreeNode altKlasorNode = new TreeNode();
                    altKlasorNode.Text = diAltKlasor.Name;
                    altKlasorNode.Tag = diAltKlasor.FullName;
                    nd.Nodes.Add(altKlasorNode);
                }
            }
            catch
            {
                // ignored
            }
        }

        void DosyalariDoldur(string path)
        {
            DirectoryInfo dosyalarDizini = new DirectoryInfo(path);
            FileInfo[] dosyalar = dosyalarDizini.GetFiles();
            listView1.Items.Clear();
            cmbFiltre.Items.Clear();
            cmbFiltre.Items.Add("*.*");
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Maximum = dosyalar.Length;
            toolStripProgressBar1.Step = 1;
            double toplamDosyaBoyutu = 0;

            foreach (FileInfo fiDosya in dosyalar)
            {
                ListViewItem itm = new ListViewItem();
                itm.Text = fiDosya.Name;
                itm.Tag = fiDosya.FullName;
                long dosyaBoyut = fiDosya.Length;
                itm.SubItems.Add(dosyaBoyut.ToString());
                DateTime olusturulmaTarihi = fiDosya.CreationTime;
                itm.SubItems.Add(olusturulmaTarihi.ToLongDateString());
                listView1.Items.Add(itm);
                toplamDosyaBoyutu += dosyaBoyut;
                toolStripProgressBar1.PerformStep();
                string uzanti = "*" + fiDosya.Extension.ToLower();
                if (!cmbFiltre.Items.Contains(uzanti))
                    cmbFiltre.Items.Add(uzanti);
            }
            toolStripProgressBar1.Visible = false;

            string sonuc = "";
            if (toplamDosyaBoyutu < 1024)
            {
                sonuc = toplamDosyaBoyutu + " Byte";
            }
            else if (toplamDosyaBoyutu < 1048576)
            {
                sonuc = (toplamDosyaBoyutu / 1024).ToString("N2") + " KB";
            }
            else
            {
                sonuc = (toplamDosyaBoyutu / 1024 / 1024).ToString("N2") + " Mb";
            }
            toolStripStatusLabel1.Text = string.Format("{0} adet dosya, {1} boyut", dosyalar.Length, sonuc);
        }

        void DosyaCalistir(string dosyaPath)
        {
            try
            {
                Process.Start(dosyaPath);
            }
            catch
            {
                MessageBox.Show(@"Dosya çalıştırılamadı. Sistem yöneticinize başvurun.");
            }
        }

        void KopyalaKes()
        {
            _kopyalanacaklarListesi = new List<string>();
            foreach (ListViewItem kopyalanacakDosya in listView1.SelectedItems)
            {
                _kopyalanacaklarListesi.Add(kopyalanacakDosya.Tag.ToString());
                if (_kesmi)
                    kopyalanacakDosya.ForeColor = Color.Gray;
            }
        }

    }
}
