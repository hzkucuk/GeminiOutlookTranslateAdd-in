namespace GeminiOutlookTranslateAdd_in
{
    partial class RibbonCeviri : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public RibbonCeviri()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        /// <param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Bileşen Tasarımcısı üretimi kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnTranslate = this.Factory.CreateRibbonButton();
            this.btnTranslateToTurkish = this.Factory.CreateRibbonButton();
            this.btnCancelTranslation = this.Factory.CreateRibbonButton();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.txtApiKey = this.Factory.CreateRibbonEditBox();
            this.btnSaveApiKey = this.Factory.CreateRibbonButton();
            this.lblApiKeyStatus = this.Factory.CreateRibbonLabel();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Groups.Add(this.group2);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnTranslate);
            this.group1.Items.Add(this.btnTranslateToTurkish);
            this.group1.Items.Add(this.btnCancelTranslation);
            this.group1.Label = "Zafer Bilgisayar Çeviri";
            this.group1.Name = "group1";
            // 
            // btnTranslate
            // 
            this.btnTranslate.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnTranslate.Image = global::GeminiOutlookTranslateAdd_in.Properties.Resources.tr_to_en;
            this.btnTranslate.Label = "Türkçe → İngilizce";
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.ShowImage = true;
            this.btnTranslate.SuperTip = "Açık mail\'i Türkçe\'den İngilizce\'ye çevirir. Otomatik imla düzeltme ve noktalama " +
    "ekleme.";
            this.btnTranslate.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnTranslate_Click);
            // 
            // btnTranslateToTurkish
            // 
            this.btnTranslateToTurkish.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnTranslateToTurkish.Image = global::GeminiOutlookTranslateAdd_in.Properties.Resources.entr;
            this.btnTranslateToTurkish.Label = "İngilizce → Türkçe";
            this.btnTranslateToTurkish.Name = "btnTranslateToTurkish";
            this.btnTranslateToTurkish.ShowImage = true;
            this.btnTranslateToTurkish.SuperTip = "Açık mail\'i İngilizce\'den Türkçe\'ye çevirir.";
            this.btnTranslateToTurkish.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnTranslateToTurkish_Click);
            // 
            // btnCancelTranslation
            // 
            this.btnCancelTranslation.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnCancelTranslation.Image = global::GeminiOutlookTranslateAdd_in.Properties.Resources.Gemini_Generated_Image_esfozfesfozfesfo;
            this.btnCancelTranslation.Label = "❌ Durdur";
            this.btnCancelTranslation.Name = "btnCancelTranslation";
            this.btnCancelTranslation.ShowImage = true;
            this.btnCancelTranslation.SuperTip = "Devam eden çeviri işlemini iptal eder.";
            this.btnCancelTranslation.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnCancelTranslation_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.txtApiKey);
            this.group2.Items.Add(this.btnSaveApiKey);
            this.group2.Items.Add(this.lblApiKeyStatus);
            this.group2.Label = "API Ayarları";
            this.group2.Name = "group2";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Label = "API Key:";
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.SizeString = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.txtApiKey.Text = null;
            // 
            // btnSaveApiKey
            // 
            this.btnSaveApiKey.Label = "Kaydet";
            this.btnSaveApiKey.Name = "btnSaveApiKey";
            this.btnSaveApiKey.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSaveApiKey_Click);
            // 
            // lblApiKeyStatus
            // 
            this.lblApiKeyStatus.Label = "";
            this.lblApiKeyStatus.Name = "lblApiKeyStatus";
            // 
            // RibbonCeviri
            // 
            this.Name = "RibbonCeviri";
            this.RibbonType = "Microsoft.Outlook.Mail.Compose, Microsoft.Outlook.Mail.Read";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.RibbonCeviri_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnTranslate;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnTranslateToTurkish;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnCancelTranslation;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox txtApiKey;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSaveApiKey;
        internal Microsoft.Office.Tools.Ribbon.RibbonLabel lblApiKeyStatus;
    }

    partial class ThisRibbonCollection
    {
        internal RibbonCeviri RibbonCeviri
        {
            get { return this.GetRibbon<RibbonCeviri>(); }
        }
    }
}
