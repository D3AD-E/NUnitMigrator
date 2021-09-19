using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace NUnitMigrator.Extention
{
    internal class StatusbarContext : IDisposable
    {
        private bool _disposed;
        private readonly IVsStatusbar m_statusbar;
        private uint _cookie;

        public StatusbarContext(IVsStatusbar statusbar)
        {
            m_statusbar = statusbar ?? throw new ArgumentNullException(nameof(statusbar));
        }

        public void UpdateProgress(string text, int complete, int total)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            m_statusbar.Progress(ref _cookie, 1, text, (uint)complete, (uint)total);
        }
        public int Clear()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return m_statusbar.Clear();
        }

        public string Text
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                m_statusbar.GetText(out var text);
                return text;
            }
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                m_statusbar.SetText(value);
            }
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!_disposed)
            {
                m_statusbar.Progress(ref _cookie, 0, string.Empty, 0, 0);
                _disposed = true;
            }
        }
    }
}