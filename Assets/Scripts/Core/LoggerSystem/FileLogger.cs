using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Solarmax
{
    public class FileLogger : Logger
    {
		private const string _LogPath = "{0}/GameLog";
        private const string _LogFormat = "{0}/{1}_{2}.{3}";
        private const int _FlusInterval = 10;

        private string mSavePath;
        private string mSaveFrontName;
        private string mSaveExtName;
        private string mFinalFilePath;
        private List<string> mWaitMessages;
        private float mTempSeconds;

        public FileLogger()
        {
            mSavePath = string.Empty;
            mSaveFrontName = "Log";
            mSaveExtName = "log";
            mFinalFilePath = string.Empty;

			mWaitMessages = new List<string>();
            mTempSeconds = 0;
        }
        public override bool Init()
        {
            return true;
        }

        public override void Tick(float interval)
        {
            mTempSeconds += interval;
            if (mTempSeconds >= _FlusInterval)
            {
                mTempSeconds = 0;

                DirectWriteAll();
            }
        }

        public override void Destroy()
        {
            DirectWriteAll();
        }

        public override void Write(string message)
        {
            mWaitMessages.Add(message);
        }

        private void DirectWriteAll()
        {
			for (int i = 0, max = mWaitMessages.Count; i < max; ++i) {
				string msg = mWaitMessages [i];
				mWaitMessages.Remove(msg);
			}
        }
        
        public void SetSavePath(string path)
        {
            mSavePath = path;
            FormatFinalFileName();
        }
        public string GetFinalFilePath()
        {
            return mFinalFilePath;
        }

        public void SetFileLogFrontName(string name)
        {
            mSaveFrontName = name;
        }

        public void SetFileLogExtName(string name)
        {
            mSaveExtName = name;
        }
        private void FormatFinalFileName()
        {
			string dir = string.Format(_LogPath, mSavePath);
			if (!Directory.Exists (dir))
			{
				Directory.CreateDirectory (dir);
			}
            mFinalFilePath = string.Format(_LogFormat, dir, mSaveFrontName, DateTime.Now.ToString("yyyy-MM-dd"), mSaveExtName);
		}
    }
}
