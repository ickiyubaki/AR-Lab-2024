using System;
using System.Collections.Generic;

namespace Common.Scripts.Utils
{
    public class NativeDataShare : Singleton<NativeDataShare>
    {
        private Func<string> _getSubject = () => "";
        private Func<string> _getText = () => "";
        private Func<List<string>> _getFiles = () => new List<string>();
        private NativeShare.ShareResultCallback _callback = (result, target) => { };

        public void SetParameters(Func<string> getSubject, Func<string> getText, Func<List<string>> getFiles,
            NativeShare.ShareResultCallback callback)
        {
            ResetParameters();
            _getSubject = getSubject;
            _getText = getText;
            _getFiles = getFiles;
            _callback = callback;
        }

        public void ResetParameters()
        {
            _getSubject = () => "";
            _getText = () => "";
            _getFiles = () => new List<string>();
            _callback = (result, target) => { };
        }

        public void Share()
        {
            var ns = new NativeShare();
            
            foreach (var path in _getFiles())
            {
                ns.AddFile(path);
            }

            ns.SetSubject(_getSubject()).SetText(_getText()).SetCallback(_callback).Share();
        }
    }
}