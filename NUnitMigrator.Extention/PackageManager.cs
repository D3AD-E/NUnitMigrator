using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using System;

namespace NUnitMigrator.Extention
{
    internal class PackageManager : IDisposable
    {
        private readonly EnvDTE.Project _project;
        private readonly IVsPackageInstallerServices _packageServices;
        private readonly IVsPackageInstaller _installer;
        private readonly IVsPackageInstallerEvents _events;
        private readonly IVsOutputWindowPane _outputWindowPane;
        private bool _disposed;

        public static PackageManager Setup(EnvDTE.Project project, IVsOutputWindowPane outputWindowPane)
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var installerServices = componentModel.GetService<IVsPackageInstallerServices>();
            var installer = componentModel.GetService<IVsPackageInstaller>();
            var events = componentModel.GetService<IVsPackageInstallerEvents>();

            return new PackageManager(project, installerServices, installer, events, outputWindowPane);
        }

        public PackageManager(EnvDTE.Project project, IVsPackageInstallerServices packageServices, IVsPackageInstaller installer,
            IVsPackageInstallerEvents events, IVsOutputWindowPane outputWindowPane)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _packageServices = packageServices ?? throw new ArgumentNullException(nameof(packageServices));
            _installer = installer ?? throw new ArgumentNullException(nameof(installer));
            _events = events;
            _outputWindowPane = outputWindowPane;

            if (_outputWindowPane != null && _events != null)
            {
                _events.PackageInstalling += OnEventsOnPackageInstalling;
                _events.PackageInstalled += OnEventsOnPackageInstalled;
            }
        }

        private void OnEventsOnPackageInstalled(IVsPackageMetadata m)
        {
           // ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputStringThreadSafe($"Installed package {m.Id}, version {m.VersionString}\n");
        }

        private void OnEventsOnPackageInstalling(IVsPackageMetadata m)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputStringThreadSafe($"Installing package {m.Id}, version {m.VersionString}\n");
        }

        public bool AddPackage(string packageId, string version = null, string source = null)
        {
            if (_packageServices.IsPackageInstalled(_project, packageId))
            {
                return false;
            }

            _installer.InstallPackage(source ?? "All", _project, packageId, version, false);
            return true;
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                if (_events != null)
                {
                    _events.PackageInstalling -= OnEventsOnPackageInstalling;
                    _events.PackageInstalled -= OnEventsOnPackageInstalled;
                }
                _disposed = true;
            }
        }
    }
}