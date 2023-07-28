using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace V_insertVSIX.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InsertDateTimeVersionCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int InsertDateTimeVersionSecondsExcludedCommandId = 0x0105;
        public const int InsertDateTimeVersionSecondsIncludedCommandId = 0x0106;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8c7b5c3c-0547-4a0c-a9ca-c4f111c6cf29");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDateTimeVersionCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private InsertDateTimeVersionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            CommandID insertDateTimeVersionSecondsExcludedCommandId = new CommandID(CommandSet, InsertDateTimeVersionSecondsExcludedCommandId);
            MenuCommand subItem1 = new MenuCommand(new EventHandler(InsertDateTimeVersionSecondsExcluded), insertDateTimeVersionSecondsExcludedCommandId);
            commandService.AddCommand(subItem1);

            CommandID insertDateTimeVersionSecondsIncludedCommandId = new CommandID(CommandSet, InsertDateTimeVersionSecondsIncludedCommandId);
            MenuCommand subItem2 = new MenuCommand(new EventHandler(InsertDateTimeVersionSecondsIncluded), insertDateTimeVersionSecondsIncludedCommandId);
            commandService.AddCommand(subItem2);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static InsertDateTimeVersionCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in InsertDateTimeVersionCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new InsertDateTimeVersionCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "InsertDateTimeVersionCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void InsertDateTimeVersionSecondsExcluded(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            TextSelection selectedText = (TextSelection)dte.ActiveDocument?.Selection;

            if (selectedText != null)
            {
                selectedText.Insert(GetVersionDateTimeText());
            }
        }

        private void InsertDateTimeVersionSecondsIncluded(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            TextSelection selectedText = (TextSelection)dte.ActiveDocument?.Selection;

            if (selectedText != null)
            {
                selectedText.Insert(GetVersionDateTimeText(true));
            }
        }

        private string GetVersionDateTimeText(bool includeSeconds = false)
        {
            string versionText = string.Empty;

            if (includeSeconds)
            {
                versionText = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            else
            {
                versionText = DateTime.Now.ToString("yyyyMMddHHmm");
            }

            return versionText;
        }
    }
}
