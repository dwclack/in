namespace AlogMkFtp
{
    partial class AlogMkFtpService
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.timerFtpUpload = new System.Timers.Timer();
            this.timerFtpDownload = new System.Timers.Timer();
            this.timerFtpDownloadSecond = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpUpload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpDownload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpDownloadSecond)).BeginInit();
            // 
            // timerFtpUpload
            // 
            this.timerFtpUpload.Interval = 600000D;
            this.timerFtpUpload.Elapsed += new System.Timers.ElapsedEventHandler(this.timerFtpUpload_Elapsed);
            // 
            // timerFtpDownload
            // 
            this.timerFtpDownload.Interval = 600000D;
            this.timerFtpDownload.Elapsed += new System.Timers.ElapsedEventHandler(this.timerFtpDownload_Elapsed);
            // 
            // timerFtpDownloadSecond
            // 
            this.timerFtpDownloadSecond.Interval = 600000D;
            this.timerFtpDownloadSecond.Elapsed += new System.Timers.ElapsedEventHandler(this.timerFtpDownloadSecond_Elapsed);
            // 
            // AlogMkFtpService
            // 
            this.ServiceName = "AlogMkFtpService";
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpUpload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpDownload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timerFtpDownloadSecond)).EndInit();

        }

        #endregion

        private System.Timers.Timer timerFtpUpload;
        private System.Timers.Timer timerFtpDownload;
        private System.Timers.Timer timerFtpDownloadSecond;
    }
}
