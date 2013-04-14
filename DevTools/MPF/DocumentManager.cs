// DocumentManager.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    ///     This abstract class handles opening, saving of items in the hierarchy.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class DocumentManager
    {
        #region fields

        private readonly HierarchyNode node;

        #endregion

        #region properties

        protected HierarchyNode Node
        {
            get { return node; }
        }

        #endregion

        #region ctors

        protected DocumentManager(HierarchyNode node)
        {
            this.node = node;
        }

        #endregion

        #region virtual methods

        /// <summary>
        ///     Open a document using the standard editor. This method has no implementation since a document is abstract in this context
        /// </summary>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="windowFrame">A reference to the window frame that is mapped to the document</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>NotImplementedException</returns>
        /// <remarks>See FileDocumentManager class for an implementation of this method</remarks>
        public virtual int Open(ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame windowFrame,
                                WindowFrameShowAction windowFrameAction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Open a document using a specific editor. This method has no implementation.
        /// </summary>
        /// <param name="editorFlags">Specifies actions to take when opening a specific editor. Possible editor flags are defined in the enumeration Microsoft.VisualStudio.Shell.Interop.__VSOSPEFLAGS</param>
        /// <param name="editorType">Unique identifier of the editor type</param>
        /// <param name="physicalView">Name of the physical view. If null, the environment calls MapLogicalView on the editor factory to determine the physical view that corresponds to the logical view. In this case, null does not specify the primary view, but rather indicates that you do not know which view corresponds to the logical view</param>
        /// <param name="logicalView">In MultiView case determines view to be activated by IVsMultiViewDocumentView. For a list of logical view GUIDS, see constants starting with LOGVIEWID_ defined in NativeMethods class</param>
        /// <param name="docDataExisting">IntPtr to the IUnknown interface of the existing document data object</param>
        /// <param name="frame">A reference to the window frame that is mapped to the document</param>
        /// <param name="windowFrameAction">Determine the UI action on the document window</param>
        /// <returns>NotImplementedException</returns>
        /// <remarks>See FileDocumentManager for an implementation of this method</remarks>
        public virtual int OpenWithSpecific(uint editorFlags, ref Guid editorType, string physicalView,
                                            ref Guid logicalView, IntPtr docDataExisting, out IVsWindowFrame frame,
                                            WindowFrameShowAction windowFrameAction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Close an open document window
        /// </summary>
        /// <param name="closeFlag">Decides how to close the document</param>
        /// <returns>S_OK if successful, otherwise an error is returned</returns>
        public virtual int Close(__FRAMECLOSE closeFlag)
        {
            if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed)
            {
                return VSConstants.E_FAIL;
            }

            // Get info about the document
            bool isDirty, isOpen, isOpenedByUs;
            uint docCookie;
            IVsPersistDocData ppIVsPersistDocData;
            GetDocInfo(out isOpen, out isDirty, out isOpenedByUs, out docCookie, out ppIVsPersistDocData);

            if (isOpenedByUs)
            {
                IVsUIShellOpenDocument shell =
                    Node.ProjectMgr.Site.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                Guid logicalView = Guid.Empty;
                uint grfIDO = 0;
                IVsUIHierarchy pHierOpen;
                uint[] itemIdOpen = new uint[1];
                IVsWindowFrame windowFrame;
                int fOpen;
                ErrorHandler.ThrowOnFailure(shell.IsDocumentOpen(Node.ProjectMgr.InteropSafeIVsUIHierarchy, Node.ID,
                                                                 Node.Url, ref logicalView, grfIDO, out pHierOpen,
                                                                 itemIdOpen, out windowFrame, out fOpen));

                if (windowFrame != null)
                {
                    docCookie = 0;
                    return windowFrame.CloseFrame((uint)closeFlag);
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Silently saves an open document
        /// </summary>
        /// <param name="saveIfDirty">Save the open document only if it is dirty</param>
        /// <remarks>The call to SaveDocData may return Microsoft.VisualStudio.Shell.Interop.PFF_RESULTS.STG_S_DATALOSS to indicate some characters could not be represented in the current codepage</remarks>
        public virtual void Save(bool saveIfDirty)
        {
            bool isDirty, isOpen, isOpenedByUs;
            uint docCookie;
            IVsPersistDocData persistDocData;
            GetDocInfo(out isOpen, out isDirty, out isOpenedByUs, out docCookie, out persistDocData);
            if (isDirty && saveIfDirty && persistDocData != null)
            {
                string name;
                int cancelled;
                ErrorHandler.ThrowOnFailure(persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out name,
                                                                       out cancelled));
            }
        }

        #endregion

        #region helper methods

        /// <summary>
        ///     Get document properties from RDT
        /// </summary>
        internal void GetDocInfo(
            out bool isOpen, // true if the doc is opened
            out bool isDirty, // true if the doc is dirty
            out bool isOpenedByUs, // true if opened by our project
            out uint docCookie, // VSDOCCOOKIE if open
            out IVsPersistDocData persistDocData)
        {
            isOpen = isDirty = isOpenedByUs = false;
            docCookie = (uint)ShellConstants.VSDOCCOOKIE_NIL;
            persistDocData = null;

            if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed)
            {
                return;
            }

            IVsHierarchy hierarchy;
            uint vsitemid = VSConstants.VSITEMID_NIL;

            VsShellUtilities.GetRDTDocumentInfo(node.ProjectMgr.Site, node.Url, out hierarchy, out vsitemid,
                                                out persistDocData, out docCookie);

            if (hierarchy == null || docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL)
            {
                return;
            }

            isOpen = true;
            // check if the doc is opened by another project
            if (Utilities.IsSameComObject(node.ProjectMgr.InteropSafeIVsHierarchy, hierarchy))
            {
                isOpenedByUs = true;
            }

            if (persistDocData != null)
            {
                int isDocDataDirty;
                ErrorHandler.ThrowOnFailure(persistDocData.IsDocDataDirty(out isDocDataDirty));
                isDirty = (isDocDataDirty != 0);
            }
        }

        protected string GetOwnerCaption()
        {
            Debug.Assert(node != null, "No node has been initialized for the document manager");

            object pvar;
            ErrorHandler.ThrowOnFailure(node.GetProperty(node.ID, (int)__VSHPROPID.VSHPROPID_Caption, out pvar));

            return (pvar as string);
        }

        protected static void CloseWindowFrame(ref IVsWindowFrame windowFrame)
        {
            if (windowFrame != null)
            {
                try
                {
                    ErrorHandler.ThrowOnFailure(windowFrame.CloseFrame(0));
                }
                finally
                {
                    windowFrame = null;
                }
            }
        }

        protected string GetFullPathForDocument()
        {
            string fullPath = String.Empty;

            Debug.Assert(node != null, "No node has been initialized for the document manager");

            // Get the URL representing the item
            fullPath = node.GetMkDocument();

            Debug.Assert(!String.IsNullOrEmpty(fullPath),
                         "Could not retrive the fullpath for the node" + Node.ID.ToString(CultureInfo.CurrentCulture));
            return fullPath;
        }

        #endregion

        #region static methods

        /// <summary>
        ///     Updates the caption for all windows associated to the document.
        /// </summary>
        /// <param name="site">The service provider.</param>
        /// <param name="caption">The new caption.</param>
        /// <param name="docData">The IUnknown interface to a document data object associated with a registered document.</param>
        public static void UpdateCaption(IServiceProvider site, string caption, IntPtr docData)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }

            if (String.IsNullOrEmpty(caption))
            {
                throw new ArgumentException(
                    SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "caption");
            }

            IVsUIShell uiShell = site.GetService(typeof(SVsUIShell)) as IVsUIShell;

            // We need to tell the windows to update their captions. 
            IEnumWindowFrames windowFramesEnum;
            ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out windowFramesEnum));
            IVsWindowFrame[] windowFrames = new IVsWindowFrame[1];
            uint fetched;
            while (windowFramesEnum.Next(1, windowFrames, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                IVsWindowFrame windowFrame = windowFrames[0];
                object data;
                ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out data));
                IntPtr ptr = Marshal.GetIUnknownForObject(data);
                try
                {
                    if (ptr == docData)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption,
                                                                            caption));
                    }
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.Release(ptr);
                    }
                }
            }
        }

        /// <summary>
        ///     Rename document in the running document table from oldName to newName.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="oldName">Full path to the old name of the document.</param>
        /// <param name="newName">Full path to the new name of the document.</param>
        /// <param name="newItemId">The new item id of the document</param>
        public static void RenameDocument(IServiceProvider site, string oldName, string newName, uint newItemId)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }

            if (String.IsNullOrEmpty(oldName))
            {
                throw new ArgumentException(
                    SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "oldName");
            }

            if (String.IsNullOrEmpty(newName))
            {
                throw new ArgumentException(
                    SR.GetString(SR.ParameterCannotBeNullOrEmpty, CultureInfo.CurrentUICulture), "newName");
            }

            if (newItemId == VSConstants.VSITEMID_NIL)
            {
                throw new ArgumentNullException("newItemId");
            }

            IVsRunningDocumentTable pRDT = site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            IVsUIShellOpenDocument doc = site.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            if (pRDT == null || doc == null)
                return;

            IVsHierarchy pIVsHierarchy;
            uint itemId;
            IntPtr docData;
            uint uiVsDocCookie;
            ErrorHandler.ThrowOnFailure(pRDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, oldName,
                                                                 out pIVsHierarchy, out itemId, out docData,
                                                                 out uiVsDocCookie));

            if (docData != IntPtr.Zero)
            {
                try
                {
                    IntPtr pUnk = Marshal.GetIUnknownForObject(pIVsHierarchy);
                    Guid iid = typeof(IVsHierarchy).GUID;
                    IntPtr pHier;
                    Marshal.QueryInterface(pUnk, ref iid, out pHier);
                    try
                    {
                        ErrorHandler.ThrowOnFailure(pRDT.RenameDocument(oldName, newName, pHier, newItemId));
                    }
                    finally
                    {
                        if (pHier != IntPtr.Zero)
                            Marshal.Release(pHier);
                        if (pUnk != IntPtr.Zero)
                            Marshal.Release(pUnk);
                    }
                }
                finally
                {
                    Marshal.Release(docData);
                }
            }
        }

        #endregion
    }
}
