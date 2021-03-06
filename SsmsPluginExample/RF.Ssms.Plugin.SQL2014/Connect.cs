using System;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using RF.Ssms.Plugin.Common;

namespace RF.Ssms.Plugin.SQL2014
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private static SsmsAbstract _ssmsAbstract;
        private static AddinInitializer _scriptInit;
        private static object _twoOnConnectionLock = new object();

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
            AssemblyLoader.LinkAssemblyResolveEventToReloadAssembly();
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                _addInInstance = (AddIn)addInInst;
                _applicationObject = (DTE2)ServiceCache.ExtensibilityModel;

                lock (_twoOnConnectionLock)
                {
                    if (_ssmsAbstract != null)
                    {
                        _ssmsAbstract.command_manager.CustomCleanup();
                        _ssmsAbstract = null;
                    }

                    if (_ssmsAbstract == null)
                    {
                        _ssmsAbstract = new SsmsAbstract(_addInInstance, _applicationObject);
                    }

                    if (_scriptInit == null)
                    {
                        _scriptInit = new AddinInitializer();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "OnConnection");
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            if (_ssmsAbstract != null)
            {
                _ssmsAbstract.OnDisconnection();
            }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            try
            {
                _scriptInit.Initialize(_ssmsAbstract);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "OnStartupComplete");
            }
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param name="CmdName"></param>
        /// <param name="ExecuteOption"></param>
        /// <param name="VariantIn"></param>
        /// <param name="VariantOut"></param>
        /// <param name="Handled"></param>
        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            _ssmsAbstract.command_manager.Exec(CmdName, ExecuteOption, ref VariantIn, ref VariantOut, ref Handled);
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param name="CmdName"></param>
        /// <param name="NeededText"></param>
        /// <param name="StatusOption"></param>
        /// <param name="CommandText"></param>
        /// <seealso class='Exec' />
        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
        {
            _ssmsAbstract.command_manager.QueryStatus(CmdName, NeededText, ref StatusOption, ref CommandText);
        }
    }
}